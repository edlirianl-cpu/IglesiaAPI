using IglesiaAPI.Data;
using IglesiaAPI.DTOs;
using IglesiaAPI.Models;
using IglesiaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

namespace IglesiaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IAuthService _authService;
        private readonly IglesiaDBContext _context;

        public UsuariosController(IUsuarioService usuarioService, IAuthService authService, IglesiaDBContext context)
        {
            _usuarioService = usuarioService;
            _authService = authService;
            _context = context;
        }

        // 🔹 LOGIN (Corregido sin afectar la estructura)
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginRequestDTO? request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email y contraseña son requeridos.");

            // 1. Buscamos al usuario por Email
            var usuario = await _context.Usuarios
                .Include(u => u.Localidad)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (usuario == null) return Unauthorized("Usuario no encontrado.");

            // 2. Validación de Password (ASEGÚRATE DE TENER EL HASH EN LA BD)
            bool passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.Password);

            if (!passwordValida) return Unauthorized("Contraseña incorrecta.");

            // 3. GENERACIÓN DEL TOKEN 
            // Cambiamos esto: En lugar de pasarle la clave otra vez al AuthService, 
            // deberías tener un método que SOLO genere el token para ese usuario.
            // Si tu AuthService no tiene ese método, puedes usar el que tienes 
            // pero asegúrate de que el AuthService use el mismo Hash.
            var jwt = await _authService.LoginAsync(usuario.Email!, request.Password);

            if (string.IsNullOrEmpty(jwt))
                return Unauthorized("Error al generar el Token. Revisa el AuthService.");

            // 4. Mapeo del DTO (lo que ya tenías)
            var usuarioDto = new UsuarioDTO
            {
                UsuarioID = usuario.UsuarioID,
                NombreUsuario = usuario.NombreUsuario ?? string.Empty,
                Email = usuario.Email ?? string.Empty,
                Rol = usuario.Rol ?? string.Empty,
                LocalidadID = usuario.LocalidadID,
                EsAdmin = usuario.Rol == "Admin" || usuario.Rol == "SuperUsuario",
                LocalidadNombre = usuario.Localidad?.NombreLocalidad ?? "Sin Sede"
            };

            return Ok(new LoginResponseDTO { Token = jwt, Usuario = usuarioDto });
        }

        [HttpPost("crear")]
        [Authorize]
        public async Task<IActionResult> CrearUsuario([FromBody] UsuarioDTO dto)
        {
            // Tomamos el miembro y su localidad directamente de la DB
            var miembro = await _context.Miembros
                .FirstOrDefaultAsync(m => m.MiembroID == dto.MiembroID);

            if (miembro == null)
                return BadRequest("El miembro seleccionado no existe en el sistema.");

            var usuarioDuplicado = await _context.Usuarios
                .AnyAsync(u => u.MiembroID == dto.MiembroID);

            if (usuarioDuplicado)
                return BadRequest("Este miembro ya tiene un usuario asignado en el sistema.");

            var nuevoUsuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                Email = dto.Email,
                Rol = dto.Rol,
                LocalidadID = miembro.LocalidadID, // ← De la DB directamente
                MiembroID = dto.MiembroID,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("actualizar/{id}")]
        [Authorize]
        public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] UsuarioDTO dto)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null) return NotFound();

            user.NombreUsuario = dto.NombreUsuario;
            user.Email = dto.Email;
            user.Rol = dto.Rol;
            user.LocalidadID = dto.LocalidadID;

            if (!string.IsNullOrWhiteSpace(dto.Password) && dto.Password != "********")
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            _context.Usuarios.Update(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("reset-password/{id}")]
        [Authorize]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null) return NotFound();

            user.Password = BCrypt.Net.BCrypt.HashPassword("Iglesia2026!");
            _context.Usuarios.Update(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("localidades-resumen")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<LocalidadResumenDTO>>> GetLocalidades()
        {
            return Ok(await _usuarioService.GetLocalidadesResumenAsync());
        }

        [HttpGet("todos")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetTodosUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Include(u => u.Localidad)
                    .ToListAsync();

                var usuariosDto = usuarios.Select(u => new UsuarioDTO
                {
                    UsuarioID = u.UsuarioID,
                    NombreUsuario = u.NombreUsuario ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Rol = u.Rol ?? string.Empty,
                    LocalidadID = u.LocalidadID,
                    LocalidadNombre = u.Localidad?.NombreLocalidad ?? "Sin Sede",
                    EsAdmin = u.Rol == "Admin" || u.Rol == "TesoreroNacional" || u.Rol == "SuperUsuario"
                });

                return Ok(usuariosDto);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("actual")]
        // Quitamos el [Authorize] para que el F5 no dispare el 401 automático del middleware
        public async Task<ActionResult<UsuarioDTO>> GetUsuarioActual()
        {
            // El servicio busca al usuario por los claims de la cookie/token actual
            var usuarioDto = await _usuarioService.GetUsuarioActualAsync();

            if (usuarioDto == null)
            {
                // Devolvemos 200 OK con null en lugar de 401. 
                // Esto le dice a Blazor: "No hay nadie, pero no es un error de seguridad".
                return Ok(null);
            }

            return Ok(usuarioDto);
        }
        [HttpDelete("eliminar/{id}")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            try
            {
                // 1. Extraemos el rol del token de forma segura
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                               ?? User.FindFirst("rol")?.Value; // Intento secundario por si el claim se llama literal

                // 2. Validación: Comparamos ignorando mayúsculas/minúsculas
                if (string.IsNullOrEmpty(userRole) || !userRole.Equals("SuperUsuario", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(403, "Acceso Denegado: Solo el SuperUsuario puede realizar esta acción.");
                }

                var usuarioParaEliminar = await _context.Usuarios.FindAsync(id);
                if (usuarioParaEliminar == null) return NotFound();

                // 3. No permitir que se elimine a sí mismo
                // Forma correcta de extraer el ID del usuario actual
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(currentUserIdClaim, out int currentId) && id == currentId)
                {
                    return BadRequest("No puedes eliminar tu propia cuenta.");
                }

                _context.Usuarios.Remove(usuarioParaEliminar);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}