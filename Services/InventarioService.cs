using IglesiaAPI.Data;
using IglesiaAPI.Models;
using IglesiaAPI.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IglesiaAPI.Services
{
    public class InventarioService : IInventarioService
    {
        private readonly IglesiaDBContext _context;

        public InventarioService(IglesiaDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Inventario>> GetAllInventariosAsync(UserContext user)
        {
            var query = _context.Inventarios
                                .Include(i => i.Localidad)
                                .OrderByDescending(i => i.InventarioID)
                                .AsQueryable();

            // 🔹 El Tesorero Nacional y el Admin tienen visión global
            if (!user.EsAdmin && user.Rol != "TesoreroNacional")
            {
                query = query.Where(i => i.LocalidadID == user.LocalidadID);
            }

            return await query.ToListAsync();
        }

        public async Task<Inventario?> GetInventarioByIdAsync(UserContext user, int id)
        {
            var inventario = await _context.Inventarios
                                           .Include(i => i.Localidad)
                                           .FirstOrDefaultAsync(i => i.InventarioID == id);

            if (inventario == null) return null;

            // 🔹 Validación de seguridad para acceso individual
            if (!user.EsAdmin && user.Rol != "TesoreroNacional" && inventario.LocalidadID != user.LocalidadID)
            {
                return null; // O podrías lanzar una excepción de seguridad
            }

            return inventario;
        }

        public async Task<Inventario> AddInventarioAsync(Inventario inventario)
        {
            // La lógica de quién registra y fecha se maneja preferiblemente en el controlador o aquí
            if (inventario.FechaRegistro == default)
                inventario.FechaRegistro = DateTime.Now;

            _context.Inventarios.Add(inventario);
            await _context.SaveChangesAsync();
            return inventario;
        }

        public async Task<bool> UpdateInventarioAsync(Inventario inventario)
        {
            var existente = await _context.Inventarios.FindAsync(inventario.InventarioID);
            if (existente == null) return false;

            // 🔹 Actualizamos los valores manteniendo la referencia de la entidad rastreada
            // Esto asegura que todos los nuevos campos (Marca, ImagenUrl, etc.) se actualicen correctamente
            _context.Entry(existente).CurrentValues.SetValues(inventario);

            // Evitamos que se sobrescriba la fecha de registro original si no viene en el objeto
            _context.Entry(existente).Property(x => x.FechaRegistro).IsModified = false;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteInventarioAsync(int id)
        {
            var inventario = await _context.Inventarios.FindAsync(id);
            if (inventario == null) return false;

            _context.Inventarios.Remove(inventario);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}