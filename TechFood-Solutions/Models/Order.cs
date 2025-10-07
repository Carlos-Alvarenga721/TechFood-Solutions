using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechFood_Solutions.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreCliente { get; set; }

        [Required]
        [StringLength(15)]
        [Phone]
        public string TelefonoCliente { get; set; }

        [Required]
        [StringLength(500)]
        public string DireccionEntrega { get; set; }

        [Required]
        public DateTime FechaOrden { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } // Pendiente, Preparando, EnCamino, Entregado, Cancelado

        [StringLength(500)]
        public string? Notas { get; set; }

        // Relación con Restaurant
        [Required]
        [ForeignKey("Restaurant")]
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }

        // Relación con OrderItems
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}