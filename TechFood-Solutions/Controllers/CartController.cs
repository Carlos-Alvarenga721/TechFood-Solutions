using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechFood_Solutions.Models;
using TechFood_Solutions.Services;
using TechFood_Solutions.ViewModels;

namespace TechFood_Solutions.Controllers
{
    [Authorize(Roles = RoleNames.Client)]
    public class CartController : Controller
    {
        private readonly TechFoodDbContext _context;
        private readonly ICartService _cartService;

        public CartController(TechFoodDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int menuItemId, int cantidad)
        {
            _cartService.UpdateQuantity(menuItemId, cantidad);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem(int menuItemId)
        {
            _cartService.RemoveFromCart(menuItemId);
            TempData["Success"] = "Item eliminado del carrito";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetCartInfo()
        {
            var cart = _cartService.GetCart();
            return Json(new
            {
                hasItems = cart.Items.Any(),
                restaurantId = cart.RestaurantId,
                restaurantName = cart.Items.FirstOrDefault()?.RestaurantName ?? "",
                itemCount = cart.TotalItems
            });
        }

        public IActionResult Checkout()
        {
            var cart = _cartService.GetCart();

            if (!cart.Items.Any())
            {
                TempData["Error"] = "Tu carrito está vacío";
                return RedirectToAction("Restaurantes", "Cliente");
            }

            var model = new CheckoutViewModel
            {
                Cart = cart
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
        {
            var cart = _cartService.GetCart();

            if (!cart.Items.Any())
            {
                TempData["Error"] = "Tu carrito está vacío";
                return RedirectToAction("Restaurantes", "Cliente");
            }

            model.Cart = cart;
            ModelState.Remove("Cart");
            ModelState.Remove("Cart.Items");

            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            // OBTENER UserId - Hardcoded para testing
            // TODO: Cuando el login esté listo, reemplazar por:
            // var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            int userId = GetCurrentUserId();

            // Crear la orden CON UserId
            var order = new Order
            {
                UserId = userId,
                NombreCliente = model.NombreCliente,
                TelefonoCliente = model.TelefonoCliente,
                DireccionEntrega = model.DireccionEntrega,
                Notas = model.Notas,
                FechaOrden = DateTime.Now,
                Total = cart.Total,
                Estado = "Pendiente",
                RestaurantId = cart.RestaurantId.Value,
                OrderItems = new List<OrderItem>()
            };

            // Crear los items de la orden
            foreach (var item in cart.Items)
            {
                order.OrderItems.Add(new OrderItem
                {
                    MenuItemId = item.MenuItemId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.Precio,
                    Subtotal = item.Subtotal,
                    NotasEspeciales = item.NotasEspeciales
                });
            }

            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _cartService.ClearCart();

                TempData["Success"] = "¡Orden realizada con éxito!";
                return RedirectToAction(nameof(OrderConfirmation), new { id = order.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al procesar la orden: " + ex.Message);
                return View("Checkout", model);
            }
        }

        // MyOrders
        // Solo muestra órdenes del usuario loguead
        public async Task<IActionResult> MyOrders()
        {
            // OBTENER UserId - Hardcoded para testing
            // TODO: Cuando el login esté listo, reemplazar por:
            // var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            int userId = GetCurrentUserId();

            // FILTRAR solo órdenes del usuario actual
            var orders = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.FechaOrden)
                .ToListAsync();

            return View(orders);
        }

        // Simular usuario logueado
        private int GetCurrentUserId()
        {
            // TEMPORAL: Usuario hardcoded para testing
            // Se puede cambiar este ID para probar diferentes usuarios

            // Obtener de sesión si existe (para simular login)
            var userIdSession = HttpContext.Session.GetInt32("TestUserId");
            if (userIdSession.HasValue)
            {
                return userIdSession.Value;
            }

            // Por defecto, usar el usuario con Id = 1
            return 1;

            // TODO: Reemplazar con esto cuando el login esté listo:
            // return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Verificar que la orden pertenece al usuario actual
            int currentUserId = GetCurrentUserId();
            if (order.UserId != currentUserId)
            {
                TempData["Error"] = "No tienes permiso para ver esta orden";
                return RedirectToAction(nameof(MyOrders));
            }

            var model = new OrderConfirmationViewModel
            {
                OrderId = order.Id,
                NombreCliente = order.NombreCliente,
                FechaOrden = order.FechaOrden,
                Total = order.Total,
                DireccionEntrega = order.DireccionEntrega,
                Estado = order.Estado,
                RestaurantName = order.Restaurant.Nombre,
                Items = order.OrderItems.ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Verificar que la orden pertenece al usuario actual
            int currentUserId = GetCurrentUserId();
            if (order.UserId != currentUserId)
            {
                TempData["Error"] = "No tienes permiso para ver esta orden";
                return RedirectToAction(nameof(MyOrders));
            }

            return View(order);
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            var count = _cartService.GetCartItemCount();
            return Json(new { count });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartAjax(int menuItemId, int cantidad = 1)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == menuItemId);

            if (menuItem == null)
            {
                return Json(new { success = false, message = "Item no encontrado." });
            }

            var cartItem = new CartItem
            {
                MenuItemId = menuItem.Id,
                Nombre = menuItem.Nombre,
                Descripcion = menuItem.Descripcion,
                Precio = menuItem.Precio,
                ImagenUrl = menuItem.ImagenUrl,
                Cantidad = cantidad,
                RestaurantId = menuItem.RestaurantId,
                RestaurantName = menuItem.Restaurant.Nombre
            };

            try
            {
                _cartService.AddToCart(cartItem);
                return Json(new
                {
                    success = true,
                    message = $"{menuItem.Nombre} agregado al carrito.",
                    itemId = menuItem.Id,
                    totalItems = _cartService.GetCartItemCount(),
                    itemQuantity = _cartService.GetItemQuantity(menuItem.Id)
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, requiresClear = true, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAndAdd(int menuItemId, int cantidad = 1, string? notas = null)
        {
            try
            {
                var menuItem = await _context.MenuItems
                    .Include(m => m.Restaurant)
                    .FirstOrDefaultAsync(m => m.Id == menuItemId);

                if (menuItem == null)
                {
                    return Json(new { success = false, message = "Item no encontrado." });
                }

                var cartItem = new CartItem
                {
                    MenuItemId = menuItem.Id,
                    Nombre = menuItem.Nombre,
                    Descripcion = menuItem.Descripcion,
                    Precio = menuItem.Precio,
                    ImagenUrl = menuItem.ImagenUrl,
                    Cantidad = cantidad,
                    RestaurantId = menuItem.RestaurantId,
                    RestaurantName = menuItem.Restaurant?.Nombre,
                    NotasEspeciales = notas
                };

                _cartService.ClearCart();
                _cartService.AddToCart(cartItem);

                return Json(new
                {
                    success = true,
                    message = "Carrito limpiado y producto agregado.",
                    totalItems = _cartService.GetCartItemCount(),
                    itemQuantity = cartItem.Cantidad
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantityAjax(int menuItemId, int cantidad)
        {
            try
            {
                if (cantidad <= 0)
                {
                    _cartService.RemoveFromCart(menuItemId);
                    return Json(new
                    {
                        success = true,
                        removed = true,
                        message = "Producto eliminado del carrito",
                        totalItems = _cartService.GetCartItemCount(),
                        cartTotal = _cartService.GetCart().Total
                    });
                }

                _cartService.UpdateQuantity(menuItemId, cantidad);
                var cart = _cartService.GetCart();
                var item = cart.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);

                return Json(new
                {
                    success = true,
                    removed = false,
                    message = "Cantidad actualizada",
                    itemQuantity = item?.Cantidad ?? 0,
                    itemSubtotal = item?.Subtotal ?? 0,
                    totalItems = cart.TotalItems,
                    cartTotal = cart.Total
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al actualizar: " + ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItemAjax(int menuItemId)
        {
            try
            {
                _cartService.RemoveFromCart(menuItemId);
                var cart = _cartService.GetCart();

                return Json(new
                {
                    success = true,
                    message = "Producto eliminado del carrito",
                    totalItems = cart.TotalItems,
                    cartTotal = cart.Total,
                    isEmpty = !cart.Items.Any()
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al eliminar: " + ex.Message
                });
            }
        }
    }
}