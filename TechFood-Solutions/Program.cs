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
            UserName = adminEmail,
            Email = adminEmail,
            Nombre = "Administrador",
            Apellido = "General",
            Dui = "00000000-0",
            Rol = UserRole.Admin,
            EmailConfirmed = true // opcional pero útil para evitar problemas al crear claims de email
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


// ---------------- FABRICA SEGURA DE CLAIMS ----------------
// Esta clase evita crear Claim con valor null (que provoca System.ArgumentNullException).
public class SafeClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, ApplicationRole>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IOptions<IdentityOptions> _options;

    public SafeClaimsPrincipalFactory(
        UserManager<User> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _options = optionsAccessor;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        // Intentamos usar la implementación base, pero protegemos contra excepciones de Claim null
        try
        {
            return await base.GenerateClaimsAsync(user);
        }
        catch (ArgumentNullException)
        {
            // Si la implementación base lanza por algún valor null, construimos una identidad segura manualmente.
            var identity = new ClaimsIdentity(
                _options.Value.ClaimsIdentity.UserIdClaimType,
                _options.Value.ClaimsIdentity.UserNameClaimType,
                _options.Value.ClaimsIdentity.RoleClaimType);

            // Id y nombre de usuario (si existen)
            var id = await _userManager.GetUserIdAsync(user);
            var userName = await _userManager.GetUserNameAsync(user);
            if (!string.IsNullOrEmpty(id))
                identity.AddClaim(new Claim(_options.Value.ClaimsIdentity.UserIdClaimType, id));
            if (!string.IsNullOrEmpty(userName))
                identity.AddClaim(new Claim(_options.Value.ClaimsIdentity.UserNameClaimType, userName));

            // Email (si existe)
            var email = await _userManager.GetEmailAsync(user);
            if (!string.IsNullOrEmpty(email))
                identity.AddClaim(new Claim(ClaimTypes.Email, email));

            // Nombre y apellido si tus propiedades existen
            if (!string.IsNullOrEmpty(user.Nombre))
                identity.AddClaim(new Claim(ClaimTypes.GivenName, user.Nombre));
            if (!string.IsNullOrEmpty(user.Apellido))
                identity.AddClaim(new Claim(ClaimTypes.Surname, user.Apellido));

            // Roles
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrEmpty(role))
                    identity.AddClaim(new Claim(_options.Value.ClaimsIdentity.RoleClaimType, role));
            }

            // Puedes añadir aquí más claims seguros (usando null-coalescing o comprobaciones)
            // Ejemplo: TenantId (si tu User tiene TenantId)
            // var tenantId = user.TenantId;
            // identity.AddClaim(new Claim("TenantId", tenantId ?? string.Empty));

            return identity;
        }
    }
}
