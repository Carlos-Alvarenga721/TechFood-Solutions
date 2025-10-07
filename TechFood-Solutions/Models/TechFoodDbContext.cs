using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechFood_Solutions.Models.Seeds;

namespace TechFood_Solutions.Models
{
    // Ahora hereda de IdentityDbContext para integrar Identity con claves int
    public class TechFoodDbContext : IdentityDbContext<User, ApplicationRole, int>
    {
        public TechFoodDbContext(DbContextOptions options) : base(options)
        {
        }

        // Tablas del dominio
        public DbSet<Restaurant> Restaurantes { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        // NOTA: Users viene desde IdentityDbContext<User, ...>
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar seeds (mantener tus seeds existentes)
            modelBuilder.ApplyConfiguration(new RestaurantSeed());
            modelBuilder.ApplyConfiguration(new MenuItemSeed());

            // Relación User → Restaurant
            modelBuilder.Entity<User>()
                .HasOne(u => u.Restaurant)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Imponer en BD la regla: sólo Associated (valor 2) puede tener RestaurantId NOT NULL
            // (Admin=0, Client=1, Associated=2)
            modelBuilder.Entity<User>().ToTable(tb =>
            {
                tb.HasCheckConstraint(
                    "CK_User_AssociatedHasRestaurant",
                    "(Rol != 2) OR (RestaurantId IS NOT NULL)"
                );
            });


            // Orders → Restaurant
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

            // OrderItems → MenuItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany()
                .HasForeignKey(oi => oi.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItems → Order
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Al eliminar orden, eliminar sus items
        }
    }
}
