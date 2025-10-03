using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechFood_Solutions.Models;

namespace TechFood_Solutions.Controllers
{
    public class AsociadoController : Controller
    {
        private readonly TechFoodDbContext _context;
        private readonly ILogger<AsociadoController> _logger;

        public AsociadoController(TechFoodDbContext context, ILogger<AsociadoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Asociado - Dashboard principal del asociado
        public async Task<IActionResult> Index()
        {
            // Por ahora, simulamos que el asociado maneja el restaurante con ID = 1
            // En una implementación real, esto vendría de la sesión/autenticación
            int restaurantId = 1; // Esto debería venir de la sesión del usuario autenticado

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
            _logger.LogInformation($"GET EditarRestaurante: ID recibido = {id}");
            
            if (id == null)
            {
                _logger.LogWarning("GET EditarRestaurante: ID es null");
                return NotFound();
            }

            var restaurant = await _context.Restaurantes.FindAsync(id);
            if (restaurant == null)
            {
                _logger.LogWarning($"GET EditarRestaurante: No se encontró restaurante con ID {id}");
                return NotFound();
            }

            _logger.LogInformation($"GET EditarRestaurante: Restaurante encontrado - {restaurant.Nombre}");
            return View(restaurant);
        }

        // POST: Asociado/EditarRestaurante/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRestaurante(int id, [Bind("Id,Nombre,LogoUrl,Descripcion")] Restaurant restaurant)
        {
            _logger.LogInformation($"POST EditarRestaurante: ID = {id}");
            _logger.LogInformation($"POST EditarRestaurante: Restaurant.Id = {restaurant.Id}");
            _logger.LogInformation($"POST EditarRestaurante: Restaurant.Nombre = {restaurant.Nombre}");
            _logger.LogInformation($"POST EditarRestaurante: Restaurant.Descripcion = {restaurant.Descripcion}");
            _logger.LogInformation($"POST EditarRestaurante: Restaurant.LogoUrl = {restaurant.LogoUrl}");
            
            if (id != restaurant.Id)
            {
                _logger.LogWarning($"POST EditarRestaurante: ID mismatch. URL ID = {id}, Model ID = {restaurant.Id}");
                return NotFound();
            }

            _logger.LogInformation($"POST EditarRestaurante: ModelState.IsValid = {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState)
                {
                    foreach (var error in modelError.Value.Errors)
                    {
                        _logger.LogWarning($"ModelState Error - {modelError.Key}: {error.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("POST EditarRestaurante: Iniciando actualización en DB");
                    
                    _context.Update(restaurant);
                    var changes = await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"POST EditarRestaurante: Cambios guardados exitosamente. {changes} registros afectados");
                    TempData["Success"] = "Información del restaurante actualizada correctamente.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, $"POST EditarRestaurante: Error de concurrencia para ID {id}");
                    
                    if (!RestaurantExists(restaurant.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"POST EditarRestaurante: Error inesperado para ID {id}");
                    TempData["Error"] = "Ocurrió un error al actualizar el restaurante: " + ex.Message;
                    return View(restaurant);
                }
                
                return RedirectToAction(nameof(Index));
            }
            
            _logger.LogWarning("POST EditarRestaurante: ModelState no válido, devolviendo vista con errores");
            return View(restaurant);
        }

        // GET: Asociado/GestionarMenu
        public async Task<IActionResult> GestionarMenu()
        {
            // Por ahora, simulamos que el asociado maneja el restaurante con ID = 1
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
        public async Task<IActionResult> EditarProducto(int id, [Bind("Id,Nombre,Descripcion,Precio,ImagenUrl,RestaurantId")] MenuItem menuItem)
        {
            _logger.LogInformation($"POST EditarProducto: ID = {id}, MenuItem.Id = {menuItem.Id}");
            
            if (id != menuItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(menuItem);
                    var changes = await _context.SaveChangesAsync();
                    _logger.LogInformation($"POST EditarProducto: {changes} cambios guardados para producto {menuItem.Nombre}");
                    TempData["Success"] = "Producto actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, $"Error de concurrencia al actualizar producto {id}");
                    if (!MenuItemExists(menuItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(GestionarMenu));
            }

            // Si hay errores, recargar el restaurante para la vista
            menuItem.Restaurant = await _context.Restaurantes.FindAsync(menuItem.RestaurantId);
            return View(menuItem);
        }

        // GET: Asociado/CrearProducto
        public async Task<IActionResult> CrearProducto()
        {
            // Por ahora, simulamos que el asociado maneja el restaurante con ID = 1
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
        public async Task<IActionResult> CrearProducto([Bind("Nombre,Descripcion,Precio,ImagenUrl,RestaurantId")] MenuItem menuItem)
        {
            _logger.LogInformation($"POST CrearProducto: Creando producto {menuItem.Nombre} para restaurante {menuItem.RestaurantId}");
            
            if (ModelState.IsValid)
            {
                _context.Add(menuItem);
                var changes = await _context.SaveChangesAsync();
                _logger.LogInformation($"POST CrearProducto: Producto creado exitosamente. {changes} registros afectados");
                TempData["Success"] = "Producto creado correctamente.";
                return RedirectToAction(nameof(GestionarMenu));
            }

            // Si hay errores, recargar el restaurante para la vista
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
                _context.MenuItems.Remove(menuItem);
                var changes = await _context.SaveChangesAsync();
                _logger.LogInformation($"POST EliminarProducto: Producto eliminado. {changes} registros afectados");
                TempData["Success"] = "Producto eliminado correctamente.";
            }

            return RedirectToAction(nameof(GestionarMenu));
        }

        // API: Obtener estadísticas del restaurante
        [HttpGet]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            int restaurantId = 1; // Simulado

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

        // Métodos helper
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