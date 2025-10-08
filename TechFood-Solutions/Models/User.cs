using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechFood_Solutions.Models
{
    public enum UserRole
    {
        Admin = 0,
        Client = 1,
        Associated = 2
    }

    public class User : IdentityUser<int>
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

        [ForeignKey(nameof(RestaurantId))]
        public virtual Restaurant? Restaurant { get; set; }

        // ✅ Validación: si es Associated debe tener RestaurantId
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Rol == UserRole.Associated && RestaurantId == null)
            {
                yield return new ValidationResult(
                    "El RestaurantId es obligatorio para usuarios con rol 'Associated'.",
                    new[] { nameof(RestaurantId) }
                );
            }

            if (Rol != UserRole.Associated && RestaurantId != null)
            {
                yield return new ValidationResult(
                    "Sólo los usuarios con rol 'Associated' pueden tener RestaurantId.",
                    new[] { nameof(RestaurantId) }
                );
            }
        }
    }
}
