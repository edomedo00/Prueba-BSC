using BSC.Api.Contracts;
using BSC.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BSC.Api.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly UsuarioService _service;

        public UsuariosController(UsuarioService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario(
            [FromBody] CrearUsuarioRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                await _service.CrearUsuarioAsync(
                    request.NombreUsuario,
                    request.Contrasena,
                    request.RolID,
                    cancellationToken);

                return StatusCode(StatusCodes.Status201Created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("roles")]
        public async Task<IActionResult> ConsultarRoles(
            CancellationToken cancellationToken)
        {
            var roles = await _service.ConsultarRolesAsync(cancellationToken);

            return Ok(roles);
        }
    }
}
