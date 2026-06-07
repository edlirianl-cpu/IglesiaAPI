using IglesiaAPI.Models;
using IglesiaAPI.Infrastructure.Auth; // para UserContext
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IglesiaAPI.Services
{
    /// <summary>
    /// Interfaz para el manejo de la lógica de negocio del Inventario.
    /// Garantiza que el acceso a los datos respete el UserContext (Localidad y Rol).
    /// </summary>
    public interface IInventarioService
    {
        // Obtiene todos los artículos según el rol (Admin/Nacional ve todo, Local ve su ID)
        Task<IEnumerable<Inventario>> GetAllInventariosAsync(UserContext user);

        // Obtiene un artículo específico validando que el usuario tenga permiso para verlo
        Task<Inventario?> GetInventarioByIdAsync(UserContext user, int id);

        // Registra un nuevo artículo (incluyendo campos técnicos e ImagenUrl)
        Task<Inventario> AddInventarioAsync(Inventario inventario);

        // Actualiza un artículo existente
        Task<bool> UpdateInventarioAsync(Inventario inventario);

        // Elimina un artículo validando pertenencia
        Task<bool> DeleteInventarioAsync(int id);
    }
}