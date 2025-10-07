using System.ComponentModel.DataAnnotations;

namespace TechFood_Solutions.ViewModels
{
    public class CreateAssociatedViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }

        [Required, MaxLength(50)]
        public string Nombre { get; set; }

        [Required, MaxLength(50)]
        public string Apellido { get; set; }

        [Required, MaxLength(10)]
        public string Dui { get; set; }

        [Required]
        public int RestaurantId { get; set; } // Asociado requiere restaurantId
    }
}
