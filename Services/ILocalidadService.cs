using IglesiaAPI.Models;
using IglesiaAPI.Infrastructure.Auth; // para UserContext
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IglesiaAPI.Services
{
    public interface ILocalidadService
    {
        Task<IEnumerable<Localidad>> GetAllLocalidadesAsync(UserContext user);
        Task<Localidad?> GetLocalidadByIdAsync(UserContext user, int id);
        Task<Localidad> AddLocalidadAsync(Localidad localidad);
        Task<bool> UpdateLocalidadAsync(Localidad localidad);
        Task<bool> DeleteLocalidadAsync(int id);
    }
}