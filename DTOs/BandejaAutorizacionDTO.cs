namespace IglesiaAPI.DTOs
{
    public class BandejaAutorizacionDTO
    {
        public int MovimientoID { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Solicitante { get; set; } = string.Empty;
        public string Localidad { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
    }
}