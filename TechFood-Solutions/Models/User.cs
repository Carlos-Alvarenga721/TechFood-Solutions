using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechFood_Solutions.Models
{
    public enum UserRole
    {
        Admin,
        Client,
        Associated
    }

    public class User : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Nombre { get; set; }

        [Required, MaxLength(50)]
        public string Apellido { get; set; }

        [Required, MaxLength(10)]
        public string Dui { get; set; }

        // 🔑 FK -> un usuario pertenece a un restaurante
        public int? RestaurantId { get; set; }

        [Required]
        public UserRole Rol { get; set; }

        [ForeignKey("RestaurantId")]
        public virtual Restaurant? Restaurant { get; set; }

        // ✅ Validación: si es Associated debe tener RestaurantId
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Rol == UserRole.Associated && RestaurantId == null)
            {
                yield return new ValidationResult(
                    "El RestaurantId es obligatorio para usuarios con rol 'Associated'.",
                    new[] { nameof(RestaurantId) });
            }
        }
    }
}
