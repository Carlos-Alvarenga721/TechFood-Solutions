using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechFood_Solutions.Models;
using TechFood_Solutions.Services; // ← AGREGAR

namespace TechFood_Solutions.Controllers
{
    public class AsociadoController : Controller
    {
        private readonly TechFoodDbContext _context;
        private readonly ILogger<AsociadoController> _logger;
        private readonly IImageService _imageService; // ← AGREGAR

        public AsociadoController(
            TechFoodDbContext context,
            ILogger<AsociadoController> logger,
            IImageService imageService) // ← AGREGAR
        {
            _context = context;
            _logger = logger;
            _imageService = imageService;
        }

        // GET: Asociado
        public async Task<IActionResult> Index()
        {
            int restaurantId = 1;

            var restaurant = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null)
            {
                return NotFound("No se encontró el restaurante asociado a este usuario.");
            }

            return View(restaurant);
        }

        // GET: Asociado/EditarRestaurante/5
        public async Task<IActionResult> EditarRestaurante(int? id)
        {
            _logger.LogInformation($"GET EditarRestaurante: ID = {id}");

            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurantes.FindAsync(id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // POST: Asociado/EditarRestaurante/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRestaurante(
            int id,
            [Bind("Id,Nombre,LogoUrl,Descripcion")] Restaurant restaurant,
            IFormFile logoFile) // ← PARÁMETRO PARA EL ARCHIVO
        {
            _logger.LogInformation($"POST EditarRestaurante: ID = {id}");

            if (id != restaurant.Id)
            {
                return NotFound();
            }

            // Si se subió una imagen nueva
            if (logoFile != null && logoFile.Length > 0)
            {
                _logger.LogInformation($"Procesando imagen: {logoFile.FileName} ({logoFile.Length} bytes)");

                // Guardar la imagen en wwwroot/images/restaurantes
                var fileName = await _imageService.SaveImageAsync(logoFile, "restaurantes");

                if (!string.IsNullOrEmpty(fileName))
                {
                    // Si había una imagen anterior, eliminarla
                    if (!string.IsNullOrEmpty(restaurant.LogoUrl))
                    {
                        _imageService.DeleteImage(restaurant.LogoUrl, "restaurantes");
                    }

                    // Actualizar con el nuevo nombre de archivo
                    restaurant.LogoUrl = fileName;
                    _logger.LogInformation($"Imagen guardada: {fileName}");
                }
                else
                {
                    ModelState.AddModelError("logoFile", "Error al guardar la imagen. Verifica el formato y tamaño.");
                    return View(restaurant);
                }
            }
            else
            {
                // Si no se subió imagen, mantener la existente
                var existingRestaurant = await _context.Restaurantes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (existingRestaurant != null)
                {
                    restaurant.LogoUrl = existingRestaurant.LogoUrl;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(restaurant);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Información del restaurante actualizada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, $"Error de concurrencia para ID {id}");

                    if (!RestaurantExists(restaurant.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error al actualizar restaurante {id}");
                    TempData["Error"] = "Error al actualizar: " + ex.Message;
                    return View(restaurant);
                }
            }

            return View(restaurant);
        }

        // GET: Asociado/GestionarMenu
        public async Task<IActionResult> GestionarMenu()
        {
            int restaurantId = 1;

            var restaurant = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // GET: Asociado/EditarProducto/5
        public async Task<IActionResult> EditarProducto(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems
                .Include(m => m.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return View(menuItem);
        }

        // POST: Asociado/EditarProducto/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProducto(
            int id,
            [Bind("Id,Nombre,Descripcion,Precio,ImagenUrl,RestaurantId")] MenuItem menuItem,
            IFormFile imagenFile) // ← Para productos también
        {
            _logger.LogInformation($"POST EditarProducto: ID = {id}");

            if (id != menuItem.Id)
            {
                return NotFound();
            }

            // Obtener el nombre del restaurante para la subcarpeta
            var restaurant = await _context.Restaurantes.FindAsync(menuItem.RestaurantId);
            if (restaurant == null)
            {
                return NotFound("Restaurante no encontrado");
            }

            // Si se subió imagen nueva
            if (imagenFile != null && imagenFile.Length > 0)
            {
                var fileName = await _imageService.SaveImageAsync(imagenFile, "items", restaurant.Nombre);

                if (!string.IsNullOrEmpty(fileName))
                {
                    if (!string.IsNullOrEmpty(menuItem.ImagenUrl))
                    {
                        _imageService.DeleteImage(menuItem.ImagenUrl, "items", restaurant.Nombre);
                    }
                    menuItem.ImagenUrl = fileName;
                }
                else
                {
                    ModelState.AddModelError("imagenFile", "Error al guardar la imagen.");
                    menuItem.Restaurant = restaurant;
                    return View(menuItem);
                }
            }
            else
            {
                var existing = await _context.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                if (existing != null)
                {
                    menuItem.ImagenUrl = existing.ImagenUrl;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(menuItem);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Producto actualizado correctamente.";
                    return RedirectToAction(nameof(GestionarMenu));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuItemExists(menuItem.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            menuItem.Restaurant = restaurant;
            return View(menuItem);
        }

        // GET: Asociado/CrearProducto
        public async Task<IActionResult> CrearProducto()
        {
            int restaurantId = 1;

            var restaurant = await _context.Restaurantes.FindAsync(restaurantId);
            if (restaurant == null)
            {
                return NotFound();
            }

            var menuItem = new MenuItem
            {
                RestaurantId = restaurantId,
                Restaurant = restaurant
            };

            return View(menuItem);
        }

        // POST: Asociado/CrearProducto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearProducto(
            [Bind("Nombre,Descripcion,Precio,ImagenUrl,RestaurantId")] MenuItem menuItem,
            IFormFile imagenFile)
        {
            if (imagenFile != null && imagenFile.Length > 0)
            {
                var fileName = await _imageService.SaveImageAsync(imagenFile, "items");
                if (!string.IsNullOrEmpty(fileName))
                {
                    menuItem.ImagenUrl = fileName;
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(menuItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Producto creado correctamente.";
                return RedirectToAction(nameof(GestionarMenu));
            }

            menuItem.Restaurant = await _context.Restaurantes.FindAsync(menuItem.RestaurantId);
            return View(menuItem);
        }

        // POST: Asociado/EliminarProducto/5
        [HttpPost, ActionName("EliminarProducto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProductoConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem != null)
            {
                // Eliminar imagen si existe
                if (!string.IsNullOrEmpty(menuItem.ImagenUrl))
                {
                    _imageService.DeleteImage(menuItem.ImagenUrl, "items");
                }

                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Producto eliminado correctamente.";
            }

            return RedirectToAction(nameof(GestionarMenu));
        }

        // API: Obtener estadísticas
        [HttpGet]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            int restaurantId = 1;

            var restaurant = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null)
            {
                return NotFound();
            }

            var estadisticas = new
            {
                TotalProductos = restaurant.MenuItems?.Count() ?? 0,
                PrecioPromedio = restaurant.MenuItems?.Any() == true ? restaurant.MenuItems.Average(m => m.Precio) : 0,
                ProductoMasCaro = restaurant.MenuItems?.MaxBy(m => m.Precio),
                ProductoMasBarato = restaurant.MenuItems?.MinBy(m => m.Precio)
            };

            return Json(estadisticas);
        }

        private bool RestaurantExists(int id)
        {
            return _context.Restaurantes.Any(e => e.Id == id);
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }
    }
}