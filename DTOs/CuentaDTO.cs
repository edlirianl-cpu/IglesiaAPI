using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IglesiaAPI.DTOs
{
    // DTO usado para crear y actualizar Cuentas Contables
    public class CuentaDTO
    {
        // Nota: Si usas este DTO para actualizaciones, podrías incluir el ID, 
        // pero usualmente se maneja en la URL del endpoint.
        [JsonPropertyName("cuentaId")]
        public int? CuentaID { get; set; }

        [Required(ErrorMessage = "El nombre de la cuenta es requerido.")]
        [MaxLength(100)]
        [JsonPropertyName("nombreCuenta")]
        public string NombreCuenta { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de cuenta es requerido ('Ingreso' o 'Egreso').")]
        [MaxLength(50)]
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [MaxLength(500)]
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        // 🔹 PROPIEDAD CLAVE: Reemplaza a 'EsGlobal'.
        // Al crear la cuenta (ej. "Diezmos Nacionales"), el Admin marca esto como true.
        [JsonPropertyName("esNacional")]
        public bool EsNacional { get; set; } = false;

        // 🔹 Contexto de localidad
        // Si la cuenta es nacional, este ID podría ser el de la oficina central o nulo.
        [Required(ErrorMessage = "El ID de la Localidad es requerido.")]
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        [JsonPropertyName("nombreLocalidad")]
        public string LocalidadNombre { get; set; } = string.Empty;

        // 🔹 Auditoría: Quién creó o modificó la cuenta
        [JsonPropertyName("creadoPorId")]
        public int? CreadoPorID { get; set; }

        [JsonPropertyName("creadoPorNombre")]
        public string? CreadoPorNombre { get; set; }

        [JsonPropertyName("saldo")]
        public decimal Saldo { get; set; }
    }
}