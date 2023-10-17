using Microsoft.AspNetCore.Http.Json;
using HipHopPizza_ServerSide.Models;
using HipHopPizza_ServerSide;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using EFCore.NamingConventions;
using System.ComponentModel.DataAnnotations;

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
app.MapGet("/products/{id}", (HipHopPizzaDbContext db, int id) =>
{
    return db.Products.FirstOrDefault(p => p.Id == id);

});

app.MapGet("/products/pizza", (HipHopPizzaDbContext db) =>
{
    return db.Products.Where(x => x.Category == "pizza");
});

app.MapGet("/products/wings", (HipHopPizzaDbContext db) =>
{
    return db.Products.Where(x => x.Category == "wings");
});

app.MapGet("/products/drinks", (HipHopPizzaDbContext db) =>
{
    return db.Products.Where(x => x.Category == "drink");
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

#region Orders

//Get All Orders
app.MapGet("/orders/all", (HipHopPizzaDbContext db) =>
{
    try
    {
        var ReturnAll = db.Orders.ToList();
        if (ReturnAll.Count == 0)
        {
            return Results.NotFound("Sorry for the inconvenience! There aren't any orders in the system.");
        }
        return Results.Ok(ReturnAll);
    }
    catch (Exception ex)
    {
            return Results.Problem(ex.Message);
    }
});

// Get Single Order
app.MapGet("/orders/{id}", (HipHopPizzaDbContext db, int id) =>
{
    try
    {
        Order SingleOrder = db.Orders.SingleOrDefault(x => x.Id == id);
        if (SingleOrder == null)
        {
            return Results.NotFound("Sorry for the inconvenience! This order does not exist.");
        }
        return Results.Ok(SingleOrder);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Get Products from Order
app.MapGet("/orders/products", (HipHopPizzaDbContext db, int id) =>
{
    try
    {
        var SingleOrder = db.Orders
            .Where(db => db.Id == id)
            .Include(Order => Order.ProductList)
            .ToList();

        if (SingleOrder == null)
        {
            return Results.NotFound("Sorry for the inconvenience! This order does not exist.");
        }
        return Results.Ok(SingleOrder);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Create Order
app.MapPost("/orders/new", (HipHopPizzaDbContext db, Order payload) =>
{
        db.Orders.Add(payload);
        db.SaveChanges();
        return Results.Ok();
});

// Add Product to Order
app.MapPost("/orders/productslist", (HipHopPizzaDbContext db, int orderId, Product payload) =>
{
    // Retrieve object reference of Orders in order to manipulate (Not a query result)
    var order = db.Orders
    .Where(o => o.Id == orderId)
    .Include(o => o.ProductList)
    .FirstOrDefault();

    if (order == null)
    {
        return Results.NotFound("Order not found.");
    }

    order.ProductList.Add(payload);

    db.SaveChanges();

    return Results.Ok(order);

});

// Delete Order
app.MapDelete("/orders/delete", (HipHopPizzaDbContext db, int Id) =>
{
    var DeletedOrder = db.Orders.FirstOrDefault(o => o.Id == Id);

    try
    {
        if (DeletedOrder == null)
        {
            return Results.NotFound("Sorry, the order you requested has not been found.");
        }
        db.Orders.Remove(DeletedOrder);
        db.SaveChanges();
        return Results.Ok(DeletedOrder);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Delete Products from Order
app.MapDelete("/orders/productslist/remove", (HipHopPizzaDbContext db, int orderId, int productId) =>
{
    var targetOrder = db.Orders.FirstOrDefault(o => o.Id == orderId);
    // grab from orders

    //grab from products

 
/*        if (targetOrder == null)
        {
           return Results.NotFound("Sorry, the order you requested has not been found.");
        } else if (targetOrder.ProductList != null)
        {
            var targetProduct = targetOrder?.ProductList.FirstOrDefault(p => p.Id == productId);
            db.Orders.
        }*/

    

  
    try
    {
        var SingleOrder = db.Orders
            .Where(db => db.Id == orderId)
            .Include(Order => Order.ProductList)
            .ToList();

        if (SingleOrder == null)
        {
            return Results.NotFound("Sorry for the inconvenience! This order does not exist.");
        }

        foreach (var item in SingleOrder)
        {
            if (item.Id == productId)
            {
                SingleOrder.Remove(item);
            }
        }

        db.SaveChanges();
        return Results.Ok(SingleOrder);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }

/*
    List<Order> y = new List<Order>();
    var result = y;
  foreach(Order order in db.Orders)
    {
        if (order.Id == orderId)
        {
            if (order.ProductList != null)
            {
                foreach (var item in order.ProductList)
                {
                    if (item.Id == productId)
                        order.ProductList.Remove(item);
                    result.Add(order);
                    db.SaveChanges();
                }          
            }
        }
        else return Results.NotFound("Order has not been found!");
    }
    return Results.Ok(result);*/

/*    SelectedOrder.ProductList.Remove(removeOrderProduct);
    db.SaveChanges();*/
/*    return Results.Ok(SelectedOrder);*/

    /*  if (SelectedOrder == null)
      {
          return Results.NotFound("Order not found.");
      }
      if (removeOrderProduct == null)
      {
          return Results.NotFound("Product not found.");
      }

      SelectedOrder.ProductList?.Remove(removeOrderProduct);
      db.SaveChanges();
      return Results.Accepted("Order removed:", removeOrderProduct);*/

});

#endregion

app.Run();