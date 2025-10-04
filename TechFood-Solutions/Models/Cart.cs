namespace TechFood_Solutions.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public decimal Total => Items.Sum(i => i.Subtotal);

        public int TotalItems => Items.Sum(i => i.Cantidad);

        public int? RestaurantId => Items.FirstOrDefault()?.RestaurantId;

        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.MenuItemId == item.MenuItemId);

            if (existingItem != null)
            {
                existingItem.Cantidad += item.Cantidad;
            }
            else
            {
                Items.Add(item);
            }
        }

        public void RemoveItem(int menuItemId)
        {
            var item = Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
            if (item != null)
            {
                Items.Remove(item);
            }
        }

        public void UpdateQuantity(int menuItemId, int cantidad)
        {
            var item = Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
            if (item != null)
            {
                if (cantidad <= 0)
                {
                    Items.Remove(item);
                }
                else
                {
                    item.Cantidad = cantidad;
                }
            }
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}