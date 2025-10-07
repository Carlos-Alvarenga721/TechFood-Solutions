namespace TechFood_Solutions.Services
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile file, string folder, string subFolder = null);
        bool DeleteImage(string fileName, string folder, string subFolder = null);
    }

    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IWebHostEnvironment environment, ILogger<ImageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(IFormFile file, string folder, string subFolder = null)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Intento de guardar archivo nulo o vacío");
                return null;
            }

            try
            {
                // Validar tamaño (máximo 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    _logger.LogWarning($"Archivo muy grande: {file.Length} bytes");
                    return null;
                }

                // Validar extensión
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning($"Extensión no permitida: {extension}");
                    return null;
                }

                // Usar el nombre original del archivo
                var fileName = file.FileName;

                // Construir la ruta completa con o sin subcarpeta
                var uploadsFolder = string.IsNullOrEmpty(subFolder)
                    ? Path.Combine(_environment.WebRootPath, "images", folder)
                    : Path.Combine(_environment.WebRootPath, "images", folder, subFolder);

                // Crear el directorio si no existe
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    _logger.LogInformation($"Directorio creado: {uploadsFolder}");
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Guardar el archivo
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                _logger.LogInformation($"Imagen guardada exitosamente: {fileName} en {folder}/{subFolder}");

                // Retornar solo el nombre del archivo
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la imagen");
                return null;
            }
        }

        public bool DeleteImage(string fileName, string folder, string subFolder = null)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            try
            {
                var filePath = string.IsNullOrEmpty(subFolder)
                    ? Path.Combine(_environment.WebRootPath, "images", folder, fileName)
                    : Path.Combine(_environment.WebRootPath, "images", folder, subFolder, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"Imagen eliminada: {fileName}");
                    return true;
                }

                _logger.LogWarning($"Archivo no encontrado: {filePath}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar imagen: {fileName}");
                return false;
            }
        }
    }
}