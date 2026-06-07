using IglesiaAPI.Infrastructure.Auth;
using IglesiaAPI.Models;

public interface IMiembroService
{
    Task<IEnumerable<Miembro>> GetAllAsync(UserContext user);
    Task<Miembro?> GetByIdAsync(UserContext user, int id);
    Task<Miembro> AddAsync(UserContext user, Miembro miembro);

    // ✅ Ahora devuelve el objeto actualizado en lugar de bool
    Task<Miembro?> UpdateAsync(UserContext user, Miembro miembro);

    Task<bool> DeleteAsync(UserContext user, int id);

    // 🔹 Nuevo método para obtener el último No_registro
    Task<string?> GetUltimoRegistroAsync();
}