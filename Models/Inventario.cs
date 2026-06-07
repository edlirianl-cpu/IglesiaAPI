using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IglesiaAPI.Models
{
    // Clase que representa un Artículo del Inventario corregida y aumentada
    public class Inventario
    {
        [Key]
        [JsonPropertyName("inventarioId")]
        public int InventarioID { get; set; }

        [Required(ErrorMessage = "El nombre del artículo es requerido.")]
        [MaxLength(150, ErrorMessage = "El nombre del artículo no puede exceder los 150 caracteres.")]
        [JsonPropertyName("nombreArticulo")]
        public string NombreArticulo { get; set; } = string.Empty;

        [JsonPropertyName("cantidad")]
        public int Cantidad { get; set; } = 1;

        [MaxLength(100, ErrorMessage = "La ubicación no puede exceder los 100 caracteres.")]
        [JsonPropertyName("ubicacion")]
        public string Ubicacion { get; set; } = string.Empty; // Ej: "Almacén", "Salón Principal"

        // --- Campos Técnicos añadidos para coincidir con la BD ---
        [JsonPropertyName("marca")]
        public string? Marca { get; set; }

        [JsonPropertyName("modelo")]
        public string? Modelo { get; set; }

        [JsonPropertyName("noSerie")]
        public string? NoSerie { get; set; }

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("categoria")]
        public string? Categoria { get; set; }

        [JsonPropertyName("estado")]
        public string? Estado { get; set; } = "Bueno";

        [JsonPropertyName("fechaAdquisicion")]
        public DateTime? FechaAdquisicion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [JsonPropertyName("valorUnitario")]
        public decimal ValorUnitario { get; set; } = 0;

        [JsonPropertyName("imagenUrl")]
        public string? ImagenUrl { get; set; }

        [JsonPropertyName("responsable")]
        public string? Responsable { get; set; }

        // --- Seguridad y Relaciones ---

        [Required]
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        [ForeignKey("LocalidadID")]
        [JsonIgnore] // Evita ciclos de referencia
        public virtual Localidad? Localidad { get; set; }

        [JsonPropertyName("usuarioid_Registra")]
        public int? UsuarioID_Registra { get; set; }

        [ForeignKey("UsuarioID_Registra")]
        [JsonIgnore]
        public virtual Usuario? CreadoPor { get; set; }

        [JsonPropertyName("fechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // --- Propiedades Auxiliares ---

        [NotMapped]
        [JsonIgnore]
        public bool EsGlobal { get; set; }

        [NotMapped]
        [JsonPropertyName("valorTotal")]
        public decimal ValorTotal => Cantidad * ValorUnitario;
    }
}