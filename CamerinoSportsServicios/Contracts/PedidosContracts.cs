using System.ComponentModel.DataAnnotations;

namespace CamerinoSportsServicios.Contracts;

public class CrearPedidoRequest
{
    public int UsuarioId { get; set; }

    [Required, StringLength(150, MinimumLength = 2)]
    [RegularExpression(@"^[\p{L}]+(?:[ '\-][\p{L}]+)*$",
        ErrorMessage = "El nombre del cliente solo puede contener letras.")]
    public string Cliente { get; set; } = "";

    [Required]
    [RegularExpression(@"^9\d{8}$", ErrorMessage = "El teléfono debe tener 9 dígitos y empezar en 9.")]
    public string Telefono { get; set; } = "";

    [EmailAddress]
    public string? Correo { get; set; }

    [Required, StringLength(200, MinimumLength = 5)]
    public string Direccion { get; set; } = "";

    [StringLength(250)]
    public string? Referencia { get; set; }

    [StringLength(500)]
    public string? Comentario { get; set; }
    public double Latitud { get; set; }
    public double Longitud { get; set; }

    [Required, RegularExpression("^(Yape|Tarjeta)$")]
    public string MetodoPago { get; set; } = "";

    [Required, RegularExpression("^Pagado$")]
    public string EstadoPago { get; set; } = "";

    public string? CodigoOperacionPago { get; set; }

    [MinLength(1)]
    public List<CrearPedidoDetalleRequest> Detalles { get; set; } = [];
}

public class CrearPedidoDetalleRequest
{
    // productos usa valuegeneratednever; el identificador 0 es valido.
    [Range(0, int.MaxValue)]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }
}

public record CambiarEstadoPedidoRequest([Required] string Estado);
