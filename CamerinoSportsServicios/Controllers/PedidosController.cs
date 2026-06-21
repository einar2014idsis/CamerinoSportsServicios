using CamerinoSportsServicios.Contracts;
using CamerinoSportsServicios.Data;
using CamerinoSportsServicios.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CamerinoSportsServicios.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    // limita los estados que puede recibir el servidor
    private static readonly HashSet<string> EstadosPermitidos =
    [
        "Pedido confirmado",
        "Preparando pedido",
        "En camino",
        "Entregado",
        "Cancelado"
    ];

    private readonly CamerinoSportsContext _context;

    public PedidosController(CamerinoSportsContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Pedido>>> GetPedidos()
    {
        return await _context.Pedidos
            .AsNoTracking()
            .Include(pedido => pedido.Detalles)
            .OrderBy(pedido => pedido.Estado == "Pedido confirmado" ? 0 : 1)
            .ThenByDescending(pedido => pedido.Id)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
        var pedido = await _context.Pedidos
            .AsNoTracking()
            .Include(item => item.Detalles)
            .FirstOrDefaultAsync(item => item.Id == id);

        return pedido == null ? NotFound() : pedido;
    }

    [HttpGet("usuario/{usuarioId:int}")]
    public async Task<ActionResult<List<Pedido>>> GetPedidosUsuario(int usuarioId)
    {
        return await _context.Pedidos
            .AsNoTracking()
            .Include(pedido => pedido.Detalles)
            .Where(pedido => pedido.UsuarioId == usuarioId)
            .OrderByDescending(pedido => pedido.Id)
            .ToListAsync();
    }

    [HttpGet("{id:int}/detalles")]
    public async Task<ActionResult<List<PedidoDetalle>>> GetDetalles(int id)
    {
        if (!await _context.Pedidos.AnyAsync(pedido => pedido.Id == id))
        {
            return NotFound();
        }

        return await _context.PedidoDetalles.AsNoTracking()
            .Where(detalle => detalle.PedidoId == id)
            .OrderBy(detalle => detalle.Id)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Pedido>> CrearPedido(CrearPedidoRequest request)
    {
        if (request.Detalles.Count == 0)
        {
            return BadRequest("El pedido debe contener al menos un producto.");
        }

        int? usuarioId = null;
        // primero se intenta relacionar el pedido con el correo de la sesion
        if (!string.IsNullOrWhiteSpace(request.Correo))
        {
            string correo = request.Correo.Trim().ToLowerInvariant();
            usuarioId = await _context.Usuarios
                .Where(usuario => usuario.Correo == correo)
                .Select(usuario => (int?)usuario.Id)
                .FirstOrDefaultAsync();
        }
        else if (request.UsuarioId > 0 &&
                 await _context.Usuarios.AnyAsync(usuario => usuario.Id == request.UsuarioId))
        {
            usuarioId = request.UsuarioId;
        }

        // la transaccion evita guardar el pedido si falla el stock de algun producto
        await using var transaccion = await _context.Database.BeginTransactionAsync();

        var pedido = new Pedido
        {
            UsuarioId = usuarioId,
            Cliente = request.Cliente.Trim(),
            Telefono = request.Telefono.Trim(),
            Correo = request.Correo?.Trim().ToLowerInvariant(),
            Direccion = request.Direccion.Trim(),
            Referencia = request.Referencia?.Trim(),
            Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            Estado = "Pedido confirmado",
            Comentario = request.Comentario?.Trim(),
            Latitud = request.Latitud,
            Longitud = request.Longitud,
            MetodoPago = request.MetodoPago.Trim(),
            EstadoPago = request.EstadoPago.Trim(),
            CodigoOperacionPago = request.CodigoOperacionPago?.Trim()
        };

        double total = 0;
        foreach (var solicitud in request.Detalles)
        {
            // el precio y el stock siempre se toman de la base de datos
            var producto = await _context.Productos.FirstOrDefaultAsync(item => item.Id == solicitud.ProductoId);
            if (producto == null)
            {
                return BadRequest($"No existe el producto {solicitud.ProductoId}.");
            }

            if (producto.Stock < solicitud.Cantidad)
            {
                return Conflict($"Stock insuficiente para {producto.Nombre}.");
            }

            double subtotal = producto.Precio * solicitud.Cantidad;
            pedido.Detalles.Add(new PedidoDetalle
            {
                ProductoId = producto.Id,
                NombreProducto = producto.Nombre,
                ImagenProducto = producto.Imagen,
                Cantidad = solicitud.Cantidad,
                Precio = producto.Precio,
                Subtotal = subtotal
            });

            producto.Stock -= solicitud.Cantidad;
            total += subtotal;
        }

        pedido.Total = total;
        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();
        await transaccion.CommitAsync();

        return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
    }

    [HttpGet("correo/{correo}")]
    public async Task<ActionResult<List<Pedido>>> GetPedidosCorreo(string correo)
    {
        string correoNormalizado = correo.Trim().ToLowerInvariant();
        return await _context.Pedidos
            .AsNoTracking()
            .Include(pedido => pedido.Detalles)
            .Where(pedido => pedido.Correo == correoNormalizado)
            .OrderByDescending(pedido => pedido.Id)
            .ToListAsync();
    }

    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoPedidoRequest request)
    {
        // se rechaza cualquier estado que no forme parte del flujo del pedido
        if (!EstadosPermitidos.Contains(request.Estado))
        {
            return BadRequest("Estado de pedido no permitido.");
        }

        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
        {
            return NotFound();
        }

        if (pedido.Estado == "Cancelado")
        {
            return Conflict("Un pedido cancelado no puede cambiar de estado.");
        }

        if (request.Estado == "Cancelado")
        {
            return await CancelarPedido(id);
        }

        pedido.Estado = request.Estado;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/cancelar")]
    public async Task<IActionResult> CancelarPedido(int id)
    {
        // al cancelar se devuelve al inventario la cantidad de cada producto
        await using var transaccion = await _context.Database.BeginTransactionAsync();
        var pedido = await _context.Pedidos
            .Include(item => item.Detalles)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (pedido == null)
        {
            return NotFound();
        }

        if (pedido.Estado != "Pedido confirmado")
        {
            return Conflict("Solo se puede cancelar un pedido confirmado.");
        }

        foreach (var detalle in pedido.Detalles)
        {
            if (detalle.ProductoId == null)
            {
                continue;
            }

            var producto = await _context.Productos.FindAsync(detalle.ProductoId);
            if (producto != null)
            {
                producto.Stock += detalle.Cantidad;
            }
        }

        pedido.Estado = "Cancelado";
        await _context.SaveChangesAsync();
        await transaccion.CommitAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarPedido(int id)
    {
        var pedido = await _context.Pedidos
            .Include(item => item.Detalles)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (pedido == null)
        {
            return NotFound();
        }

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
