using IglesiaAPI.DTOs;
using IglesiaAPI.Infrastructure.Auth;
using IglesiaAPI.Models;
using IglesiaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace IglesiaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MiembrosController : ControllerBase
    {
        private readonly IMiembroService _miembroService;

        public MiembrosController(IMiembroService miembroService)
        {
            _miembroService = miembroService;
        }

        private UserContext CurrentUser => UserContextFactory.FromClaims(User);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MiembroDTO>>> GetMiembros()
        {
            var miembros = await _miembroService.GetAllAsync(CurrentUser);

            if (!CurrentUser.EsAdmin)
                miembros = miembros.Where(m => m.LocalidadID == CurrentUser.LocalidadID);

            var dtos = miembros.Select(m =>
            {
                var dto = MapToDto(m);
                dto.EsGlobal = CurrentUser.EsAdmin;
                return dto;
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MiembroDTO>> GetMiembro(int id)
        {
            var miembro = await _miembroService.GetByIdAsync(CurrentUser, id);
            if (miembro == null) return NotFound();

            if (!CurrentUser.EsAdmin && miembro.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            var dto = MapToDto(miembro);
            dto.EsGlobal = CurrentUser.EsAdmin;

            return Ok(dto);
        }

        [HttpGet("porLocalidad/{id:int}")]
        public async Task<ActionResult<IEnumerable<MiembroDTO>>> GetPorLocalidad(int id)
        {
            if (!CurrentUser.EsAdmin)
                return Forbid();

            var miembros = await _miembroService.GetAllAsync(CurrentUser);
            var filtrados = miembros.Where(m => m.LocalidadID == id);

            var dtos = filtrados.Select(m =>
            {
                var dto = MapToDto(m);
                dto.EsGlobal = true;
                return dto;
            }).ToList();

            return Ok(dtos);
        }

        [HttpPost]
        public async Task<ActionResult<MiembroDTO>> PostMiembro([FromBody] MiembroDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            dto.LocalidadID = CurrentUser.EsAdmin ? dto.LocalidadID : CurrentUser.LocalidadID;

            var miembro = MapFromDto(dto);

            // Asignación de auditoría basada en tus reglas de Seguridad
            miembro.CreadoPorID = CurrentUser.UsuarioID;

            if (string.IsNullOrEmpty(miembro.No_registro) || miembro.No_registro == "string")
            {
                var ultimo = await _miembroService.GetUltimoRegistroAsync();
                int siguiente = 1;
                if (!string.IsNullOrEmpty(ultimo) && ultimo != "0")
                {
                    var limpia = ultimo.Replace("REG-", "");
                    if (int.TryParse(limpia, out int ultimoNum))
                        siguiente = ultimoNum + 1;
                }
                miembro.No_registro = "REG-" + siguiente.ToString("D4");
            }

            var creado = await _miembroService.AddAsync(CurrentUser, miembro);
            var resultDto = MapToDto(creado);
            resultDto.EsGlobal = CurrentUser.EsAdmin;

            return CreatedAtAction(nameof(GetMiembro), new { id = creado.MiembroID }, resultDto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutMiembro(int id, [FromBody] MiembroDTO dto)
        {
            System.Diagnostics.Debug.WriteLine($"===> FOTO RECIBIDA DEL CLIENTE: '{dto.FotoPath}'");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.MiembroID)
                return BadRequest("ID del cuerpo no coincide con la ruta.");

            dto.LocalidadID = CurrentUser.EsAdmin ? dto.LocalidadID : CurrentUser.LocalidadID;

            var miembro = MapFromDto(dto);

            System.Diagnostics.Debug.WriteLine($"===> FOTO DESPUÉS DE MAPEO: '{dto.FotoPath}'");
            miembro.CreadoPorID = CurrentUser.UsuarioID;

            var actualizado = await _miembroService.UpdateAsync(CurrentUser, miembro);
            if (actualizado is null) return NotFound();

            var resultDto = MapToDto(actualizado);
            resultDto.EsGlobal = CurrentUser.EsAdmin;

            return Ok(resultDto);
        }

        [HttpGet("siguiente")]
        public async Task<ActionResult<string>> ObtenerSiguienteRegistro()
        {
            var ultimo = await _miembroService.GetUltimoRegistroAsync();
            int siguiente = 1;
            if (!string.IsNullOrEmpty(ultimo) && ultimo != "0")
            {
                var limpia = ultimo.Replace("REG-", "");
                if (int.TryParse(limpia, out int ultimoNum))
                    siguiente = ultimoNum + 1;
            }

            return Ok("REG-" + siguiente.ToString("D4"));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMiembro(int id)
        {
            var miembro = await _miembroService.GetByIdAsync(CurrentUser, id);
            if (miembro == null) return NotFound();

            if (!CurrentUser.EsAdmin && miembro.LocalidadID != CurrentUser.LocalidadID)
                return Forbid();

            var ok = await _miembroService.DeleteAsync(CurrentUser, id);
            return ok ? NoContent() : NotFound();
        }

        // --- MAPEOS ROBUSTOS ---

        private static MiembroDTO MapToDto(Miembro m)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            return new MiembroDTO
            {
                MiembroID = m.MiembroID,
                No_registro = m.No_registro,
                LocalidadID = m.LocalidadID,
                NombreLocalidad = m.Localidad?.NombreLocalidad ?? string.Empty,
                FechaBautizado = m.FechaBautizado,
                NombreCompleto = m.NombreCompleto,
                FechaNacimiento = m.FechaNacimiento,
                Lugar = m.Lugar ?? "",
                Direccion = m.Direccion ?? "",
                Telefono = m.Telefono ?? "",
                Provincia = m.Provincia ?? "",
                Ciudad = m.Ciudad ?? "",
                Nacionalidad = m.Nacionalidad ?? "",
                Sexo = m.Sexo ?? "",
                EstadoCivil = m.EstadoCivil ?? "",
                EsSellado = m.EsSellado,
                Categoria = m.Categoria ?? "Simpatizante",
                Estado = m.Estado ?? "Activo",
                FotoPath = m.FotoPath,
                Correo = m.Correo ?? "",
                TipoDocumento = m.TipoDocumento ?? "",
                NumeroDoc = m.NumeroDoc ?? "",
                NivelAcademico = m.NivelAcademico ?? "",
                Profesion = m.Profesion ?? "",
                Conyugue = m.Conyugue ?? "",
                Madre = m.Madre ?? "",
                Padre = m.Padre ?? "",
                Hijos = m.Hijos ?? ""
            };
        }

        private static Miembro MapFromDto(MiembroDTO d)
        {
            if (d == null) throw new ArgumentNullException(nameof(d));

            string? nombreFoto = d.FotoPath;

            if (!string.IsNullOrEmpty(nombreFoto))
            {
                // 1. Si es una URL completa de localhost, extraemos solo el final
                if (nombreFoto.Contains("://"))
                {
                    var uri = new Uri(nombreFoto);
                    nombreFoto = Path.GetFileName(uri.LocalPath);
                }
                // 2. Si es una ruta absoluta del sistema de archivos, extraemos solo el nombre
                else if (nombreFoto.Contains("/") || nombreFoto.Contains("\\"))
                {
                    nombreFoto = Path.GetFileName(nombreFoto);
                }

                // 3. Evitar guardar el avatar por defecto
                if (nombreFoto.ToLower() == "default-avatar.png")
                    nombreFoto = null;
            }


            return new Miembro
            {
                MiembroID = d.MiembroID,    
                No_registro = d.No_registro,
                LocalidadID = d.LocalidadID,
                FechaBautizado = d.FechaBautizado,
                NombreCompleto = d.NombreCompleto ?? "",
                FechaNacimiento = d.FechaNacimiento,
                Lugar = d.Lugar ?? "",
                Direccion = d.Direccion ?? "",
                Telefono = d.Telefono ?? "",
                Provincia = d.Provincia ?? "",
                Ciudad = d.Ciudad ?? "",
                Nacionalidad = d.Nacionalidad ?? "",
                Sexo = d.Sexo ?? "",
                EstadoCivil = d.EstadoCivil ?? "",
                EsSellado = d.EsSellado,
                Categoria = d.Categoria ?? "Simpatizante",
                Estado = d.Estado ?? "Activo",
                FotoPath = nombreFoto,
                Correo = d.Correo ?? "",
                TipoDocumento = d.TipoDocumento ?? "",
                NumeroDoc = d.NumeroDoc ?? "",
                NivelAcademico = d.NivelAcademico ?? "",
                Profesion = d.Profesion ?? "",
                Conyugue = d.Conyugue ?? "",
                Madre = d.Madre ?? "",
                Padre = d.Padre ?? "",
                Hijos = d.Hijos ?? ""
            };
        }
    }
}