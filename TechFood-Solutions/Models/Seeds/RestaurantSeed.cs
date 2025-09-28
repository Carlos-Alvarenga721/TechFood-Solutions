using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TechFood_Solutions.Models.Seeds
{
    public class RestaurantSeed : IEntityTypeConfiguration<Restaurant>
    {
        public void Configure(EntityTypeBuilder<Restaurant> builder)
        {
            builder.HasData(
                new Restaurant
                {
                    Id = 1,
                    Nombre = "Pizzería Napoli",
                    LogoUrl = "pizzeria-logo.png",
                    Descripcion = "Auténtica pizza napolitana al horno de leña."
                },
                new Restaurant
                {
                    Id = 2,
                    Nombre = "Sushi House",
                    LogoUrl = "sushi-logo.png",
                    Descripcion = "Sushi fresco y cocina japonesa."
                }
            );
        }
    }
}
