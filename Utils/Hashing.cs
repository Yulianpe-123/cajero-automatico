using System.Security.Cryptography;
using System.Text;

namespace CajeroApp.Utils
{
    public static class Hashing
    {
        public static string Sha256(string texto)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(texto);
            var hashBytes = sha.ComputeHash(bytes);

            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // hex string
            }
            return sb.ToString();
        }
    }
}