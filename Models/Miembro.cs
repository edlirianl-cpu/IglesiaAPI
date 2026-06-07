using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IglesiaAPI.Models
{
    public class Miembro
    {
        [Key]
        [JsonPropertyName("miembroId")]
        public int MiembroID { get; set; }

        [Required(ErrorMessage = "El número de registro es requerido.")]
        [MaxLength(50, ErrorMessage = "El número de registro no puede exceder los 50 caracteres.")]
        [JsonPropertyName("noRegistro")]
        public string No_registro { get; set; } = string.Empty;

        [ForeignKey("Localidad")]
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        [JsonPropertyName("fechaBautizado")]
        public DateTime? FechaBautizado { get; set; }

        [Required(ErrorMessage = "El nombre completo es requerido.")]
        [MaxLength(150, ErrorMessage = "El nombre completo no puede exceder los 150 caracteres.")]
        [JsonPropertyName("nombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        [JsonPropertyName("fechaNacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        // Agregamos '?' a los strings que pueden ser NULL en la base de datos
        [JsonPropertyName("lugar")]
        public string? Lugar { get; set; } = string.Empty;

        [JsonPropertyName("direccion")]
        public string? Direccion { get; set; } = string.Empty;

        [JsonPropertyName("telefono")]
        public string? Telefono { get; set; } = string.Empty;

        [JsonPropertyName("provincia")]
        public string? Provincia { get; set; } = string.Empty;

        [JsonPropertyName("ciudad")]
        public string? Ciudad { get; set; } = string.Empty;

        [JsonPropertyName("nacionalidad")]
        public string? Nacionalidad { get; set; } = string.Empty;

        [JsonPropertyName("sexo")]
        public string? Sexo { get; set; } = string.Empty;

        [JsonPropertyName("estadoCivil")]
        public string? EstadoCivil { get; set; } = string.Empty;

        // 🔹 NUEVOS CAMPOS AGREGADOS PARA EL DASHBOARD (SIN ELIMINAR NADA)
        [JsonPropertyName("esSellado")]
        public bool EsSellado { get; set; } = false;

        [Required(ErrorMessage = "La categoría es requerida.")]
        [MaxLength(50)]
        [JsonPropertyName("categoria")]
        public string Categoria { get; set; } = "Simpatizante";

        [Required(ErrorMessage = "El estado es requerido.")]
        [MaxLength(50)]
        [JsonPropertyName("estado")]
        public string Estado { get; set; } = "Activo";
        // -----------------------------------------------------------

        [JsonPropertyName("correo")]
        public string? Correo { get; set; } = string.Empty;

        [JsonPropertyName("tipoDocumento")]
        public string? TipoDocumento { get; set; } = string.Empty;

        [JsonPropertyName("numeroDoc")]
        public string? NumeroDoc { get; set; } = string.Empty;

        [JsonPropertyName("nivelAcademico")]
        public string? NivelAcademico { get; set; } = string.Empty;

        [JsonPropertyName("profesion")]
        public string? Profesion { get; set; } = string.Empty;

        [JsonPropertyName("conyugue")]
        public string? Conyugue { get; set; } = string.Empty;

        [JsonPropertyName("madre")]
        public string? Madre { get; set; } = string.Empty;

        [JsonPropertyName("padre")]
        public string? Padre { get; set; } = string.Empty;

        [JsonPropertyName("hijos")]
        public string? Hijos { get; set; } = string.Empty;

        // 🔹 Relación con Localidad
        [JsonIgnore]
        public Localidad? Localidad { get; set; }

        // 🔹 Relación inversa con Usuarios
        [JsonIgnore]
        public ICollection<Usuario>? Usuarios { get; set; }

        // 🔹 Auditoría
        public int? CreadoPorID { get; set; }
        [JsonIgnore]
        public Usuario? CreadoPor { get; set; }

        // 🔹 Propiedad auxiliar para rol/localidad
        [NotMapped]
        [JsonIgnore]
        public bool EsGlobal { get; set; }

        [JsonPropertyName("fotoPath")]
        public string? FotoPath { get; set; }
    }

}