using IglesiaAPI.Data;
using IglesiaAPI.DTOs;
using IglesiaAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IglesiaAPI.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IglesiaDBContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuarioDTO? UsuarioActual { get; set; }

        public UsuarioService(IglesiaDBContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Usuario?> LoginAsync(string email, string password)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Localidad)
                .FirstOrDefaultAsync(u => u.Email == email.Trim());

            if (usuario == null) return null;

            if (!string.Equals((usuario.Password ?? string.Empty).Trim(), password.Trim()))
                return null;

            return usuario;
        }

        public async Task<UsuarioDTO?> GetUsuarioActualAsync()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;

            if (!int.TryParse(userIdClaim, out var userId)) return null;

            var usuario = await _context.Usuarios
                .Include(u => u.Localidad)
                .FirstOrDefaultAsync(u => u.UsuarioID == userId);

            if (usuario == null) return null;

            var esAdmin = string.Equals(usuario.Rol, "Admin", StringComparison.OrdinalIgnoreCase);
            List<LocalidadResumenDTO>? localidades = null;

            if (esAdmin)
            {
                localidades = await _context.Localidades
                    .Select(l => new LocalidadResumenDTO
                    {
                        Id = l.LocalidadID,
                        NombreLocalidad = l.NombreLocalidad
                    })
                    .ToListAsync();
            }

            UsuarioActual = new UsuarioDTO
            {
                UsuarioID = usuario.UsuarioID,
                NombreUsuario = usuario.NombreUsuario ?? string.Empty,
                Email = usuario.Email ?? string.Empty,
                Rol = usuario.Rol ?? string.Empty,
                LocalidadID = usuario.LocalidadID,
                MiembroID = usuario.MiembroID,
                EsAdmin = esAdmin,
                LocalidadNombre = usuario.Localidad?.NombreLocalidad ?? "Sin Sede",
                LocalidadesDisponibles = localidades
            };

            return UsuarioActual;
        }

        public async Task<List<LocalidadResumenDTO>> GetLocalidadesResumenAsync()
        {
            return await _context.Localidades
                .Select(l => new LocalidadResumenDTO
                {
                    Id = l.LocalidadID,
                    NombreLocalidad = l.NombreLocalidad
                })
                .ToListAsync();
        }

        public async Task<Usuario?> GetUsuarioByIdAsync(int id) =>
            await _context.Usuarios.Include(u => u.Localidad).FirstOrDefaultAsync(u => u.UsuarioID == id);

        public async Task<IEnumerable<Usuario>> GetAllUsuariosAsync() =>
            await _context.Usuarios.Include(u => u.Localidad).ToListAsync();

        public async Task<Localidad?> GetLocalidadByIdAsync(int id) =>
            await _context.Localidades.FindAsync(id);

        public async Task<Usuario> AddUsuarioAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<bool> UpdateUsuarioAsync(Usuario usuario)
        {
            _context.Entry(usuario).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }



        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;
            _context.Usuarios.Remove(usuario);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ResetPasswordAsync(int id, string newPassword)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null) return false;
            user.Password = newPassword; // Aquí deberías aplicar Hash si usas Identity
            return await _context.SaveChangesAsync() > 0;
        }
    }
}