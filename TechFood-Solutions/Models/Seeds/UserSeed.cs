using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TechFood_Solutions.Models.Seeds
{
    public class UserSeed : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Usuarios hardcoded solo para testing de órdenes
            builder.HasData(
                // Cliente 1 - Para probar órdenes
                new User
                {
                    Id = 1,
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    Dui = "12345678-9",
                    Rol = UserRole.Client,
                    RestaurantId = null
                },

                // Cliente 2 - Para probar múltiples usuarios
                new User
                {
                    Id = 2,
                    Nombre = "María",
                    Apellido = "García",
                    Dui = "98765432-1",
                    Rol = UserRole.Client,
                    RestaurantId = null
                },

                // Cliente 3 - Usuario de prueba adicional
                new User
                {
                    Id = 3,
                    Nombre = "Carlos",
                    Apellido = "López",
                    Dui = "11223344-5",
                    Rol = UserRole.Client,
                    RestaurantId = null
                }
            );
        }
    }
}