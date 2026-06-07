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
    public class RegistrosSecretariaController : ControllerBase
    {
        private readonly IglesiaDBContext _context;

        public RegistrosSecretariaController(IglesiaDBContext context)
        {
            _context = context;
        }

        private UserContext CurrentUser => UserContextFactory.FromClaims(User);

        // GET: api/RegistrosSecretaria
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistroSecretaria>>> GetRegistrosSecretaria()
        {
            bool esGlobal = CurrentUser.Rol == RolesSistema.SuperUsuario ||
                            CurrentUser.Rol == RolesSistema.Administrador ||
                            CurrentUser.Rol == RolesSistema.SecretarioNacional;

            var query = _context.RegistrosSecretaria
                                .Include(r => r.Localidad)
                                .Include(r => r.CreadoPor)
                                .OrderByDescending(r => r.FechaRegistro)
                                .AsQueryable();

            if (!esGlobal)
                query = query.Where(r => r.LocalidadID == CurrentUser.LocalidadID);

            var registros = await query.ToListAsync();

            // Seteamos el flag de visibilidad para el frontend
            foreach (var r in registros) { r.EsGlobal = esGlobal; }

            return Ok(registros);
        }

        // GET: api/RegistrosSecretaria/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RegistroSecretaria>> GetRegistroSecretaria(int id)
        {
            var registro = await _context.RegistrosSecretaria
                                         .Include(r => r.Localidad)
                                         .Include(r => r.CreadoPor)
                                         .FirstOrDefaultAsync(r => r.RegistroID == id);

            if (registro == null)
                return NotFound();

            bool esGlobal = CurrentUser.Rol == RolesSistema.SuperUsuario ||
                            CurrentUser.Rol == RolesSistema.Administrador ||
                            CurrentUser.Rol == RolesSistema.SecretarioNacional;

            if (!esGlobal && registro.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            registro.EsGlobal = esGlobal;
            return Ok(registro);
        }

        // POST: api/RegistrosSecretaria
        [HttpPost]
        public async Task<ActionResult<RegistroSecretaria>> PostRegistroSecretaria([FromBody] RegistroSecretariaDTO registroDto)
        {
            if (registroDto == null) return BadRequest("Datos no recibidos");

            bool esGlobal = CurrentUser.Rol == RolesSistema.SuperUsuario ||
                            CurrentUser.Rol == RolesSistema.Administrador ||
                            CurrentUser.Rol == RolesSistema.SecretarioNacional;

            if (!esGlobal && registroDto.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            // Lógica de IDs
            int localidadIdFinal = esGlobal ? (registroDto.LocalidadID > 0 ? registroDto.LocalidadID : CurrentUser.LocalidadID)
                                            : CurrentUser.LocalidadID;

            int? creadorIdFinal = (registroDto.CreadoPorID > 0) ? registroDto.CreadoPorID : CurrentUser.UsuarioID;

            var registro = new RegistroSecretaria
            {
                FechaRegistro = registroDto.FechaRegistro.Date,
                TipoRegistro = registroDto.TipoRegistro ?? "Evento",
                Titulo = registroDto.Titulo ?? "Sin Titulo",
                Descripcion = registroDto.Descripcion,
                DocumentoURL = registroDto.DocumentoURL,
                LocalidadID = localidadIdFinal,
                CreadoPorID = creadorIdFinal > 0 ? creadorIdFinal : null
            };

            try
            {
                _context.RegistrosSecretaria.Add(registro);
                await _context.SaveChangesAsync(); // 🔹 SOLO ESTE SaveChanges
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error al guardar en base de datos: {ex.InnerException?.Message ?? ex.Message}");
            }

            // Recuperar completo para el frontend
            var nuevoRegistro = await _context.RegistrosSecretaria
                                                .Include(r => r.Localidad)
                                                .Include(r => r.CreadoPor)
                                                .FirstOrDefaultAsync(r => r.RegistroID == registro.RegistroID);

            if (nuevoRegistro != null) nuevoRegistro.EsGlobal = esGlobal;

            return CreatedAtAction(nameof(GetRegistroSecretaria), new { id = registro.RegistroID }, nuevoRegistro);
        }

        // PUT: api/RegistrosSecretaria/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutRegistroSecretaria(int id, [FromBody] RegistroSecretariaDTO registroDto)
        {
            var registroExistente = await _context.RegistrosSecretaria.FindAsync(id);
            if (registroExistente == null)
                return NotFound($"Registro de Secretaría con ID {id} no encontrado.");

            bool esGlobal = CurrentUser.Rol == RolesSistema.SuperUsuario ||
                            CurrentUser.Rol == RolesSistema.Administrador ||
                            CurrentUser.Rol == RolesSistema.SecretarioNacional;

            if (!esGlobal && registroExistente.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            if (!esGlobal && registroDto.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            // 🔹 Ajuste para MySQL: Solo la fecha
            registroExistente.FechaRegistro = registroDto.FechaRegistro.Date;
            registroExistente.TipoRegistro = registroDto.TipoRegistro;
            registroExistente.Titulo = registroDto.Titulo?? String.Empty;
            registroExistente.Descripcion = registroDto.Descripcion;
            registroExistente.DocumentoURL = registroDto.DocumentoURL;

            // Solo roles globales pueden reasignar sede o autor de registro
            registroExistente.LocalidadID = esGlobal ? registroDto.LocalidadID : CurrentUser.LocalidadID;
            registroExistente.CreadoPorID = esGlobal ? (registroDto.CreadoPorID ?? registroExistente.CreadoPorID) : CurrentUser.UsuarioID;

            _context.Entry(registroExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.RegistrosSecretaria.Any(e => e.RegistroID == id))
                    return NotFound();
                else
                    throw;
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error al actualizar el registro: {ex.InnerException?.Message ?? ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/RegistrosSecretaria/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRegistroSecretaria(int id)
        {
            var registro = await _context.RegistrosSecretaria.FindAsync(id);
            if (registro == null)
                return NotFound();

            bool esGlobal = CurrentUser.Rol == RolesSistema.SuperUsuario ||
                            CurrentUser.Rol == RolesSistema.Administrador ||
                            CurrentUser.Rol == RolesSistema.SecretarioNacional;

            if (!esGlobal && registro.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            _context.RegistrosSecretaria.Remove(registro);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}