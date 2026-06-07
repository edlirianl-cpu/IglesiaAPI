using Microsoft.AspNetCore.Mvc;
using IglesiaAPI.DTOs;
using IglesiaAPI.Services;
using IglesiaAPI.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using ExcelDataReader;
using System.Data;

namespace IglesiaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Paso 1: Nadie entra sin un Token válido
    public class ConciliacionController : ControllerBase
    {
        private readonly IConciliacionService _conciliacionService;

        public ConciliacionController(IConciliacionService conciliacionService)
        {
            _conciliacionService = conciliacionService;
        }

        private UserContext CurrentUser => UserContextFactory.FromClaims(User);

        [HttpPost("subir-extracto")]
        public async Task<IActionResult> SubirExtracto(IFormFile archivo)
        {
            // Paso 2: BLOQUEO RADICAL. 
            // Si el rol en el Token no es exactamente "TesoreroNacional", se expulsa al usuario.
            if (CurrentUser.Rol != "TesoreroNacional")
            {
                Console.WriteLine($"⚠️ INTENTO DE ACCESO NO AUTORIZADO: Usuario {CurrentUser.UsuarioID} con rol {CurrentUser.Rol} intentó conciliar.");
                return StatusCode(StatusCodes.Status403Forbidden, "ACCESO DENEGADO: Solo el Tesorero Nacional puede realizar conciliaciones bancarias.");
            }

            if (archivo == null || archivo.Length == 0)
                return BadRequest("Archivo no proporcionado.");

            // Si llegamos aquí, es 100% seguro que es el Tesorero Nacional
            var listaTransacciones = await ParsearArchivoBanco(archivo);

            if (listaTransacciones == null || !listaTransacciones.Any())
                return BadRequest("El archivo no contiene datos válidos.");

            var resultado = await _conciliacionService.ProcesarMatchingAutomaticoAsync(listaTransacciones, CurrentUser.UsuarioID);

            return Ok(resultado);
        }

        private async Task<List<TransaccionBancoDTO>> ParsearArchivoBanco(IFormFile file)
        {
            var lista = new List<TransaccionBancoDTO>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = file.OpenReadStream())
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    bool isHeaderSkipped = false;
                    while (reader.Read())
                    {
                        if (!isHeaderSkipped) { isHeaderSkipped = true; continue; }
                        try
                        {
                            // EXPLICACIÓN DEL MAPEO SEGÚN TU EXCEL:
                            // GetValue(0) -> Columna A: Fecha
                            // GetValue(2) -> Columna C: Monto
                            // GetValue(5) -> Columna F: DESCRIPCIÓN REAL (Donde dice IPUL AZUA, etc.)

                            lista.Add(new TransaccionBancoDTO
                            {
                                FechaPost = Convert.ToDateTime(reader.GetValue(0)),
                                Monto = Convert.ToDecimal(reader.GetValue(2)),

                                // Mantenemos Referencia y Serial por si el banco los provee en esas columnas
                                NoReferencia = reader.GetValue(3)?.ToString()?.Trim(),
                                NoSerial = reader.GetValue(4)?.ToString()?.Trim(),

                                // CAMBIO CLAVE: Antes era GetValue(1) que traía "DEPOSITO". 
                                // Ahora GetValue(5) trae el nombre de la congregación.
                                DescripcionCorta = reader.GetValue(5)?.ToString()?.Trim() ?? ""
                            });
                        }
                        catch
                        {
                            // Si hay una fila con formato de fecha inválido o vacía, saltamos a la siguiente
                            continue;
                        }
                    }
                }
            }
            return await Task.FromResult(lista);
        }
    }
}