using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechFood_Solutions.Models;
using TechFood_Solutions.ViewModels;

namespace TechFood_Solutions.Controllers
{
    [Authorize(Roles = RoleNames.Associated)]
    public class AsociadoController : Controller
    {
        private readonly TechFoodDbContext _context;
        private readonly ILogger<AsociadoController> _logger;
        private const int CURRENT_RESTAURANT_ID = 1;

        public AsociadoController(TechFoodDbContext context, ILogger<AsociadoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var restaurant = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == CURRENT_RESTAURANT_ID);

            if (restaurant == null)
                return NotFound("No se encontró el restaurante asociado.");

            return View(restaurant);
        }

        public async Task<IActionResult> EditarRestaurante(int? id)
        {
            if (id == null) return NotFound();

            var restaurant = await _context.Restaurantes.FindAsync(id);
            if (restaurant == null) return NotFound();

            var viewModel = new EditarRestauranteViewModel
            {
                Id = restaurant.Id,
                Nombre = restaurant.Nombre,
                Descripcion = restaurant.Descripcion,
                LogoUrlActual = restaurant.LogoUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRestaurante(int id, EditarRestauranteViewModel viewModel, IFormFile? logoFile)
        {
            if (id != viewModel.Id) return NotFound();

            var existingRestaurant = await _context.Restaurantes.FindAsync(id);
            if (existingRestaurant == null) return NotFound();

            if (logoFile != null && logoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "restaurantes");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, logoFile.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(fileStream);
                }
                existingRestaurant.LogoUrl = logoFile.FileName;
            }

            existingRestaurant.Nombre = viewModel.Nombre.Trim();
            existingRestaurant.Descripcion = viewModel.Descripcion.Trim();

            await _context.SaveChangesAsync();
            TempData["Success"] = "Restaurante actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GestionarMenu()
        {
            var restaurant = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == CURRENT_RESTAURANT_ID);

            if (restaurant == null) return NotFound();
            return View(restaurant);
        }

        public async Task<IActionResult> EditarProducto(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems.Include(m => m.Restaurant).FirstOrDefaultAsync(m => m.Id == id);
            if (menuItem == null) return NotFound();

            return View(menuItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProducto(int id, [Bind("Id,Nombre,Descripcion,Precio,ImagenUrl,RestaurantId")] MenuItem menuItem, IFormFile? imagenFile)
        {
            if (id != menuItem.Id) return NotFound();

            var restaurant = await _context.Restaurantes.FindAsync(menuItem.RestaurantId);
            if (restaurant == null) return NotFound();

            if (imagenFile != null && imagenFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items", $"restaurant_{restaurant.Id}");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, imagenFile.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imagenFile.CopyToAsync(fileStream);
                }
                menuItem.ImagenUrl = imagenFile.FileName;
            }
            else
            {
                var existing = await _context.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                if (existing != null)
                    menuItem.ImagenUrl = existing.ImagenUrl;
            }

            _context.Update(menuItem);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Producto actualizado correctamente.";
            return RedirectToAction(nameof(GestionarMenu));
        }

        public async Task<IActionResult> CrearProducto()
        {
            var restaurant = await _context.Restaurantes.FindAsync(CURRENT_RESTAURANT_ID);
            if (restaurant == null) return NotFound();

            var menuItem = new MenuItem
            {
                RestaurantId = CURRENT_RESTAURANT_ID,
                Restaurant = restaurant
            };
            return View(menuItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearProducto([Bind("Nombre,Descripcion,Precio,ImagenUrl,RestaurantId")] MenuItem menuItem, IFormFile? imagenFile)
        {
            var restaurant = await _context.Restaurantes.FindAsync(menuItem.RestaurantId);
            if (restaurant == null) return NotFound();

            if (imagenFile != null && imagenFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items", $"restaurant_{restaurant.Id}");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, imagenFile.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imagenFile.CopyToAsync(fileStream);
                }
                menuItem.ImagenUrl = imagenFile.FileName;
            }
            else
            {
                ModelState.AddModelError("imagenFile", "Se requiere una imagen para el producto.");
                return View(menuItem);
            }

            _context.Add(menuItem);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Producto creado correctamente.";
            return RedirectToAction(nameof(GestionarMenu));
        }

        [HttpPost, ActionName("EliminarProducto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProductoConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.Include(m => m.Restaurant).FirstOrDefaultAsync(m => m.Id == id);
            if (menuItem != null)
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items", $"restaurant_{menuItem.Restaurant.Id}", menuItem.ImagenUrl);
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);

                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Producto eliminado correctamente.";
            }
            return RedirectToAction(nameof(GestionarMenu));
        }
    }
}
