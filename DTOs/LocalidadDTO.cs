using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IglesiaAPI.DTOs
{
    // DTO usado para crear y actualizar Localidades (no incluye LocalidadID en creación)
    public class LocalidadDTO
    {
        [Required(ErrorMessage = "El nombre de la localidad es requerido.")]
        [MaxLength(100)]
        [JsonPropertyName("nombreLocalidad")]
        public string NombreLocalidad { get; set; } = string.Empty;

        [MaxLength(255)]
        [JsonPropertyName("direccion")]
        public string? Direccion { get; set; }

        [JsonPropertyName("fechaFundacion")]
        public DateTime? FechaFundacion { get; set; }

        // 🔹 Propiedad auxiliar para mostrar en frontend si la localidad es visible globalmente (solo Admin)
        [JsonPropertyName("esGlobal")]
        public bool EsGlobal { get; set; } = false;

        // 🔹 Propiedad opcional para mostrar el ID cuando se use en edición
        [JsonPropertyName("localidadId")]
        public int? LocalidadID { get; set; }
    }
}