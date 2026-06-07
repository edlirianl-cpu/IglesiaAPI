using IglesiaAPI.DTOs;

namespace IglesiaAPI.Services
{
    public interface IReporteService
    {
        Task<ReporteConsolidadoDTO> GenerarReporteAsync(DateTime inicio, DateTime fin, int? localidadId);
        Task<HojaVidaDTO> GenerarHojaVidaAsync(int miembroId);
        Task<List<CumpleaneroDTO>> GenerarCumpleanerosAsync(int mes, int? localidadId);
        Task<List<BautizadoDTO>> GenerarListaBautizadosAsync(DateTime inicio, DateTime fin, int? localidadId);
        Task<List<InventarioDetalleDTO>> GenerarInventarioAsync(int? localidadId);
        Task<List<CelulaDetalleDTO>> GenerarReporteCelulasAsync(int? localidadId);
        Task<List<RegistroSecretariaDetalleDTO>> GenerarInformeSecretariaAsync(DateTime inicio, DateTime fin, int? localidadId);
    }
}