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

        // GET: Cart/Index - Ver carrito
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        // POST: Cart/UpdateQuantity - Actualizar cantidad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int menuItemId, int cantidad)
        {
            _cartService.UpdateQuantity(menuItemId, cantidad);
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/RemoveItem - Eliminar item del carrito
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem(int menuItemId)
        {
            _cartService.RemoveFromCart(menuItemId);
            TempData["Success"] = "Item eliminado del carrito";
            return RedirectToAction(nameof(Index));
        }

        // GET: Cart/GetCartInfo - Obtener info del carrito (para validación)
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

        // GET: Cart/Checkout - Página de checkout
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

        // POST: Cart/ProcessCheckout - Procesar la orden
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

            // IMPORTANTE: Recargar el Cart en el modelo antes de validar
            model.Cart = cart;

            // Ahora validar solo los campos del formulario, ignorando Cart
            ModelState.Remove("Cart");
            ModelState.Remove("Cart.Items");

            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            // Crear la orden
            var order = new Order
            {
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

                // Limpiar el carrito
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

        // GET: Cart/OrderConfirmation/5 - Confirmación de orden
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

        // GET: Cart/MyOrders - Ver mis órdenes (opcional, para historial)
        public async Task<IActionResult> MyOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.FechaOrden)
                .ToListAsync();

            return View(orders);
        }

        // GET: Cart/OrderDetails/5 - Ver detalles de una orden
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

            return View(order);
        }

        // API: Get cart count para mostrar en navbar
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var count = _cartService.GetCartItemCount();
            return Json(new { count });
        }

        // POST: Cart/AddToCartAjax
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

        // Utilizado por el modal para la eliminacion del carrito actual y agregar el nuevo elemento de otro restaurante
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

        // POST: Cart/UpdateQuantityAjax - Actualizar cantidad sin recargar
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

        // POST: Cart/RemoveItemAjax - Eliminar item sin recargar
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