using System.Text.Json.Serialization;

namespace IglesiaAPI.DTOs
{
    public class LoginResponseDTO
    {
        // 🔹 Token JWT generado en el login
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        // 🔹 Información del usuario autenticado
        [JsonPropertyName("usuario")]
        public UsuarioDTO Usuario { get; set; } = new UsuarioDTO();

        // 🔹 Propiedad auxiliar para saber si el usuario es Admin
        [JsonPropertyName("esAdmin")]
        public bool EsAdmin => Usuario.EsAdmin;

        // 🔹 Localidad actual del usuario (para mostrar en Dashboard)
        [JsonPropertyName("localidadActual")]
        public string LocalidadActual => Usuario.LocalidadNombre;

        // 🔹 Indica si el usuario puede ver todas las localidades (solo Admin)
        [JsonPropertyName("puedeVerTodasLocalidades")]
        public bool PuedeVerTodasLocalidades => Usuario.PuedeVerTodasLocalidades;
    }
}