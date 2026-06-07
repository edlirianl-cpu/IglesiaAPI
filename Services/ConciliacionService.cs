using IglesiaAPI.Data;
using IglesiaAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IglesiaAPI.Services
{
    public interface IConciliacionService
    {
        Task<ResultadoConciliacionDTO> ProcesarMatchingAutomaticoAsync(List<TransaccionBancoDTO> transacciones, int autorizadorId);
    }

    public class ConciliacionService : IConciliacionService
    {
        private readonly IglesiaDBContext _context;

        public ConciliacionService(IglesiaDBContext context)
        {
            _context = context;
        }

        public async Task<ResultadoConciliacionDTO> ProcesarMatchingAutomaticoAsync(List<TransaccionBancoDTO> transacciones, int autorizadorId)
        {
            int validados = 0;

            // 1. Cargamos los movimientos con estado 'Pendiente' de la base de datos
            var pendientes = await _context.Movimientos
                .Where(m => m.EstadoValidacion == "Pendiente")
                .ToListAsync();

            // 2. Procesamos cada transacción enviada desde el Excel (ya con la columna correcta)
            foreach (var banco in transacciones)
            {
                // Normalización de datos del banco para la comparación
                decimal montoBanco = Math.Abs(Math.Round(banco.Monto, 2));
                string descBanco = (banco.DescripcionCorta ?? "").Trim().ToUpper();

                // 3. Buscamos el registro en la base de datos que coincida
                var match = pendientes.FirstOrDefault(m =>
                {
                    decimal montoDB = Math.Abs(Math.Round(m.Monto, 2));
                    string descDB = (m.Descripcion ?? "").Trim().ToUpper();

                    // CRITERIO A: El monto debe ser igual (tolerancia de 0.01 por posibles redondeos en DB)
                    bool montoIgual = Math.Abs(montoDB - montoBanco) < 0.01m;

                    if (!montoIgual) return false;

                    // CRITERIO B: Coincidencia de texto flexible
                    // Como el controlador ahora envía el nombre real de la congregación, 
                    // esta comparación será exitosa.
                    bool textoCoincide = !string.IsNullOrEmpty(descDB) &&
                                        (descBanco.Contains(descDB) || descDB.Contains(descBanco));

                    return textoCoincide;
                });

                // 4. Si hay coincidencia, actualizamos el registro
                if (match != null)
                {
                    match.EstadoValidacion = "Validado";
                    match.EsEditable = false; // Bloqueamos el registro para evitar cambios posteriores
                    match.UsuarioID_Aprobador = autorizadorId;
                    match.FechaRegistro = DateTime.Now; // Fecha de la validación bancaria

                    validados++;

                    // Importante: Eliminar de la lista local para no validar dos veces el mismo 
                    // registro si existen montos duplicados en el extracto.
                    pendientes.Remove(match);
                }
            }

            // 5. Guardamos todos los cambios en una sola transacción a la BD
            await _context.SaveChangesAsync();

            return new ResultadoConciliacionDTO
            {
                TotalProcesados = transacciones.Count,
                TotalValidados = validados
            };
        }
    }

    public class ResultadoConciliacionDTO
    {
        public int TotalProcesados { get; set; }
        public int TotalValidados { get; set; }
    }
}