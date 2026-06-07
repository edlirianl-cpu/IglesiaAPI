namespace IglesiaAPI.Infrastructure.Auth
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpireHours { get; set; } = 2; // por defecto 2 horas
    }
}