using IglesiaAPI.DTOs;
using IglesiaAPI.Data;
using IglesiaAPI.Infrastructure.Auth; // para UserContext
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IglesiaAPI.Services
{
    public class EventoImportanteService : IEventoImportanteService
    {
        private readonly IglesiaDBContext _context;

        public EventoImportanteService(IglesiaDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EventoImportanteDTO>> ObtenerEventosAsync(UserContext user, int? localidadId = null)
        {
            // 🔹 Determinar localidad final según rol
            int locFinal = user.EsAdmin ? (localidadId ?? user.LocalidadID) : user.LocalidadID;

            var eventos = await _context.RegistrosSecretaria
                .Where(r => r.LocalidadID == locFinal && r.TipoRegistro == "Evento")
                .Select(r => new EventoImportanteDTO
                {
                    Titulo = r.Titulo ?? "Sin título",
                    Fecha = r.FechaRegistro,
                    Localidad = r.LocalidadID.ToString()
                })
                .OrderBy(r => r.Fecha)
                .ToListAsync();

            return eventos;
        }
    }
}