using Microsoft.EntityFrameworkCore;
using IglesiaAPI.Data;
using IglesiaAPI.DTOs;

namespace IglesiaAPI.Services
{
    public class ReporteService : IReporteService
    {
        private readonly IglesiaDBContext _context;
        public ReporteService(IglesiaDBContext context) => _context = context;

        // =============================================
        // 1. REPORTE CONSOLIDADO (Ya existente, mejorado)
        // =============================================
        public async Task<ReporteConsolidadoDTO> GenerarReporteAsync(DateTime inicio, DateTime fin, int? localidadId)
        {
            var hoy = DateTime.Today;
            var anioActual = hoy.Year;

            var dto = new ReporteConsolidadoDTO
            {
                Periodo = $"{inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}"
            };

            // Localidad
            if (localidadId.HasValue)
            {
                var localidad = await _context.Localidades.FindAsync(localidadId.Value);
                dto.NombreSede = localidad?.NombreLocalidad ?? "Sede General";
            }
            else
            {
                dto.NombreSede = "Nacional";
            }

            // Miembros
            var queryMiembros = _context.Miembros.AsQueryable();
            if (localidadId.HasValue)
                queryMiembros = queryMiembros.Where(m => m.LocalidadID == localidadId.Value);

            var listaM = await queryMiembros.ToListAsync();

            // Estadísticas generales
            dto.TotalIglesias = localidadId.HasValue ? 1 :
                await _context.Localidades.CountAsync();
            dto.TotalBautizados = listaM.Count(m => m.Categoria == "Bautizado");
            dto.TotalSimpatizantes = listaM.Count(m => m.Categoria == "Simpatizante");
            dto.TotalNiños = listaM.Count(m => m.Categoria == "Niño");
            dto.TotalSellados = listaM.Count(m => m.EsSellado);
            dto.TotalGeneral = listaM.Count;

            // Resumen parcial del año actual
            dto.BautizadosAnio = listaM.Count(m =>
                m.FechaBautizado.HasValue &&
                m.FechaBautizado.Value.Year == anioActual);

            dto.SealladosAnio = listaM.Count(m =>
                m.EsSellado &&
                m.FechaBautizado.HasValue &&
                m.FechaBautizado.Value.Year == anioActual);

            dto.ApartadosAnio = listaM.Count(m =>
                m.Estado == "Inactivo" &&
                m.FechaBautizado.HasValue);

            dto.FallecidosAnio = listaM.Count(m => m.Estado == "Fallecido");

            dto.TrasladosAnio = listaM.Count(m => m.Estado == "Trasladado");

            dto.LlegaronAnio = listaM.Count(m =>
                m.Estado == "Activo" &&
                m.Categoria == "Bautizado" &&
                m.FechaBautizado.HasValue &&
                m.FechaBautizado.Value.Year < anioActual);

            // Desglose por edad/sexo
            dto.HombresMenores35 = listaM.Count(m => m.Sexo == "Masculino" && GetEdad(m.FechaNacimiento, hoy) < 35);
            dto.Hombres35a63 = listaM.Count(m => m.Sexo == "Masculino" && GetEdad(m.FechaNacimiento, hoy) >= 35 && GetEdad(m.FechaNacimiento, hoy) <= 63);
            dto.HombresMayores64 = listaM.Count(m => m.Sexo == "Masculino" && GetEdad(m.FechaNacimiento, hoy) >= 64);
            dto.DamasMenores35 = listaM.Count(m => m.Sexo == "Femenino" && GetEdad(m.FechaNacimiento, hoy) < 35);
            dto.Damas35a63 = listaM.Count(m => m.Sexo == "Femenino" && GetEdad(m.FechaNacimiento, hoy) >= 35 && GetEdad(m.FechaNacimiento, hoy) <= 63);
            dto.DamasMayores64 = listaM.Count(m => m.Sexo == "Femenino" && GetEdad(m.FechaNacimiento, hoy) >= 64);
            dto.NiñasHasta12 = listaM.Count(m => m.Sexo == "Femenino" && GetEdad(m.FechaNacimiento, hoy) <= 12);
            dto.NiñosHasta12 = listaM.Count(m => m.Sexo == "Masculino" && GetEdad(m.FechaNacimiento, hoy) <= 12);

            // Movimientos financieros
            var queryMovs = _context.Movimientos
                .Where(m => m.FechaMovimiento >= inicio && m.FechaMovimiento <= fin);
            if (localidadId.HasValue)
                queryMovs = queryMovs.Where(m => m.LocalidadID == localidadId.Value);

            var movimientos = await queryMovs.OrderBy(m => m.FechaMovimiento).ToListAsync();

            dto.Movimientos = movimientos.Select(m => new MovimientoDetalleDTO
            {
                Fecha = m.FechaMovimiento,
                Descripcion = m.Descripcion ?? "S/D",
                LoteReferencia = m.LoteReferencia ?? "N/A",
                Ingreso = m.TipoMovimiento == "Ingreso" ? m.Monto : 0,
                Egreso = m.TipoMovimiento == "Egreso" ? m.Monto : 0,
                Estado = m.EstadoValidacion ?? "Pendiente"
            }).ToList();

            dto.TotalIngresos = dto.Movimientos.Sum(x => x.Ingreso);
            dto.TotalEgresos = dto.Movimientos.Sum(x => x.Egreso);

            return dto;
        }

