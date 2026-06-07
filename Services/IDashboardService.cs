using System.Collections.Generic;
using System.Threading.Tasks;
using IglesiaAPI.DTOs;

namespace IglesiaAPI.Services
{
    public interface IDashboardService
    {
        Task<DashboardDTO> ObtenerDatosAsync(int localidadId);
        Task<TendenciaDTO> ObtenerMiembrosPorMesAsync(int localidadId);
        Task<IEnumerable<ActividadDTO>> ObtenerActividadAsync(int localidadId);
    }
}