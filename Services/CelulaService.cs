using IglesiaAPI.Data;
using IglesiaAPI.Models;
using IglesiaAPI.Infrastructure.Auth; // para UserContext
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IglesiaAPI.Services
{
    public class CelulaService : ICelulaService
    {
        private readonly IglesiaDBContext _context;

        public CelulaService(IglesiaDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Celula>> GetAllCelulasAsync(UserContext user)
        {
            var query = _context.Celulas
                                .Include(c => c.Localidad)
                                .Include(c => c.Miembro)
                                .AsQueryable(); // 🔹 fuerza IQueryable<Celula>

            if (!user.EsAdmin)
                query = query.Where(c => c.LocalidadID == user.LocalidadID);

            return await query.ToListAsync();
        }

        public async Task<Celula?> GetCelulaByIdAsync(UserContext user, int id)
        {
            var query = _context.Celulas
                                .Include(c => c.Localidad)
                                .Include(c => c.Miembro)
                                .AsQueryable(); // 🔹 fuerza IQueryable<Celula>

            query = query.Where(c => c.CelulaID == id);

            if (!user.EsAdmin)
                query = query.Where(c => c.LocalidadID == user.LocalidadID);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Celula> AddCelulaAsync(Celula celula)
        {
            _context.Celulas.Add(celula);
            await _context.SaveChangesAsync();
            return celula;
        }

        public async Task<bool> UpdateCelulaAsync(Celula celula)
        {
            _context.Entry(celula).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteCelulaAsync(int id)
        {
            var celula = await _context.Celulas.FindAsync(id);
            if (celula == null) return false;

            _context.Celulas.Remove(celula);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}