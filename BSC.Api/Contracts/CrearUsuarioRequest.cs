namespace BSC.Api.Contracts
{
    public class CrearUsuarioRequest
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public int RolID { get; set; }
    }
}
