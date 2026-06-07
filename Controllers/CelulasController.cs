using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IglesiaAPI.Data;
using IglesiaAPI.Models;
using IglesiaAPI.DTOs;
using IglesiaAPI.Infrastructure.Auth; // 🔹 Para UserContext

namespace IglesiaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CelulasController : ControllerBase
    {
        private readonly IglesiaDBContext _context;

        public CelulasController(IglesiaDBContext context)
        {
            _context = context;
        }

        // 🔹 Obtener contexto del usuario autenticado
        private UserContext CurrentUser => UserContextFactory.FromClaims(User);

        // GET: api/Celulas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CelulaDTO>>> GetCelulas()
        {
            var query = _context.Celulas
                                .Include(c => c.Miembro)
                                .Include(c => c.Localidad)
                                .Include(c => c.CreadoPor)
                                .AsQueryable();

            // Lógica de visibilidad: SuperUsuario y Administrador ven todo
            bool esGlobal = CurrentUser.Rol == RolesSistema.SuperUsuario ||
                            CurrentUser.Rol == RolesSistema.Administrador;

            if (!esGlobal)
                query = query.Where(c => c.LocalidadID == CurrentUser.LocalidadID);

            var celulas = await query.ToListAsync();

            var dtoList = celulas.Select(c => new CelulaDTO
            {
                // 🔹 CORRECCIÓN: Se agrega el CelulaID para que el Frontend sepa qué borrar
                CelulaID = c.CelulaID,
                NombreCelula = c.NombreCelula,
                DiaReunion = c.DiaReunion,
                HoraReunion = c.HoraReunion,
                MiembroID = c.MiembroID,
                NombreLider = c.Miembro?.NombreCompleto,   // 🔹 propiedad del modelo Miembro
                LocalidadID = c.LocalidadID,
                LocalidadNombre = c.Localidad?.NombreLocalidad ?? string.Empty,
                CreadoPorID = c.CreadoPorID,
                CreadoPorNombre = c.CreadoPor?.NombreUsuario,
                EsGlobal = esGlobal
            }).ToList();

            return Ok(dtoList);
        }

        // GET: api/Celulas/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CelulaDTO>> GetCelula(int id)
        {
            var celula = await _context.Celulas
                                       .Include(c => c.Miembro)
                                       .Include(c => c.Localidad)
                                       .Include(c => c.CreadoPor)
                                       .FirstOrDefaultAsync(c => c.CelulaID == id);

            if (celula == null)
                return NotFound();

            bool esGlobal = CurrentUser.Rol == RolesSistema.SuperUsuario ||
                            CurrentUser.Rol == RolesSistema.Administrador;

            if (!esGlobal && celula.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            var dto = new CelulaDTO
            {
                // 🔹 CORRECCIÓN: Se agrega el CelulaID
                CelulaID = celula.CelulaID,
                NombreCelula = celula.NombreCelula,
                DiaReunion = celula.DiaReunion,
                HoraReunion = celula.HoraReunion,
                MiembroID = celula.MiembroID,
                NombreLider = celula.Miembro?.NombreCompleto,
                LocalidadID = celula.LocalidadID,
                LocalidadNombre = celula.Localidad?.NombreLocalidad ?? string.Empty,
                CreadoPorID = celula.CreadoPorID,
                CreadoPorNombre = celula.CreadoPor?.NombreUsuario,
                EsGlobal = esGlobal
            };

            return Ok(dto);
        }

        // POST: api/Celulas
        [HttpPost]
        public async Task<ActionResult<CelulaDTO>> PostCelula([FromBody] CelulaDTO celulaDto)
        {
            bool esGlobal = CurrentUser.Rol == RolesSistema.SuperUsuario ||
                            CurrentUser.Rol == RolesSistema.Administrador;

            if (!esGlobal && celulaDto.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            if (!esGlobal)
                celulaDto.LocalidadID = CurrentUser.LocalidadID;

            var celula = new Celula
            {
                NombreCelula = celulaDto.NombreCelula,
                DiaReunion = celulaDto.DiaReunion,
                HoraReunion = celulaDto.HoraReunion,
                MiembroID = celulaDto.MiembroID ?? 0,
                LocalidadID = celulaDto.LocalidadID,
                CreadoPorID = CurrentUser.UsuarioID
            };

            _context.Celulas.Add(celula);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error al guardar la célula. Verifique MiembroID o LocalidadID. Detalle: {ex.Message}");
            }

            var nuevaCelula = await _context.Celulas
                                            .Include(c => c.Miembro)
                                            .Include(c => c.Localidad)
                                            .Include(c => c.CreadoPor)
                                            .FirstOrDefaultAsync(c => c.CelulaID == celula.CelulaID);

            var dto = new CelulaDTO
            {
                // 🔹 CORRECCIÓN: Se agrega el CelulaID recién creado
                CelulaID = nuevaCelula!.CelulaID,
                NombreCelula = nuevaCelula.NombreCelula,
                DiaReunion = nuevaCelula.DiaReunion,
                HoraReunion = nuevaCelula.HoraReunion,
                MiembroID = nuevaCelula.MiembroID,
                NombreLider = nuevaCelula.Miembro?.NombreCompleto,
                LocalidadID = nuevaCelula.LocalidadID,
                LocalidadNombre = nuevaCelula.Localidad?.NombreLocalidad ?? string.Empty,
                CreadoPorID = nuevaCelula.CreadoPorID,
                CreadoPorNombre = nuevaCelula.CreadoPor?.NombreUsuario,
                EsGlobal = esGlobal
            };

            return CreatedAtAction(nameof(GetCelula), new { id = celula.CelulaID }, dto);
        }

        // PUT: api/Celulas/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutCelula(int id, [FromBody] CelulaDTO celulaDto)
        {
            var celulaExistente = await _context.Celulas.FindAsync(id);
            if (celulaExistente == null)
                return NotFound($"Célula con ID {id} no encontrada.");

            bool esGlobal = CurrentUser.Rol == RolesSistema.SuperUsuario ||
                            CurrentUser.Rol == RolesSistema.Administrador;

            if (!esGlobal && celulaExistente.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            if (!esGlobal && celulaDto.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            celulaExistente.NombreCelula = celulaDto.NombreCelula;
            celulaExistente.DiaReunion = celulaDto.DiaReunion;
            celulaExistente.HoraReunion = celulaDto.HoraReunion;
            celulaExistente.MiembroID = celulaDto.MiembroID ?? celulaExistente.MiembroID;
            celulaExistente.LocalidadID = esGlobal ? celulaDto.LocalidadID : CurrentUser.LocalidadID;
            celulaExistente.CreadoPorID = esGlobal ? celulaDto.CreadoPorID ?? celulaExistente.CreadoPorID : CurrentUser.UsuarioID;

            _context.Entry(celulaExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Celulas.Any(e => e.CelulaID == id))
                    return NotFound();
                else
                    throw;
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error al actualizar la célula. Verifique MiembroID o LocalidadID. Detalle: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/Celulas/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCelula(int id)
        {
            var celula = await _context.Celulas.FindAsync(id);
            if (celula == null)
                return NotFound();

            bool esGlobal = CurrentUser.Rol == RolesSistema.SuperUsuario ||
                            CurrentUser.Rol == RolesSistema.Administrador;

            if (!esGlobal && celula.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            _context.Celulas.Remove(celula);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}