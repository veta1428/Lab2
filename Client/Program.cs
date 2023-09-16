using System.Security.Cryptography;
using System.Text;

namespace Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        HttpClient client = new HttpClient();
        var rsa = RSA.Create();
        var public_key = rsa.ExportRSAPublicKey();
        var encodedPK = Encoding.Unicode.GetString(public_key);
        var answ = await client.GetAsync($"https://localhost:7164/api/session/{Encoding.Unicode.GetString(public_key)}");
        Console.WriteLine("Hello, World!");
    }
}
