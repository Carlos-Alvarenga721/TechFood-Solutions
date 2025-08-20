using Microsoft.AspNetCore.Mvc;

namespace TechFood_Solutions.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
