namespace IglesiaAPI.Infrastructure.Auth
{
    public static class RolesSistema
    {
        // 1. Las etiquetas oficiales (Deben coincidir con la Base de Datos y Blazor)
        public const string SuperUsuario = "SuperUsuario";
        public const string Administrador = "Administrador";
        public const string TesoreroNacional = "Tesorero Nacional";
        public const string TesoreroLocal = "Tesorero Local";
        public const string PastorLocal = "Pastor Local";
        public const string SecretarioNacional = "Secretario Nacional";
        public const string SecretarioLocal = "Secretario Local";

        // 2. Agrupaciones útiles para validaciones rápidas en el Backend
        public static readonly List<string> ListaRoles = new()
        {
            SuperUsuario,
            Administrador,
            TesoreroNacional,
            TesoreroLocal,
            PastorLocal,
            SecretarioNacional,
            SecretarioLocal
        };
    }
}
