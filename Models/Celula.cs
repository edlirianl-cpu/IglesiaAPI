using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IglesiaAPI.Models
{
    // Corresponde a la tabla Celulas
    public class Celula
    {
        [Key]
        [JsonPropertyName("celulaId")]
        public int CelulaID { get; set; }

        [Required(ErrorMessage = "El nombre de la célula es requerido.")]
        [MaxLength(100, ErrorMessage = "El nombre de la célula no puede exceder los 100 caracteres.")]
        [JsonPropertyName("nombreCelula")]
        public string NombreCelula { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "El día de reunión no puede exceder los 50 caracteres.")]
        [JsonPropertyName("diaReunion")]
        public string? DiaReunion { get; set; }

        // 🔹 Ahora como string, no TimeSpan
        [JsonPropertyName("horaReunion")]
        public string? HoraReunion { get; set; }

        // 🔹 Claves Foráneas
        [ForeignKey("Miembro")]
        [JsonPropertyName("miembroId")]
        public int? MiembroID { get; set; }   // antes era LiderID
       


        [ForeignKey("Localidad")]
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        // 🔹 Propiedades de Navegación
        [JsonIgnore] // evita ciclos de referencia al serializar
        public Miembro Miembro { get; set; } = null!;   // antes era Lider

        [JsonIgnore]
        public Localidad Localidad { get; set; } = null!;

        // 🔹 Auditoría
        public int? CreadoPorID { get; set; }

        [JsonIgnore]
        public Usuario? CreadoPor { get; set; }

        // 🔹 Propiedad auxiliar para rol/localidad
        [NotMapped]
        [JsonIgnore]
        public bool EsGlobal { get; set; }
    }
}