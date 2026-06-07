using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IglesiaAPI.DTOs
{
    // DTO usado para transferir datos de Artículos del Inventario entre Frontend y Backend
    public class InventarioDTO
    {
        [JsonPropertyName("inventarioId")]
        public int InventarioID { get; set; } // Añadido para procesos de actualización (Edit)

        [Required(ErrorMessage = "El nombre del artículo es requerido.")]
        [MaxLength(150)]
        [JsonPropertyName("nombreArticulo")]
        public string NombreArticulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cantidad es requerida.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        [JsonPropertyName("cantidad")]
        public int Cantidad { get; set; }

        [MaxLength(100)]
        [JsonPropertyName("ubicacion")]
        public string Ubicacion { get; set; } = string.Empty;

        // --- Campos Técnicos añadidos para coincidir con el Modelo y la BD ---
        [JsonPropertyName("marca")]
        public string? Marca { get; set; }

        [JsonPropertyName("fechaRegistro")]
        public DateTime? FechaRegistro { get; set; }

        [JsonPropertyName("modelo")]
        public string? Modelo { get; set; }

        [JsonPropertyName("noSerie")]
        public string? NoSerie { get; set; }

        [JsonPropertyName("estado")]
        public string? Estado { get; set; } = "Bueno";

        [JsonPropertyName("valorUnitario")]
        public decimal ValorUnitario { get; set; }

        [JsonPropertyName("imagenUrl")]
        public string? ImagenUrl { get; set; } // 🔹 Aquí viajará la ruta de la foto

        [JsonPropertyName("responsable")]
        public string? Responsable { get; set; }

        // --- Datos de Relación y Seguridad ---
        [Required(ErrorMessage = "El ID de la Localidad es requerido.")]
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        [JsonPropertyName("nombreLocalidad")]
        public string LocalidadNombre { get; set; } = string.Empty;

        [JsonPropertyName("usuarioid_Registra")]
        public int? UsuarioID_Registra { get; set; }

        [JsonPropertyName("creadoPorNombre")]
        public string? CreadoPorNombre { get; set; }

        [JsonPropertyName("esGlobal")]
        public bool EsGlobal { get; set; } = false;

        // Propiedad calculada para mostrar el valor total en las listas de Blazor
        [JsonPropertyName("valorTotal")]
        public decimal ValorTotal => Cantidad * ValorUnitario;
    }
}