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
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aplicar seeds existentes
            modelBuilder.ApplyConfiguration(new RestaurantSeed());
            modelBuilder.ApplyConfiguration(new MenuItemSeed());

            // Solo configuraciones que NO se pueden hacer con Data Annotations

            // Order: Configurar comportamiento de eliminación y valores por defecto
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Restaurant)
                .WithMany()
                .HasForeignKey(o => o.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict); // No eliminar restaurante si tiene órdenes

            modelBuilder.Entity<Order>()
                .Property(o => o.Estado)
                .HasDefaultValue("Pendiente");

            modelBuilder.Entity<Order>()
                .Property(o => o.FechaOrden)
                .HasDefaultValueSql("GETDATE()");

            // OrderItem: Configurar comportamientos de eliminación
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany()
                .HasForeignKey(oi => oi.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict); // No eliminar item del menú si está en órdenes
        }
    }
}