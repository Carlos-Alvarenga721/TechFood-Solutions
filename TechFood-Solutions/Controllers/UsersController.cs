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

        // ✅ CONSTRUCTOR FALTANTE - Inyección de dependencias
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

                // ✅ Deshabilitar validación temporal para evitar conflicto con IValidatableObject
                _context.ChangeTracker.AutoDetectChangesEnabled = false;
                var result = await _userManager.CreateAsync(newUser, password);
                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                if (!result.Succeeded)
                {
                    // Limpiar restaurante creado si falló
                    if (selectedRestaurantId != user.RestaurantId && selectedRestaurantId.HasValue)
                    {
                        var tempRestaurant = await _context.Restaurantes.FindAsync(selectedRestaurantId.Value);
                        if (tempRestaurant != null)
                        {
                            _context.Restaurantes.Remove(tempRestaurant);
                            await _context.SaveChangesAsync();
                        }
                    }

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
                // ✅ Remover validación de RestaurantId del ModelState
                ModelState.Remove("RestaurantId");

                var user = await _context.Users
                    .Include(u => u.Restaurant)
                    .FirstOrDefaultAsync(u => u.Id == id && u.Rol == UserRole.Associated);

                if (user == null)
                {
                    TempData["Error"] = "Usuario no encontrado o no es un asociado.";
                    return RedirectToAction(nameof(Index));
                }

                // Actualizar datos personales
                user.Nombre = updatedUser.Nombre;
                user.Apellido = updatedUser.Apellido;
                user.Dui = updatedUser.Dui;
                user.Rol = UserRole.Associated; // Mantener rol

                // ✅ Manejar cambio de restaurante
                if (updatedUser.RestaurantId == 0 || updatedUser.RestaurantId == null)
                {
                    // Crear nuevo restaurante
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
                else if (user.RestaurantId != updatedUser.RestaurantId)
                {
                    // Validar que el restaurante existe
                    var restaurantExists = await _context.Restaurantes
                        .AnyAsync(r => r.Id == updatedUser.RestaurantId);

                    if (!restaurantExists)
                    {
                        ModelState.AddModelError("", "El restaurante seleccionado no existe.");
                        await LoadRestaurantsAsync(updatedUser.RestaurantId);
                        return View("~/Views/Admin/Edit.cshtml", updatedUser);
                    }

                    user.RestaurantId = updatedUser.RestaurantId;
                }

                // ✅ Actualizar con EF Core evitando validación de IValidatableObject
                _context.Entry(user).State = EntityState.Modified;
                _context.ChangeTracker.AutoDetectChangesEnabled = false;
                await _context.SaveChangesAsync();
                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                _logger.LogInformation($"Usuario {user.Email} actualizado exitosamente.");

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

        // ✅ MOSTRAR CONFIRMACIÓN DE ELIMINACIÓN (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users
                .Include(u => u.Restaurant)
                .FirstOrDefaultAsync(u => u.Id == id && u.Rol == UserRole.Associated);

            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado o no es un asociado.";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/Delete.cshtml", user);
        }

        // ✅ ELIMINAR USUARIO ASOCIADO (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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

                // Guardar info del restaurante antes de eliminar
                int? restaurantId = user.RestaurantId;
                Restaurant? restaurant = user.Restaurant;

                // Eliminar usuario primero usando UserManager
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    TempData["Error"] = "Error al eliminar el usuario.";
                    foreach (var error in result.Errors)
                        _logger.LogError($"Error al eliminar usuario: {error.Description}");

                    return RedirectToAction(nameof(Index));
                }

                // Verificar si hay otros usuarios asociados al mismo restaurante
                if (restaurantId.HasValue)
                {
                    var otrosUsuarios = await _context.Users
                        .Where(u => u.RestaurantId == restaurantId.Value && u.Id != id)
                        .AnyAsync();

                    // Solo eliminar el restaurante si no hay más usuarios asociados
                    if (restaurant != null && !otrosUsuarios)
                    {
                        _context.Restaurantes.Remove(restaurant);
                        _logger.LogInformation($"Restaurante {restaurant.Nombre} eliminado junto con el usuario.");
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Usuario con ID {id} eliminado exitosamente.");

                TempData["Info"] = "Usuario asociado eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar usuario asociado: {ex.Message}");
                TempData["Error"] = $"Error al eliminar el usuario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
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