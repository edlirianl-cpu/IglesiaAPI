namespace IglesiaAPI.DTOs
{
    public class TransaccionBancoDTO
    {
        public DateTime FechaPost { get; set; }
        public string DescripcionCorta { get; set; } = string.Empty;
        public decimal Monto { get; set; } // Columna "Monto Tran"
        public string? NoReferencia { get; set; } // Columna "No. Referen"
        public string? NoSerial { get; set; } // Columna "No. Serial"
    }
}
