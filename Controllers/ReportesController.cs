using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IglesiaAPI.Services;
using IglesiaAPI.Infrastructure.Reports;

namespace IglesiaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reporteService;
        private readonly PdfGenerator _pdfGenerator;

        public ReportesController(IReporteService reporteService, PdfGenerator pdfGenerator)
        {
            _reporteService = reporteService;
            _pdfGenerator = pdfGenerator;
        }

        // =============================================
        // 1. REPORTE CONSOLIDADO
        // =============================================
        [HttpGet("consolidado")]
        public async Task<IActionResult> DescargarConsolidado(
            [FromQuery] DateTime inicio,
            [FromQuery] DateTime fin,
            [FromQuery] int? localidadId)
        {
            try
            {
                var data = await _reporteService.GenerarReporteAsync(inicio, fin, localidadId);
                var pdfBytes = _pdfGenerator.GenerarInforme(data);
                return File(pdfBytes, "application/pdf", $"Consolidado_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al generar el reporte", error = ex.Message });
            }
        }

        // =============================================
        // 2. HOJA DE VIDA
        // =============================================
        [HttpGet("hoja-vida/{miembroId:int}")]
        public async Task<IActionResult> DescargarHojaVida(int miembroId)
        {
            try
            {
                var data = await _reporteService.GenerarHojaVidaAsync(miembroId);
                var pdfBytes = _pdfGenerator.GenerarHojaVida(data);
                return File(pdfBytes, "application/pdf", $"HojaVida_{data.NombreCompleto.Replace(" ", "_")}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al generar la hoja de vida", error = ex.Message });
            }
        }

        // =============================================
        // 3. CUMPLEAÑEROS DEL MES
        // =============================================
        [HttpGet("cumpleaneros")]
        public async Task<IActionResult> DescargarCumpleaneros(
            [FromQuery] int mes,
            [FromQuery] int? localidadId)
        {
            try
            {
                var data = await _reporteService.GenerarCumpleanerosAsync(mes, localidadId);
                var pdfBytes = _pdfGenerator.GenerarCumpleaneros(data, mes);
                return File(pdfBytes, "application/pdf", $"Cumpleaneros_Mes{mes}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al generar el reporte", error = ex.Message });
            }
        }

        // =============================================
        // 4. LISTA DE BAUTIZADOS
        // =============================================
        [HttpGet("bautizados")]
        public async Task<IActionResult> DescargarBautizados(
            [FromQuery] DateTime inicio,
            [FromQuery] DateTime fin,
            [FromQuery] int? localidadId)
        {
            try
            {
                var data = await _reporteService.GenerarListaBautizadosAsync(inicio, fin, localidadId);
                var periodo = $"{inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}";
                var pdfBytes = _pdfGenerator.GenerarListaBautizados(data, periodo);
                return File(pdfBytes, "application/pdf", $"Bautizados_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al generar el reporte", error = ex.Message });
            }
        }

        // =============================================
        // 5. INVENTARIO DE ACTIVOS
        // =============================================
        [HttpGet("inventario")]
        public async Task<IActionResult> DescargarInventario([FromQuery] int? localidadId)
        {
            try
            {
                var data = await _reporteService.GenerarInventarioAsync(localidadId);
                var sede = localidadId.HasValue ? $"Localidad {localidadId}" : "Nacional";
                var pdfBytes = _pdfGenerator.GenerarInventario(data, sede);
                return File(pdfBytes, "application/pdf", $"Inventario_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al generar el reporte", error = ex.Message });
            }
        }

        // =============================================
        // 6. REPORTE DE CÉLULAS
        // =============================================
        [HttpGet("celulas")]
        public async Task<IActionResult> DescargarCelulas([FromQuery] int? localidadId)
        {
            try
            {
                var data = await _reporteService.GenerarReporteCelulasAsync(localidadId);
                var sede = localidadId.HasValue ? $"Localidad {localidadId}" : "Nacional";
                var pdfBytes = _pdfGenerator.GenerarReporteCelulas(data, sede);
                return File(pdfBytes, "application/pdf", $"Celulas_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al generar el reporte", error = ex.Message });
            }
        }

        // =============================================
        // 7. INFORME DE SECRETARÍA
        // =============================================
        [HttpGet("secretaria")]
        public async Task<IActionResult> DescargarSecretaria(
            [FromQuery] DateTime inicio,
            [FromQuery] DateTime fin,
            [FromQuery] int? localidadId)
        {
            try
            {
                var data = await _reporteService.GenerarInformeSecretariaAsync(inicio, fin, localidadId);
                var periodo = $"{inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}";
                var pdfBytes = _pdfGenerator.GenerarInformeSecretaria(data, periodo);
                return File(pdfBytes, "application/pdf", $"Secretaria_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al generar el reporte", error = ex.Message });
            }
        }
    }
}