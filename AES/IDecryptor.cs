using System.Collections;

namespace AES;

internal interface IDecryptor
{

    byte[] Decrypt(byte[] ciphertext);
}