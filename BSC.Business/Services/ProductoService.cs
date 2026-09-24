using BSC.DataAccess.Models;
using BSC.DataAccess.Respositories;

namespace BSC.Business.Services
{
    public class ProductoService
    {
        private readonly ProductoRepository _repository;

        public ProductoService(ProductoRepository repository)
        {
            _repository = repository;
        }

        public Task<List<ProductoInventario>> ConsultarInventarioAsync(
            CancellationToken cancellationToken = default)
        {
            return _repository.ConsultarInventarioAsync(cancellationToken);
        }

        public async Task RegistrarProductoAsync(
            string nombre,
            int inventario,
            string claveProducto,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException("El nombre es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(claveProducto))
            {
                throw new ArgumentException("La clave es obligatoria");
            }

            nombre = nombre.Trim();
            claveProducto = claveProducto.Trim();

            if (nombre.Length > 100)
            {
                throw new ArgumentException("El nombre no debe ser mayor a 100 caracteres.");
            }

            if (claveProducto.Length > 50)
            {
                throw new ArgumentException("La clave no debe ser mayor a 50 caracteres.");
            }

            if (inventario < 0)
            {
                throw new ArgumentException("El inventario no puede ser negativo.");
            }

            await _repository.RegistrarProductoAsync(
                nombre, inventario, claveProducto, cancellationToken);

        }


        public async Task RegistrarInventarioAsync (
            int productoId,
            int cantidad,
            CancellationToken cancellationToken = default)
        {
            if (productoId <= 0)
            {
                throw new ArgumentException("El ID del producto no es válido.");
            }

            if (cantidad <= 0)
            {
                throw new ArgumentException("La cantidad no es válida.");
            }

            await _repository.RegistrarInventarioAsync(productoId, cantidad, cancellationToken);
        }
    }
}
