using System.Collections;

namespace AES;

internal interface IEncryptor
{
    byte[] Encrypt(string text);
    byte[] Encrypt(byte[] input);
}