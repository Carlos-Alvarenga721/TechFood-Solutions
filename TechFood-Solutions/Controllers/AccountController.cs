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

        public AccountController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email y contraseña son requeridos.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            // ✅ CORRECCIÓN: Usar user.UserName en lugar de email
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,      // UserName (que es igual al email)
                password,
                isPersistent: true,
                lockoutOnFailure: false
            );

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            _logger.LogInformation($"Usuario {user.Email} inició sesión correctamente.");

            // Redirección según returnUrl o rol
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (await _userManager.IsInRoleAsync(user, RoleNames.Admin))
                return RedirectToAction("Index", "Users");
            else if (await _userManager.IsInRoleAsync(user, RoleNames.Associated))
                return RedirectToAction("Index", "Asociado");
            else
                return RedirectToAction("Index", "Home");
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