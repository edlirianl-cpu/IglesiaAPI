using IglesiaAPI.DTOs;
using IglesiaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IglesiaAPI.Infrastructure.Auth; // 🔹 Para UserContext

namespace IglesiaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IEventoImportanteService _eventoService;

        public DashboardController(
            IDashboardService dashboardService,
            IEventoImportanteService eventoService)
        {
            _dashboardService = dashboardService;
            _eventoService = eventoService;
        }

        // 🔹 Obtener contexto del usuario autenticado
        private UserContext CurrentUser => UserContextFactory.FromClaims(User);

        // GET: api/Dashboard
        [HttpGet]
        public async Task<ActionResult<DashboardDTO>> GetDatos([FromQuery] int? localidadId)
        {
            if (!CurrentUser.EsAdmin && localidadId.HasValue && localidadId.Value != CurrentUser.LocalidadID)
                return Forbid();

            int localidadFinal = CurrentUser.EsAdmin
                ? (localidadId ?? CurrentUser.LocalidadID)
                : CurrentUser.LocalidadID;

            var datos = await _dashboardService.ObtenerDatosAsync(localidadFinal);

            datos.EsGlobal = CurrentUser.EsAdmin;
            datos.LocalidadID = localidadFinal;

            return Ok(datos);
        }

        // GET: api/Dashboard/eventos
        [HttpGet("eventos")]
        public async Task<ActionResult<IEnumerable<EventoImportanteDTO>>> GetEventos([FromQuery] int? localidadId)
        {
            if (!CurrentUser.EsAdmin && localidadId.HasValue && localidadId.Value != CurrentUser.LocalidadID)
                return Forbid();

            var eventos = await _eventoService.ObtenerEventosAsync(CurrentUser, localidadId);

            return Ok(eventos.Select(e =>
            {
                e.EsGlobal = CurrentUser.EsAdmin;
                return e;
            }));
        }

        // GET: api/Dashboard/tendencias
        [HttpGet("tendencias")]
        public async Task<ActionResult<TendenciaDTO>> GetTendencias([FromQuery] int? localidadId)
        {
            if (!CurrentUser.EsAdmin && localidadId.HasValue && localidadId.Value != CurrentUser.LocalidadID)
                return Forbid();

            int localidadFinal = CurrentUser.EsAdmin
                ? (localidadId ?? CurrentUser.LocalidadID)
                : CurrentUser.LocalidadID;

            var tendencia = await _dashboardService.ObtenerMiembrosPorMesAsync(localidadFinal);

            tendencia.EsGlobal = CurrentUser.EsAdmin;
            tendencia.LocalidadID = localidadFinal;

            return Ok(tendencia);
        }

        // GET: api/Dashboard/actividad
        [HttpGet("actividad")]
        public async Task<ActionResult<IEnumerable<ActividadDTO>>> GetActividad([FromQuery] int? localidadId)
        {
            if (!CurrentUser.EsAdmin && localidadId.HasValue && localidadId.Value != CurrentUser.LocalidadID)
                return Forbid();

            int localidadFinal = CurrentUser.EsAdmin
                ? (localidadId ?? CurrentUser.LocalidadID)
                : CurrentUser.LocalidadID;

            var actividad = await _dashboardService.ObtenerActividadAsync(localidadFinal);

            return Ok(actividad.Select(a =>
            {
                a.EsGlobal = CurrentUser.EsAdmin;
                a.LocalidadID = localidadFinal;
                return a;
            }));
        }
    }
}