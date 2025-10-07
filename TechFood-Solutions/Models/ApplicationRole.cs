using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TechFood_Solutions.Models
{
    public class ApplicationRole : IdentityRole<int>
    {

        public string? Description { get; set; }

        public ApplicationRole() : base() { }

        public ApplicationRole(string roleName, string description = null)
            : base(roleName)
        {
            Description = description ?? $"Rol del sistema: {roleName}";
        }
    }
}
