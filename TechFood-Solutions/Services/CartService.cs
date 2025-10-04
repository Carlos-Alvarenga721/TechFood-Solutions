using TechFood_Solutions.Models;
using Newtonsoft.Json;

namespace TechFood_Solutions.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "ShoppingCart";

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Cart GetCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var cartJson = session.GetString(CartSessionKey);

            if (string.IsNullOrEmpty(cartJson))
            {
                return new Cart();
            }

            return JsonConvert.DeserializeObject<Cart>(cartJson) ?? new Cart();
        }

        private void SaveCart(Cart cart)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var cartJson = JsonConvert.SerializeObject(cart);
            session.SetString(CartSessionKey, cartJson);
        }

        public void AddToCart(CartItem item)
        {
            var cart = GetCart();

            // Verificar que todos los items sean del mismo restaurante
            if (cart.Items.Any() && cart.RestaurantId != item.RestaurantId)
            {
                throw new InvalidOperationException(
                    "No puedes agregar items de diferentes restaurantes. Por favor, vacía tu carrito primero.");
            }

            cart.AddItem(item);
            SaveCart(cart);
        }

        public void RemoveFromCart(int menuItemId)
        {
            var cart = GetCart();
            cart.RemoveItem(menuItemId);
            SaveCart(cart);
        }

        public void UpdateQuantity(int menuItemId, int cantidad)
        {
            var cart = GetCart();
            cart.UpdateQuantity(menuItemId, cantidad);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            session.Remove(CartSessionKey);
        }

        public int GetCartItemCount()
        {
            var cart = GetCart();
            return cart.TotalItems;
        }
        public int GetItemQuantity(int menuItemId)
        {
            var cart = GetCart();
            return cart.Items.FirstOrDefault(i => i.MenuItemId == menuItemId)?.Cantidad ?? 0;
        }

    }
}