using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using TechFood_Solutions.Models;

namespace TechFood_Solutions.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email y contraseña son requeridos.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View();
            }

            Console.WriteLine($"user.Id: {user.Id}");
            Console.WriteLine($"user.Email: {user.Email}");
            Console.WriteLine($"user.UserName: {user.UserName}");

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                Console.WriteLine("⚠️ UserName está vacío o null. Este es el valor que causa la excepción.");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, isPersistent: true, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View();
            }

            _logger.LogInformation($"Usuario {user.Email} inició sesión correctamente.");

            // ✅ Redirección según rol
            if (await _userManager.IsInRoleAsync(user, RoleNames.Admin))
                return RedirectToAction("Index", "Users"); // Vista Admin
            else if (await _userManager.IsInRoleAsync(user, RoleNames.Associated))
                return RedirectToAction("Index", "Asociado"); // Vista Asociado
            else
                return RedirectToAction("Index", "Home"); // Otros (cliente)
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Sesión cerrada correctamente.");
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied() => View();
    }
}
