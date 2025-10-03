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
        public DbSet<User> Users { get; set; }  

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new RestaurantSeed());
            modelBuilder.ApplyConfiguration(new MenuItemSeed());

            modelBuilder.Entity<User>()
                .HasOne(u => u.Restaurant)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
