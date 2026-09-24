using BSC.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BSC.Api.Contracts;

namespace BSC.Api.Controllers
{
    [ApiController]
    [Route("api/pedidos")]
    public class PedidosController : ControllerBase
    {
        private readonly PedidoService _service;

        public PedidosController(PedidoService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Personal administrativo")]
        public async Task<IActionResult> ConsultarPedidos(
            CancellationToken cancellationToken)
        {
            var pedidos = await _service.ConsultarPedidosAsync(cancellationToken);

            return Ok(pedidos);
        }

        [HttpPost]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> CrearPedido(
            [FromBody] CrearPedidoRequest request,
            CancellationToken cancellationToken)
        {
            // obtiene el id del usuario en sesion
            var identificador = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            // verifica que el id sea valido, si lo es, lo almacena en vendedorId
            if (!int.TryParse(identificador, out var vendedorId)
                || vendedorId <= 0)
            {
                return Unauthorized();
            }

            try
            {
                var pedidoId = await _service.ProcesarPedidoAsync(
                    vendedorId,
                    request.Cliente,
                    request.Productos,
                    cancellationToken);

                return StatusCode(
                    StatusCodes.Status201Created,
                    new { pedidoId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
