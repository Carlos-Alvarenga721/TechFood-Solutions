using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechFood_Solutions.Models;
using Microsoft.AspNetCore.Authorization;

namespace TechFood_Solutions.Controllers
{

    [Authorize(Roles = "Cliente")]
    public class UsersController : Controller
    {
        private readonly TechFoodDbContext _context;
        private readonly IWebHostEnvironment _env; // Para la ruta wwwroot

        public UsersController(TechFoodDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // LISTAR USUARIOS
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Restaurant)
                .Where(u => u.Rol == UserRole.Associated)
                .ToListAsync();
            return View("~/Views/Admin/Index.cshtml", users);
        }

        // CREAR USUARIO - GET
        public async Task<IActionResult> Create()
        {
            await LoadRestaurantsAsync();
            return View("~/Views/Admin/Create.cshtml");
        }

        // CREAR USUARIO - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, IFormFile? logoFile)
        {
            if (user.RestaurantId == 0 || user.RestaurantId == null)
                ModelState.Remove("RestaurantId");

            if (ModelState.IsValid)
            {
                user.Rol = UserRole.Associated;

                if (user.RestaurantId == null || user.RestaurantId == 0)
                {
                    var restauranteGenerico = new Restaurant
                    {
                        Nombre = $"Restaurante {user.Nombre} {user.Apellido}",
                        Descripcion = "Restaurante creado automáticamente. Actualizar información."
                    };

                    // Guardar imagen si se subió
                    if (logoFile != null && logoFile.Length > 0)
                    {
                        string uploads = Path.Combine(_env.WebRootPath, "images", "restaurantes");
                        Directory.CreateDirectory(uploads);
                        string fileName = Guid.NewGuid() + Path.GetExtension(logoFile.FileName);
                        string filePath = Path.Combine(uploads, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await logoFile.CopyToAsync(fileStream);
                        }

                        restauranteGenerico.LogoUrl = $"images/restaurantes/{fileName}";
                    }

                    _context.Restaurantes.Add(restauranteGenerico);
                    await _context.SaveChangesAsync();
                    user.RestaurantId = restauranteGenerico.Id;
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await LoadRestaurantsAsync(user.RestaurantId);
            return View("~/Views/Admin/Create.cshtml", user);
        }

        // EDITAR USUARIO - GET
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            await LoadRestaurantsAsync(user.RestaurantId);
            return View("~/Views/Admin/Edit.cshtml", user);
        }

        // EDITAR USUARIO - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user, IFormFile? logoFile)
        {
            if (id != user.Id) return NotFound();

            if (user.RestaurantId == 0 || user.RestaurantId == null)
                ModelState.Remove("RestaurantId");

            if (ModelState.IsValid)
            {
                user.Rol = UserRole.Associated;

                if (user.RestaurantId == null || user.RestaurantId == 0)
                {
                    var restauranteGenerico = new Restaurant
                    {
                        Nombre = $"Restaurante {user.Nombre} {user.Apellido}",
                        Descripcion = "Restaurante creado automáticamente. Actualizar información."
                    };

                    if (logoFile != null && logoFile.Length > 0)
                    {
                        string uploads = Path.Combine(_env.WebRootPath, "images", "restaurantes");
                        Directory.CreateDirectory(uploads);
                        string fileName = Guid.NewGuid() + Path.GetExtension(logoFile.FileName);
                        string filePath = Path.Combine(uploads, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await logoFile.CopyToAsync(fileStream);
                        }

                        restauranteGenerico.LogoUrl = $"images/restaurantes/{fileName}";
                    }

                    _context.Restaurantes.Add(restauranteGenerico);
                    await _context.SaveChangesAsync();
                    user.RestaurantId = restauranteGenerico.Id;
                }

                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await LoadRestaurantsAsync(user.RestaurantId);
            return View("~/Views/Admin/Edit.cshtml", user);
        }

        // ELIMINAR USUARIO - GET
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users
                .Include(u => u.Restaurant)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            return View("~/Views/Admin/Delete.cshtml", user);
        }

        // ELIMINAR USUARIO - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Cargar lista de restaurantes en ViewBag
        private async Task LoadRestaurantsAsync(int? selectedId = null)
        {
            var restaurants = await _context.Restaurantes.ToListAsync();
            var restaurantesConGenerico = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "-- Crear Restaurante Genérico --" }
            };
            restaurantesConGenerico.AddRange(restaurants.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Nombre,
                Selected = r.Id == selectedId
            }));
            ViewBag.Restaurantes = restaurantesConGenerico;
        }
    }
}
