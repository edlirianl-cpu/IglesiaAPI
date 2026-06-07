namespace IglesiaAPI.DTOs
{
    public class UsuarioDTO
    {
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int LocalidadID { get; set; }
        public int? MiembroID { get; set; }

        public List<LocalidadResumenDTO>? LocalidadesDisponibles { get; set; }

        // ✅ con setter para poder asignar en el controller
        public bool EsAdmin { get; set; }

        public string LocalidadNombre { get; set; } = string.Empty;

        public bool PuedeVerTodasLocalidades => EsAdmin;
    }

    public class LocalidadResumenDTO
    {
        public int Id { get; set; }
        public string NombreLocalidad { get; set; } = string.Empty;
    }
}