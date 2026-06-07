using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IglesiaAPI.Models
{
    // Clase que representa una Sede o Localidad
    public class Localidad
    {
        [Key]
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        [Required(ErrorMessage = "El nombre de la localidad es requerido.")]
        [MaxLength(100, ErrorMessage = "El nombre de la localidad no puede exceder los 100 caracteres.")]
        [JsonPropertyName("nombreLocalidad")]
        public string NombreLocalidad { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "La dirección no puede exceder los 255 caracteres.")]
        [JsonPropertyName("direccion")]
        public string? Direccion { get; set; }

        [JsonPropertyName("fechaFundacion")]
        public DateTime? FechaFundacion { get; set; }

        // 🔹 Propiedades de navegación (solo para EF Core)
        [JsonIgnore]
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

        [JsonIgnore]
        public ICollection<Miembro> Miembros { get; set; } = new List<Miembro>();

        [JsonIgnore]
        public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();

        [JsonIgnore]
        public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

        [JsonIgnore]
        public ICollection<Celula> Celulas { get; set; } = new List<Celula>();

        [JsonIgnore]
        public ICollection<RegistroSecretaria> RegistrosSecretaria { get; set; } = new List<RegistroSecretaria>();

        // 🔹 Propiedad auxiliar para rol/localidad
        [NotMapped]
        [JsonIgnore]
        public bool EsGlobal { get; set; }
    }
}