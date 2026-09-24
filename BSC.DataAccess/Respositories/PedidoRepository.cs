using BSC.DataAccess.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSC.DataAccess.Respositories
{
    public class PedidoRepository
    {
        private readonly string _connectionString;

        public PedidoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<PedidoDetalle>> ConsultarPedidosAsync(
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    PedidoID,
                    Vendedor,
                    Cliente,
                    Fecha_Pedido,
                    ProductoID,
                    Nombre_Producto,
                    Clave_Producto,
                    Cantidad
                FROM dbo.vw_PedidosDetalle
                ORDER BY Fecha_Pedido DESC, PedidoID DESC, ProductoID;
            """;

            var pedidos = new List<PedidoDetalle>();

            await using var connection = new SqlConnection(_connectionString);

            using var command = new SqlCommand(sql, connection);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                pedidos.Add(new PedidoDetalle
                {
                    PedidoID = reader.GetInt32(reader.GetOrdinal("PedidoID")),
                    Vendedor = reader.GetString(reader.GetOrdinal("Vendedor")),
                    Cliente = reader.GetString(reader.GetOrdinal("Cliente")),
                    FechaPedido = reader.GetDateTime(reader.GetOrdinal("Fecha_Pedido")),
                    ProductoID = reader.GetInt32(reader.GetOrdinal("ProductoID")),
                    NombreProducto = reader.GetString(reader.GetOrdinal("Nombre_Producto")),
                    ClaveProducto = reader.GetString(reader.GetOrdinal("Clave_Producto")),
                    Cantidad = reader.GetInt32(reader.GetOrdinal("Cantidad"))
                });
            }

            return pedidos;
        }


        public async Task<int> ProcesarPedidoAsync(
            int vendedorId,
            string cliente,
            List<ProductoPedido> productos,
            CancellationToken cancellationToken = default)
        {
            using var tabla = new DataTable();
            tabla.Columns.Add("ProductoID", typeof(int));
            tabla.Columns.Add("Cantidad", typeof(int));

            foreach (var producto in productos)
            {
                tabla.Rows.Add(producto.ProductoID, producto.Cantidad);
            }

            await using var connection = new SqlConnection(_connectionString);

            using var command = new SqlCommand("dbo.ProcesarPedido", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@VendedorID", SqlDbType.Int).Value = vendedorId;
            command.Parameters.Add("@Cliente", SqlDbType.VarChar, 100).Value = cliente;

            var parametro = command.Parameters.Add("@Productos", SqlDbType.Structured);

            parametro.TypeName = "dbo.ProductosPedido";
            parametro.Value = tabla;

            await connection.OpenAsync(cancellationToken);

            try
            {
                var resultado = await command.ExecuteScalarAsync(cancellationToken);

                return Convert.ToInt32(resultado);
            }
            catch (SqlException ex) when (
                ex.Number is 50001 or 50002 or 50003 or 50004) // errores declarados en el procedimiento de sql
            {
                throw new ArgumentException(ex.Message, ex);
            }
        }
    }
}
