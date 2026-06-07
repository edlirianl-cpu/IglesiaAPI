using System.Security.Claims;

namespace IglesiaAPI.Infrastructure.Auth
{
    public static class UserContextFactory
    {
        public static UserContext FromClaims(ClaimsPrincipal user)
        {
            // 🔹 Usamos tus constantes para asegurar que coincidan con el Token generado
            var usuarioIdClaim = user.FindFirst(ClaimTypesConst.UsuarioId)?.Value;
            var rolClaim = user.FindFirst(ClaimTypesConst.Rol)?.Value;
            var localidadClaim = user.FindFirst(ClaimTypesConst.LocalidadId)?.Value;
            var nombreClaim = user.FindFirst(ClaimTypesConst.NombreUsuario)?.Value;
            var emailClaim = user.FindFirst(ClaimTypesConst.Email)?.Value;

            int usuarioId = 0;
            int localidadId = 0;

            if (!string.IsNullOrEmpty(usuarioIdClaim))
                int.TryParse(usuarioIdClaim, out usuarioId);

            if (!string.IsNullOrEmpty(localidadClaim))
                int.TryParse(localidadClaim, out localidadId);

            return new UserContext
            {
                UsuarioID = usuarioId,
                LocalidadID = localidadId,
                Rol = rolClaim ?? string.Empty,
                NombreUsuario = nombreClaim ?? string.Empty,
                Email = emailClaim ?? string.Empty
            };
        }
    }
}