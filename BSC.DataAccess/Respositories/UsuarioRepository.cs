
using System.Data;
using BSC.DataAccess.Models;
using Microsoft.Data.SqlClient;

namespace BSC.DataAccess.Respositories
{
    public class UsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository (string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Usuario?> ObtenerPorNombreAsync(
            string nombreUsuario,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT 
                    U.UsuarioID,
                    U.NombreUsuario,
                    U.ContrasenaHash,
                    R.Nombre AS Rol
                FROM dbo.Usuario AS U
                JOIN dbo.Rol AS R ON R.RolID = U.RolID
                WHERE U.NombreUsuario = @NombreUsuario;
                """;

            await using var connection = new SqlConnection(_connectionString);

            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 100).Value = nombreUsuario;

            await connection.OpenAsync(cancellationToken);
            
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken)) return null;

            return new Usuario
            {
                UsuarioID =
                reader.GetInt32(reader.GetOrdinal("UsuarioID")),
                NombreUsuario =
                reader.GetString(reader.GetOrdinal("NombreUsuario")),
                ContrasenaHash =
                reader.GetString(reader.GetOrdinal("ContrasenaHash")),
                Rol =
                reader.GetString(reader.GetOrdinal("Rol"))
            };
        }


        public async Task CrearUsuarioAsync(
            string nombreUsuario,
            string contrasenaHash,
            int rolId,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                INSERT INTO dbo.Usuario (
                    NombreUsuario,
                    ContrasenaHash,
                    RolID
                )
                VALUES (@NombreUsuario, @ContrasenaHash, @RolID);
            """;

            await using var connection = new SqlConnection(_connectionString);

            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 100).Value = nombreUsuario;
            command.Parameters.Add("@ContrasenaHash", SqlDbType.VarChar, 512).Value = contrasenaHash;
            command.Parameters.Add("@RolID", SqlDbType.Int).Value = rolId;

            await connection.OpenAsync(cancellationToken);

            try
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627) // violacion de unicidad en sql
            {
                throw new ArgumentException("El nombre de usuario ya está registrado.", ex);
            }
            catch (SqlException ex) when (ex.Number == 547) // error de llave foranea
            {
                throw new ArgumentException("El rol seleccionado no existe.", ex);
            }

        }

        public async Task<List<Rol>> ConsultarRolesAsync(
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT RolID, Nombre
                FROM dbo.Rol
                ORDER BY Nombre;
            """;

            var roles = new List<Rol>();

            await using var connection = new SqlConnection(_connectionString);

            using var command = new SqlCommand(sql, connection);

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                roles.Add(new Rol
                {
                    RolID = reader.GetInt32(reader.GetOrdinal("RolID")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre"))
                });
            }

            return roles;
        }


    }
}
