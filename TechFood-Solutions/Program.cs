using TechFood_Solutions.Models;
using Microsoft.EntityFrameworkCore;
using TechFood_Solutions.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddNewtonsoftJson();

builder.Services.AddDbContext<TechFoodDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TechFoodCN")));

builder.Services.AddIdentity<User, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<TechFoodDbContext>()
.AddDefaultTokenProviders();

// Registramos una fábrica de Claims segura (evita pasar null a Claim ctor)
builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, SafeClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.Cookie.Name = "TechFoodAuth";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICartService, CartService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 🌱 Ejecutar los Seeds
await SeedRolesAndAdminAsync(app);
await SeedClientsAsync(app);

app.Run();

// ---------------- MÉTODOS DE SEED ----------------
async Task SeedRolesAndAdminAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    string[] roles = new[] { "Admin", "Associated", "Client" };
    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
        }
    }

    var adminEmail = app.Configuration["AdminUser:Email"] ?? "admin@techfood.com";
    var adminPassword = app.Configuration["AdminUser:Password"] ?? "Admin123!";

    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        var adminUser = new User
        {
            UserName = adminEmail,              // ✅ CRÍTICO: UserName debe tener un valor
            Email = adminEmail,
            EmailConfirmed = true,
            Nombre = "Administrador",
            Apellido = "General",
            Dui = "00000000-0",
            Rol = UserRole.Admin,
            NormalizedUserName = adminEmail.ToUpper(),  // ✅ IMPORTANTE
            NormalizedEmail = adminEmail.ToUpper()      // ✅ IMPORTANTE
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            foreach (var error in createResult.Errors)
                logger.LogError("Error al crear el admin: {Error}", error.Description);
        }
    }
}

// ---------------- SEED DE CLIENTES ----------------
async Task SeedClientsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    await TechFood_Solutions.Models.Seed.ClientSeed.SeedClientsAsync(services);
}