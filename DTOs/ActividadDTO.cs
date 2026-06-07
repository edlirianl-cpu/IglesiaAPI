using System;
using System.Text.Json.Serialization;

namespace IglesiaAPI.DTOs
{
    public class ActividadDTO
    {
        [JsonPropertyName("fecha")]
        public DateTime Fecha { get; set; }

        // 🔹 Tipo de actividad (ej. Evento, Documento, etc.)
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        // 🔹 Contexto de localidad
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        [JsonPropertyName("nombreLocalidad")]
        public string LocalidadNombre { get; set; } = string.Empty;

        // 🔹 Usuario que creó la actividad
        [JsonPropertyName("creadoPorId")]
        public int? CreadoPorID { get; set; }

        [JsonPropertyName("creadoPorNombre")]
        public string? CreadoPorNombre { get; set; }

        // 🔹 Propiedad auxiliar para saber si la actividad es global (solo Admin)
        [JsonPropertyName("esGlobal")]
        public bool EsGlobal { get; set; } = false;
    }
}