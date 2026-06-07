using IglesiaAPI.Models;
using IglesiaAPI.Infrastructure.Auth; // para UserContext
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IglesiaAPI.Services
{
    // Define el contrato para la lógica de negocio del módulo de Células.
    public interface ICelulaService
    {
        // 🔹 Lectura filtrada por rol/localidad
        Task<IEnumerable<Celula>> GetAllCelulasAsync(UserContext user);
        Task<Celula?> GetCelulaByIdAsync(UserContext user, int id);

        // 🔹 Escritura (no necesita UserContext porque se valida en controller)
        Task<Celula> AddCelulaAsync(Celula celula);
        Task<bool> UpdateCelulaAsync(Celula celula);
        Task<bool> DeleteCelulaAsync(int id);
    }
}