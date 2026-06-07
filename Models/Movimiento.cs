using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IglesiaAPI.Models
{
    [Table("Movimientos")]
    public class Movimiento
    {
        [Key]
        [JsonPropertyName("movimientoId")]
        public int MovimientoID { get; set; }

        [Required(ErrorMessage = "La fecha del movimiento es requerida.")]
        [JsonPropertyName("fechaMovimiento")]
        public DateTime FechaMovimiento { get; set; }

        [Required(ErrorMessage = "El monto es requerido.")]
        [Column(TypeName = "decimal(18, 2)")]
        [JsonPropertyName("monto")]
        public decimal Monto { get; set; }

        [Required]
        [MaxLength(50)]
        [JsonPropertyName("tipoMovimiento")]
        public string TipoMovimiento { get; set; } = "Ingreso";

        [MaxLength(500)]
        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        // 🔹 Claves Foráneas
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        [JsonPropertyName("cuentaId")]
        public int CuentaID { get; set; }

        // 🔹 Propiedades de Navegación
        [ForeignKey("LocalidadID")]
        [JsonIgnore]
        public virtual Localidad? Localidad { get; set; }

        [ForeignKey("CuentaID")]
        [JsonIgnore]
        public virtual Cuenta? Cuenta { get; set; }

        // 🔹 CONCILIACIÓN BANCARIA (Sincronizados con SQL)
        [JsonPropertyName("noReferencia")]
        public string? NoReferencia { get; set; }

        [JsonPropertyName("noSerial")]
        public string? NoSerial { get; set; }

        [JsonPropertyName("voucherUrl")]
        public string? VoucherUrl { get; set; }

        [JsonPropertyName("estadoValidacion")]
        public string? EstadoValidacion { get; set; } = "Pendiente";

        // 🔹 NUEVO: Campo para agrupación de depósitos (Matching)
        [MaxLength(50)]
        [JsonPropertyName("loteReferencia")]
        public string? LoteReferencia { get; set; } // Identificador del grupo de depósito

        // 🔹 SEGURIDAD Y REGLA DE ORO
        [JsonPropertyName("usuarioId_Creador")]
        public int UsuarioID_Creador { get; set; }

        [JsonPropertyName("usuarioId_Aprobador")]
        public int? UsuarioID_Aprobador { get; set; }

        [JsonPropertyName("esEditable")]
        public bool EsEditable { get; set; } = false;

        [JsonPropertyName("tieneSolicitudEdicion")]
        public bool TieneSolicitudEdicion { get; set; } = false;

        [JsonPropertyName("fechaRegistro")]
        public DateTime? FechaRegistro { get; set; } = DateTime.Now;

        [ForeignKey("UsuarioID_Creador")]
        [JsonIgnore]
        public virtual Usuario? CreadoPor { get; set; }
    }
}