using IglesiaAPI.DTOs;
using IglesiaAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IglesiaAPI.Services
{
    public interface IUsuarioService
    {
        Task<IEnumerable<Usuario>> GetAllUsuariosAsync();
        Task<Usuario?> GetUsuarioByIdAsync(int id);
        Task<Usuario> AddUsuarioAsync(Usuario usuario);   // ✅ sin nullable
        Task<bool> UpdateUsuarioAsync(Usuario usuario);
        Task<bool> DeleteUsuarioAsync(int id);
        Task<Usuario?> LoginAsync(string email, string password);

        // Devuelve DTO para el usuario actual (por claims)
        Task<UsuarioDTO?> GetUsuarioActualAsync();

        Task<Localidad?> GetLocalidadByIdAsync(int id);
        Task<List<LocalidadResumenDTO>> GetLocalidadesResumenAsync();
        Task<bool> ResetPasswordAsync(int id, string newPassword);


        UsuarioDTO? UsuarioActual { get; set; }
        
    }
}