using IglesiaAPI.Data;
using IglesiaAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IglesiaAPI.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IglesiaDBContext _context;

        public DashboardService(IglesiaDBContext context)
        {
            _context = context;
        }

        public async Task<DashboardDTO> ObtenerDatosAsync(int localidadId)
        {
            var dto = new DashboardDTO();

            // Filtro de seguridad por LocalidadID
            var miembrosQuery = _context.Miembros.Where(m => m.LocalidadID == localidadId);

            dto.MiembrosActivos = await miembrosQuery.CountAsync();
            dto.Celulas = await _context.Celulas.CountAsync(c => c.LocalidadID == localidadId);
            dto.InventarioTotal = await _context.Inventarios.CountAsync(i => i.LocalidadID == localidadId);

            // Propiedades de DashboardDTO (según tu archivo)
            dto.OfrendasMensuales = 0;
            dto.BalanceFinanzas = 0;
            dto.NuevosMes = 0;
            dto.Alertas = 0;

            return dto;
        }

        public async Task<IEnumerable<ActividadDTO>> ObtenerActividadAsync(int localidadId)
        {
            return await _context.Movimientos
                .Where(m => m.LocalidadID == localidadId)
                .OrderByDescending(m => m.MovimientoID)
                .Take(10)
                .Select(m => new ActividadDTO
                {
                    Fecha = DateTime.Now,
                    Tipo = "Movimiento",
                    Descripcion = m.Descripcion ?? "Sin descripción"
                })
                .ToListAsync();
        }

        public async Task<TendenciaDTO> ObtenerMiembrosPorMesAsync(int localidadId)
        {
            var tendencia = new TendenciaDTO
            {
                Meses = new List<string> { "Ene", "Feb", "Mar" },
                NuevosMiembros = new List<int> { 0, 0, 0 },
                Bautismos = new List<int> { 0, 0, 0 },
                Ingresos = new List<decimal> { 0, 0, 0 },
                Egresos = new List<decimal> { 0, 0, 0 }
            };

            return await Task.FromResult(tendencia);
        }
    }
}