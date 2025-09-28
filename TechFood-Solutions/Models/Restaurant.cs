using System.ComponentModel.DataAnnotations;

namespace TechFood_Solutions.Models
{
    public class Restaurant
    {
        public int Id { get; set; }

        [StringLength(250)]
        [Required]
        public string Nombre { get; set; }

        [Required]
        public string LogoUrl { get; set; }

        [StringLength(500)]
        [Required]
        public string Descripcion { get; set; }

        public ICollection<MenuItem> MenuItems { get; set; }
    }
}
