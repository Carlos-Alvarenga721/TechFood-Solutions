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
        [Required, MaxLength(50)]
        public string Nombre { get; set; }

        [Required, MaxLength(50)]
        public string Apellido { get; set; }

        [Required, MaxLength(10)]
        public string Dui { get; set; }

        // Ahora todos los usuarios pueden tener RestaurantId nulo
        public int? RestaurantId { get; set; }

        [Required]
        public UserRole Rol { get; set; }

        [ForeignKey(nameof(RestaurantId))]
        public virtual Restaurant? Restaurant { get; set; }
    }
}
