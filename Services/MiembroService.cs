using IglesiaAPI.Data;
using IglesiaAPI.Infrastructure.Auth;
using IglesiaAPI.Models;
using Microsoft.EntityFrameworkCore;

public class MiembroService : IMiembroService
{
    private readonly IglesiaDBContext _context;

    public MiembroService(IglesiaDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Miembro>> GetAllAsync(UserContext user)
    {
        var query = _context.Miembros
                            .Include(m => m.Localidad)
                            .AsQueryable();

        if (!user.EsAdmin)
            query = query.Where(m => m.LocalidadID == user.LocalidadID);

        var miembros = await query.ToListAsync();

        foreach (var m in miembros)
        {
            m.NombreCompleto ??= "Sin Nombre";
            m.No_registro ??= "";
            m.Correo ??= "";
            m.Telefono ??= "";
            m.Direccion ??= "";
            m.Conyugue ??= "";
            m.Madre ??= "";
            m.Padre ??= "";
            m.Profesion ??= "";
            m.Categoria ??= "Simpatizante";
            m.Estado ??= "Activo";
            // Nota: No forzamos FotoPath a vacío aquí para que el DTO 
            // pueda detectar si es nulo y manejar el avatar por defecto.
        }

        return miembros;
    }

    public async Task<Miembro?> GetByIdAsync(UserContext user, int id)
    {
        var miembro = await _context.Miembros
                                    .Include(m => m.Localidad)
                                    .FirstOrDefaultAsync(m => m.MiembroID == id);

        if (miembro == null) return null;
        if (!user.EsAdmin && miembro.LocalidadID != user.LocalidadID) return null;

        return miembro;
    }

    public async Task<Miembro> AddAsync(UserContext user, Miembro miembro)
    {
        miembro.LocalidadID = user.EsAdmin ? miembro.LocalidadID : user.LocalidadID;
        miembro.CreadoPorID = user.UsuarioID;

        _context.Miembros.Add(miembro);
        await _context.SaveChangesAsync();
        return miembro;
    }

    public async Task<Miembro?> UpdateAsync(UserContext user, Miembro miembro)
    {
        // 1. Cargamos el registro original incluyendo Localidad para el retorno
        var existente = await _context.Miembros
                                      .Include(m => m.Localidad)
                                      .FirstOrDefaultAsync(m => m.MiembroID == miembro.MiembroID);

        if (existente == null) return null;

        // Seguridad de Localidad
        if (!user.EsAdmin && existente.LocalidadID != user.LocalidadID) return null;

        // 2. Mapeo manual de campos
        existente.NombreCompleto = miembro.NombreCompleto ?? "";
        existente.LocalidadID = user.EsAdmin ? miembro.LocalidadID : user.LocalidadID;

        existente.No_registro = miembro.No_registro;
        existente.FechaBautizado = miembro.FechaBautizado;
        existente.FechaNacimiento = miembro.FechaNacimiento;
        existente.Lugar = miembro.Lugar;
        existente.Direccion = miembro.Direccion;
        existente.Telefono = miembro.Telefono;
        existente.Provincia = miembro.Provincia;
        existente.Ciudad = miembro.Ciudad;
        existente.Nacionalidad = miembro.Nacionalidad;
        existente.Sexo = miembro.Sexo;
        existente.EstadoCivil = miembro.EstadoCivil;
        existente.Correo = miembro.Correo;
        existente.TipoDocumento = miembro.TipoDocumento;
        existente.NumeroDoc = miembro.NumeroDoc;
        existente.NivelAcademico = miembro.NivelAcademico;
        existente.Profesion = miembro.Profesion;
        existente.Conyugue = miembro.Conyugue;
        existente.Madre = miembro.Madre;
        existente.Padre = miembro.Padre;
        existente.Hijos = miembro.Hijos;

        // 🔹 ACTUALIZACIÓN DE LOS NUEVOS CAMPOS DEL DASHBOARD
        existente.EsSellado = miembro.EsSellado;
        existente.Categoria = miembro.Categoria ?? "Simpatizante";
        existente.Estado = miembro.Estado ?? "Activo";

        // 🛠 LÓGICA DE FOTO CORREGIDA:
        // Si el DTO trae una foto nueva, la actualizamos.
        // Si el DTO trae nulo o vacío, NO tocamos lo que ya hay en la DB.
        if (!string.IsNullOrWhiteSpace(miembro.FotoPath))
        {
            existente.FotoPath = miembro.FotoPath;
        }

        _context.Entry(existente).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return existente;
    }

    public async Task<bool> DeleteAsync(UserContext user, int id)
    {
        var miembro = await _context.Miembros.FindAsync(id);
        if (miembro == null) return false;

        if (!user.EsAdmin && miembro.LocalidadID != user.LocalidadID) return false;

        _context.Miembros.Remove(miembro);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> GetUltimoRegistroAsync()
    {
        var ultimo = await _context.Miembros
            .OrderByDescending(m => m.MiembroID)
            .Select(m => m.No_registro)
            .FirstOrDefaultAsync();

        return ultimo ?? "0";
    }
}