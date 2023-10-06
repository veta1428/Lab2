using System.Security.Cryptography;
using System.Text;
using Serpent;

namespace Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        string test = "Test text ffffff";
        var key = Encoding.ASCII.GetBytes("ABCDEFJWDHWDUHWH");

        Serpent.Serpent s = new Serpent.Serpent();
        var encrypted = s.Encrypt(key, key);
        var decrypted = s.Decrypt(encrypted, key);
        Console.WriteLine(Encoding.ASCII.GetString(decrypted));
    }
}
