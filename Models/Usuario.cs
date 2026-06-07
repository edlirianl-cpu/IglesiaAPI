using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IglesiaAPI.Models
{
    public class Usuario
    {
        [Key]
        [JsonPropertyName("usuarioId")]
        public int UsuarioID { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre de usuario no puede exceder los 100 caracteres.")]
        [JsonPropertyName("nombreUsuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Formato inválido de correo.")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es requerido.")]
        [JsonPropertyName("rol")]
        public string Rol { get; set; } = string.Empty;

        // Relación con Localidad
        [ForeignKey("Localidad")]
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        [JsonIgnore]
        public Localidad? Localidad { get; set; }

        // Relación con Miembro (opcional)
        [ForeignKey("Miembro")]
        [JsonPropertyName("miembroId")]
        public int? MiembroID { get; set; }

        [JsonIgnore]
        public Miembro? Miembro { get; set; }

        // Propiedad calculada: no se persiste ni se serializa
        [NotMapped]
        [JsonIgnore]
        public bool EsAdmin => Rol.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }
}