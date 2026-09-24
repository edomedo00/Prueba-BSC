using BSC.DataAccess.Models;
using BSC.DataAccess.Respositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSC.Business.Services
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _repository;
        private readonly PasswordHasher<Usuario> _passwordHasher = new();

        public UsuarioService ( UsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task CrearUsuarioAsync (
            string nombreUsuario,
            string contrasena,
            int rolId, 
            CancellationToken cancellationToken = default )
        {
            if(string.IsNullOrWhiteSpace(nombreUsuario)) 
                throw new ArgumentException("El nombre de usuario es obligatorio.");

            nombreUsuario = nombreUsuario.Trim();

            if (nombreUsuario.Length > 100) 
                throw new ArgumentException("El nombre de usuario no puede ser mayor a 100 caracteres.");

            if (string.IsNullOrWhiteSpace(contrasena)
                || contrasena.Length < 10
                || !contrasena.Any(char.IsLetter)
                || !contrasena.Any(char.IsDigit))
            {
                throw new ArgumentException("La contraseña debe tener al menos 10 caracteres, una letra y un número.");
            }

            if (rolId <= 0)
                throw new ArgumentException("Selecciona un rol válido.");

            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario
            };

            var hash = _passwordHasher.HashPassword(usuario, contrasena);

            await _repository.CrearUsuarioAsync(
                nombreUsuario,
                hash,
                rolId,
                cancellationToken
            );

        }


        public async Task<Usuario?> ValidarCredencialesAsync(
            string nombreUsuario,
            string contrasena,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrEmpty(contrasena))
            {
                return null;
            }

            var usuario = await _repository.ObtenerPorNombreAsync( nombreUsuario.Trim(),cancellationToken);

            if (usuario is null) return null;

            var resultado = _passwordHasher.VerifyHashedPassword( 
                usuario, 
                usuario.ContrasenaHash, 
                contrasena);

            if (resultado == PasswordVerificationResult.Failed)
                return null;

            return usuario;
        }

        public Task<List<Rol>> ConsultarRolesAsync(
            CancellationToken cancellationToken = default)
        {
            return _repository.ConsultarRolesAsync(cancellationToken);
        }

    }
}
