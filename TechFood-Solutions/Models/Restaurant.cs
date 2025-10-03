using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace TechFood_Solutions.Models
{
    public class Restaurant
    {
        [Key]
        public int Id { get; set; }

        [StringLength(250)]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre del Restaurante")]
        public string Nombre { get; set; }

        [Display(Name = "Logo")]
        public string? LogoUrl { get; set; }

        [StringLength(500)]
        [Required(ErrorMessage = "La descripción es obligatoria")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        public ICollection<MenuItem>? MenuItems { get; set; }

        [NotMapped]
        [Display(Name = "Subir Logo")]
        public IFormFile? LogoFile { get; set; }

        // 🔑 Relación 1:N con usuarios
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
