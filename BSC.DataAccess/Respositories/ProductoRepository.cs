using BSC.DataAccess.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BSC.DataAccess.Respositories
{
    public class ProductoRepository
    {
        private readonly string _connectionString;

        public ProductoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<ProductoInventario>> ConsultarInventarioAsync(
            CancellationToken cancellationToken = default)
        {
            var productos = new List<ProductoInventario>();

            await using var connection = new SqlConnection(_connectionString);

            using var command = new SqlCommand("dbo.ConsultarInventario", connection);

            command.CommandType = System.Data.CommandType.StoredProcedure;

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                productos.Add(new ProductoInventario
                {
                    ProductoID = reader.GetInt32(reader.GetOrdinal("ProductoID")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    ClaveProducto = reader.GetString(reader.GetOrdinal("ClaveProducto")),
                    Inventario = reader.GetInt32(reader.GetOrdinal("Inventario"))
                });

            }

            return productos;
        }

        public async Task RegistrarProductoAsync(
            string nombre, 
            int inventario,
            string claveProducto,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection =
                new SqlConnection(_connectionString);

            using var command = new SqlCommand("dbo.RegistrarProducto", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = nombre;
            command.Parameters.Add("@Inventario", SqlDbType.Int).Value = inventario;
            command.Parameters.Add("@ClaveProducto", SqlDbType.VarChar, 50).Value = claveProducto;

            await connection.OpenAsync(cancellationToken);

            try
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627) // Claves de error por violacion de unicidad
            {
                throw new ArgumentException(
                    "Ya existe un producto con esa clave.", ex);
            }

        }

        public async Task RegistrarInventarioAsync(
            int productoId,
            int cantidad, 
            CancellationToken cancellationToken = default)
        {
            await using var connection = new SqlConnection(_connectionString);

            using var command = new SqlCommand("dbo.RegistrarInventario", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@ProductoID", SqlDbType.Int).Value = productoId;
            command.Parameters.Add("@Cantidad", SqlDbType.Int).Value = cantidad;

            await connection.OpenAsync(cancellationToken);

            try
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (SqlException ex) when (ex.Number is 50001 or 50002) // Errores definidos en el procedimiento de sql
            {
                throw new ArgumentException(ex.Message, ex);
            }


        }
    }
}