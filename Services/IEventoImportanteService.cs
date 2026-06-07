using IglesiaAPI.DTOs;
using IglesiaAPI.Infrastructure.Auth; // para UserContext
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IglesiaAPI.Services
{
    public interface IEventoImportanteService
    {
        Task<IEnumerable<EventoImportanteDTO>> ObtenerEventosAsync(UserContext user, int? localidadId = null);
    }
}