using BSC.DataAccess.Models;
using BSC.DataAccess.Respositories;


namespace BSC.Business.Services
{
    public class PedidoService
    {
        private readonly PedidoRepository _repository;

        public PedidoService(PedidoRepository repository)
        {
            _repository = repository;
        }

        public Task<List<PedidoDetalle>> ConsultarPedidosAsync(
            CancellationToken cancellationToken = default)
        {
            return _repository.ConsultarPedidosAsync(cancellationToken);
        }

        public async Task<int> ProcesarPedidoAsync(
            int vendedorId,
            string cliente,
            List<ProductoPedido> productos,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cliente))
                throw new ArgumentException("El nombre del cliente es obligatorio.");

            cliente = cliente.Trim();

            if (cliente.Length > 100)
                throw new ArgumentException("El nombre del cliente no puede ser mayor a 100 caracteres.");

            if (productos is null || productos.Count == 0)
                throw new ArgumentException("El pedido debe contener productos.");

            if (productos.Any(p => p is null
                || p.ProductoID <= 0
                || p.Cantidad <= 0))
            {
                throw new ArgumentException("Cada producto debe tener un ID válido y cantidad positiva.");
            }

            if (productos.Select(p => p.ProductoID).Distinct().Count() != productos.Count)
            {
                throw new ArgumentException("No puedes repetir un producto dentro del pedido.");
            }

            return await _repository.ProcesarPedidoAsync(
                vendedorId,
                cliente,
                productos,
                cancellationToken);
        }
    }
}
