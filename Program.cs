using HipHopPizza_ServerSide.Models;
using HipHopPizza_ServerSide;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using EFCore.NamingConventions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:7143",
                                              "http://localhost:3000")
                                               .AllowAnyHeader()
                                               .AllowAnyMethod();
                      });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core
builder.Services.AddNpgsql<HipHopPizzaDbContext>(builder.Configuration["HipHopPizzaDbConnectionString"]);

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// API

#region Auth

app.MapPost("/register", (HipHopPizzaDbContext db, Cashier payload) =>
{
    Cashier NewUser = new Cashier()
    {
    Name = payload.Name,
    Email = payload.Email,
    PhoneNumber = payload.PhoneNumber,
    ImageURL = payload.ImageURL,
    Uid = payload.Uid,
    };
    db.Cashier.Add(NewUser);
    db.SaveChanges();
    return Results.Ok(NewUser.Name);
});

app.MapGet("/checkuser/{uid}", (HipHopPizzaDbContext db, string uid) =>
{
    var user = db.Cashier.Where(x => x.Uid == uid).ToList();
    if (uid == null)
    {
        return Results.NotFound("Sorry, Cashier not found!");
    }
    else
    {
        return Results.Ok(user);
    }
});

// Get users
app.MapGet("/cashier", (HipHopPizzaDbContext db) =>
{
    return db.Cashier.ToList();
});

// View Single User

app.MapGet("/cashier/{uid}", (HipHopPizzaDbContext db, string uid) =>
{
    return db.Cashier.FirstOrDefault(x => x.Uid == uid);
});

// Update User
app.MapPut("/cashier/update/{uid}", (HipHopPizzaDbContext db, string uid, Cashier NewUser) =>
{
    Cashier SelectedCashier = db.Cashier.FirstOrDefault(x => x.Uid == uid);
    if (SelectedCashier == null)
    {
        return Results.NotFound("This cashier is not found in the database. Please Try again!");
    }

    SelectedCashier.Name = NewUser.Name;
    SelectedCashier.Email = NewUser.Email;
    SelectedCashier.PhoneNumber = NewUser.PhoneNumber;
    SelectedCashier.ImageURL = NewUser.ImageURL;
    db.SaveChanges();
    return Results.Created("/cashier/update/{uid}", SelectedCashier);

});

// View Users post
app.MapGet("/cashier/{ID}/posts", (HipHopPizzaDbContext db, int ID) =>
{
/*
    return db.Cashier.Where(x => x.Id == ID)
                   .Include(p => p.Posts)
                   .ToList();*/
});

#endregion

#region Products

// Get All Products
app.MapGet("/products", (HipHopPizzaDbContext db) =>
{
    return db.Products.ToList();
});

// Get Single Product 
app.MapGet("/products/{Id}", (HipHopPizzaDbContext db, int Id) =>
{
    return db.Products.FirstOrDefault(p => p.Id == Id);

});

// Add New Product
app.MapPost("/products/new", async (HipHopPizzaDbContext db, Product NewProduct) =>
{
    try
    {
        db.Products.Add(NewProduct);
        await db.SaveChangesAsync();

        // Return only the label
        return Results.Ok(NewProduct.Title);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = "Failed to create product", message = ex.Message });
    }
});

// Update an existing Product
app.MapPut("/Product/Update", async (HipHopPizzaDbContext db, int id, Product UpdatedProduct) =>
{
    var existingProduct = await db.Products.FindAsync(id);
    if (existingProduct == null)
        return Results.NotFound();

    existingProduct.Title = UpdatedProduct.Title;
    existingProduct.Description = UpdatedProduct.Description;
    existingProduct.ImageURL = UpdatedProduct.ImageURL;
    existingProduct.Price = UpdatedProduct.Price;
    existingProduct.Category = UpdatedProduct.Category;
    await db.SaveChangesAsync();

    return Results.Ok(existingProduct);
});

// Delete a Product
app.MapDelete("/Product/Delete", async (HipHopPizzaDbContext db, int id) =>
{
    var prod = await db.Products.FindAsync(id);
    if (prod == null)
        return Results.NotFound();

    db.Products.Remove(prod);
    await db.SaveChangesAsync();

    return Results.Ok(prod);
});

#endregion


app.Run();