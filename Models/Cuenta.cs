using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IglesiaAPI.Models
{
    // Corresponde a la tabla Cuentas
    [Table("Cuentas")]
    public class Cuenta
    {
        [Key]
        [JsonPropertyName("cuentaId")]
        public int CuentaID { get; set; }

        [Required(ErrorMessage = "El nombre de la cuenta es requerido.")]
        [MaxLength(100, ErrorMessage = "El nombre de la cuenta no puede exceder los 100 caracteres.")]
        [JsonPropertyName("nombreCuenta")]
        public string NombreCuenta { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de cuenta es requerido.")]
        [MaxLength(50, ErrorMessage = "El tipo de cuenta no puede exceder los 50 caracteres.")]
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = string.Empty; // 'Ingreso' o 'Egreso'

        [JsonPropertyName("saldo")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Saldo { get; set; }

        // 🔹 PROPIEDAD QUE SOLUCIONA EL ERROR DE COMPILACIÓN
        // Determina si la cuenta es visible a nivel nacional (Tesorero Nacional)
        [JsonPropertyName("esNacional")]
        public bool EsNacional { get; set; } = false;

        // 🔹 Clave Foránea a la tabla Localidades
        [ForeignKey("Localidad")]
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        // 🔹 Propiedad de navegación
        [JsonIgnore] // evita ciclos de referencia al serializar
        public virtual Localidad? Localidad { get; set; }

        // 🔹 Propiedad de navegación: Lista de movimientos asociados a esta cuenta
        [JsonIgnore]
        public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();

        // 🔹 Auditoría
        [JsonPropertyName("creadoPorId")]
        public int? CreadoPorID { get; set; }

        [ForeignKey("CreadoPorID")]
        [JsonIgnore]
        public virtual Usuario? CreadoPor { get; set; }

        // 🔹 Propiedad auxiliar para rol/localidad
        [NotMapped]
        [JsonPropertyName("esGlobal")]
        public bool EsGlobal { get; set; }
    }
}