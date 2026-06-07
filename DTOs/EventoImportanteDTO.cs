using System;
using System.Text.Json.Serialization;

namespace IglesiaAPI.DTOs
{
    public class EventoImportanteDTO
    {
        [JsonPropertyName("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [JsonPropertyName("fecha")]
        public DateTime Fecha { get; set; }

        [JsonPropertyName("hora")]
        public TimeSpan Hora { get; set; }   // ✅ derivado de FechaRegistro.TimeOfDay

        // 🔹 Contexto de localidad
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        [JsonPropertyName("nombreLocalidad")]
        public string Localidad { get; set; } = string.Empty;

        // 🔹 Usuario que creó el evento
        [JsonPropertyName("creadoPorId")]
        public int? CreadoPorID { get; set; }

        [JsonPropertyName("creadoPorNombre")]
        public string? CreadoPorNombre { get; set; }

        // 🔹 Propiedad auxiliar para saber si el evento es global (solo Admin)
        [JsonPropertyName("esGlobal")]
        public bool EsGlobal { get; set; } = false;
    }
}