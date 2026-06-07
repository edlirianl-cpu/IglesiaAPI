using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IglesiaAPI.DTOs
{
    // DTO usado para crear y actualizar Células
    public class CelulaDTO
    {
        [JsonPropertyName("celulaId")]
        public int? CelulaID { get; set; }

        [Required(ErrorMessage = "El nombre de la célula es requerido.")]
        [MaxLength(100)]
        [JsonPropertyName("nombreCelula")]
        public string NombreCelula { get; set; } = string.Empty;

        [MaxLength(50)]
        [JsonPropertyName("diaReunion")]
        public string? DiaReunion { get; set; }

        [JsonPropertyName("horaReunion")]
        public string? HoraReunion { get; set; }

        [JsonPropertyName("miembroId")]
        public int? MiembroID { get; set; } // ID del Miembro que es Líder

        [Required(ErrorMessage = "El ID de la Localidad es requerido.")]
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        // 🔹 Nombre de la localidad (para mostrar en frontend)
        [JsonPropertyName("nombreLocalidad")]
        public string LocalidadNombre { get; set; } = string.Empty;

        // 🔹 Nombre del líder (para mostrar en frontend)
        [JsonPropertyName("nombreLider")]
        public string? NombreLider { get; set; }

        // 🔹 Usuario que creó la célula
        [JsonPropertyName("creadoPorId")]
        public int? CreadoPorID { get; set; }

        [JsonPropertyName("creadoPorNombre")]
        public string? CreadoPorNombre { get; set; }

        // 🔹 Propiedad auxiliar para saber si la célula es global (solo Admin)
        [JsonPropertyName("esGlobal")]
        public bool EsGlobal { get; set; } = false;
    }
}