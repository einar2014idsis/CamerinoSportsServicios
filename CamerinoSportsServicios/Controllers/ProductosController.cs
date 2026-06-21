using CamerinoSportsServicios.Data;
using CamerinoSportsServicios.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CamerinoSportsServicios.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly CamerinoSportsContext _context;

    public ProductosController(CamerinoSportsContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Producto>>> GetProductos()
    {
        return await _context.Productos
            .AsNoTracking()
            .OrderBy(producto => producto.Nombre)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> GetProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        return producto == null ? NotFound() : producto;
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> PostProducto(Producto producto)
    {
        // si android no envia un id, el servidor asigna el siguiente disponible
        if (producto.Id <= 0)
        {
            var ultimoId = await _context.Productos
                .Select(item => (int?)item.Id)
                .MaxAsync() ?? 0;

            producto.Id = ultimoId + 1;
        }
        else if (await _context.Productos.AnyAsync(item => item.Id == producto.Id))
        {
            return Conflict("Ya existe un producto con ese ID.");
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutProducto(int id, Producto producto)
    {
        var guardado = await _context.Productos.FindAsync(id);
        // permite recuperar un producto que todavia no existe en la base de datos
        if (guardado == null)
        {
            producto.Id = id;
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        guardado.Nombre = producto.Nombre;
        guardado.Descripcion = producto.Descripcion;
        guardado.Categoria = producto.Categoria;
        guardado.Talla = producto.Talla;
        guardado.Color = producto.Color;
        guardado.Precio = producto.Precio;
        guardado.Stock = producto.Stock;
        guardado.Imagen = producto.Imagen;
        guardado.Sostenible = producto.Sostenible;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        // la eliminacion se confirma como una sola operacion
        await using var transaccion = await _context.Database.BeginTransactionAsync();
        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
        await transaccion.CommitAsync();
        return NoContent();
    }
}
