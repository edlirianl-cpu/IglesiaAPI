using System.Text.Json.Serialization;

namespace IglesiaAPI.DTOs
{
    public class DashboardDTO
    {
        [JsonPropertyName("miembrosActivos")]
        public int MiembrosActivos { get; set; }

        [JsonPropertyName("nuevosMes")]
        public int NuevosMes { get; set; }

        [JsonPropertyName("celulas")]
        public int Celulas { get; set; }

        [JsonPropertyName("ofrendasMensuales")]
        public decimal OfrendasMensuales { get; set; }

        [JsonPropertyName("inventarioTotal")]
        public int InventarioTotal { get; set; }

        [JsonPropertyName("balanceFinanzas")]
        public decimal BalanceFinanzas { get; set; }

        [JsonPropertyName("cumpleaniosHoy")]
        public int CumpleaniosHoy { get; set; }

        [JsonPropertyName("alertas")]
        public int Alertas { get; set; }

        [JsonPropertyName("eventosProximos")]
        public int EventosProximos { get; set; }

        [JsonPropertyName("actividadReciente")]
        public int ActividadReciente { get; set; }

        // 🔹 Contexto de localidad
        [JsonPropertyName("localidadId")]
        public int LocalidadID { get; set; }

        [JsonPropertyName("nombreLocalidad")]
        public string LocalidadNombre { get; set; } = string.Empty;

        // 🔹 Propiedad auxiliar para saber si el dashboard es global (solo Admin)
        [JsonPropertyName("esGlobal")]
        public bool EsGlobal { get; set; } = false;
    }
}