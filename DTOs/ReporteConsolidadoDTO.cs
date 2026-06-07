namespace IglesiaAPI.DTOs
{
    // DTO CONSOLIDADO ACTUALIZADO
    public class ReporteConsolidadoDTO
    {
        public string Periodo { get; set; } = string.Empty;
        public string NombreSede { get; set; } = "Sede General";

        // Estadísticas generales
        public int TotalIglesias { get; set; }
        public int TotalBautizados { get; set; }
        public int TotalSimpatizantes { get; set; }
        public int TotalNiños { get; set; }
        public int TotalSellados { get; set; }
        public int TotalGeneral { get; set; }

        // Resumen parcial del año
        public int BautizadosAnio { get; set; }
        public int SealladosAnio { get; set; }
        public int ApartadosAnio { get; set; }
        public int FallecidosAnio { get; set; }
        public int TrasladosAnio { get; set; }
        public int LlegaronAnio { get; set; }

        // Desglose por edad
        public int HombresMenores35 { get; set; }
        public int Hombres35a63 { get; set; }
        public int HombresMayores64 { get; set; }
        public int DamasMenores35 { get; set; }
        public int Damas35a63 { get; set; }
        public int DamasMayores64 { get; set; }
        public int NiñasHasta12 { get; set; }
        public int NiñosHasta12 { get; set; }

        // Finanzas
        public List<MovimientoDetalleDTO> Movimientos { get; set; } = new();
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal BalanceNeto => TotalIngresos - TotalEgresos;
    }

    public class MovimientoDetalleDTO
    {
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string LoteReferencia { get; set; } = string.Empty;
        public decimal Ingreso { get; set; }
        public decimal Egreso { get; set; }
        public string Estado { get; set; } = "Pendiente";
    }

    public class HojaVidaDTO
    {
        public int MiembroID { get; set; }
        public string NoRegistro { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public DateTime? FechaNacimiento { get; set; }
        public DateTime? FechaBautizado { get; set; }
        public string Sexo { get; set; } = "";
        public string EstadoCivil { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Provincia { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string Nacionalidad { get; set; } = "";
        public string NivelAcademico { get; set; } = "";
        public string Profesion { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Estado { get; set; } = "";
        public bool EsSellado { get; set; }
        public string Conyugue { get; set; } = "";
        public string Madre { get; set; } = "";
        public string Padre { get; set; } = "";
        public string Hijos { get; set; } = "";
        public string NombreSede { get; set; } = "";
        public string FotoPath { get; set; } = "";
    }

    public class CumpleaneroDTO
    {
        public string NombreCompleto { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Telefono { get; set; } = "";
        public string NombreSede { get; set; } = "";
        public int Edad { get; set; }
    }

    public class BautizadoDTO
    {
        public string NoRegistro { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public DateTime FechaBautizado { get; set; }
        public string Sexo { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string NombreSede { get; set; } = "";
    }

    public class InventarioDetalleDTO
    {
        public string NombreArticulo { get; set; } = "";
        public string Categoria { get; set; } = "";
        public int Cantidad { get; set; }
        public string Estado { get; set; } = "";
        public string Ubicacion { get; set; } = "";
        public string Marca { get; set; } = "";
        public string Modelo { get; set; } = "";
        public string NoSerie { get; set; } = "";
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal { get; set; }
        public string Responsable { get; set; } = "";
        public string NombreSede { get; set; } = "";
    }

    public class CelulaDetalleDTO
    {
        public string NombreCelula { get; set; } = "";
        public string Lider { get; set; } = "";
        public string DiaReunion { get; set; } = "";
        public string HoraReunion { get; set; } = "";
        public string NombreSede { get; set; } = "";
    }

    public class RegistroSecretariaDetalleDTO
    {
        public DateTime Fecha { get; set; }
        public string TipoRegistro { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string NombreSede { get; set; } = "";
    }

    public class DashboardNacionalDTO
    {
        public decimal TotalEntradas { get; set; }
        public decimal TotalSalidas { get; set; }
        public decimal SaldoNeto => TotalEntradas - TotalSalidas;
        public List<ResumenCuentaDTO> ResumenCuentas { get; set; } = new();
    }

    public class ResumenCuentaDTO
    {
        public string Cuenta { get; set; } = "";
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = "";
    }
}