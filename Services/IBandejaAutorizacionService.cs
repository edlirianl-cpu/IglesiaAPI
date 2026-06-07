using IglesiaAPI.Models;

namespace IglesiaAPI.Services
{
    public interface IBandejaAutorizacionService
    {
        // Trae las peticiones de desbloqueo según la jerarquía (Local/Nacional)
        Task<List<BandejaAutorizacion>> GetSolicitudesAsync();

        // Procesa la decisión: true para autorizar, false para denegar
        Task<bool> ProcesarSolicitudAsync(int id, bool aprobado, int autorizadorId);
    }
}
