using System;
using System.Text.Json.Serialization;

namespace IglesiaAPI.DTOs
{
    public class MiembroDTO
    {
        [JsonPropertyName("miembroId")]
        public int MiembroID { get; set; }

        [JsonPropertyName("noRegistro")]
        public string No_registro { get; set; } = string.Empty;

        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        // 🔹 Nombre de la localidad (para mostrar en frontend)
        [JsonPropertyName("nombreLocalidad")]
        public string? NombreLocalidad { get; set; }

        [JsonPropertyName("fechaBautizado")]
        public DateTime? FechaBautizado { get; set; }

        [JsonPropertyName("nombreCompleto")]
        public string? NombreCompleto { get; set; } = string.Empty;

        [JsonPropertyName("fechaNacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [JsonPropertyName("lugar")]
        public string Lugar { get; set; } = string.Empty;

        [JsonPropertyName("direccion")]
        public string Direccion { get; set; } = string.Empty;

        [JsonPropertyName("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [JsonPropertyName("provincia")]
        public string Provincia { get; set; } = string.Empty;

        [JsonPropertyName("ciudad")]
        public string Ciudad { get; set; } = string.Empty;

        [JsonPropertyName("nacionalidad")]
        public string Nacionalidad { get; set; } = string.Empty;

        [JsonPropertyName("sexo")]
        public string Sexo { get; set; } = string.Empty;

        [JsonPropertyName("estadoCivil")]
        public string EstadoCivil { get; set; } = string.Empty;

        // 🔹 NUEVOS CAMPOS INTEGRADOS AL DTO
        [JsonPropertyName("esSellado")]
        public bool EsSellado { get; set; }

        [JsonPropertyName("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = string.Empty;
        // ------------------------------------------

        [JsonPropertyName("correo")]
        public string Correo { get; set; } = string.Empty;

        [JsonPropertyName("tipoDocumento")]
        public string TipoDocumento { get; set; } = string.Empty;

        [JsonPropertyName("numeroDoc")]
        public string NumeroDoc { get; set; } = string.Empty;

        [JsonPropertyName("nivelAcademico")]
        public string NivelAcademico { get; set; } = string.Empty;

        [JsonPropertyName("profesion")]
        public string Profesion { get; set; } = string.Empty;

        [JsonPropertyName("conyugue")]
        public string Conyugue { get; set; } = string.Empty;

        [JsonPropertyName("madre")]
        public string Madre { get; set; } = string.Empty;

        [JsonPropertyName("padre")]
        public string Padre { get; set; } = string.Empty;

        [JsonPropertyName("hijos")]
        public string Hijos { get; set; } = string.Empty;

        // 🔹 Propiedad auxiliar para saber si el miembro es visible globalmente (solo Admin)
        [JsonPropertyName("esGlobal")]
        public bool EsGlobal { get; set; } = false;

        [JsonPropertyName("fotoPath")]
        public string? FotoPath { get; set; }
    }
}