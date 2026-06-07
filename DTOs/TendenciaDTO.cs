using System.Collections.Generic;

namespace IglesiaAPI.DTOs
{
    public class TendenciaDTO
    {
        // 🔹 Eje temporal (meses)
        public List<string> Meses { get; set; } = new List<string>();

        // 🔹 Datos de miembros
        public List<int> NuevosMiembros { get; set; } = new List<int>();
        public List<int> Bautismos { get; set; } = new List<int>();

        // 🔹 Datos financieros
        public List<decimal> Ingresos { get; set; } = new List<decimal>();
        public List<decimal> Egresos { get; set; } = new List<decimal>();

        // 🔹 Contexto de localidad
        public int LocalidadID { get; set; }
        public string LocalidadNombre { get; set; } = string.Empty;

        // 🔹 Propiedad auxiliar para saber si los datos son globales (solo Admin)
        public bool EsGlobal { get; set; } = false;
    }
}