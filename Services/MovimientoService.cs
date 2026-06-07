using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IglesiaAPI.Data;
using IglesiaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IglesiaAPI.Services
{
    public class MovimientoService : IMovimientoService
    {
        private readonly IglesiaDBContext _context;

        public MovimientoService(IglesiaDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Movimiento>> GetAllMovimientosAsync()
        {
            return await _context.Movimientos
                                 .Include(m => m.Cuenta)
                                 .Include(m => m.Localidad)
                                 .OrderByDescending(m => m.FechaMovimiento)
                                 .ToListAsync();
        }

        public async Task<Movimiento?> GetMovimientoByIdAsync(int id)
        {
            return await _context.Movimientos
                                 .Include(m => m.Cuenta)
                                 .Include(m => m.Localidad)
                                 .FirstOrDefaultAsync(m => m.MovimientoID == id);
        }

        public async Task AddMovimientoAsync(Movimiento movimiento)
        {
            var cuenta = await _context.Cuentas.FindAsync(movimiento.CuentaID);
            if (cuenta == null) throw new InvalidOperationException("La cuenta especificada no existe.");

            // Lógica contable: Afectar saldo de la cuenta
            if (movimiento.TipoMovimiento.Equals("Ingreso", StringComparison.OrdinalIgnoreCase))
                cuenta.Saldo += movimiento.Monto;
            else if (movimiento.TipoMovimiento.Equals("Egreso", StringComparison.OrdinalIgnoreCase))
                cuenta.Saldo -= movimiento.Monto;

            // Auditoría y estado inicial
            movimiento.EstadoValidacion = "Pendiente";
            movimiento.FechaRegistro = DateTime.Now;
            movimiento.EsEditable = true;
            movimiento.LoteReferencia = null; // Inicialmente sin lote

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMovimientoAsync(Movimiento movimiento)
        {
            var existente = await _context.Movimientos.AsNoTracking()
                                         .FirstOrDefaultAsync(m => m.MovimientoID == movimiento.MovimientoID);

            if (existente == null) throw new InvalidOperationException("El movimiento no existe.");

            // 🔹 REGLA DE ORO ACTUALIZADA: Bloqueo por Validación O por estar en un Lote
            if (existente.EstadoValidacion == "Validado")
                throw new InvalidOperationException("No se puede modificar un movimiento ya validado por el banco.");

            if (!string.IsNullOrEmpty(existente.LoteReferencia))
                throw new InvalidOperationException("No se puede modificar un movimiento que ya ha sido agrupado en un lote para depósito.");

            _context.Entry(movimiento).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMovimientoAsync(int id)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento != null)
            {
                if (movimiento.EstadoValidacion == "Validado")
                    throw new InvalidOperationException("No se puede eliminar un registro ya validado.");

                if (!string.IsNullOrEmpty(movimiento.LoteReferencia))
                    throw new InvalidOperationException("No se puede eliminar un registro que ya pertenece a un lote de depósito.");

                var cuenta = await _context.Cuentas.FindAsync(movimiento.CuentaID);
                if (cuenta != null)
                {
                    // Revertir el saldo
                    if (movimiento.TipoMovimiento.Equals("Ingreso", StringComparison.OrdinalIgnoreCase))
                        cuenta.Saldo -= movimiento.Monto;
                    else
                        cuenta.Saldo += movimiento.Monto;
                }

                _context.Movimientos.Remove(movimiento);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Movimiento>> GetMovimientosByCuentaIdAsync(int cuentaId)
        {
            return await _context.Movimientos
                                 .Where(m => m.CuentaID == cuentaId)
                                 .Include(m => m.Cuenta)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Movimiento>> GetMovimientosPendientesValidacionAsync()
        {
            return await _context.Movimientos
                                 .Where(m => m.EstadoValidacion == "Pendiente")
                                 .Include(m => m.Cuenta)
                                 .Include(m => m.Localidad)
                                 .OrderBy(m => m.FechaMovimiento)
                                 .ToListAsync();
        }

        public async Task ValidarMovimientoBancarioAsync(int movimientoId, string noReferenciaBanco, int usuarioAprobadorId)
        {
            var movimiento = await _context.Movimientos.FindAsync(movimientoId);

            if (movimiento == null)
                throw new InvalidOperationException("Movimiento no encontrado.");

            movimiento.EstadoValidacion = "Validado";
            movimiento.NoReferencia = noReferenciaBanco;
            movimiento.UsuarioID_Aprobador = usuarioAprobadorId;
            movimiento.EsEditable = false;

            await _context.SaveChangesAsync();
        }

        // 🔹 NUEVO: Lógica para agrupar movimientos en un lote (Backend Service)
        public async Task<string> GenerarLoteAgrupadoAsync(List<int> ids, string prefijoLocalidad)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string codigoLote = $"L-{prefijoLocalidad}-{DateTime.Now:ddMMyy}-{new Random().Next(10, 99)}";

                var movimientos = await _context.Movimientos
                    .Where(m => ids.Contains(m.MovimientoID) && string.IsNullOrEmpty(m.LoteReferencia))
                    .ToListAsync();

                foreach (var m in movimientos)
                {
                    m.LoteReferencia = codigoLote;
                    m.EsEditable = false; // Bloqueo preventivo
                    m.EstadoValidacion = "En Lote";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return codigoLote;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}