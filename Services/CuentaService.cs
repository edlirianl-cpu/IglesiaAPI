using IglesiaAPI.Data;
using IglesiaAPI.Models;
using IglesiaAPI.Infrastructure.Auth; // para UserContext
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IglesiaAPI.Services
{
    public class CuentaService : ICuentaService
    {
        private readonly IglesiaDBContext _context;

        public CuentaService(IglesiaDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cuenta>> GetAllCuentasAsync(UserContext user)
        {
            var query = _context.Cuentas.AsQueryable();

            // 🚀 Lógica de Visibilidad Integrada:
            // Si no es Admin, ve las cuentas de su LocalidadID O las que son marcadas como EsNacional.
            if (!user.EsAdmin)
            {
                query = query.Where(c => c.LocalidadID == user.LocalidadID || c.EsNacional == true);
            }

            return await query.ToListAsync();
        }

        public async Task<Cuenta?> GetCuentaByIdAsync(UserContext user, int id)
        {
            var query = _context.Cuentas.AsQueryable();

            // Primero filtramos por el ID solicitado
            query = query.Where(c => c.CuentaID == id);

            // 🔐 Validación de Seguridad:
            // Un usuario solo puede ver el detalle si la cuenta le pertenece 
            // por localidad o si es una cuenta de alcance nacional.
            if (!user.EsAdmin)
            {
                query = query.Where(c => c.LocalidadID == user.LocalidadID || c.EsNacional == true);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Cuenta> AddCuentaAsync(Cuenta cuenta)
        {
            // Toda cuenta nueva comienza con saldo cero para integridad contable
            cuenta.Saldo = 0;

            _context.Cuentas.Add(cuenta);
            await _context.SaveChangesAsync();
            return cuenta;
        }

        public async Task<bool> UpdateCuentaAsync(Cuenta cuenta)
        {
            // Evitamos que se modifique el saldo directamente desde este método 
            // para proteger la integridad de los movimientos.
            _context.Entry(cuenta).State = EntityState.Modified;
            _context.Entry(cuenta).Property(x => x.Saldo).IsModified = false;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteCuentaAsync(int id)
        {
            var cuenta = await _context.Cuentas
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.CuentaID == id);

            if (cuenta == null) return false;

            // 🛑 Regla de integridad: No se puede borrar una cuenta que ya tiene historia
            if (cuenta.Movimientos.Any())
            {
                throw new System.InvalidOperationException("No se puede eliminar una cuenta con movimientos asociados.");
            }

            _context.Cuentas.Remove(cuenta);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}