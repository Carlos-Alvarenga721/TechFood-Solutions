using System.ComponentModel.DataAnnotations;

namespace TechFood_Solutions.ViewModels
{
    public class EditarRestauranteViewModel
    {
        public int Id { get; set; }

        [StringLength(250)]
        [Required(ErrorMessage = "El nombre del restaurante es obligatorio")]
        [Display(Name = "Nombre del Restaurante")]
        public string Nombre { get; set; }

        [StringLength(500)]
        [Required(ErrorMessage = "La descripción del restaurante es obligatoria")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        // LogoUrl NO es requerido en el ViewModel porque puede mantenerse el actual
        [Display(Name = "Logo Actual")]
        public string? LogoUrlActual { get; set; }
    }
}