using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CamerinoSportsServicios.Models;

public class Pedido
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }

    [Required, StringLength(220)]
    public string Cliente { get; set; } = "";

    [Required, StringLength(30)]
    public string Telefono { get; set; } = "";

    [StringLength(180)]
    public string? Correo { get; set; }

    [Required, StringLength(300)]
    public string Direccion { get; set; } = "";

    [StringLength(300)]
    public string? Referencia { get; set; }

    [Required, StringLength(30)]
    public string Fecha { get; set; } = "";

    [Range(0.01, double.MaxValue)]
    public double Total { get; set; }

    [Required, StringLength(40)]
    public string Estado { get; set; } = "Pedido confirmado";

    [StringLength(500)]
    public string? Comentario { get; set; }

    public double Latitud { get; set; }
    public double Longitud { get; set; }

    [Required, StringLength(40)]
    public string MetodoPago { get; set; } = "";

    [Required, StringLength(30)]
    public string EstadoPago { get; set; } = "Pendiente";

    [StringLength(100)]
    public string? CodigoOperacionPago { get; set; }

    [JsonIgnore]
    public Usuario? Usuario { get; set; }

    public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
}
