using System.ComponentModel.DataAnnotations;
using TechFood_Solutions.Models;

namespace TechFood_Solutions.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre completo")]
        public string NombreCliente { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [Display(Name = "Teléfono")]
        public string TelefonoCliente { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(500)]
        [Display(Name = "Dirección de entrega")]
        public string DireccionEntrega { get; set; }

        [StringLength(500)]
        [Display(Name = "Notas adicionales")]
        public string? Notas { get; set; }

        // Para mostrar el resumen
        public Cart Cart { get; set; }
    }
}