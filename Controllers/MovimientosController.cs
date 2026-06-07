using ExcelDataReader;
using IglesiaAPI.Data;
using IglesiaAPI.DTOs;
using IglesiaAPI.Infrastructure.Auth;
using IglesiaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace IglesiaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MovimientosController : ControllerBase
    {
        private readonly IglesiaDBContext _context;

        public MovimientosController(IglesiaDBContext context)
        {
            _context = context;
        }

        private UserContext CurrentUser => UserContextFactory.FromClaims(User);

        // --- 🔹 MÉTODOS EXISTENTES (Mantenidos y actualizados con LoteReferencia) ---

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimientoDTO>>> GetMovimientos()
        {
            var query = _context.Movimientos
                                .Include(m => m.Localidad)
                                .Include(m => m.Cuenta)
                                .OrderByDescending(m => m.FechaMovimiento)
                                .AsQueryable();

            if (!CurrentUser.EsAdmin && CurrentUser.Rol != "TesoreroNacional")
            {
                query = query.Where(m => m.LocalidadID == CurrentUser.LocalidadID);
            }
            else if (CurrentUser.Rol == "TesoreroNacional")
            {
                query = query.Where(m => (m.Cuenta != null && m.Cuenta.EsNacional == true) || m.LocalidadID == CurrentUser.LocalidadID);
            }

            return await query.Select(m => new MovimientoDTO
            {
                MovimientoID = m.MovimientoID,
                FechaMovimiento = m.FechaMovimiento,
                Monto = m.Monto,
                TipoMovimiento = m.TipoMovimiento,
                Descripcion = m.Descripcion ?? "",
                LocalidadID = m.LocalidadID,
                CuentaID = m.CuentaID,
                NombreCuenta = m.Cuenta != null ? m.Cuenta.NombreCuenta : "Sin asignar",
                NoReferencia = m.NoReferencia,
                NoSerial = m.NoSerial,
                VoucherUrl = m.VoucherUrl,
                EstadoValidacion = m.EstadoValidacion,
                UsuarioID_Creador = m.UsuarioID_Creador,
                EsEditable = m.EsEditable,
                TieneSolicitudEdicion = m.TieneSolicitudEdicion,
                LoteReferencia = m.LoteReferencia // Campo añadido al mapeo
            }).ToListAsync();
        }

        // --- 🔹 NUEVOS MÉTODOS PARA CONCILIACIÓN POR LOTES ---

        /// <summary>
        /// Obtiene los movimientos de la localidad del usuario que aún no han sido agrupados en un lote.
        /// </summary>
        [HttpGet("pendientes-agrupar")]
        public async Task<ActionResult<IEnumerable<MovimientoDTO>>> GetMovimientosPendientesDeLote()
        {
            var query = await _context.Movimientos
                .Include(m => m.Cuenta)
                .Where(m => string.IsNullOrEmpty(m.LoteReferencia) && m.TipoMovimiento == "Ingreso")
                .OrderByDescending(m => m.FechaMovimiento)
                .Select(m => new MovimientoDTO
                {
                    MovimientoID = m.MovimientoID,
                    FechaMovimiento = m.FechaMovimiento,
                    Monto = m.Monto,
                    NombreCuenta = m.Cuenta != null ? m.Cuenta.NombreCuenta : "Sin asignar",
                    Descripcion = m.Descripcion ?? "",
                    TipoMovimiento = m.TipoMovimiento,
                    LoteReferencia = m.LoteReferencia
                })
                .ToListAsync();

            if (query == null) return NotFound();

            return Ok(query);
        }

        /// <summary>
        /// Genera el código de lote y bloquea los movimientos seleccionados.
        /// </summary>
        [HttpPost("generar-lote")]
        public async Task<IActionResult> GenerarLote([FromBody] LotePeticionDTO peticion)
        {
            if (peticion.MovimientoIds == null || !peticion.MovimientoIds.Any())
                return BadRequest("No se seleccionaron movimientos.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string codigoLote = $"L-{peticion.PrefijoLocalidad}-{DateTime.Now:ddMMyy}-{new Random().Next(10, 99)}";

                var movimientos = await _context.Movimientos
                    .Where(m => peticion.MovimientoIds.Contains(m.MovimientoID))
                    .ToListAsync();

                foreach (var m in movimientos)
                {
                    m.LoteReferencia = codigoLote;
                    m.EsEditable = false;
                    m.EstadoValidacion = "En Lote";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { codigoLote = codigoLote });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error al procesar el lote: {ex.Message}");
            }
        }

        [HttpGet("buscar-coincidencias-bancarias")]
        public async Task<ActionResult<IEnumerable<MovimientoDTO>>> BuscarCoincidenciasBancarias([FromQuery] string textoBanco, [FromQuery] decimal monto)
        {
            var query = _context.Movimientos
                .Include(m => m.Cuenta)
                .Where(m => m.Monto == monto && m.EstadoValidacion != "Validado");

            var resultados = await query.ToListAsync();

            var filtrados = resultados.Where(m =>
                (m.LoteReferencia != null && textoBanco.Contains(m.LoteReferencia)) ||
                (m.NoReferencia != null && textoBanco.Contains(m.NoReferencia))
            ).ToList();

            return Ok(filtrados.Select(m => new MovimientoDTO
            {
                MovimientoID = m.MovimientoID,
                LoteReferencia = m.LoteReferencia,
                Monto = m.Monto,
                NombreCuenta = m.Cuenta?.NombreCuenta
            }));
        }

        [HttpPost]
        public async Task<ActionResult<Movimiento>> PostMovimiento([FromBody] MovimientoDTO dto)
        {
            // Validación de seguridad para los IDs recibidos
            if (!dto.CuentaID.HasValue || dto.CuentaID <= 0)
                return BadRequest("ID de cuenta no válido.");

            var cuenta = await _context.Cuentas.FindAsync(dto.CuentaID.Value);
            if (cuenta == null) return BadRequest("La cuenta seleccionada no existe en la base de datos.");

            var movimiento = new Movimiento
            {
                // Usamos .Value porque ahora son nulables en el DTO
                FechaMovimiento = dto.FechaMovimiento,
                Monto = dto.Monto,
                TipoMovimiento = dto.TipoMovimiento,
                Descripcion = dto.Descripcion,
                LocalidadID = CurrentUser.EsAdmin ? (dto.LocalidadID ?? CurrentUser.LocalidadID) : CurrentUser.LocalidadID,
                CuentaID = dto.CuentaID.Value,
                NoReferencia = dto.NoReferencia,
                NoSerial = dto.NoSerial,
                VoucherUrl = dto.VoucherUrl,
                EstadoValidacion = "Pendiente",
                UsuarioID_Creador = CurrentUser.UsuarioID,
                EsEditable = true,
                FechaRegistro = DateTime.Now
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMovimiento), new { id = movimiento.MovimientoID }, movimiento);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Movimiento>> GetMovimiento(int id)
        {
            var movimiento = await _context.Movimientos
                                           .Include(m => m.Localidad)
                                           .Include(m => m.Cuenta)
                                           .FirstOrDefaultAsync(m => m.MovimientoID == id);

            if (movimiento == null) return NotFound();

            if (!CurrentUser.EsAdmin && CurrentUser.Rol != "TesoreroNacional" && movimiento.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            return Ok(movimiento);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutMovimiento(int id, [FromBody] MovimientoDTO dto)
        {
            var existente = await _context.Movimientos.FindAsync(id);
            if (existente == null) return NotFound();

            // --- 🔹 AJUSTE PARA LA VITÁCORA (Evita el Error 400 y el Null Warning) ---
            if (!dto.CuentaID.HasValue)
            {
                return BadRequest("El ID de la cuenta es obligatorio.");
            }
            // -----------------------------------------------------------------------

            // --- Mantenemos tus validaciones originales ---
            if (!string.IsNullOrEmpty(existente.LoteReferencia) && !CurrentUser.EsAdmin)
                return BadRequest("Este registro ya está agrupado en un lote para depósito y no puede editarse.");

            if (existente.EstadoValidacion == "Validado" && !CurrentUser.EsAdmin)
                return BadRequest("No se puede editar un registro validado por el banco.");

            // --- Actualización de campos ---
            existente.Monto = dto.Monto;
            existente.FechaMovimiento = dto.FechaMovimiento;
            existente.Descripcion = dto.Descripcion;

            // Ahora el compilador aceptará el .Value sin errores porque ya validamos arriba
            existente.CuentaID = dto.CuentaID.Value;

            existente.NoReferencia = dto.NoReferencia;
            existente.NoSerial = dto.NoSerial;

            // --- 🔹 LO QUE HACE FALTA (Agregado) 🔹 ---
            existente.EsEditable = false;
            existente.TieneSolicitudEdicion = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("autorizados-para-editar")]
        public async Task<ActionResult<IEnumerable<MovimientoDTO>>> GetMovimientosAutorizados()
        {
            try
            {
                var claim = User.FindFirst("LocalidadID");
                string localidadClaim = claim?.Value ?? string.Empty;

                if (string.IsNullOrEmpty(localidadClaim))
                    return Unauthorized("No se pudo determinar la localidad del usuario.");

                if (!int.TryParse(localidadClaim, out int localidadId))
                    return BadRequest("El ID de localidad no es válido.");

                var movimientos = await _context.Movimientos
                    .Where(m => m.LocalidadID == localidadId && m.EsEditable == true)
                    .Select(m => new MovimientoDTO
                    {
                        MovimientoID = m.MovimientoID,
                        FechaMovimiento = m.FechaMovimiento,
                        TipoMovimiento = m.TipoMovimiento ?? "Ingreso",
                        Monto = m.Monto,
                        Descripcion = m.Descripcion ?? string.Empty,
                        NoReferencia = m.NoReferencia ?? string.Empty,
                        CuentaID = m.CuentaID,
                        LocalidadID = m.LocalidadID,
                        UsuarioID_Creador = m.UsuarioID_Creador,
                        // --- CAMPOS FALTANTES QUE CAUSABAN EL ERROR ---
                        EsEditable = m.EsEditable,
                        TieneSolicitudEdicion = m.TieneSolicitudEdicion,
                        LoteReferencia = m.LoteReferencia ?? string.Empty
                    })
                    .ToListAsync();

                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("pendientes")]
        public async Task<ActionResult<IEnumerable<MovimientoDTO>>> GetMovimientosPendientes()
        {
            var localidadId = int.Parse(User.FindFirst("LocalidadID")?.Value ?? "0");

            var movimientos = await _context.Movimientos
                .Where(m => m.LocalidadID == localidadId
                         && m.EsEditable == false
                         && m.TieneSolicitudEdicion == false)
                .Select(m => new MovimientoDTO
                {
                    MovimientoID = m.MovimientoID,
                    FechaMovimiento = m.FechaMovimiento,
                    Monto = m.Monto,
                    Descripcion = m.Descripcion ?? string.Empty,
                    TipoMovimiento = m.TipoMovimiento
                })
                .ToListAsync();

            return Ok(movimientos);
        }

        [HttpPost("solicitar-correccion/{id}")]
        public async Task<IActionResult> SolicitarCorreccion(int id)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento == null) return NotFound();

            movimiento.TieneSolicitudEdicion = true;
            movimiento.EsEditable = false;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("solicitudes-pendientes")]
        public async Task<ActionResult<IEnumerable<MovimientoDTO>>> GetSolicitudesPendientes()
        {
            // 1. Filtro Base: Solo solicitudes activas y NUNCA las del propio usuario (Gobernanza de 4 ojos)
            var query = _context.Movimientos
                .Include(m => m.Cuenta)
                .Include(m => m.Localidad)
                .Where(m => m.TieneSolicitudEdicion == true && m.UsuarioID_Creador != CurrentUser.UsuarioID);

            // 2. Filtro de Jurisdicción (Seguridad por Localidad)
            if (CurrentUser.Rol == "Pastor")
            {
                // EL PASTOR SOLO VE SU PROPIA IGLESIA/LOCALIDAD
                // Esto evita que el Pastor XXX vea lo del Pastor ZZZ
                query = query.Where(m => m.LocalidadID == CurrentUser.LocalidadID);
            }
            else if (CurrentUser.Rol == "TesoreroNacional" || CurrentUser.EsAdmin)
            {
                // El Tesorero Nacional ve todo el país (excepto lo que él mismo creó)
            }
            else
            {
                // Cualquier otro rol (como Tesorero Local) no tiene permiso de ver esta lista
                return Ok(new List<MovimientoDTO>());
            }

            var solicitudes = await query.Select(m => new MovimientoDTO
            {
                MovimientoID = m.MovimientoID,
                FechaMovimiento = m.FechaMovimiento,
                Monto = m.Monto,
                TipoMovimiento = m.TipoMovimiento ?? "Ingreso",
                Descripcion = m.Descripcion ?? string.Empty,
                LocalidadID = m.LocalidadID,
                NombreLocalidad = m.Localidad != null ? m.Localidad.NombreLocalidad : "Sin nombre",
                CuentaID = m.CuentaID,
                NombreCuenta = m.Cuenta != null ? m.Cuenta.NombreCuenta : "Sin asignar",
                UsuarioID_Creador = m.UsuarioID_Creador,
                TieneSolicitudEdicion = m.TieneSolicitudEdicion
            })
            .ToListAsync();

            return Ok(solicitudes);
        }


        [HttpPost("procesar-extracto")]
        public async Task<IActionResult> ProcesarExtracto(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Archivo no recibido.");

            try
            {
                var movimientosBD = await _context.Movimientos
                    .Where(m => m.EstadoValidacion != "Validado" || m.EstadoValidacion == null)
                    .ToListAsync();

                using var stream = file.OpenReadStream();
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using var reader = ExcelReaderFactory.CreateReader(stream);

                int colFecha = -1, colMonto = -1, colDesc = -1, colRef = -1;
                int validados = 0;
                int totalLeidos = 0;

                while (reader.Read())
                {
                    if (colFecha == -1)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var h = reader.GetValue(i)?.ToString()?.ToLower().Trim() ?? "";
                            if (h.Contains("fecha") && h.Contains("post")) colFecha = i;
                            if (h.Contains("monto") && h.Contains("tran")) colMonto = i;
                            if (h.Contains("descrip")) colDesc = i;
                            if (h.Contains("refere")) colRef = i;
                        }
                        continue;
                    }

                    decimal montoEx = ParsearMonto(reader.GetValue(colMonto)?.ToString());
                    if (montoEx == 0) continue;

                    totalLeidos++; // Contador de filas reales en el banco

                    DateTime fechaEx = ParsearFecha(reader.GetValue(colFecha)?.ToString());
                    string descEx = reader.GetValue(colDesc)?.ToString()?.ToLower().Trim() ?? "";
                    string refEx = colRef != -1 ? (reader.GetValue(colRef)?.ToString()?.ToLower().Trim() ?? "") : "";

                    var ganador = movimientosBD
                        .Select(m => new { Mov = m, Pts = CalcularPuntos(m, montoEx, fechaEx, descEx, refEx) })
                        .Where(x => x.Pts >= 70)
                        .OrderByDescending(x => x.Pts)
                        .FirstOrDefault();

                    if (ganador != null)
                    {
                        ganador.Mov.EstadoValidacion = "Validado";
                        ganador.Mov.EsEditable = false;
                        _context.Entry(ganador.Mov).State = EntityState.Modified;
                        validados++;
                        movimientosBD.Remove(ganador.Mov);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { totalLeidos = totalLeidos, totalValidados = validados });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private int CalcularPuntos(Movimiento m, decimal montoEx, DateTime fechaEx, string descEx, string refEx)
        {
            int score = 0;
            bool montoMatch = (Math.Abs(m.Monto - montoEx) <= 0.01m);
            bool loteMatch = false;

            if (!string.IsNullOrEmpty(m.LoteReferencia))
            {
                string loteBD = m.LoteReferencia.ToLower().Trim();
                if (refEx.Contains(loteBD) || descEx.Contains(loteBD)) loteMatch = true;
            }

            if (loteMatch) score += 50;
            if (montoMatch) score += 30;

            double dias = Math.Abs((m.FechaMovimiento.Date - fechaEx.Date).TotalDays);
            if (dias <= 1) score += 15;
            else if (dias <= 7) score += 10;
            else if (dias <= 15) score += 5;

            return score;
        }

        private decimal ParsearMonto(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            string limpio = Regex.Replace(s, @"[^0-9.,-]", "");
            if (limpio.Contains(",") && limpio.Contains(".")) limpio = limpio.Replace(",", "");
            else if (limpio.Contains(",")) limpio = limpio.Replace(",", ".");
            return decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal r) ? r : 0;
        }

        private DateTime ParsearFecha(string? s) => DateTime.TryParse(s, out DateTime d) ? d : DateTime.MinValue;


        [HttpPost("procesar-solicitud/{id}")]
        public async Task<IActionResult> ProcesarSolicitud(int id, [FromBody] DecisionDTO decision)
        {
            // 1. Buscamos el movimiento incluyendo al creador
            var movimiento = await _context.Movimientos.FindAsync(id);

            if (movimiento == null)
            {
                return NotFound("El movimiento no existe.");
            }

            // 2. VALIDACIÓN DE GOBERNANZA: Segregación de funciones
            // Obtenemos el ID del usuario que intenta aprobar desde el DTO o los Claims
            int idUsuarioAprobador = decision.AutorizadorID;

            if (movimiento.UsuarioID_Creador == idUsuarioAprobador)
            {
                // El sistema detecta que es la misma persona. Bloqueo automático.
                return BadRequest("Conflicto de intereses detectado: Un usuario no puede aprobar su propia solicitud de corrección. Esta acción ha sido escalada al Tesorero Nacional por auditoría.");
            }

            // 3. Procesar la decisión si pasó la validación
            if (decision.Aprobado)
            {
                // APROBADO: El Tesorero Nacional (o alguien distinto al creador) dio el permiso
                movimiento.EsEditable = true;
                // Mantenemos TieneSolicitudEdicion en true para que aparezca en el selector del Local
            }
            else
            {
                // RECHAZADO: Se cierran las banderas
                movimiento.EsEditable = false;
                movimiento.TieneSolicitudEdicion = false;
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = decision.Aprobado ? "Registro desbloqueado con éxito." : "Solicitud rechazada." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al procesar: {ex.Message}");
            }
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardNacionalDTO>> GetDashboardNacional(
    [FromQuery] int mes,
    [FromQuery] int localidadId)
        {
            var query = _context.Movimientos
                .Include(m => m.Cuenta)
                .Where(m => m.FechaMovimiento.Month == mes);

            // Si localidadId es 0, muestra todas las localidades
            if (localidadId > 0)
                query = query.Where(m => m.LocalidadID == localidadId);

            // Solo admins y tesoreros nacionales pueden ver todo
            if (!CurrentUser.EsAdmin && CurrentUser.Rol != "TesoreroNacional")
                query = query.Where(m => m.LocalidadID == CurrentUser.LocalidadID);

            var movimientos = await query.ToListAsync();

            var dto = new DashboardNacionalDTO
            {
                TotalEntradas = movimientos
                    .Where(m => m.TipoMovimiento == "Ingreso")
                    .Sum(m => m.Monto),

                TotalSalidas = movimientos
                    .Where(m => m.TipoMovimiento == "Egreso")
                    .Sum(m => m.Monto),

                ResumenCuentas = movimientos
                    .GroupBy(m => new { m.Cuenta!.NombreCuenta, m.TipoMovimiento })
                    .Select(g => new ResumenCuentaDTO
                    {
                        Cuenta = g.Key.NombreCuenta ?? "Sin cuenta",
                        Tipo = g.Key.TipoMovimiento == "Ingreso" ? "Ingreso" : "Egreso",
                        Monto = g.Sum(m => m.Monto)
                    })
                    .ToList()
            };

            return Ok(dto);
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMovimiento(int id)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento == null) return NotFound();

            if (!string.IsNullOrEmpty(movimiento.LoteReferencia) && !CurrentUser.EsAdmin)
                return BadRequest("No se puede eliminar un registro que ya pertenece a un lote.");

            if (movimiento.EstadoValidacion == "Validado" && !CurrentUser.EsAdmin)
                return BadRequest("No se puede eliminar un registro validado.");

            _context.Movimientos.Remove(movimiento);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}