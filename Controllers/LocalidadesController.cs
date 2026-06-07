using IglesiaAPI.Data;
using IglesiaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IglesiaAPI.Infrastructure.Auth; // 🔹 Para UserContext

namespace IglesiaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocalidadesController : ControllerBase
    {
        private readonly IglesiaDBContext _context;

        public LocalidadesController(IglesiaDBContext context)
        {
            _context = context;
        }

        // 🔹 Obtener contexto del usuario autenticado
        private UserContext CurrentUser => UserContextFactory.FromClaims(User);

        // --- 1. GET (Leer Todas) ---
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Localidad>>> GetLocalidades()
        {
            if (_context.Localidades == null)
                return NotFound();

            var query = _context.Localidades.AsQueryable();

            // 🔹 Si no es Admin, solo puede ver su localidad
            if (!CurrentUser.EsAdmin)
                query = query.Where(l => l.LocalidadID == CurrentUser.LocalidadID);

            var localidades = await query.ToListAsync();

            return Ok(localidades.Select(l =>
            {
                l.EsGlobal = CurrentUser.EsAdmin;
                return l;
            }));
        }

        // --- 2. GET (Leer por ID) ---
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Localidad>> GetLocalidad(int id)
        {
            if (_context.Localidades == null)
                return NotFound();

            var localidad = await _context.Localidades.FindAsync(id);

            if (localidad == null)
                return NotFound();

            if (!CurrentUser.EsAdmin && localidad.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            localidad.EsGlobal = CurrentUser.EsAdmin;
            return Ok(localidad);
        }

        // --- 3. POST (Crear Nuevo) ---
        [HttpPost]
        public async Task<ActionResult<Localidad>> PostLocalidad([FromBody] Localidad localidad)
        {
            if (_context.Localidades == null)
                return Problem("Entity set 'IglesiaDBContext.Localidades' is null.");

            // 🔹 Solo el SuperUsuario puede crear
            if (!User.IsInRole("SuperUsuario"))
            {
                return Forbid(); // Aquí es donde te daba el 403
            }

            _context.Localidades.Add(localidad);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLocalidad), new { id = localidad.LocalidadID }, localidad);
        }

        // --- 4. PUT (Actualizar Existente) ---

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutLocalidad(int id, [FromBody] Localidad localidad)
        {
            if (id != localidad.LocalidadID)
                return BadRequest("El ID de la ruta no coincide con el del cuerpo.");

            // 🔹 Validamos directamente contra el rol "SuperUsuario" que viene en tu Token
            // Usamos el Claim de Rol para mayor seguridad
            var usuarioRol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                             ?? User.FindFirst("rol")?.Value;

            if (usuarioRol != "SuperUsuario")
            {
                return Forbid(); // Si no dice exactamente SuperUsuario, 403.
            }

            _context.Entry(localidad).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocalidadExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // --- 5. DELETE (Eliminar) ---
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteLocalidad(int id)
        {
            var localidad = await _context.Localidades.FindAsync(id);
            if (localidad == null) return NotFound();

            // 🔹 Solo el SuperUsuario puede eliminar
            if (!User.IsInRole("SuperUsuario"))
            {
                return Forbid();
            }

            _context.Localidades.Remove(localidad);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // --- Método Auxiliar ---
        private bool LocalidadExists(int id)
        {
            return (_context.Localidades?.Any(e => e.LocalidadID == id)).GetValueOrDefault();
        }
    }
}