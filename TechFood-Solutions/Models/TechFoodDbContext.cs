using Microsoft.EntityFrameworkCore;
using TechFood_Solutions.Models.Seeds;

namespace TechFood_Solutions.Models
{
    public class TechFoodDbContext : DbContext
    {
        public TechFoodDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Restaurant> Restaurantes { get; set; }
        
        public DbSet<MenuItem> MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new RestaurantSeed());
            modelBuilder.ApplyConfiguration(new MenuItemSeed());
        }
    }
}
