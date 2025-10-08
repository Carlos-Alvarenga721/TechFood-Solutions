using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechFood_Solutions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace TechFood_Solutions.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    public class UsersController : Controller
    {
        private readonly TechFoodDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            TechFoodDbContext context,
            IWebHostEnvironment env,
            UserManager<User> userManager,
            ILogger<UsersController> logger)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
            _logger = logger;
        }

        // LISTAR USUARIOS ASOCIADOS
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Restaurant)
                .Where(u => u.Rol == UserRole.Associated)
                .ToListAsync();

            return View("~/Views/Admin/Index.cshtml", users);
        }

        // CREAR USUARIO (GET)
        public async Task<IActionResult> Create()
        {
            await LoadRestaurantsAsync();
            return View("~/Views/Admin/Create.cshtml");
        }

        // CREAR USUARIO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password, IFormFile? logoFile)
        {
            try
            {
                // 🔒 Este controlador solo puede crear usuarios "Associated"
                user.Rol = UserRole.Associated;

                // Validar contraseña
                if (string.IsNullOrWhiteSpace(password))
                {
                    ModelState.AddModelError("password", "La contraseña es requerida.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadRestaurantsAsync(user.RestaurantId);
                    return View("~/Views/Admin/Create.cshtml", user);
                }

                int? selectedRestaurantId = user.RestaurantId;

                // ✅ Crear restaurante si no se seleccionó uno existente
                if (selectedRestaurantId == 0 || selectedRestaurantId == null)
                {
                    var restaurante = new Restaurant
                    {
                        Nombre = $"Restaurante {user.Nombre} {user.Apellido}",
                        Descripcion = "Restaurante creado automáticamente."
                    };

                    if (logoFile != null && logoFile.Length > 0)
                    {
                        string uploads = Path.Combine(_env.WebRootPath, "images", "restaurantes");
                        Directory.CreateDirectory(uploads);

                        string fileName = Guid.NewGuid() + Path.GetExtension(logoFile.FileName);
                        string filePath = Path.Combine(uploads, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                            await logoFile.CopyToAsync(stream);

                        restaurante.LogoUrl = fileName;
                    }

                    _context.Restaurantes.Add(restaurante);
                    await _context.SaveChangesAsync();

                    selectedRestaurantId = restaurante.Id;
                }

                // ✅ Crear usuario asociado al restaurante
                var newUser = new User
                {
                    Nombre = user.Nombre,
                    Apellido = user.Apellido,
                    Dui = user.Dui,
                    Email = user.Email,
                    UserName = user.Email,
                    NormalizedUserName = user.Email.ToUpper(),
                    NormalizedEmail = user.Email.ToUpper(),
                    EmailConfirmed = true,
                    Rol = UserRole.Associated,
                    RestaurantId = selectedRestaurantId,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await _userManager.CreateAsync(newUser, password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);

                    await LoadRestaurantsAsync(user.RestaurantId);
                    return View("~/Views/Admin/Create.cshtml", user);
                }

                // Asignar rol "Associated"
                await _userManager.AddToRoleAsync(newUser, RoleNames.Associated);

                _logger.LogInformation($"Usuario {newUser.Email} creado exitosamente con restaurante {selectedRestaurantId}.");

                TempData["Info"] = "Usuario asociado creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al crear usuario asociado: {ex.Message}");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                await LoadRestaurantsAsync(user.RestaurantId);
                return View("~/Views/Admin/Create.cshtml", user);
            }
        }

        // EDITAR USUARIO ASOCIADO (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users
                .Include(u => u.Restaurant)
                .FirstOrDefaultAsync(u => u.Id == id && u.Rol == UserRole.Associated);

            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado o no es un asociado.";
                return RedirectToAction(nameof(Index));
            }

            await LoadRestaurantsAsync(user.RestaurantId);
            return View("~/Views/Admin/Edit.cshtml", user);
        }

        // EDITAR USUARIO ASOCIADO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User updatedUser, IFormFile? logoFile)
        {
            if (id != updatedUser.Id)
                return NotFound();

            try
            {
                var user = await _context.Users
                    .Include(u => u.Restaurant)
                    .FirstOrDefaultAsync(u => u.Id == id && u.Rol == UserRole.Associated);

                if (user == null)
                {
                    TempData["Error"] = "Usuario no encontrado o no es un asociado.";
                    return RedirectToAction(nameof(Index));
                }

                user.Nombre = updatedUser.Nombre;
                user.Apellido = updatedUser.Apellido;
                user.Dui = updatedUser.Dui;

                // 🔒 Solo rol asociado
                user.Rol = UserRole.Associated;

                // Manejar cambio de restaurante
                if (updatedUser.RestaurantId == 0 || updatedUser.RestaurantId == null)
                {
                    var nuevoRestaurante = new Restaurant
                    {
                        Nombre = $"Restaurante {user.Nombre} {user.Apellido}",
                        Descripcion = "Restaurante creado automáticamente."
                    };

                    if (logoFile != null && logoFile.Length > 0)
                    {
                        string uploads = Path.Combine(_env.WebRootPath, "images", "restaurantes");
                        Directory.CreateDirectory(uploads);

                        string fileName = Guid.NewGuid() + Path.GetExtension(logoFile.FileName);
                        string filePath = Path.Combine(uploads, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                            await logoFile.CopyToAsync(stream);

                        nuevoRestaurante.LogoUrl = fileName;
                    }

                    _context.Restaurantes.Add(nuevoRestaurante);
                    await _context.SaveChangesAsync();

                    user.RestaurantId = nuevoRestaurante.Id;
                }
                else
                {
                    user.RestaurantId = updatedUser.RestaurantId;
                }

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["Info"] = "Usuario asociado actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al editar usuario asociado: {ex.Message}");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                await LoadRestaurantsAsync(updatedUser.RestaurantId);
                return View("~/Views/Admin/Edit.cshtml", updatedUser);
            }
        }

        // ELIMINAR USUARIO ASOCIADO
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users
                .Include(u => u.Restaurant)
                .FirstOrDefaultAsync(u => u.Id == id && u.Rol == UserRole.Associated);

            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado o no es un asociado.";
                return RedirectToAction(nameof(Index));
            }

            // Eliminar restaurante solo si no hay más asociados a él
            var otros = await _context.Users
                .Where(u => u.RestaurantId == user.RestaurantId && u.Id != user.Id)
                .AnyAsync();

            if (user.Restaurant != null && !otros)
                _context.Restaurantes.Remove(user.Restaurant);

            await _userManager.DeleteAsync(user);
            await _context.SaveChangesAsync();

            TempData["Info"] = "Usuario asociado eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // Cargar restaurantes al ViewBag
        private async Task LoadRestaurantsAsync(int? selectedId = null)
        {
            var restaurantes = await _context.Restaurantes.ToListAsync();
            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "-- Crear Restaurante Genérico --" }
            };

            items.AddRange(restaurantes.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Nombre,
                Selected = r.Id == selectedId
            }));

            ViewBag.Restaurantes = items;
        }
    }
}
