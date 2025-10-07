using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TechFood_Solutions.Models.Seeds
{
    public class MenuItemSeed : IEntityTypeConfiguration<MenuItem>
    {
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            builder.HasData(
                new MenuItem
                {
                    Id = 1,
                    Nombre = "Pizza Margherita",
                    Descripcion = "Tomate, mozzarella, albahaca",
                    Precio = 8.99m,
                    ImagenUrl = "margherita.jpg",
                    RestaurantId = 1
                },
                new MenuItem
                {
                    Id = 2,
                    Nombre = "Pizza Pepperoni",
                    Descripcion = "Mozzarella y pepperoni",
                    Precio = 9.99m,
                    ImagenUrl = "spepperoni.jpg",
                    RestaurantId = 1
                },
                new MenuItem
                {
                    Id = 3,
                    Nombre = "Sushi Roll de Salmón",
                    Descripcion = "Rollos con salmón fresco",
                    Precio = 12.50m,
                    ImagenUrl = "salmon-roll.jpg",
                    RestaurantId = 2
                }
            );
        }
    }
}
