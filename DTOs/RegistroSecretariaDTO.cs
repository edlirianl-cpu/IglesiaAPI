using System;
using System.ComponentModel.DataAnnotations;

namespace IglesiaAPI.DTOs
{
    // DTO usado para crear y actualizar Registros de Secretaría
    public class RegistroSecretariaDTO
    {
        // 🔹 CLAVE PARA LA COMUNICACIÓN: 
        // Agregamos RegistroID para que coincida con la Llave Primaria de tu tabla.
        // Esto permite que el Frontend sepa qué registro está editando.
        public int RegistroID { get; set; }

        [Required(ErrorMessage = "La fecha de registro es requerida.")]
        public DateTime FechaRegistro { get; set; }

        [Required(ErrorMessage = "El tipo de registro es requerido.")]
        [MaxLength(50)]
        public string TipoRegistro { get; set; } = string.Empty; // Ej: 'Evento', 'Actividad', 'Documento'

        [Required(ErrorMessage = "El título es requerido.")]
        [MaxLength(200)]
        public string? Titulo { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [MaxLength(500)]
        public string? DocumentoURL { get; set; } // URL donde se almacena el documento real

        // 🔹 Contexto de localidad
        public int LocalidadID { get; set; }
        public string? LocalidadNombre { get; set; } = string.Empty;

        // 🔹 Usuario que creó el registro
        public int? CreadoPorID { get; set; }
        public string? CreadoPorNombre { get; set; }

        // 🔹 Propiedad auxiliar para saber si el registro es visible globalmente (solo Admin)
        public bool EsGlobal { get; set; } = false;
    }
}