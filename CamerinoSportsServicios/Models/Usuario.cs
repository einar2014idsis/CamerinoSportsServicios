using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CamerinoSportsServicios.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(120)]
    public string Apellidos { get; set; } = "";

    [Required, EmailAddress, StringLength(180)]
    public string Correo { get; set; } = "";

    [Required, StringLength(500)]
    [JsonIgnore]
    public string PasswordHash { get; set; } = "";

    [Required, StringLength(20)]
    public string Rol { get; set; } = "cliente";

    [JsonIgnore]
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
