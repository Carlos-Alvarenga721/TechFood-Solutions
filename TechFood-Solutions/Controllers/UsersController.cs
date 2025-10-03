using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechFood_Solutions.Models;

namespace TechFood_Solutions.Controllers
{
    public class UsersController : Controller
    {
        private readonly TechFoodDbContext _context;

        public UsersController(TechFoodDbContext context)
        {
            _context = context;
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
            var restaurants = await _context.Restaurantes.ToListAsync();

            // Crear lista con opción de restaurante genérico
            var restaurantesConGenerico = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "-- Crear Restaurante Genérico --" }
            };

            restaurantesConGenerico.AddRange(restaurants.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Nombre
            }));

            ViewBag.Restaurantes = restaurantesConGenerico;
            return View("~/Views/Admin/Create.cshtml");
        }

        // CREAR USUARIO - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            // Remover validación de RestaurantId del ModelState si se va a crear genérico
            if (user.RestaurantId == 0 || user.RestaurantId == null)
            {
                ModelState.Remove("RestaurantId");
            }

            if (ModelState.IsValid)
            {
                user.Rol = UserRole.Associated;

                // Si seleccionó crear restaurante genérico (RestaurantId = 0 o null)
                if (user.RestaurantId == null || user.RestaurantId == 0)
                {
                    var restauranteGenerico = new Restaurant
                    {
                        Nombre = $"Restaurante {user.Nombre} {user.Apellido}",
                        Descripcion = "Restaurante creado automáticamente. Actualizar información.",
                        LogoUrl = null
                    };

                    _context.Restaurantes.Add(restauranteGenerico);
                    await _context.SaveChangesAsync();

                    user.RestaurantId = restauranteGenerico.Id;
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var restaurants = await _context.Restaurantes.ToListAsync();
            var restaurantesConGenerico = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "-- Crear Restaurante Genérico --" }
            };
            restaurantesConGenerico.AddRange(restaurants.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Nombre,
                Selected = r.Id == user.RestaurantId
            }));
            ViewBag.Restaurantes = restaurantesConGenerico;

            return View("~/Views/Admin/Create.cshtml", user);
        }

        // EDITAR USUARIO - GET
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var restaurants = await _context.Restaurantes.ToListAsync();
            var restaurantesConGenerico = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "-- Crear Restaurante Genérico --" }
            };
            restaurantesConGenerico.AddRange(restaurants.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Nombre,
                Selected = r.Id == user.RestaurantId
            }));
            ViewBag.Restaurantes = restaurantesConGenerico;

            return View("~/Views/Admin/Edit.cshtml", user);
        }

        // EDITAR USUARIO - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id) return NotFound();

            // Remover validación de RestaurantId del ModelState si se va a crear genérico
            if (user.RestaurantId == 0 || user.RestaurantId == null)
            {
                ModelState.Remove("RestaurantId");
            }

            if (ModelState.IsValid)
            {
                user.Rol = UserRole.Associated;

                // Si seleccionó crear restaurante genérico
                if (user.RestaurantId == null || user.RestaurantId == 0)
                {
                    var restauranteGenerico = new Restaurant
                    {
                        Nombre = $"Restaurante {user.Nombre} {user.Apellido}",
                        Descripcion = "Restaurante creado automáticamente. Actualizar información.",
                        LogoUrl = null
                    };

                    _context.Restaurantes.Add(restauranteGenerico);
                    await _context.SaveChangesAsync();

                    user.RestaurantId = restauranteGenerico.Id;
                }

                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var restaurants = await _context.Restaurantes.ToListAsync();
            var restaurantesConGenerico = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "-- Crear Restaurante Genérico --" }
            };
            restaurantesConGenerico.AddRange(restaurants.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Nombre,
                Selected = r.Id == user.RestaurantId
            }));
            ViewBag.Restaurantes = restaurantesConGenerico;

            return View("~/Views/Admin/Edit.cshtml", user);
        }

        // ELIMINAR USUARIO - GET (Mostrar confirmación)
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users
                .Include(u => u.Restaurant)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            return View("~/Views/Admin/Delete.cshtml", user);
        }

        // ELIMINAR USUARIO - POST (Confirmado)
        [HttpPost]
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
    }
}