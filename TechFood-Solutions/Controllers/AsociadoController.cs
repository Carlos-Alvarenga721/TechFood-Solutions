using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AsociadoController> _logger;

        public AsociadoController(TechFoodDbContext context, UserManager<User> userManager, ILogger<AsociadoController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // PANEL PRINCIPAL DEL ASOCIADO
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.RestaurantId == null)
                return NotFound("No se encontró un restaurante asociado a este usuario.");

            var restaurant = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == user.RestaurantId);

            if (restaurant == null)
                return NotFound("No se encontró el restaurante asociado.");

            return View(restaurant);
        }

        // EDITAR RESTAURANTE - GET
        public async Task<IActionResult> EditarRestaurante(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user.RestaurantId != id)
                return Forbid(); // No puede editar restaurantes de otros asociados

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

        // EDITAR RESTAURANTE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRestaurante(int id, EditarRestauranteViewModel viewModel, IFormFile? logoFile)
        {
            if (id != viewModel.Id) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user.RestaurantId != id) return Forbid();

            var restaurant = await _context.Restaurantes.FindAsync(id);
            if (restaurant == null) return NotFound();

            if (logoFile != null && logoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "restaurantes");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(logoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(fileStream);
                }

                restaurant.LogoUrl = $"/images/restaurantes/{fileName}";
            }

            restaurant.Nombre = viewModel.Nombre.Trim();
            restaurant.Descripcion = viewModel.Descripcion.Trim();

            await _context.SaveChangesAsync();
            TempData["Success"] = "Restaurante actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GESTIONAR MENÚ DEL RESTAURANTE
        public async Task<IActionResult> GestionarMenu()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.RestaurantId == null) return NotFound();

            var restaurant = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == user.RestaurantId);

            if (restaurant == null) return NotFound();

            return View(restaurant);
        }

        // CREAR PRODUCTO - GET
        public async Task<IActionResult> CrearProducto()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.RestaurantId == null) return NotFound();

            var menuItem = new MenuItem
            {
                RestaurantId = user.RestaurantId.Value
            };
            return View(menuItem);
        }

        // CREAR PRODUCTO - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearProducto([Bind("Nombre,Descripcion,Precio,ImagenUrl,RestaurantId")] MenuItem menuItem, IFormFile? imagenFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.RestaurantId == null || user.RestaurantId != menuItem.RestaurantId)
                return Forbid();

            if (imagenFile == null || imagenFile.Length == 0)
            {
                ModelState.AddModelError("imagenFile", "Se requiere una imagen para el producto.");
                return View(menuItem);
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items", $"restaurant_{menuItem.RestaurantId}");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(imagenFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imagenFile.CopyToAsync(fileStream);
            }

            menuItem.ImagenUrl = fileName;
            _context.Add(menuItem);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Producto creado correctamente.";
            return RedirectToAction(nameof(GestionarMenu));
        }

        // EDITAR PRODUCTO - GET
        public async Task<IActionResult> EditarProducto(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);

            var user = await _userManager.GetUserAsync(User);
            if (menuItem == null || user.RestaurantId != menuItem.RestaurantId) return Forbid();

            return View(menuItem);
        }

        // EDITAR PRODUCTO - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProducto(int id, [Bind("Id,Nombre,Descripcion,Precio,ImagenUrl,RestaurantId")] MenuItem menuItem, IFormFile? imagenFile)
        {
            if (id != menuItem.Id) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user.RestaurantId != menuItem.RestaurantId) return Forbid();

            var existingItem = await _context.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (existingItem == null) return NotFound();

            if (imagenFile != null && imagenFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items", $"restaurant_{menuItem.RestaurantId}");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(imagenFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imagenFile.CopyToAsync(fileStream);
                }
                menuItem.ImagenUrl = fileName;
            }
            else
            {
                menuItem.ImagenUrl = existingItem.ImagenUrl;
            }

            _context.Update(menuItem);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Producto actualizado correctamente.";
            return RedirectToAction(nameof(GestionarMenu));
        }

        // ELIMINAR PRODUCTO - POST
        [HttpPost, ActionName("EliminarProducto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProductoConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.Include(m => m.Restaurant).FirstOrDefaultAsync(m => m.Id == id);
            var user = await _userManager.GetUserAsync(User);

            if (menuItem == null || user.RestaurantId != menuItem.RestaurantId) return Forbid();

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items", $"restaurant_{menuItem.Restaurant.Id}", menuItem.ImagenUrl);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Producto eliminado correctamente.";
            return RedirectToAction(nameof(GestionarMenu));
        }
    }
}
