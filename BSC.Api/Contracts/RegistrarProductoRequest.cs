namespace BSC.Api.Contracts
{
    public class RegistrarProductoRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public int Inventario { get; set; }
        public string ClaveProducto { get; set; } = string.Empty;
    }
}
