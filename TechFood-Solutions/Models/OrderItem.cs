using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechFood_Solutions.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Required]
        [ForeignKey("MenuItem")]
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [StringLength(500)]
        public string? NotasEspeciales { get; set; }
    }
}