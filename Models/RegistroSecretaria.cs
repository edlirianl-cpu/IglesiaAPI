using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IglesiaAPI.Models
{
    // Corresponde a la tabla RegistroSecretaria
    public class RegistroSecretaria
    {
        [Key]
        [JsonPropertyName("registroId")]
        public int RegistroID { get; set; }

        [Column(TypeName = "DATE")]
        [Required(ErrorMessage = "La fecha del registro es requerida.")]
        [JsonPropertyName("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [Required(ErrorMessage = "El tipo de registro es requerido.")]
        [MaxLength(50, ErrorMessage = "El tipo de registro no puede exceder los 50 caracteres.")]
        [JsonPropertyName("tipoRegistro")]
        public string TipoRegistro { get; set; } = string.Empty; // Ej: 'Evento', 'Actividad', 'Documento'

        [Required(ErrorMessage = "El título es requerido.")]
        [MaxLength(200, ErrorMessage = "El título no puede exceder los 200 caracteres.")]
        [JsonPropertyName("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Column(TypeName = "longtext")] // Cambiado de TEXT a nvarchar(max)
        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        [MaxLength(500, ErrorMessage = "La URL del documento no puede exceder los 500 caracteres.")]
        [JsonPropertyName("documentoUrl")]
        public string? DocumentoURL { get; set; }

        // 🔹 Claves Foráneas
        [ForeignKey("Localidad")]
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        [ForeignKey("CreadoPor")]
        [JsonPropertyName("creadoPorId")]
        public int? CreadoPorID { get; set; }

        // 🔹 Propiedades de Navegación
        [JsonIgnore] // evita ciclos de referencia al serializar
        public Localidad Localidad { get; set; } = null!;

        [JsonIgnore]
        public Usuario? CreadoPor { get; set; }

        // 🔹 Propiedad auxiliar para rol/localidad
        [NotMapped]
        [JsonIgnore]
        public bool EsGlobal { get; set; }
    }
}