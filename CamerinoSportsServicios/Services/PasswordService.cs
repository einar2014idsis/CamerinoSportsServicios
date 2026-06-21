using System.Security.Cryptography;

namespace CamerinoSportsServicios.Services;

public class PasswordService
{
    private const int Iteraciones = 100_000;
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;

    public string CrearHash(string password)
    {
        // la sal aleatoria hace que dos contraseñas iguales produzcan resultados distintos
        byte[] sal = RandomNumberGenerator.GetBytes(TamanoSal);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            sal,
            Iteraciones,
            HashAlgorithmName.SHA256,
            TamanoHash);

        return $"PBKDF2-SHA256${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string password, string hashGuardado)
    {
        // el texto guardado contiene el algoritmo, las iteraciones, la sal y el hash
        string[] partes = hashGuardado.Split('$');
        if (partes.Length != 4 ||
            partes[0] != "PBKDF2-SHA256" ||
            !int.TryParse(partes[1], out int iteraciones))
        {
            return false;
        }

        try
        {
            byte[] sal = Convert.FromBase64String(partes[2]);
            byte[] hashEsperado = Convert.FromBase64String(partes[3]);
            byte[] hashActual = Rfc2898DeriveBytes.Pbkdf2(
                password,
                sal,
                iteraciones,
                HashAlgorithmName.SHA256,
                hashEsperado.Length);

            // esta comparacion evita revelar diferencias de tiempo entre los valores
            return CryptographicOperations.FixedTimeEquals(hashActual, hashEsperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
