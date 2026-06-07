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
    public class CuentasController : ControllerBase
    {
        private readonly IglesiaDBContext _context;

        public CuentasController(IglesiaDBContext context)
        {
            _context = context;
        }

        // 🔹 Obtener contexto del usuario autenticado
        private UserContext CurrentUser => UserContextFactory.FromClaims(User);

        // GET: api/Cuentas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cuenta>>> GetCuentas()
        {
            var query = _context.Cuentas.AsQueryable();

            // 🚀 Lógica de Visibilidad:
            // 1. Admin ve todas.
            // 2. Usuarios ven cuentas de su localidad O cuentas marcadas como Nacionales.
            if (!CurrentUser.EsAdmin)
            {
                query = query.Where(c => c.LocalidadID == CurrentUser.LocalidadID || c.EsNacional == true);
            }

            var cuentas = await query.ToListAsync();

            // Sincronizamos la propiedad auxiliar EsGlobal para el frontend
            foreach (var c in cuentas)
            {
                c.EsGlobal = CurrentUser.EsAdmin;
            }

            return Ok(cuentas);
        }

        // GET: api/Cuentas/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Cuenta>> GetCuenta(int id)
        {
            var cuenta = await _context.Cuentas.FindAsync(id);

            if (cuenta == null)
                return NotFound();

            // Un usuario puede ver una cuenta si es de su localidad o si es Nacional
            if (!CurrentUser.EsAdmin && cuenta.LocalidadID != CurrentUser.LocalidadID && !cuenta.EsNacional)
                return Forbid();

            cuenta.EsGlobal = CurrentUser.EsAdmin;
            return Ok(cuenta);
        }

        // POST: api/Cuentas
        [HttpPost]
        public async Task<ActionResult<Cuenta>> PostCuenta([FromBody] CuentaDTO cuentaDto)
        {
            // Solo el Admin puede crear cuentas marcadas como Nacionales
            if (cuentaDto.EsNacional && !CurrentUser.EsAdmin)
                return Forbid();

            // Si no es admin, la cuenta se asigna forzosamente a su localidad
            if (!CurrentUser.EsAdmin)
            {
                cuentaDto.LocalidadID = CurrentUser.LocalidadID;
            }

            var cuenta = new Cuenta
            {
                NombreCuenta = cuentaDto.NombreCuenta,
                Tipo = cuentaDto.Tipo,
                LocalidadID = cuentaDto.LocalidadID,
                EsNacional = cuentaDto.EsNacional, // Se asigna la propiedad nacional
                CreadoPorID = CurrentUser.UsuarioID,
                Saldo = 0 // Toda cuenta nueva inicia en cero
            };

            _context.Cuentas.Add(cuenta);
            await _context.SaveChangesAsync();

            cuenta.EsGlobal = CurrentUser.EsAdmin;

            return CreatedAtAction(nameof(GetCuenta), new { id = cuenta.CuentaID }, cuenta);
        }

        // PUT: api/Cuentas/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutCuenta(int id, [FromBody] CuentaDTO cuentaDto)
        {
            var cuentaExistente = await _context.Cuentas.FindAsync(id);
            if (cuentaExistente == null)
                return NotFound($"Cuenta con ID {id} no encontrada.");

            // Protección de seguridad
            if (!CurrentUser.EsAdmin && cuentaExistente.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            // Solo admin puede cambiar el estado de EsNacional
            if (cuentaExistente.EsNacional != cuentaDto.EsNacional && !CurrentUser.EsAdmin)
                return Forbid();

            cuentaExistente.NombreCuenta = cuentaDto.NombreCuenta;
            cuentaExistente.Tipo = cuentaDto.Tipo;

            // Si es admin, permite cambiar localidad, si no, mantiene la del usuario
            cuentaExistente.LocalidadID = CurrentUser.EsAdmin ? cuentaDto.LocalidadID : CurrentUser.LocalidadID;
            cuentaExistente.EsNacional = cuentaDto.EsNacional;

            _context.Entry(cuentaExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Cuentas.Any(e => e.CuentaID == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Cuentas/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCuenta(int id)
        {
            var cuenta = await _context.Cuentas.Include(c => c.Movimientos).FirstOrDefaultAsync(c => c.CuentaID == id);

            if (cuenta == null)
                return NotFound();

            if (!CurrentUser.EsAdmin && cuenta.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            // 🛑 Validación: No eliminar cuentas que ya tengan historial contable
            if (cuenta.Movimientos.Any())
            {
                return BadRequest("No se puede eliminar la cuenta porque ya tiene movimientos asociados. Considere desactivarla o renombrarla.");
            }

            _context.Cuentas.Remove(cuenta);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}