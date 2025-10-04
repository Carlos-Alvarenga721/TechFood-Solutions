using TechFood_Solutions.Models;
using Microsoft.EntityFrameworkCore;
using TechFood_Solutions.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(); // Importante: solo una vez aquí

// Add DbContext
builder.Services.AddDbContext<TechFoodDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TechFoodCN")));

// Add Session support - IMPORTANTE
builder.Services.AddDistributedMemoryCache(); // Requerido para Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContextAccessor for session access
builder.Services.AddHttpContextAccessor();

// Add CartService
builder.Services.AddScoped<ICartService, CartService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// CRÍTICO: UseSession debe estar ANTES de UseAuthorization
app.UseSession();

app.UseAuthorization();

// OPCIÓN 1: Eliminar la ruta personalizada y usar solo la ruta default
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();