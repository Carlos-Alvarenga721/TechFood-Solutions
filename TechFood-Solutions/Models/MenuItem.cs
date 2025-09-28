using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechFood_Solutions.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [StringLength(250)]
        [Required]
        public string Nombre { get; set; }

        [StringLength(500)]
        [Required]
        public string Descripcion { get; set; }

        [Required]
        public decimal Precio { get; set; }

        [Required]
        public string ImagenUrl { get; set; }

        [Required]
        [ForeignKey("Restaurant")]
        public int RestaurantId { get; set;}
        public Restaurant Restaurant { get; set; }
    }
}