        // =============================================
        // 2. HOJA DE VIDA DEL MIEMBRO
        // =============================================
        public async Task<HojaVidaDTO> GenerarHojaVidaAsync(int miembroId)
        {
            var miembro = await _context.Miembros
                .Include(m => m.Localidad)
                .FirstOrDefaultAsync(m => m.MiembroID == miembroId)
                ?? throw new Exception("Miembro no encontrado.");

            return new HojaVidaDTO
            {
                MiembroID = miembro.MiembroID,
                NoRegistro = miembro.No_registro,
                NombreCompleto = miembro.NombreCompleto,
                FechaNacimiento = miembro.FechaNacimiento,
                FechaBautizado = miembro.FechaBautizado,
                Sexo = miembro.Sexo ?? "",
                EstadoCivil = miembro.EstadoCivil ?? "",
                Telefono = miembro.Telefono ?? "",
                Correo = miembro.Correo ?? "",
                Direccion = miembro.Direccion ?? "",
                Provincia = miembro.Provincia ?? "",
                Ciudad = miembro.Ciudad ?? "",
                Nacionalidad = miembro.Nacionalidad ?? "",
                NivelAcademico = miembro.NivelAcademico ?? "",
                Profesion = miembro.Profesion ?? "",
                Categoria = miembro.Categoria,
                Estado = miembro.Estado,
                EsSellado = miembro.EsSellado,
                Conyugue = miembro.Conyugue ?? "",
                Madre = miembro.Madre ?? "",
                Padre = miembro.Padre ?? "",
                Hijos = miembro.Hijos ?? "",
                NombreSede = miembro.Localidad?.NombreLocalidad ?? "",
                FotoPath = miembro.FotoPath ?? ""
            };
        }

        // =============================================
        // 3. CUMPLEAÑEROS DEL MES
        // =============================================
        public async Task<List<CumpleaneroDTO>> GenerarCumpleanerosAsync(int mes, int? localidadId)
        {
            var query = _context.Miembros
                .Include(m => m.Localidad)
                .Where(m => m.FechaNacimiento.HasValue &&
                            m.FechaNacimiento.Value.Month == mes &&
                            m.Estado == "Activo");

            if (localidadId.HasValue)
                query = query.Where(m => m.LocalidadID == localidadId.Value);

            var miembros = await query
                .OrderBy(m => m.FechaNacimiento!.Value.Day)
                .ToListAsync();

            return miembros.Select(m => new CumpleaneroDTO
            {
                NombreCompleto = m.NombreCompleto,
                FechaNacimiento = m.FechaNacimiento!.Value,
                Telefono = m.Telefono ?? "",
                NombreSede = m.Localidad?.NombreLocalidad ?? "",
                Edad = GetEdad(m.FechaNacimiento, DateTime.Today)
            }).ToList();
        }

