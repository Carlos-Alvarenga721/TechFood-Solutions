using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechFood_Solutions.Models;
using TechFood_Solutions.Services;

namespace TechFood_Solutions.Controllers
{
    public class AsociadoController : Controller
    {
        private readonly TechFoodDbContext _context;
        private readonly ILogger<AsociadoController> _logger;
        private readonly IImageService _imageService;

        public AsociadoController(
            TechFoodDbContext context,
            ILogger<AsociadoController> logger,
            IImageService imageService)
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
     [Bind("Id,Nombre,Descripcion,LogoUrl")] Restaurant restaurant,
     IFormFile logoFile)
        {
            _logger.LogInformation($"POST EditarRestaurante iniciado - ID: {id}");

            if (id != restaurant.Id)
            {
                return NotFound();
            }

            // ⭐ AGREGA ESTE LOG AQUÍ
            _logger.LogInformation($"LogoUrl recibido del formulario: '{restaurant.LogoUrl}'");
            _logger.LogInformation($"Archivo recibido: {(logoFile != null ? logoFile.FileName : "NINGUNO")}");

            ModelState.Remove("MenuItems");

            // Si se subió archivo nuevo
            if (logoFile != null && logoFile.Length > 0)
            {
                var fileName = await _imageService.SaveImageAsync(logoFile, "restaurantes");

                if (!string.IsNullOrEmpty(fileName))
                {
                    if (!string.IsNullOrEmpty(restaurant.LogoUrl) && restaurant.LogoUrl != fileName)
                    {
                        _imageService.DeleteImage(restaurant.LogoUrl, "restaurantes");
                    }
                    restaurant.LogoUrl = fileName;
                    _logger.LogInformation($"LogoUrl actualizado a: {fileName}");
                }
                else
                {
                    ModelState.AddModelError("", "Error al guardar la imagen.");
                    return View(restaurant);
                }
            }

            _logger.LogInformation($"LogoUrl FINAL antes de guardar: '{restaurant.LogoUrl}'");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(restaurant);
                    var changes = await _context.SaveChangesAsync();
                    _logger.LogInformation($"✅ Guardado exitoso - {changes} filas");
                    TempData["Success"] = "Restaurante actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar");
                    TempData["Error"] = "Error: " + ex.Message;
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
            IFormFile imagenFile)
        {
            if (id != menuItem.Id)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurantes.FindAsync(menuItem.RestaurantId);
            if (restaurant == null)
            {
                return NotFound("Restaurante no encontrado");
            }

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
            var restaurant = await _context.Restaurantes.FindAsync(menuItem.RestaurantId);
            if (restaurant == null)
            {
                return NotFound("Restaurante no encontrado");
            }

            if (imagenFile != null && imagenFile.Length > 0)
            {
                var fileName = await _imageService.SaveImageAsync(imagenFile, "items", restaurant.Nombre);
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

            menuItem.Restaurant = restaurant;
            return View(menuItem);
        }

        // POST: Asociado/EliminarProducto/5
        [HttpPost, ActionName("EliminarProducto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProductoConfirmed(int id)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem != null)
            {
                if (!string.IsNullOrEmpty(menuItem.ImagenUrl) && menuItem.Restaurant != null)
                {
                    _imageService.DeleteImage(menuItem.ImagenUrl, "items", menuItem.Restaurant.Nombre);
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