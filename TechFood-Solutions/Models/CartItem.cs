using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechFood_Solutions.Models
{
    // Este modelo NO va a la base de datos, solo se usa en memoria/sesión
    public class CartItem
    {
        public int MenuItemId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; }
        public int Cantidad { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string? NotasEspeciales { get; set; }

        public decimal Subtotal => Precio * Cantidad;
    }
}
