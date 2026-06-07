using System.Collections.Generic;
using System.Threading.Tasks;
using IglesiaAPI.Models;

namespace IglesiaAPI.Services
{
    // Define el contrato para la lógica de negocio de los Movimientos.
    public interface IMovimientoService
    {
        // --- CRUD Básico ---

        /// <summary>
        /// Obtiene todos los movimientos registrados, incluyendo sus relaciones.
        /// </summary>
        Task<IEnumerable<Movimiento>> GetAllMovimientosAsync();

        /// <summary>
        /// Busca un movimiento específico por su ID único.
        /// </summary>
        Task<Movimiento?> GetMovimientoByIdAsync(int id);

        /// <summary>
        /// Registra un nuevo movimiento y actualiza el saldo de la cuenta afectada.
        /// </summary>
        Task AddMovimientoAsync(Movimiento movimiento);

        /// <summary>
        /// Actualiza un movimiento existente, recalculando el saldo si el monto cambió.
        /// Bloquea la edición si el registro ya está validado o en un lote.
        /// </summary>
        Task UpdateMovimientoAsync(Movimiento movimiento);

        /// <summary>
        /// Elimina un movimiento y revierte su efecto en el saldo de la cuenta.
        /// </summary>
        Task DeleteMovimientoAsync(int id);


        // --- Métodos de Negocio y Filtrado ---

        /// <summary>
        /// Obtiene el historial de movimientos asociado a una cuenta contable específica.
        /// </summary>
        Task<IEnumerable<Movimiento>> GetMovimientosByCuentaIdAsync(int cuentaId);

        /// <summary>
        /// Obtiene movimientos que aún no han sido conciliados con el extracto bancario.
        /// </summary>
        Task<IEnumerable<Movimiento>> GetMovimientosPendientesValidacionAsync();

        /// <summary>
        /// Cambia el estado de validación de un movimiento tras el cruce con el banco.
        /// </summary>
        Task ValidarMovimientoBancarioAsync(int movimientoId, string noReferenciaBanco, int usuarioAprobadorId);

        // --- 🔹 NUEVO: Gestión de Lotes ---

        /// <summary>
        /// Agrupa múltiples movimientos bajo una referencia única para depósito bancario.
        /// </summary>
        /// <param name="ids">Lista de IDs de movimientos a agrupar.</param>
        /// <param name="prefijoLocalidad">Prefijo para identificar la zona (ej: SDE).</param>
        /// <returns>El código de LoteReferencia generado.</returns>
        Task<string> GenerarLoteAgrupadoAsync(List<int> ids, string prefijoLocalidad);
    }
}