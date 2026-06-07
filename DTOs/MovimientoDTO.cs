using System;
using System.Collections.Generic; // Añadido para soportar listas
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IglesiaAPI.DTOs
{
    public class MovimientoDTO
    {
        public int MovimientoID { get; set; }

        [Required(ErrorMessage = "La fecha del movimiento es requerida.")]
        public DateTime FechaMovimiento { get; set; }

        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0.01, 1000000000.00, ErrorMessage = "El monto debe ser positivo.")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es requerido.")]
        [MaxLength(50)]
        public string TipoMovimiento { get; set; } = "Ingreso";

        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID de la Localidad es requerido.")]
        [JsonPropertyName("localidadID")]
        public int? LocalidadID { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta.")]
        [JsonPropertyName("cuentaID")]
        public int? CuentaID { get; set; }

        public string? NoReferencia { get; set; }
        public string? NoSerial { get; set; }
        public string? VoucherUrl { get; set; }
        public string? EstadoValidacion { get; set; }

        public int UsuarioID_Creador { get; set; }

        public bool EsEditable { get; set; } = false;
        public bool TieneSolicitudEdicion { get; set; } = false;
        public string? NombreCuenta { get; set; }

        // Mantenemos la consistencia con el nombre de la base de datos
        [JsonPropertyName("loteReferencia")]
        public string? LoteReferencia { get; set; }

        public string? NombreLocalidad { get; set; } // Agrega esta línea
    }

    public class DecisionDTO
    {
        public bool Aprobado { get; set; }
        public int AutorizadorID { get; set; }
    }

    // --- 🔹 NUEVO DTO PARA LA CREACIÓN DEL LOTE ---
    // Este objeto es el que enviará Blazor al Controller con los IDs seleccionados
    public class LotePeticionDTO
    {
        public List<int> MovimientoIds { get; set; } = new();
        public string PrefijoLocalidad { get; set; } = string.Empty;
    }
}