using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechFood_Solutions.Models;

namespace TechFood_Solutions.Controllers
{
    [Authorize(Roles = RoleNames.Client)]
    public class ClienteController : Controller
    {
        private readonly TechFoodDbContext _context;

        public ClienteController(TechFoodDbContext context)
        {
            _context = context;
        }

        // GET: Cliente/Restaurantes - Vista principal de restaurantes para clientes
        public async Task<IActionResult> Restaurantes()
        {
            var restaurantes = await _context.Restaurantes // Correcto: usar "Restaurantes"
                .Include(r => r.MenuItems)
                .ToListAsync();

            return View("Restaurantes/Index", restaurantes); // Especificar la ruta de la vista
        }

        // GET: Cliente/Menu/5 - Ver menú de un restaurante específico
        public async Task<IActionResult> Menu(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
            {
                return NotFound();
            }

            return View("Restaurantes/Menu", restaurant); // Especificar la ruta de la vista
        }

        // GET: Cliente/BuscarRestaurantes - Búsqueda de restaurantes
        public async Task<IActionResult> BuscarRestaurantes(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction(nameof(Restaurantes));
            }

            var restaurants = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .Where(r => r.Nombre.Contains(searchTerm) ||
                           r.Descripcion.Contains(searchTerm) ||
                           r.MenuItems.Any(m => m.Nombre.Contains(searchTerm)))
                .ToListAsync();

            ViewData["SearchTerm"] = searchTerm;
            return View("Restaurantes/Index", restaurants);
        }

        // GET: Cliente/DetalleRestaurante/5 - Detalles de un restaurante
        public async Task<IActionResult> DetalleRestaurante(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (restaurant == null)
            {
                return NotFound();
            }

            return View("Restaurantes/Details", restaurant);
        }

        // API endpoint para obtener restaurantes (útil para JavaScript/AJAX)
        [HttpGet]
        public async Task<IActionResult> GetRestaurants()
        {
            var restaurants = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .Select(r => new
                {
                    r.Id,
                    r.Nombre,
                    r.LogoUrl,
                    r.Descripcion,
                    MenuItems = r.MenuItems.Select(m => new
                    {
                        m.Id,
                        m.Nombre,
                        m.Descripcion,
                        m.Precio,
                        m.ImagenUrl
                    })
                })
                .ToListAsync();

            return Json(restaurants);
        }

        // API endpoint para obtener menú de un restaurante específico
        [HttpGet]
        public async Task<IActionResult> GetMenu(int id)
        {
            var restaurant = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
            {
                return NotFound();
            }

            var menuData = new
            {
                RestaurantId = restaurant.Id,
                RestaurantName = restaurant.Nombre,
                RestaurantLogo = restaurant.LogoUrl,
                RestaurantDescription = restaurant.Descripcion,
                MenuItems = restaurant.MenuItems.Select(m => new
                {
                    m.Id,
                    m.Nombre,
                    m.Descripcion,
                    m.Precio,
                    m.ImagenUrl
                })
            };

            return Json(menuData);
        }

        // Método para filtrar por categoría o tipo de comida (opcional)
        public async Task<IActionResult> FiltrarRestaurantes(string categoria)
        {
            var query = _context.Restaurantes.Include(r => r.MenuItems).AsQueryable();

            if (!string.IsNullOrEmpty(categoria))
            {
                // Ejemplo: filtrar por descripción que contenga el tipo de comida
                query = query.Where(r => r.Descripcion.Contains(categoria) ||
                                        r.MenuItems.Any(m => m.Descripcion.Contains(categoria)));
            }

            var restaurants = await query.ToListAsync();
            ViewData["CategoriaActual"] = categoria;

            return View("Restaurantes/Index", restaurants);
        }

        // Método helper para verificar si existe un restaurante
        private async Task<bool> RestaurantExistsAsync(int id)
        {
            return await _context.Restaurantes.AnyAsync(e => e.Id == id);
        }
    }
}