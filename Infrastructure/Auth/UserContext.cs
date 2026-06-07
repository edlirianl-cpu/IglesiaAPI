namespace IglesiaAPI.Infrastructure.Auth
{
    public class UserContext
    {
        public int UsuarioID { get; set; }
        public int LocalidadID { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;

        // 🔹 Propiedad corregida usando las constantes
        public bool EsAdmin => Rol == RolesSistema.SuperUsuario ||
                              Rol == RolesSistema.Administrador;

        // 🔹 Útil para el módulo de Finanzas
        public bool EsTesorero => Rol == RolesSistema.TesoreroLocal ||
                                  Rol == RolesSistema.TesoreroNacional ||
                                  Rol == RolesSistema.SuperUsuario;

        // 🔹 Útil para permisos nacionales o globales
        public bool EsNivelNacional => Rol == RolesSistema.SuperUsuario ||
                                       Rol == RolesSistema.TesoreroNacional ||
                                       Rol == RolesSistema.SecretarioNacional;
    }
}