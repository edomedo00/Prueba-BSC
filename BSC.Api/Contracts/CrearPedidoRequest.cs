using BSC.DataAccess.Models;

namespace BSC.Api.Contracts
{
    public class CrearPedidoRequest
    {
        public string Cliente { get; set; } = string.Empty;
        public List<ProductoPedido> Productos { get; set; } = new();
    }
}
