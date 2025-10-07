using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechFood_Solutions.Models;
using TechFood_Solutions.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TechFood_Solutions.Controllers
{
    [Authorize(Roles = "Asociado")]
    public class AsociadoController : Controller
    {
        private readonly TechFoodDbContext _context;
        private readonly ILogger<AsociadoController> _logger;

        // 🔥 CONSTANTE GLOBAL PARA EVITAR INCONSISTENCIAS
        private const int CURRENT_RESTAURANT_ID = 1; // ← CAMBIAR AQUÍ PARA TODOS LOS MÉTODOS

        public AsociadoController(
            TechFoodDbContext context,
            ILogger<AsociadoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Asociado
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation($"🏠 Accediendo al Index con Restaurant ID: {CURRENT_RESTAURANT_ID}");

            var restaurant = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == CURRENT_RESTAURANT_ID);

            if (restaurant == null)
            {
                _logger.LogWarning($"❌ No se encontró el restaurante con ID: {CURRENT_RESTAURANT_ID}");
                return NotFound("No se encontró el restaurante asociado a este usuario.");
            }

            _logger.LogInformation($"✅ Restaurante encontrado: '{restaurant.Nombre}' con {restaurant.MenuItems?.Count() ?? 0} productos");
            return View(restaurant);
        }

        // GET: Asociado/EditarRestaurante/5
        public async Task<IActionResult> EditarRestaurante(int? id)
        {
            _logger.LogInformation($"GET EditarRestaurante: ID = {id}");

            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurantes.FindAsync(id);
            if (restaurant == null)
            {
                return NotFound();
            }

            var viewModel = new EditarRestauranteViewModel
            {
                Id = restaurant.Id,
                Nombre = restaurant.Nombre,
                Descripcion = restaurant.Descripcion,
                LogoUrlActual = restaurant.LogoUrl
            };

            return View(viewModel);
        }

        // POST: Asociado/EditarRestaurante/5 - VERSIÓN COMPLETAMENTE ARREGLADA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRestaurante(
            int id,
            EditarRestauranteViewModel viewModel,
            IFormFile? logoFile) // 🔥 MARCADO COMO NULLABLE
        {
            _logger.LogCritical("🚀 INICIANDO EDICIÓN DE RESTAURANTE");
            _logger.LogCritical($"ID recibido: {id}");
            _logger.LogCritical($"ViewModel.Id: {viewModel.Id}");
            _logger.LogCritical($"ViewModel.Nombre: '{viewModel.Nombre}'");
            _logger.LogCritical($"ViewModel.Descripcion: '{viewModel.Descripcion}'");
            _logger.LogCritical($"logoFile: {(logoFile != null && logoFile.Length > 0 ? $"'{logoFile.FileName}' ({logoFile.Length} bytes)" : "NULL/VACÍO")}");

            if (id != viewModel.Id)
            {
                _logger.LogCritical("❌ ID MISMATCH");
                return NotFound();
            }

            // 🔥 REMOVER TODAS LAS VALIDACIONES PROBLEMÁTICAS
            ModelState.Remove("logoFile");
            ModelState.Remove("LogoUrl");
            ModelState.Remove("LogoUrlActual");

            // Obtener el restaurante actual
            var existingRestaurant = await _context.Restaurantes.FindAsync(id);
            if (existingRestaurant == null)
            {
                _logger.LogCritical("❌ RESTAURANTE NO ENCONTRADO EN BD");
                return NotFound();
            }

            _logger.LogCritical($"🏪 Restaurante encontrado: '{existingRestaurant.Nombre}' con LogoUrl: '{existingRestaurant.LogoUrl}'");

            // 🔥 PROCESAR IMAGEN SOLO SI SE PROPORCIONÓ
            if (logoFile != null && logoFile.Length > 0)
            {
                _logger.LogCritical($"📸 PROCESANDO NUEVA IMAGEN: {logoFile.FileName}");

                try
                {
                    var fileName = logoFile.FileName;
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "restaurantes");
                    
                    // Crear directorio si no existe
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                        _logger.LogCritical($"📁 Directorio creado: {uploadsFolder}");
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Guardar archivo
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await logoFile.CopyToAsync(fileStream);
                    }

                    // 🔥 ACTUALIZAR EL LogoUrl SOLO SI SE GUARDÓ LA IMAGEN
                    existingRestaurant.LogoUrl = fileName;
                    _logger.LogCritical($"✅ IMAGEN GUARDADA Y LogoUrl ACTUALIZADO: {fileName}");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "💥 ERROR AL GUARDAR IMAGEN");
                    TempData["Error"] = $"Error al guardar la imagen: {ex.Message}";
                    viewModel.LogoUrlActual = existingRestaurant.LogoUrl;
                    return View(viewModel);
                }
            }
            else
            {
                _logger.LogCritical("📷 NO SE PROPORCIONÓ IMAGEN - MANTENIENDO LogoUrl ACTUAL");
            }

            // 🔥 VERIFICAR MODELSTATE DESPUÉS DE LIMPIAR VALIDACIONES
            _logger.LogCritical($"🔍 ModelState.IsValid después de limpiar: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                _logger.LogCritical("❌ ModelState SIGUE SIENDO INVÁLIDO:");
                foreach (var error in ModelState)
                {
                    foreach (var errorMsg in error.Value.Errors)
                    {
                        _logger.LogCritical($"- Campo '{error.Key}': {errorMsg.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogCritical("✅ MODELSTATE VÁLIDO - GUARDANDO CAMBIOS...");
                    
                    // 🔥 ACTUALIZAR SOLO NOMBRE Y DESCRIPCIÓN
                    existingRestaurant.Nombre = viewModel.Nombre.Trim();
                    existingRestaurant.Descripcion = viewModel.Descripcion.Trim();
                    // LogoUrl ya se actualizó arriba si había imagen nueva

                    var changes = await _context.SaveChangesAsync();
                    _logger.LogCritical($"💾 CAMBIOS GUARDADOS EXITOSAMENTE: {changes} filas afectadas");
                    
                    TempData["Success"] = "¡Restaurante actualizado correctamente!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "💥 ERROR AL GUARDAR EN BASE DE DATOS");
                    TempData["Error"] = $"Error al guardar los cambios: {ex.Message}";
                }
            }

            // 🔥 SI LLEGAMOS AQUÍ, ALGO SALIÓ MAL
            _logger.LogCritical("⚠️ RETORNANDO VISTA CON ERRORES");
            viewModel.LogoUrlActual = existingRestaurant.LogoUrl;
            return View(viewModel);
        }

        // GET: Asociado/GestionarMenu
        public async Task<IActionResult> GestionarMenu()
        {
            _logger.LogInformation($"🍽️ Accediendo a GestionarMenu con Restaurant ID: {CURRENT_RESTAURANT_ID}");

            var restaurant = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == CURRENT_RESTAURANT_ID);

            if (restaurant == null)
            {
                _logger.LogWarning($"❌ No se encontró el restaurante con ID: {CURRENT_RESTAURANT_ID}");
                return NotFound();
            }

            _logger.LogInformation($"✅ Restaurante encontrado: '{restaurant.Nombre}' con {restaurant.MenuItems?.Count() ?? 0} productos");
            return View(restaurant);
        }

        // GET: Asociado/EditarProducto/5
        public async Task<IActionResult> EditarProducto(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems
                .Include(m => m.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return View(menuItem);
        }

        // POST: Asociado/EditarProducto/5 - 🔥 COMPLETAMENTE ARREGLADO CON DEBUGGING
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProducto(
            int id,
            [Bind("Id,Nombre,Descripcion,Precio,ImagenUrl,RestaurantId")] MenuItem menuItem,
            IFormFile? imagenFile)
        {
            _logger.LogCritical("🚀 INICIANDO EDICIÓN DE PRODUCTO");
            _logger.LogCritical($"📊 DATOS RECIBIDOS:");
            _logger.LogCritical($"- ID: {id}");
            _logger.LogCritical($"- menuItem.Id: {menuItem.Id}");
            _logger.LogCritical($"- Nombre: '{menuItem.Nombre}'");
            _logger.LogCritical($"- Descripción: '{menuItem.Descripcion}'");
            _logger.LogCritical($"- Precio: {menuItem.Precio}");
            _logger.LogCritical($"- ImagenUrl: '{menuItem.ImagenUrl}'");
            _logger.LogCritical($"- RestaurantId: {menuItem.RestaurantId}");
            _logger.LogCritical($"- imagenFile: {(imagenFile != null && imagenFile.Length > 0 ? $"'{imagenFile.FileName}' ({imagenFile.Length} bytes)" : "NULL/VACÍO")}");

            if (id != menuItem.Id)
            {
                _logger.LogCritical("❌ ID MISMATCH");
                return NotFound();
            }

            var restaurant = await _context.Restaurantes.FindAsync(menuItem.RestaurantId);
            if (restaurant == null)
            {
                _logger.LogCritical($"❌ RESTAURANTE NO ENCONTRADO CON ID: {menuItem.RestaurantId}");
                return NotFound("Restaurante no encontrado");
            }

            _logger.LogCritical($"🏪 Restaurante encontrado: '{restaurant.Nombre}' (ID: {restaurant.Id})");

            // 🔥 LIMPIAR VALIDACIONES PROBLEMÁTICAS
            ModelState.Remove("imagenFile");
            ModelState.Remove("Restaurant");

            // 🚀 PROCESAR IMAGEN SI SE PROPORCIONÓ
            if (imagenFile != null && imagenFile.Length > 0)
            {
                _logger.LogCritical($"📸 PROCESANDO NUEVA IMAGEN: {imagenFile.FileName}");

                try
                {
                    var fileName = imagenFile.FileName;
                    // 🔥 CAMBIO CLAVE: Usar restaurant.Id en lugar de restaurant.Nombre
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items", $"restaurant_{restaurant.Id}");
                    
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                        _logger.LogCritical($"📁 Directorio creado: {uploadsFolder}");
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagenFile.CopyToAsync(fileStream);
                    }

                    menuItem.ImagenUrl = fileName;
                    _logger.LogCritical($"✅ NUEVA IMAGEN GUARDADA: restaurant_{restaurant.Id}/{fileName}");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "💥 ERROR AL GUARDAR NUEVA IMAGEN");
                    ModelState.AddModelError("imagenFile", "Error al guardar la imagen.");
                    menuItem.Restaurant = restaurant;
                    TempData["Error"] = $"Error al guardar la imagen: {ex.Message}";
                    return View(menuItem);
                }
            }
            else
            {
                // Mantener la imagen actual si no se subió nueva
                var existing = await _context.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                if (existing != null)
                {
                    menuItem.ImagenUrl = existing.ImagenUrl;
                    _logger.LogCritical($"📷 MANTENIENDO IMAGEN ACTUAL: {existing.ImagenUrl}");
                }
                else
                {
                    _logger.LogCritical("⚠️ NO SE ENCONTRÓ REGISTRO EXISTENTE");
                }
            }

            // 🔥 VERIFICAR MODELSTATE DESPUÉS DE LIMPIAR VALIDACIONES
            _logger.LogCritical($"🔍 ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                _logger.LogCritical("❌ ERRORES DE VALIDACIÓN:");
                foreach (var error in ModelState)
                {
                    foreach (var errorMsg in error.Value.Errors)
                    {
                        _logger.LogCritical($"- Campo '{error.Key}': {errorMsg.ErrorMessage}");
                    }
                }

                // Devolver vista con errores
                menuItem.Restaurant = restaurant;
                TempData["Error"] = "Por favor, corrige los errores del formulario.";
                return View(menuItem);
            }

            // 🔥 GUARDAR CAMBIOS EN BASE DE DATOS
            try
            {
                _logger.LogCritical("💾 ACTUALIZANDO PRODUCTO EN BASE DE DATOS...");
                
                _context.Update(menuItem);
                var changes = await _context.SaveChangesAsync();
                
                _logger.LogCritical($"✅ PRODUCTO ACTUALIZADO EXITOSAMENTE - ID: {menuItem.Id}");
                _logger.LogCritical($"📊 Filas afectadas: {changes}");
                
                TempData["Success"] = "¡Producto actualizado correctamente!";
                return RedirectToAction(nameof(GestionarMenu));
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogCritical("💥 ERROR DE CONCURRENCIA");
                if (!MenuItemExists(menuItem.Id))
                {
                    _logger.LogCritical("❌ PRODUCTO YA NO EXISTE");
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "💥 ERROR AL ACTUALIZAR EN BASE DE DATOS");
                
                menuItem.Restaurant = restaurant;
                TempData["Error"] = $"Error al actualizar el producto: {ex.Message}";
                return View(menuItem);
            }
        }

        // GET: Asociado/CrearProducto
        public async Task<IActionResult> CrearProducto()
        {
            _logger.LogInformation($"➕ Accediendo a CrearProducto con Restaurant ID: {CURRENT_RESTAURANT_ID}");

            var restaurant = await _context.Restaurantes.FindAsync(CURRENT_RESTAURANT_ID);
            if (restaurant == null)
            {
                _logger.LogWarning($"❌ No se encontró el restaurante con ID: {CURRENT_RESTAURANT_ID}");
                return NotFound();
            }

            var menuItem = new MenuItem
            {
                RestaurantId = CURRENT_RESTAURANT_ID,
                Restaurant = restaurant
            };

            _logger.LogInformation($"✅ Preparando creación de producto para: '{restaurant.Nombre}'");
            return View(menuItem);
        }

        // POST: Asociado/CrearProducto - 🔥 COMPLETAMENTE ARREGLADO CON DEBUGGING
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearProducto(
            [Bind("Nombre,Descripcion,Precio,ImagenUrl,RestaurantId")] MenuItem menuItem,
            IFormFile? imagenFile)
        {
            _logger.LogCritical("🚀 INICIANDO CREACIÓN DE PRODUCTO");
            _logger.LogCritical($"📊 DATOS RECIBIDOS:");
            _logger.LogCritical($"- Nombre: '{menuItem.Nombre}'");
            _logger.LogCritical($"- Descripción: '{menuItem.Descripcion}'");
            _logger.LogCritical($"- Precio: {menuItem.Precio}");
            _logger.LogCritical($"- ImagenUrl: '{menuItem.ImagenUrl}'");
            _logger.LogCritical($"- RestaurantId: {menuItem.RestaurantId}");
            _logger.LogCritical($"- imagenFile: {(imagenFile != null && imagenFile.Length > 0 ? $"'{imagenFile.FileName}' ({imagenFile.Length} bytes)" : "NULL/VACÍO")}");

            var restaurant = await _context.Restaurantes.FindAsync(menuItem.RestaurantId);
            if (restaurant == null)
            {
                _logger.LogCritical($"❌ RESTAURANTE NO ENCONTRADO CON ID: {menuItem.RestaurantId}");
                return NotFound("Restaurante no encontrado");
            }

            _logger.LogCritical($"🏪 Restaurante encontrado: '{restaurant.Nombre}' (ID: {restaurant.Id})");

            // 🔥 LIMPIAR VALIDACIONES PROBLEMÁTICAS
            ModelState.Remove("imagenFile");
            ModelState.Remove("Restaurant");
            ModelState.Remove("Id"); // El ID se genera automáticamente

            // 🚀 PROCESAR IMAGEN SI SE PROPORCIONÓ
            if (imagenFile != null && imagenFile.Length > 0)
            {
                _logger.LogCritical($"📸 PROCESANDO IMAGEN: {imagenFile.FileName}");

                try
                {
                    var fileName = imagenFile.FileName;
                    // 🔥 USAR ESTRUCTURA BASADA EN ID
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items", $"restaurant_{restaurant.Id}");
                    
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                        _logger.LogCritical($"📁 Directorio creado: {uploadsFolder}");
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagenFile.CopyToAsync(fileStream);
                    }

                    // 🔥 ASIGNAR NOMBRE DEL ARCHIVO AL MODELO
                    menuItem.ImagenUrl = fileName;
                    _logger.LogCritical($"✅ IMAGEN GUARDADA: restaurant_{restaurant.Id}/{fileName}");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "💥 ERROR AL GUARDAR IMAGEN");
                    ModelState.AddModelError("imagenFile", "Error al guardar la imagen.");
                    menuItem.Restaurant = restaurant;
                    TempData["Error"] = $"Error al guardar la imagen: {ex.Message}";
                    return View(menuItem);
                }
            }
            else if (string.IsNullOrEmpty(menuItem.ImagenUrl))
            {
                // Si no hay archivo ni ImagenUrl, agregar error
                _logger.LogCritical("❌ NO HAY IMAGEN NI ARCHIVO");
                ModelState.AddModelError("imagenFile", "Se requiere una imagen para el producto.");
                menuItem.Restaurant = restaurant;
                TempData["Error"] = "Se requiere una imagen para el producto.";
                return View(menuItem);
            }

            // 🔥 VERIFICAR MODELSTATE DESPUÉS DE LIMPIAR VALIDACIONES
            _logger.LogCritical($"🔍 ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                _logger.LogCritical("❌ ERRORES DE VALIDACIÓN:");
                foreach (var error in ModelState)
                {
                    foreach (var errorMsg in error.Value.Errors)
                    {
                        _logger.LogCritical($"- Campo '{error.Key}': {errorMsg.ErrorMessage}");
                    }
                }

                // Devolver vista con errores
                menuItem.Restaurant = restaurant;
                TempData["Error"] = "Por favor, corrige los errores del formulario.";
                return View(menuItem);
            }

            // 🔥 GUARDAR EN BASE DE DATOS
            try
            {
                _logger.LogCritical("💾 AGREGANDO PRODUCTO A LA BASE DE DATOS...");
                
                _context.Add(menuItem);
                var changes = await _context.SaveChangesAsync();
                
                _logger.LogCritical($"✅ PRODUCTO CREADO EXITOSAMENTE - ID: {menuItem.Id}");
                _logger.LogCritical($"📊 Filas afectadas: {changes}");
                
                TempData["Success"] = "¡Producto creado correctamente!";
                return RedirectToAction(nameof(GestionarMenu));
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "💥 ERROR AL GUARDAR EN BASE DE DATOS");
                
                // Si hay error, eliminar la imagen guardada
                if (!string.IsNullOrEmpty(menuItem.ImagenUrl))
                {
                    try
                    {
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items", $"restaurant_{restaurant.Id}", menuItem.ImagenUrl);
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                            _logger.LogCritical($"🗑️ Imagen eliminada por error: {menuItem.ImagenUrl}");
                        }
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "No se pudo eliminar la imagen tras el error");
                    }
                }

                menuItem.Restaurant = restaurant;
                TempData["Error"] = $"Error al guardar el producto: {ex.Message}";
                return View(menuItem);
            }
        }

        // POST: Asociado/EliminarProducto/5 - 🚀 MEJORADO CON ESTRUCTURA BASADA EN ID
        [HttpPost, ActionName("EliminarProducto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProductoConfirmed(int id)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem != null)
            {
                // 🚀 ELIMINAR imagen física usando la nueva estructura
                if (!string.IsNullOrEmpty(menuItem.ImagenUrl) && menuItem.Restaurant != null)
                {
                    try
                    {
                        // 🔥 CAMBIO CLAVE: Usar restaurant.Id en lugar de restaurant.Nombre
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items", $"restaurant_{menuItem.Restaurant.Id}", menuItem.ImagenUrl);
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                            _logger.LogInformation($"🗑️ Imagen eliminada: restaurant_{menuItem.Restaurant.Id}/{menuItem.ImagenUrl}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"No se pudo eliminar la imagen: {menuItem.ImagenUrl}");
                        // Continuamos aunque no se pueda eliminar la imagen
                    }
                }

                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Producto eliminado correctamente.";
            }

            return RedirectToAction(nameof(GestionarMenu));
        }

        // API: Obtener estadísticas
        [HttpGet]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            _logger.LogInformation($"📊 Obteniendo estadísticas para Restaurant ID: {CURRENT_RESTAURANT_ID}");

            var restaurant = await _context.Restaurantes
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == CURRENT_RESTAURANT_ID);

            if (restaurant == null)
            {
                return NotFound();
            }

            var estadisticas = new
            {
                TotalProductos = restaurant.MenuItems?.Count() ?? 0,
                PrecioPromedio = restaurant.MenuItems?.Any() == true ? restaurant.MenuItems.Average(m => m.Precio) : 0,
                ProductoMasCaro = restaurant.MenuItems?.MaxBy(m => m.Precio),
                ProductoMasBarato = restaurant.MenuItems?.MinBy(m => m.Precio)
            };

            return Json(estadisticas);
        }

        private bool RestaurantExists(int id)
        {
            return _context.Restaurantes.Any(e => e.Id == id);
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }
    }
}