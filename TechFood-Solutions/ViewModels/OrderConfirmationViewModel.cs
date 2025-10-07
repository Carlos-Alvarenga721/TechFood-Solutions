using TechFood_Solutions.Models;

namespace TechFood_Solutions.ViewModels
{
    public class OrderConfirmationViewModel
    {
        public int OrderId { get; set; }
        public string NombreCliente { get; set; }
        public DateTime FechaOrden { get; set; }
        public decimal Total { get; set; }
        public string DireccionEntrega { get; set; }
        public string Estado { get; set; }
        public string RestaurantName { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}