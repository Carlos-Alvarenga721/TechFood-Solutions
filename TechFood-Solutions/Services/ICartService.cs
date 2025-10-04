using TechFood_Solutions.Models;

namespace TechFood_Solutions.Services
{
    public interface ICartService
    {
        Cart GetCart();
        void AddToCart(CartItem item);
        void RemoveFromCart(int menuItemId);
        void UpdateQuantity(int menuItemId, int cantidad);
        void ClearCart();
        int GetCartItemCount();
        int GetItemQuantity(int menuItemId);
    }
}