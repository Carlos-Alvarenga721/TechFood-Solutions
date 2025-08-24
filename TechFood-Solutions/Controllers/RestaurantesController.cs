using Microsoft.AspNetCore.Mvc;
using TechFood_Solutions.Models;

namespace TuProyecto.Controllers.Cliente
{
    public class RestaurantesController : Controller
    {
        private static List<Restaurant> ObtenerRestaurantesMock()
        {
            return new List<Restaurant>
            {
                new Restaurant
                {
                    Id = 1,
                    Nombre = "Pizzería Napoli",
                    LogoUrl = "/images/pizzeria-logo.png",
                    Descripcion = "Auténtica pizza napolitana al horno de leña.",
                    MenuItems = new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Id = 1,
                            Nombre = "Pizza Margherita",
                            Descripcion = "Tomate, mozzarella, albahaca",
                            Precio = 8.99m,
                            ImagenUrl = "/images/margherita.jpg"
                        },
                        new MenuItem
                        {
                            Id = 2,
                            Nombre = "Pizza Pepperoni",
                            Descripcion = "Mozzarella y pepperoni",
                            Precio = 9.99m,
                            ImagenUrl = "/images/pepperoni.jpg"
                        }
                    }
                },
                new Restaurant
                {
                    Id = 2,
                    Nombre = "Sushi House",
                    LogoUrl = "/images/sushi-logo.png",
                    Descripcion = "Sushi fresco y cocina japonesa.",
                    MenuItems = new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Id = 3,
                            Nombre = "Sushi Roll de Salmón",
                            Descripcion = "Rollos con salmón fresco",
                            Precio = 12.50m,
                            ImagenUrl = "/images/salmon-roll.jpg"
                        }
                    }
                }
            };
        }

        public IActionResult Index()
        {
            var restaurantes = ObtenerRestaurantesMock();
            return View("~/Views/Cliente/Restaurantes/Index.cshtml", restaurantes);
        }

        public IActionResult Menu(int id)
        {
            var restaurante = ObtenerRestaurantesMock().FirstOrDefault(r => r.Id == id);
            if (restaurante == null) return NotFound();

            return View("~/Views/Cliente/Restaurantes/Menu.cshtml", restaurante);
        }

    }
}
