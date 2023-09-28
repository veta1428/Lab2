using Serpent;

namespace SerpentConsoleClient;

internal class Program
{
    static void Main(string[] args)
    {
        SerpentCipher cipher = new SerpentCipher();
        //cipher.Encrypt("input.txt", new byte[] {23, 11, 10, 138, 32, 123, 12, 68, 39, 23, 11, 10, 138, 32, 123, 12, 68, 39, 23, 11, 10, 138, 32, 123, 12, 68, 39, 12, 12, 12, 12, 36, 32, 78, 89}, 32, EncryptionMode.CBC);
        cipher.Decrypt("input.serpent", new byte[] { 23, 11, 10, 138, 32, 123, 12, 68, 39, 23, 11, 10, 138, 32, 123, 12, 68, 39, 23, 11, 10, 138, 32, 123, 12, 68, 39, 12, 12, 12, 12, 36, 32, 78, 89 }, 32, Mode.Standard, EncryptionMode.CBC);
    }
}
