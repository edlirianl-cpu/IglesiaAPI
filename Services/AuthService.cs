using IglesiaAPI.Data;
using IglesiaAPI.Models;
using IglesiaAPI.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IglesiaAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IglesiaDBContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IglesiaDBContext context, JwtSettings jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings;
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            // 1. Buscamos al usuario por Email
            // No validamos el password en el 'Where' porque en la BD hay un HASH
            var usuario = await _context.Usuarios
                .Include(u => u.Localidad)
                .FirstOrDefaultAsync(u => u.Email == email);

            // Si el usuario no existe, el controlador recibirá null y dará 401
            if (usuario == null) return null;

            // 2. Definimos los Claims (La identidad que usará el Frontend)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypesConst.UsuarioId, usuario.UsuarioID.ToString()),
                new Claim(ClaimTypesConst.NombreUsuario, usuario.NombreUsuario ?? ""),
                new Claim(ClaimTypesConst.Email, usuario.Email ?? ""),
                
                // ROL: Para definir accesos (Admin, Tesorero, etc.)
                new Claim(ClaimTypesConst.Rol, usuario.Rol ?? "Usuario"),
                
                // LOCALIDAD: Para filtrar datos por sede (Si es null, enviamos "0")
                new Claim(ClaimTypesConst.LocalidadId, usuario.LocalidadID.ToString())
            };

            // 3. Generación técnica del Token JWT
            // Convertimos la clave secreta de string a bytes
            var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpireHours),
                signingCredentials: creds
            );

            // Devolvemos el Token como un string largo
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}