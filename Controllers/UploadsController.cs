using Microsoft.AspNetCore.Mvc;

namespace IglesiaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("fotos")]
        public async Task<IActionResult> UploadFoto(IFormFile file)
        {
            // 1. Validación de existencia
            if (file == null || file.Length == 0)
                return BadRequest("No se seleccionó ningún archivo.");

            // 2. Restricción de Tamaño: Máximo 2MB (2 * 1024 * 1024 bytes)
            if (file.Length > 2 * 1024 * 1024)
                return BadRequest("La foto es muy pesada. El tamaño máximo permitido es 2MB.");

            // 3. Restricción de Formato (Extensiones permitidas)
            var extension = Path.GetExtension(file.FileName).ToLower();
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };

            if (!extensionesPermitidas.Contains(extension))
                return BadRequest("Formato no válido. Solo se permiten imágenes JPG o PNG.");

            try
            {
                // 4. Preparar la ruta de guardado (wwwroot/uploads/fotos)
                // Usamos _env.WebRootPath que apunta a la carpeta 'wwwroot'
                var folderPath = Path.Combine(_env.WebRootPath, "uploads", "fotos");

                // Si la carpeta no existe en el servidor, la creamos automáticamente
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // 5. Crear un nombre único para el archivo (usando un GUID)
                // Esto evita que si dos personas suben "foto.jpg", una borre la otra.
                var nombreUnico = $"{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(folderPath, nombreUnico);

                // 6. Guardar el archivo físicamente en el disco
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 7. Retornar éxito y el nombre del archivo generado
                // Este nombre es el que guardaremos en la base de datos más adelante.
                return Ok(new { fileName = nombreUnico });
            }
            catch (Exception ex)
            {
                // En caso de un error inesperado (permisos de carpeta, etc.)
                return StatusCode(500, $"Error interno al guardar la imagen: {ex.Message}");
            }
        }
    }
}