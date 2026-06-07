using IglesiaAPI.Models;
using IglesiaAPI.Infrastructure.Auth; // para UserContext
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IglesiaAPI.Services
{
    /// <summary>
    /// Define las operaciones permitidas para la gestión de Cuentas Contables.
    /// Incluye filtros automáticos por Localidad y alcance Nacional.
    /// </summary>
    public interface ICuentaService
    {
        /// <summary>
        /// Obtiene las cuentas permitidas para el usuario (Propias + Nacionales).
        /// </summary>
        Task<IEnumerable<Cuenta>> GetAllCuentasAsync(UserContext user);

        /// <summary>
        /// Obtiene el detalle de una cuenta validando el permiso de acceso.
        /// </summary>
        Task<Cuenta?> GetCuentaByIdAsync(UserContext user, int id);

        /// <summary>
        /// Registra una nueva cuenta en el catálogo.
        /// </summary>
        Task<Cuenta> AddCuentaAsync(Cuenta cuenta);

        /// <summary>
        /// Actualiza los datos informativos de una cuenta (No modifica el saldo directamente).
        /// </summary>
        Task<bool> UpdateCuentaAsync(Cuenta cuenta);

        /// <summary>
        /// Elimina una cuenta si no tiene movimientos asociados.
        /// </summary>
        Task<bool> DeleteCuentaAsync(int id);
    }
}