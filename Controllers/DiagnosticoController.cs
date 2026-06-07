using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcelDataReader;
using System.Text.RegularExpressions;
using System.Globalization;
using IglesiaAPI.Data; // Asegúrate de que este sea el namespace de tu ApplicationDbContext

namespace IglesiaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticoController : ControllerBase
    {
        private readonly IglesiaDBContext _context;

        public DiagnosticoController(IglesiaDBContext context)
        {
            _context = context;
        }

        [HttpPost("inspeccionar")]
        public async Task<IActionResult> InspeccionarExcel(IFormFile file)
        {
            if (file == null) return BadRequest("Archivo no recibido.");

            var reporteTecnico = new List<object>();

            // Cargamos todos los movimientos que el sistema "debería" ver
            var movimientosBD = await _context.Movimientos
                .Where(m => m.EstadoValidacion != "Validado")
                .ToListAsync();

            using (var stream = file.OpenReadStream())
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    int filaActual = 0;
                    while (reader.Read())
                    {
                        filaActual++;
                        if (filaActual <= 10) continue; // Encabezado bancario

                        // Captura de datos del Excel (Basado en tu estructura de extracto)
                        var valorFecha = reader.GetValue(0)?.ToString()??"";
                        var valorMonto = reader.GetValue(2)?.ToString()??"";
                        var valorLote = reader.GetValue(4)?.ToString() ?? "";

                        decimal montoEx = ParsearMontoTecnico(valorMonto);
                        DateTime fechaEx = ParsearFechaTecnico(valorFecha);

                        // BUSQUEDA DE PROXIMIDAD: Buscamos el registro que más se parezca por monto en la BD
                        var sugerencia = movimientosBD
                            .OrderBy(m => Math.Abs(m.Monto - montoEx))
                            .FirstOrDefault();

                        reporteTecnico.Add(new
                        {
                            Fila = filaActual,
                            Excel = new { Monto = montoEx, Fecha = fechaEx.ToShortDateString(), Lote = valorLote },
                            BaseDatos = sugerencia == null ? null : new
                            {
                                Monto = sugerencia.Monto,
                                Fecha = sugerencia.FechaMovimiento.ToShortDateString(),
                                Lote = sugerencia.LoteReferencia
                            },
                            Analisis = sugerencia == null ? "BD sin pendientes" :
                                       (montoEx == sugerencia.Monto && fechaEx.Date == sugerencia.FechaMovimiento.Date ? "MATCH OK" : "DIFERENCIA DETECTADA")
                        });

                        if (filaActual > 25) break; // Límite de 15 registros de datos para el diagnóstico
                    }
                }
            }
            return Ok(reporteTecnico);
        }

        private decimal ParsearMontoTecnico(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            string limpio = Regex.Replace(input, @"[^0-9.,-]", "");
            if (limpio.Contains(",") && limpio.Contains(".")) limpio = limpio.Replace(",", "");
            else if (limpio.Contains(",")) limpio = limpio.Replace(",", ".");
            decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal res);
            return res;
        }

        private DateTime ParsearFechaTecnico(string input) =>
            DateTime.TryParse(input, out DateTime dt) ? dt : DateTime.MinValue;
    }
}