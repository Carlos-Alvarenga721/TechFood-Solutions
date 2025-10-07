using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TechFood_Solutions.Models;

namespace TechFood_Solutions.Models.Seed
{
    public static class ClientSeed
    {
        public static async Task SeedClientsAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

            // ✅ Asegurar que el rol "Client" exista
            if (!await roleManager.RoleExistsAsync("Client"))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = "Client" });
            }

            // ✅ Crear dos usuarios cliente predeterminados
            var clients = new[]
            {
                new
                {
                    Email = "cliente1@techfood.com",
                    Nombre = "Carlos",
                    Apellido = "Ramírez",
                    Dui = "12345678-9",
                    Password = "Cliente123!"
                },
                new
                {
                    Email = "cliente2@techfood.com",
                    Nombre = "Ana",
                    Apellido = "López",
                    Dui = "98765432-1",
                    Password = "Cliente123!"
                }
            };

            foreach (var c in clients)
            {
                var existingUser = await userManager.FindByEmailAsync(c.Email);
                if (existingUser == null)
                {
                    var user = new User
                    {
                        UserName = c.Email,
                        Email = c.Email,
                        Nombre = c.Nombre,
                        Apellido = c.Apellido,
                        Dui = c.Dui,
                        Rol = UserRole.Client
                    };

                    var result = await userManager.CreateAsync(user, c.Password);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Client");
                    }
                    else
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        foreach (var error in result.Errors)
                        {
                            logger.LogError("❌ Error creando cliente {Email}: {Error}", c.Email, error.Description);
                        }
                    }
                }
            }
        }
    }
}
