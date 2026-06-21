using CamerinoSportsServicios.Models;
using CamerinoSportsServicios.Services;
using Microsoft.EntityFrameworkCore;

namespace CamerinoSportsServicios.Data;

public static class DataSeeder
{
    public static async Task InicializarAsync(
        CamerinoSportsContext context,
        PasswordService passwordService)
    {
        const string correoAdmin = "admin@camerinosports.com";

        if (!await context.Usuarios.AnyAsync(usuario => usuario.Correo == correoAdmin))
        {
            context.Usuarios.Add(new Usuario
            {
                Nombre = "Administrador",
                Apellidos = "Camerino Sports",
                Correo = correoAdmin,
                PasswordHash = passwordService.CrearHash("123456"),
                Rol = "admin"
            });
        }

        if (!await context.Consejos.AnyAsync())
        {
            context.Consejos.AddRange(
                CrearConsejo(
                    "Consumo responsable",
                    "Pequeños cuidados ayudan a conservar mejor tu ropa y calzado deportivo.",
                    "Cuidado general"),
                CrearConsejo(
                    "Lava con agua fría",
                    "Evita usar agua caliente para conservar mejor los colores y las fibras.",
                    "Lavado"),
                CrearConsejo(
                    "Seca a la sombra",
                    "La luz directa del sol puede desgastar los colores y dañar algunos materiales.",
                    "Secado"),
                CrearConsejo(
                    "No uses blanqueador",
                    "Los químicos fuertes pueden deteriorar las telas deportivas y los estampados.",
                    "Lavado"),
                CrearConsejo(
                    "Limpia tus zapatillas",
                    "Retira polvo y barro después de usarlas para mantenerlas en buen estado.",
                    "Calzado"));
        }

        await context.SaveChangesAsync();
    }

    private static Consejo CrearConsejo(string titulo, string descripcion, string categoria) =>
        new()
        {
            Titulo = titulo,
            Descripcion = descripcion,
            Categoria = categoria,
            Activo = true
        };
}
