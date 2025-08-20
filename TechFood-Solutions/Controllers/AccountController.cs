using Microsoft.AspNetCore.Mvc;
using TechFood_Solutions.Models;

namespace TechFood_Solutions.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        // GET: /Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            // (temporal: redirigir al Home)
            if (email == "admin@correo.com" && password == "123456")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Aquí iría la lógica para guardar usuario en BD
                TempData["Success"] = "Usuario registrado correctamente.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

    }
}
