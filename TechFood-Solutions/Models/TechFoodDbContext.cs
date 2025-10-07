using Microsoft.EntityFrameworkCore;
using TechFood_Solutions.Models.Seeds;

namespace TechFood_Solutions.Models
{
    public class TechFoodDbContext : DbContext
    {
        public TechFoodDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Restaurant> Restaurantes { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Orders: Relación uno a muchos
            modelBuilder.Entity<User>()
                .HasMany<Order>()
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict); // No eliminar usuario si tiene órdenes

            // Order - Restaurant: Relación muchos a uno
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Restaurant)
                .WithMany()
                .HasForeignKey(o => o.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order: Valores por defecto
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
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Aplicar seeds existentes
            modelBuilder.ApplyConfiguration(new RestaurantSeed());
            modelBuilder.ApplyConfiguration(new MenuItemSeed());

            modelBuilder.ApplyConfiguration(new UserSeed());
        }
    }
}