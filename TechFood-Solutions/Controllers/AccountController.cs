using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using TechFood_Solutions.Models;
using TechFood_Solutions.Services;

namespace TechFood_Solutions.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly ICartService _cartService;

        public AccountController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<AccountController> logger,
            ICartService cartService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _cartService = cartService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Guardar returnUrl en ViewData para usarlo en la vista
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

            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            _logger.LogInformation($"Usuario {user.Email} inició sesión correctamente.");

            // ✅ Si hay returnUrl y es seguro, redirigir allí
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // ✅ Redirección según rol (por defecto)
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
            // ✅ LIMPIAR EL CARRITO AL CERRAR SESIÓN
            try
            {
                _cartService.ClearCart();
                _logger.LogInformation("Carrito limpiado al cerrar sesión.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar el carrito durante logout.");
            }

            await _signInManager.SignOutAsync();
            _logger.LogInformation("Sesión cerrada correctamente.");

            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied() => View();
    }
}