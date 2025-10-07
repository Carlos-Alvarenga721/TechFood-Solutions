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

        // LISTAR USUARIOS
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Restaurant)
                    .Where(u => u.Rol == UserRole.Associated)
                    .ToListAsync();

                _logger.LogInformation($"Se encontraron {users.Count} usuarios asociados");

                return View("~/Views/Admin/Index.cshtml", users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al listar usuarios: {ex.Message}");
                TempData["Error"] = "Error al cargar la lista de usuarios";
                return View("~/Views/Admin/Index.cshtml", new List<User>());
            }
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
        public async Task<IActionResult> Create(User user, string password, IFormFile? logoFile)
        {
            try
            {
                // IMPORTANTE: Este controlador solo crea usuarios con rol Associated
                // Forzar el rol Associated antes de cualquier validación
                user.Rol = UserRole.Associated;

                // Remover validación de RestaurantId si es 0 o null
                // Los usuarios Associated DEBEN tener un restaurante
                if (user.RestaurantId == 0 || user.RestaurantId == null)
                {
                    ModelState.Remove("RestaurantId");
                }

                // Validaciones manuales
                if (string.IsNullOrEmpty(user.Email))
                {
                    ModelState.AddModelError("Email", "El email es obligatorio.");
                }

                if (string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("Password", "La contraseña es obligatoria.");
                }

                if (string.IsNullOrEmpty(user.Nombre))
                {
                    ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
                }

                if (string.IsNullOrEmpty(user.Apellido))
                {
                    ModelState.AddModelError("Apellido", "El apellido es obligatorio.");
                }

                // Verificar si el email ya existe
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Este email ya está registrado.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadRestaurantsAsync(user.RestaurantId);
                    return View("~/Views/Admin/Create.cshtml", user);
                }

                // IMPORTANTE: Establecer propiedades de Identity
                user.UserName = user.Email;
                user.NormalizedUserName = user.Email.ToUpper();
                user.NormalizedEmail = user.Email.ToUpper();
                user.EmailConfirmed = true;
                user.SecurityStamp = Guid.NewGuid().ToString();

                // Crear restaurante genérico si no se selecciona ninguno
                if (user.RestaurantId == null || user.RestaurantId == 0)
                {
                    var restauranteGenerico = new Restaurant
                    {
                        Nombre = $"Restaurante {user.Nombre} {user.Apellido}",
                        Descripcion = "Restaurante creado automáticamente. Actualizar información."
                    };

                    // Procesar logo si se proporcionó
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

                        restauranteGenerico.LogoUrl = $"/images/restaurantes/{fileName}";
                    }

                    _context.Restaurantes.Add(restauranteGenerico);
                    await _context.SaveChangesAsync();
                    user.RestaurantId = restauranteGenerico.Id;

                    _logger.LogInformation($"Restaurante genérico creado: {restauranteGenerico.Nombre} (ID: {restauranteGenerico.Id})");
                }

                // Crear usuario con Identity
                var createResult = await _userManager.CreateAsync(user, password);

                if (!createResult.Succeeded)
                {
                    _logger.LogError($"Error al crear usuario: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    await LoadRestaurantsAsync(user.RestaurantId);
                    return View("~/Views/Admin/Create.cshtml", user);
                }

                // Asignar rol de Usuario Asociado
                await _userManager.AddToRoleAsync(user, RoleNames.Associated);

                _logger.LogInformation($"Usuario creado exitosamente: {user.Email} (ID: {user.Id})");

                TempData["Info"] = $"Usuario '{user.Email}' creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción al crear usuario: {ex.Message}\n{ex.StackTrace}");
                ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                await LoadRestaurantsAsync(user.RestaurantId);
                return View("~/Views/Admin/Create.cshtml", user);
            }
        }

        // EDITAR USUARIO - GET
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Restaurant)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                await LoadRestaurantsAsync(user.RestaurantId);
                return View("~/Views/Admin/Edit.cshtml", user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cargar usuario para editar: {ex.Message}");
                TempData["Error"] = "Error al cargar el usuario";
                return RedirectToAction(nameof(Index));
            }
        }

        // EDITAR USUARIO - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user, string? newPassword, IFormFile? logoFile)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            try
            {
                // Remover validación de RestaurantId si es 0 o null
                if (user.RestaurantId == 0 || user.RestaurantId == null)
                {
                    ModelState.Remove("RestaurantId");
                }

                if (!ModelState.IsValid)
                {
                    await LoadRestaurantsAsync(user.RestaurantId);
                    return View("~/Views/Admin/Edit.cshtml", user);
                }

                // Obtener usuario actual de la base de datos
                var existingUser = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (existingUser == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Mantener propiedades de Identity originales
                user.UserName = existingUser.UserName;
                user.NormalizedUserName = existingUser.NormalizedUserName;
                user.NormalizedEmail = user.Email.ToUpper();
                user.EmailConfirmed = existingUser.EmailConfirmed;
                user.PasswordHash = existingUser.PasswordHash;
                user.SecurityStamp = existingUser.SecurityStamp;
                user.ConcurrencyStamp = existingUser.ConcurrencyStamp;
                user.Rol = UserRole.Associated;

                // Crear restaurante genérico si no se selecciona ninguno
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

                        restauranteGenerico.LogoUrl = $"/images/restaurantes/{fileName}";
                    }

                    _context.Restaurantes.Add(restauranteGenerico);
                    await _context.SaveChangesAsync();
                    user.RestaurantId = restauranteGenerico.Id;
                }

                // Actualizar usuario
                _context.Update(user);
                await _context.SaveChangesAsync();

                // Cambiar contraseña si se proporcionó una nueva
                if (!string.IsNullOrEmpty(newPassword))
                {
                    var currentUser = await _userManager.FindByIdAsync(user.Id.ToString());
                    var token = await _userManager.GeneratePasswordResetTokenAsync(currentUser);
                    var resetResult = await _userManager.ResetPasswordAsync(currentUser, token, newPassword);

                    if (!resetResult.Succeeded)
                    {
                        _logger.LogWarning($"No se pudo cambiar la contraseña del usuario {user.Email}");
                    }
                }

                _logger.LogInformation($"Usuario actualizado: {user.Email}");

                TempData["Info"] = "Usuario actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar usuario: {ex.Message}");
                ModelState.AddModelError("", "Error al actualizar el usuario");
                await LoadRestaurantsAsync(user.RestaurantId);
                return View("~/Views/Admin/Edit.cshtml", user);
            }
        }

        // ELIMINAR USUARIO - GET
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Restaurant)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View("~/Views/Admin/Delete.cshtml", user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cargar usuario para eliminar: {ex.Message}");
                TempData["Error"] = "Error al cargar el usuario";
                return RedirectToAction(nameof(Index));
            }
        }
        // ELIMINAR USUARIO - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Cargar usuario y restaurante
                var user = await _context.Users
                    .Include(u => u.Restaurant)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Guardar nombre del restaurante antes de eliminar para logging
                string nombreRestaurante = user.Restaurant?.Nombre;

                // Eliminar restaurante si existe
                if (user.Restaurant != null)
                {
                    _context.Restaurantes.Remove(user.Restaurant);
                    _logger.LogInformation($"Restaurante '{nombreRestaurante}' eliminado junto con el usuario {user.Email}");
                }

                // Eliminar usuario con Identity
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    string errores = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError($"Error al eliminar usuario {user.Email}: {errores}");
                    TempData["Error"] = "Error al eliminar el usuario.";
                    return RedirectToAction(nameof(Index));
                }

                // Guardar cambios pendientes en EF Core (por ejemplo, eliminar restaurante)
                await _context.SaveChangesAsync();

                TempData["Info"] = $"Usuario '{user.Email}' y su restaurante han sido eliminados correctamente.";
                _logger.LogInformation($"Usuario '{user.Email}' eliminado correctamente.");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción al eliminar usuario: {ex.Message}");
                TempData["Error"] = "Ocurrió un error inesperado al eliminar el usuario.";
                return RedirectToAction(nameof(Index));
            }
        }



        // MÉTODO AUXILIAR: Cargar restaurantes para dropdown
        private async Task LoadRestaurantsAsync(int? selectedId = null)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError($"Error al cargar restaurantes: {ex.Message}");
                ViewBag.Restaurantes = new List<SelectListItem>
                {
                    new SelectListItem { Value = "0", Text = "-- Crear Restaurante Genérico --" }
                };
            }
        }
    }
}