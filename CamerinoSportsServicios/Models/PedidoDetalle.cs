using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CamerinoSportsServicios.Models;

public class PedidoDetalle
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public int? ProductoId { get; set; }

    [Required, StringLength(150)]
    public string NombreProducto { get; set; } = "";

    [StringLength(2000)]
    public string? ImagenProducto { get; set; }

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }

    [Range(0.01, double.MaxValue)]
    public double Precio { get; set; }

    [Range(0.01, double.MaxValue)]
    public double Subtotal { get; set; }

    [JsonIgnore]
    public Pedido Pedido { get; set; } = null!;

    [JsonIgnore]
    public Producto? Producto { get; set; }
}
