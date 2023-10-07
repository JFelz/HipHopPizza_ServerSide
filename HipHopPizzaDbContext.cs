using Microsoft.EntityFrameworkCore;
using HipHopPizza_ServerSide.Models;
using EFCore.NamingConventions;

namespace HipHopPizza_ServerSide
{
    public class HipHopPizzaDbContext : DbContext
    {
        public DbSet<Cashier> Cashier { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }

        public HipHopPizzaDbContext(DbContextOptions<HipHopPizzaDbContext> context) : base(context)
        {
            
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder
        .UseNpgsql()
        .UseSnakeCaseNamingConvention();
    }
}
