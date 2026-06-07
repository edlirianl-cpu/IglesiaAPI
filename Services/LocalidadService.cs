using IglesiaAPI.Data;
using IglesiaAPI.Models;
using IglesiaAPI.Infrastructure.Auth; // para UserContext
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IglesiaAPI.Services
{
    public class LocalidadService : ILocalidadService
    {
        private readonly IglesiaDBContext _context;

        public LocalidadService(IglesiaDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Localidad>> GetAllLocalidadesAsync(UserContext user)
        {
            var query = _context.Localidades.AsQueryable();

            // 🔹 Filtrar por localidad si no es Admin
            if (!user.EsAdmin)
                query = query.Where(l => l.LocalidadID == user.LocalidadID);

            return await query.ToListAsync();
        }

        public async Task<Localidad?> GetLocalidadByIdAsync(UserContext user, int id)
        {
            var query = _context.Localidades.AsQueryable();

            query = query.Where(l => l.LocalidadID == id);

            if (!user.EsAdmin)
                query = query.Where(l => l.LocalidadID == user.LocalidadID);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Localidad> AddLocalidadAsync(Localidad localidad)
        {
            _context.Localidades.Add(localidad);
            await _context.SaveChangesAsync();
            return localidad;
        }

        public async Task<bool> UpdateLocalidadAsync(Localidad localidad)
        {
            var existente = await _context.Localidades.FindAsync(localidad.LocalidadID);
            if (existente == null) return false;

            _context.Entry(existente).CurrentValues.SetValues(localidad);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteLocalidadAsync(int id)
        {
            var localidad = await _context.Localidades.FindAsync(id);
            if (localidad == null) return false;

            _context.Localidades.Remove(localidad);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}