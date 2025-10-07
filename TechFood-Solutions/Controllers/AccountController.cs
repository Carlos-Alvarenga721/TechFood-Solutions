using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechFood_Solutions.Models;

namespace TechFood_Solutions.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // ============================================
        // LOGIN
        // ============================================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Debe ingresar correo y contraseña.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Usuario no encontrado.");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Credenciales incorrectas.");
                return View();
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "Admin");
            if (await _userManager.IsInRoleAsync(user, "Associated"))
                return RedirectToAction("Index", "Associated");

            return RedirectToAction("Index", "Client");
        }

        // ============================================
        // LOGOUT
        // ============================================
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ============================================
        // REGISTRO DE CLIENTE
        // ============================================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string nombre, string apellido, string dui, string email, string password)
        {
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) ||
                string.IsNullOrEmpty(dui) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Todos los campos son obligatorios.");
                return View();
            }

            var user = new User
            {
                UserName = email,
                Email = email,
                Nombre = nombre,
                Apellido = apellido,
                Dui = dui,
                Rol = UserRole.Client
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View();
            }

            if (!await _roleManager.RoleExistsAsync("Client"))
                await _roleManager.CreateAsync(new ApplicationRole { Name = "Client" });

            await _userManager.AddToRoleAsync(user, "Client");
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Client");
        }

        // ============================================
        // CREAR USUARIO ASOCIADO (ADMIN)
        // ============================================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateAssociated() => View();

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAssociated(string nombre, string apellido, string dui, string email, string password, int restaurantId)
        {
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) ||
                string.IsNullOrEmpty(dui) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Todos los campos son obligatorios.");
                return View();
            }

            var user = new User
            {
                UserName = email,
                Email = email,
                Nombre = nombre,
                Apellido = apellido,
                Dui = dui,
                RestaurantId = restaurantId,
                Rol = UserRole.Associated
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View();
            }

            if (!await _roleManager.RoleExistsAsync("Associated"))
                await _roleManager.CreateAsync(new ApplicationRole { Name = "Associated" });

            await _userManager.AddToRoleAsync(user, "Associated");

            return RedirectToAction("ManageAssociatedUsers", "Admin");
        }
    }
}
