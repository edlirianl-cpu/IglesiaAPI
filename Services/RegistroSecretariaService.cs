using IglesiaAPI.Data;
using IglesiaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IglesiaAPI.Services
{
    public class RegistroSecretariaService : IRegistroSecretariaService
    {
        private readonly IglesiaDBContext _context;

        public RegistroSecretariaService(IglesiaDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RegistroSecretaria>> GetAllRegistrosSecretariaAsync()
        {
            return await _context.RegistrosSecretaria
                                 .Include(r => r.Localidad)
                                 .Include(r => r.CreadoPor)
                                 .ToListAsync();
        }

        public async Task<RegistroSecretaria?> GetRegistroSecretariaByIdAsync(int id)
        {
            // Usamos RegistroID que es la PK de tu tabla
            return await _context.RegistrosSecretaria
                                 .Include(r => r.Localidad)
                                 .Include(r => r.CreadoPor)
                                 .FirstOrDefaultAsync(r => r.RegistroID == id);
        }

        public async Task AddRegistroSecretariaAsync(RegistroSecretaria registro)
        {
            _context.RegistrosSecretaria.Add(registro);
            await _context.SaveChangesAsync();
            // Al guardar, 'registro.RegistroID' se actualiza automáticamente con el valor de la BD
        }

        public async Task UpdateRegistroSecretariaAsync(RegistroSecretaria registro)
        {
            // Buscamos por la propiedad correcta: RegistroID
            var existente = await _context.RegistrosSecretaria.FindAsync(registro.RegistroID);
            if (existente == null) return;

            _context.Entry(existente).CurrentValues.SetValues(registro);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRegistroSecretariaAsync(int id)
        {
            var registro = await _context.RegistrosSecretaria.FindAsync(id);
            if (registro == null) return;

            _context.RegistrosSecretaria.Remove(registro);
            await _context.SaveChangesAsync();
        }
    }
}