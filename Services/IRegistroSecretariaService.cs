using IglesiaAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IglesiaAPI.Services
{
    // Define el contrato para la lógica de negocio de los Registros de Secretaría
    public interface IRegistroSecretariaService
    {
        // Obtiene todos los registros (con Localidad y CreadoPor incluidos)
        Task<IEnumerable<RegistroSecretaria>> GetAllRegistrosSecretariaAsync();

        // Obtiene un registro por su ID único (RegistroID)
        Task<RegistroSecretaria?> GetRegistroSecretariaByIdAsync(int id);

        // Agrega un nuevo registro (El RegistroID será generado por la BD)
        Task AddRegistroSecretariaAsync(RegistroSecretaria registro);

        // Actualiza un registro existente identificándolo por su RegistroID
        Task UpdateRegistroSecretariaAsync(RegistroSecretaria registro);

        // Elimina un registro por su identificador único
        Task DeleteRegistroSecretariaAsync(int id);
    }
}