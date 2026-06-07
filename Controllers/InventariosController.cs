using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IglesiaAPI.Data;
using IglesiaAPI.Models;
using IglesiaAPI.DTOs;
using IglesiaAPI.Infrastructure.Auth;

namespace IglesiaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventariosController : ControllerBase
    {
        private readonly IglesiaDBContext _context;
        private readonly IWebHostEnvironment _env;

        public InventariosController(IglesiaDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private UserContext CurrentUser => UserContextFactory.FromClaims(User);

        // GET: api/Inventarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventario>>> GetInventarios()
        {
            var query = _context.Inventarios
                                .Include(i => i.Localidad)
                                .OrderByDescending(i => i.InventarioID)
                                .AsQueryable();

            if (!CurrentUser.EsAdmin && CurrentUser.Rol != "TesoreroNacional")
                query = query.Where(i => i.LocalidadID == CurrentUser.LocalidadID);

            var inventarios = await query.ToListAsync();

            return Ok(inventarios.Select(i =>
            {
                i.EsGlobal = CurrentUser.EsAdmin || CurrentUser.Rol == "TesoreroNacional";
                return i;
            }));
        }

        // GET: api/Inventarios/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Inventario>> GetInventario(int id)
        {
            var inventario = await _context.Inventarios
                                           .Include(i => i.Localidad)
                                           .FirstOrDefaultAsync(i => i.InventarioID == id);

            if (inventario == null) return NotFound();

            if (!CurrentUser.EsAdmin && CurrentUser.Rol != "TesoreroNacional" && inventario.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            inventario.EsGlobal = CurrentUser.EsAdmin || CurrentUser.Rol == "TesoreroNacional";
            return Ok(inventario);
        }

        // POST: api/Inventarios
        [HttpPost]
        public async Task<ActionResult<Inventario>> PostInventario([FromBody] InventarioDTO inventarioDto)
        {
            if (!CurrentUser.EsAdmin && CurrentUser.Rol != "TesoreroNacional" && inventarioDto.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            var inventario = new Inventario
            {
                NombreArticulo = inventarioDto.NombreArticulo,
                Cantidad = inventarioDto.Cantidad,
                Ubicacion = inventarioDto.Ubicacion,
                Marca = inventarioDto.Marca,
                Modelo = inventarioDto.Modelo,
                NoSerie = inventarioDto.NoSerie,
                Estado = inventarioDto.Estado ?? "Bueno",
                ValorUnitario = inventarioDto.ValorUnitario,
                ImagenUrl = inventarioDto.ImagenUrl,
                Responsable = inventarioDto.Responsable,
                LocalidadID = (CurrentUser.EsAdmin || CurrentUser.Rol == "TesoreroNacional") ? inventarioDto.LocalidadID : CurrentUser.LocalidadID,
                UsuarioID_Registra = CurrentUser.UsuarioID,
                FechaRegistro = DateTime.Now
            };

            _context.Inventarios.Add(inventario);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error al guardar el inventario: {ex.Message}");
            }

            var nuevoInventario = await _context.Inventarios
                                                .Include(i => i.Localidad)
                                                .Include(i => i.CreadoPor)
                                                .FirstOrDefaultAsync(i => i.InventarioID == inventario.InventarioID);

            return CreatedAtAction(nameof(GetInventario), new { id = nuevoInventario!.InventarioID }, nuevoInventario);
        }

        // PUT: api/Inventarios/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutInventario(int id, [FromBody] InventarioDTO inventarioDto)
        {
            var inventarioExistente = await _context.Inventarios.FindAsync(id);
            if (inventarioExistente == null) return NotFound();

            if (!CurrentUser.EsAdmin && CurrentUser.Rol != "TesoreroNacional" && inventarioExistente.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            inventarioExistente.NombreArticulo = inventarioDto.NombreArticulo;
            inventarioExistente.Cantidad = inventarioDto.Cantidad;
            inventarioExistente.Ubicacion = inventarioDto.Ubicacion;
            inventarioExistente.Marca = inventarioDto.Marca;
            inventarioExistente.Modelo = inventarioDto.Modelo;
            inventarioExistente.NoSerie = inventarioDto.NoSerie;
            inventarioExistente.Estado = inventarioDto.Estado;
            inventarioExistente.ValorUnitario = inventarioDto.ValorUnitario;
            inventarioExistente.ImagenUrl = inventarioDto.ImagenUrl;
            inventarioExistente.Responsable = inventarioDto.Responsable;

            if (CurrentUser.EsAdmin || CurrentUser.Rol == "TesoreroNacional")
                inventarioExistente.LocalidadID = inventarioDto.LocalidadID;

            _context.Entry(inventarioExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error al actualizar: {ex.Message}");
            }

            return NoContent();
        }

        // SUBIDA DE IMAGEN CORREGIDA
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0) return BadRequest("Archivo no seleccionado");

                // Usamos _env que es el nombre definido en tu constructor
                string folderPath = Path.Combine(_env.WebRootPath, "uploads", "inventario");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = $"{Guid.NewGuid()}_{file.FileName.Replace(" ", "_")}";
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string dbUrl = $"/uploads/inventario/{fileName}";

                return Ok(new { url = dbUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al guardar en el servidor: {ex.Message}");
            }
        }

        // DELETE: api/Inventarios/5 con borrado de imagen física
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteInventario(int id)
        {
            var inventario = await _context.Inventarios.FindAsync(id);
            if (inventario == null) return NotFound();

            if (!CurrentUser.EsAdmin && inventario.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            // Borrar imagen física si existe
            if (!string.IsNullOrEmpty(inventario.ImagenUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, inventario.ImagenUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Inventarios.Remove(inventario);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}