using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IglesiaAPI.DTOs
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Formato inválido de correo.")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        // 🔹 Opcional: permitir login filtrado por localidad (útil si el usuario pertenece a varias)
        [JsonPropertyName("localidadId")]
        public int? LocalidadID { get; set; }

        // 🔹 Opcional: recordar sesión en frontend
        [JsonPropertyName("recordarSesion")]
        public bool RecordarSesion { get; set; } = false;
    }
}