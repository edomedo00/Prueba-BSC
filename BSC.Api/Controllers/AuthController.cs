using System.Security.Claims;
using BSC.Api.Contracts;
using BSC.Business.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace BSC.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly UsuarioService _service;

        public AuthController(UsuarioService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            var usuario = await _service.ValidarCredencialesAsync(
                request.NombreUsuario,
                request.Contrasena,
                cancellationToken);

            if (usuario is null)
            {
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
            }

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.UsuarioID.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
               CookieAuthenticationDefaults.AuthenticationScheme,
               new ClaimsPrincipal(identity));

            return Ok(new
            {
                usuario.NombreUsuario,
                usuario.Rol
            });

        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return NoContent();
        }


        [Authorize]
        [HttpGet("me")]
        public IActionResult ObtenerSesion()
        {
            return Ok(
                new
                {
                    usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                    nombreUsuario = User.Identity!.Name,
                    rol = User.FindFirstValue(ClaimTypes.Role)
                }
            );
        }
    }

}
