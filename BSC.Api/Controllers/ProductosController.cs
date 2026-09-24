using BSC.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BSC.Api.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace BSC.Api.Controllers
{
    [ApiController]
    [Route("api/productos")]
    public class ProductosController : ControllerBase
    {
        private readonly ProductoService _service;

        public ProductosController(ProductoService service) {
            _service = service;
        }

        [HttpGet("inventario")]
        [Authorize(Roles = "Personal administrativo,Vendedor")]
        public async Task<IActionResult> ConsultarInventario (
            CancellationToken cancellationToken)
        {
            var productos = await _service.ConsultarInventarioAsync(cancellationToken);

            return Ok(productos);
        }


        [HttpPost]
        [Authorize(Roles = "Personal administrativo")]
        public async Task<IActionResult> RegistrarProducto(
            [FromBody] RegistrarProductoRequest request,
            CancellationToken cancellationToken)
        {

            try
            {
                await _service.RegistrarProductoAsync(
                    request.Nombre,
                    request.Inventario,
                    request.ClaveProducto,
                    cancellationToken);

                return StatusCode(StatusCodes.Status201Created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("{productoId:int}/entradas")]
        [Authorize(Roles = "Personal administrativo")]
        public async Task<IActionResult> RegistrarEntrada(
            int productoId,
            [FromBody] RegistrarInventarioRequest request,
            CancellationToken cancellationToken)
        {

            try
            {
                await _service.RegistrarInventarioAsync(
                    productoId,
                    request.Cantidad,
                    cancellationToken);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }



    }
}
