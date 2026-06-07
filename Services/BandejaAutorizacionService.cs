
using global::IglesiaAPI.Models;
using IglesiaAPI.DTOs;
using System.Net.Http.Json;

namespace IglesiaAPI.Services
{
    public class BandejaAutorizacionService : IBandejaAutorizacionService
    {
        private readonly HttpClient _http;

        public BandejaAutorizacionService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BandejaAutorizacion>> GetSolicitudesAsync()
        {
            try
            {
                // Apunta al endpoint de gobernanza que definimos en el Controller
                var response = await _http.GetFromJsonAsync<List<BandejaAutorizacion>>("api/Movimientos/solicitudes-pendientes");
                return response ?? new List<BandejaAutorizacion>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Servicio de Autorización: {ex.Message}");
                return new List<BandejaAutorizacion>();
            }
        }

        public async Task<bool> ProcesarSolicitudAsync(int id, bool aprobado, int autorizadorId)
        {
            // Enviamos el objeto DecisionDTO al API
            var decision = new { Aprobado = aprobado, AutorizadorID = autorizadorId };

            var response = await _http.PostAsJsonAsync($"api/Movimientos/procesar-solicitud/{id}", decision);

            return response.IsSuccessStatusCode;
        }
    }
}