        // =============================================
        // 4. LISTA DE BAUTIZADOS
        // =============================================
        public async Task<List<BautizadoDTO>> GenerarListaBautizadosAsync(DateTime inicio, DateTime fin, int? localidadId)
        {
            var query = _context.Miembros
                .Include(m => m.Localidad)
                .Where(m => m.FechaBautizado.HasValue &&
                            m.FechaBautizado.Value >= inicio &&
                            m.FechaBautizado.Value <= fin);

            if (localidadId.HasValue)
                query = query.Where(m => m.LocalidadID == localidadId.Value);

            var miembros = await query
                .OrderBy(m => m.FechaBautizado)
                .ToListAsync();

            return miembros.Select(m => new BautizadoDTO
            {
                NoRegistro = m.No_registro,
                NombreCompleto = m.NombreCompleto,
                FechaBautizado = m.FechaBautizado!.Value,
                Sexo = m.Sexo ?? "",
                Telefono = m.Telefono ?? "",
                NombreSede = m.Localidad?.NombreLocalidad ?? ""
            }).ToList();
        }

        // =============================================
        // 5. INVENTARIO DE ACTIVOS
        // =============================================
        public async Task<List<InventarioDetalleDTO>> GenerarInventarioAsync(int? localidadId)
        {
            var query = _context.Inventarios
                .Include(i => i.Localidad)
                .AsQueryable();

            if (localidadId.HasValue)
                query = query.Where(i => i.LocalidadID == localidadId.Value);

            var items = await query
                .OrderBy(i => i.Categoria)
                .ThenBy(i => i.NombreArticulo)
                .ToListAsync();

            return items.Select(i => new InventarioDetalleDTO
            {
                NombreArticulo = i.NombreArticulo,
                Categoria = i.Categoria ?? "",
                Cantidad = i.Cantidad,
                Estado = i.Estado ?? "",
                Ubicacion = i.Ubicacion,
                Marca = i.Marca ?? "",
                Modelo = i.Modelo ?? "",
                NoSerie = i.NoSerie ?? "",
                ValorUnitario = i.ValorUnitario,
                ValorTotal = i.ValorTotal,
                Responsable = i.Responsable ?? "",
                NombreSede = i.Localidad?.NombreLocalidad ?? ""
            }).ToList();
        }

        // =============================================
        // 6. REPORTE DE CÉLULAS
        // =============================================
        public async Task<List<CelulaDetalleDTO>> GenerarReporteCelulasAsync(int? localidadId)
        {
            var query = _context.Celulas
                .Include(c => c.Miembro)
                .Include(c => c.Localidad)
                .AsQueryable();

            if (localidadId.HasValue)
                query = query.Where(c => c.LocalidadID == localidadId.Value);

            var celulas = await query
                .OrderBy(c => c.NombreCelula)
                .ToListAsync();

            return celulas.Select(c => new CelulaDetalleDTO
            {
                NombreCelula = c.NombreCelula,
                Lider = c.Miembro?.NombreCompleto ?? "Sin asignar",
                DiaReunion = c.DiaReunion ?? "",
                HoraReunion = c.HoraReunion ?? "",
                NombreSede = c.Localidad?.NombreLocalidad ?? ""
            }).ToList();
        }

        // =============================================
        // 7. INFORME DE SECRETARÍA
        // =============================================
        public async Task<List<RegistroSecretariaDetalleDTO>> GenerarInformeSecretariaAsync(DateTime inicio, DateTime fin, int? localidadId)
        {
            var query = _context.RegistrosSecretaria
                .Include(r => r.Localidad)
                .Where(r => r.FechaRegistro >= inicio && r.FechaRegistro <= fin);

            if (localidadId.HasValue)
                query = query.Where(r => r.LocalidadID == localidadId.Value);

            var registros = await query
                .OrderBy(r => r.FechaRegistro)
                .ToListAsync();

            return registros.Select(r => new RegistroSecretariaDetalleDTO
            {
                Fecha = r.FechaRegistro,
                TipoRegistro = r.TipoRegistro,
                Titulo = r.Titulo,
                Descripcion = r.Descripcion ?? "",
                NombreSede = r.Localidad?.NombreLocalidad ?? ""
            }).ToList();
        }

        // =============================================
        // HELPER
        // =============================================
        private int GetEdad(DateTime? nacimiento, DateTime hoy)
        {
            if (!nacimiento.HasValue) return 0;
            var edad = hoy.Year - nacimiento.Value.Year;
            if (nacimiento.Value.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }
    }
}