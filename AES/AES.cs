using System.Security.Cryptography;

public class AES_
{
    // https://en.wikipedia.org/wiki/Rijndael_S-box

    private static readonly byte[] SBox =
    {
        0x63, 0x7C, 0x77, 0x7B, 0xF2, 0x6B, 0x6F, 0xC5, 0x30, 0x01, 0x67, 0x2B, 0xFE, 0xD7, 0xAB, 0x76,
        0xCA, 0x82, 0xC9, 0x7D, 0xFA, 0x59, 0x47, 0xF0, 0xAD, 0xD4, 0xA2, 0xAF, 0x9C, 0xA4, 0x72, 0xC0,
        0xB7, 0xFD, 0x93, 0x26, 0x36, 0x3F, 0xF7, 0xCC, 0x34, 0xA5, 0xE5, 0xF1, 0x71, 0xD8, 0x31, 0x15,
        0x04, 0xC7, 0x23, 0xC3, 0x18, 0x96, 0x05, 0x9A, 0x07, 0x12, 0x80, 0xE2, 0xEB, 0x27, 0xB2, 0x75,
        0x09, 0x83, 0x2C, 0x1A, 0x1B, 0x6E, 0x5A, 0xA0, 0x52, 0x3B, 0xD6, 0xB3, 0x29, 0xE3, 0x2F, 0x84,
        0x53, 0xD1, 0x00, 0xED, 0x20, 0xFC, 0xB1, 0x5B, 0x6A, 0xCB, 0xBE, 0x39, 0x4A, 0x4C, 0x58, 0xCF,
        0xD0, 0xEF, 0xAA, 0xFB, 0x43, 0x4D, 0x33, 0x85, 0x45, 0xF9, 0x02, 0x7F, 0x50, 0x3C, 0x9F, 0xA8,
        0x51, 0xA3, 0x40, 0x8F, 0x92, 0x9D, 0x38, 0xF5, 0xBC, 0xB6, 0xDA, 0x21, 0x10, 0xFF, 0xF3, 0xD2,
        0xCD, 0x0C, 0x13, 0xEC, 0x5F, 0x97, 0x44, 0x17, 0xC4, 0xA7, 0x7E, 0x3D, 0x64, 0x5D, 0x19, 0x73,
        0x60, 0x81, 0x4F, 0xDC, 0x22, 0x2A, 0x90, 0x88, 0x46, 0xEE, 0xB8, 0x14, 0xDE, 0x5E, 0x0B, 0xDB,
        0xE0, 0x32, 0x3A, 0x0A, 0x49, 0x06, 0x24, 0x5C, 0xC2, 0xD3, 0xAC, 0x62, 0x91, 0x95, 0xE4, 0x79,
        0xE7, 0xC8, 0x37, 0x6D, 0x8D, 0xD5, 0x4E, 0xA9, 0x6C, 0x56, 0xF4, 0xEA, 0x65, 0x7A, 0xAE, 0x08,
        0xBA, 0x78, 0x25, 0x2E, 0x1C, 0xA6, 0xB4, 0xC6, 0xE8, 0xDD, 0x74, 0x1F, 0x4B, 0xBD, 0x8B, 0x8A,
        0x70, 0x3E, 0xB5, 0x66, 0x48, 0x03, 0xF6, 0x0E, 0x61, 0x35, 0x57, 0xB9, 0x86, 0xC1, 0x1D, 0x9E,
        0xE1, 0xF8, 0x98, 0x11, 0x69, 0xD9, 0x8E, 0x94, 0x9B, 0x1E, 0x87, 0xE9, 0xCE, 0x55, 0x28, 0xDF,
        0x8C, 0xA1, 0x89, 0x0D, 0xBF, 0xE6, 0x42, 0x68, 0x41, 0x99, 0x2D, 0x0F, 0xB0, 0x54, 0xBB, 0x16
    };

    private static readonly byte[] InvSBox = {
        0x52, 0x09, 0x6A, 0xD5, 0x30, 0x36, 0xA5, 0x38, 0xBF, 0x40, 0xA3, 0x9E, 0x81, 0xF3, 0xD7, 0xFB,
        0x7C, 0xE3, 0x39, 0x82, 0x9B, 0x2F, 0xFF, 0x87, 0x34, 0x8E, 0x43, 0x44, 0xC4, 0xDE, 0xE9, 0xCB,
        0x54, 0x7B, 0x94, 0x32, 0xA6, 0xC2, 0x23, 0x3D, 0xEE, 0x4C, 0x95, 0x0B, 0x42, 0xFA, 0xC3, 0x4E,
        0x08, 0x2E, 0xA1, 0x66, 0x28, 0xD9, 0x24, 0xB2, 0x76, 0x5B, 0xA2, 0x49, 0x6D, 0x8B, 0xD1, 0x25,
        0x72, 0xF8, 0xF6, 0x64, 0x86, 0x68, 0x98, 0x16, 0xD4, 0xA4, 0x5C, 0xCC, 0x5D, 0x65, 0xB6, 0x92,
        0x6C, 0x70, 0x48, 0x50, 0xFD, 0xED, 0xB9, 0xDA, 0x5E, 0x15, 0x46, 0x57, 0xA7, 0x8D, 0x9D, 0x84,
        0x90, 0xD8, 0xAB, 0x00, 0x8C, 0xBC, 0xD3, 0x0A, 0xF7, 0xE4, 0x58, 0x05, 0xB8, 0xB3, 0x45, 0x06,
        0xD0, 0x2C, 0x1E, 0x8F, 0xCA, 0x3F, 0x0F, 0x02, 0xC1, 0xAF, 0xBD, 0x03, 0x01, 0x13, 0x8A, 0x6B,
        0x3A, 0x91, 0x11, 0x41, 0x4F, 0x67, 0xDC, 0xEA, 0x97, 0xF2, 0xCF, 0xCE, 0xF0, 0xB4, 0xE6, 0x73,
        0x96, 0xAC, 0x74, 0x22, 0xE7, 0xAD, 0x35, 0x85, 0xE2, 0xF9, 0x37, 0xE8, 0x1C, 0x75, 0xDF, 0x6E,
        0x47, 0xF1, 0x1A, 0x71, 0x1D, 0x29, 0xC5, 0x89, 0x6F, 0xB7, 0x62, 0x0E, 0xAA, 0x18, 0xBE, 0x1B,
        0xFC, 0x56, 0x3E, 0x4B, 0xC6, 0xD2, 0x79, 0x20, 0x9A, 0xDB, 0xC0, 0xFE, 0x78, 0xCD, 0x5A, 0xF4,
        0x1f, 0xdd, 0xa8, 0x33, 0x88, 0x07, 0xc7, 0x31, 0xb1, 0x12, 0x10, 0x59, 0x27, 0x80, 0xec, 0x5f,
        0x60, 0x51, 0x7f, 0xa9, 0x19, 0xb5, 0x4a, 0x0d, 0x2d, 0xe5, 0x7a, 0x9f, 0x93, 0xc9, 0x9c, 0xef,
        0xa0, 0xe0, 0x3b, 0x4d, 0xae, 0x2a, 0xf5, 0xb0, 0xc8, 0xeb, 0xbb, 0x3c, 0x83, 0x53, 0x99, 0x61,
        0x17, 0x2b, 0x04, 0x7e, 0xba, 0x77, 0xd6, 0x26, 0xe1, 0x69, 0x14, 0x63, 0x55, 0x21, 0x0c, 0x7d
    };

    // https://captanu.wordpress.com/2015/04/25/aes/

    private static readonly byte[] RoundConstants = {
        0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1B, 0x36
    };

    private static void SubBytes(byte[] state)
    {
        for (int i = 0; i < 16; i++)
        {
            state[i] = SBox[state[i]];
        }
    }

    private static void ShiftRows(byte[] state)
    {
        byte tmp;

        // Row 1
        tmp = state[1];
        state[1] = state[5];
        state[5] = state[9];
        state[9] = state[13];
        state[13] = tmp;

        // Row 2
        tmp = state[2];
        state[2] = state[10];
        state[10] = tmp;
        tmp = state[6];
        state[6] = state[14];
        state[14] = tmp;

        // Row 3
        tmp = state[15];
        state[15] = state[11];
        state[11] = state[7];
        state[7] = state[3];
        state[3] = tmp;
    }

    private static void MixColumns(byte[] state)
    {
        for (int i = 0; i < 4; i++)
        {
            byte s0 = state[i];
            byte s1 = state[i + 4];
            byte s2 = state[i + 8];
            byte s3 = state[i + 12];

            state[i] = (byte)(GFMultiply(0x02, s0) ^ GMultiply(0x03, s1) ^ s2 ^ s3);
            state[i + 4] = (byte)(s0 ^ GMultiply(0x02, s1) ^ GMultiply(0x03, s2) ^ s3);
            state[i + 8] = (byte)(s0 ^ s1 ^ GMultiply(0x02, s2) ^ GMultiply(0x03, s3));
            state[i + 12] = (byte)(GMultiply(0x03, s0) ^ s1 ^ s2 ^ GMultiply(0x02, s3));
        }
    }

    private static byte GMultiply(byte a, byte b)
    {
        byte p = 0;
        byte counter;
        byte hi_bit_set;

        for (counter = 0; counter < 8; counter++)
        {
            if ((b & 1) != 0)
            {
                p ^= a;
            }

            hi_bit_set = (byte)(a & 0x80);
            a <<= 1;
            if (hi_bit_set != 0)
            {
                a ^= 0x1B; // irreducible polynomial: x^8 + x^4 + x^3 + x + 1
            }
            b >>= 1;
        }

        return p;
    }

    private static byte GFMultiply(byte a, byte b)
    {
        if (a == 1)
        {
            return b;
        }
        else if (a == 2)
        {
            return GMultiplyBy2(b);
        }
        else if (a == 3)
        {
            return (byte)(GMultiplyBy2(b) ^ b);
        }
        else
        {
            throw new ArgumentException("Invalid argument for GF multiplication.");
        }
    }

    private static byte GMultiplyBy2(byte b)
    {
        if ((b & 0x80) != 0)
        {
            return (byte)((b << 1) ^ 0x1B);
        }
        else
        {
            return (byte)(b << 1);
        }
    }

    private static void AddRoundKey(byte[] state, byte[] roundKey)
    {
        for (int i = 0; i < 16; i++)
        {
            state[i] ^= roundKey[i];
        }
    }

    private static void KeyExpansion(byte[] key, byte[] expandedKey)
    {
        Array.Copy(key, expandedKey, 16);

        for (int i = 4; i < 44; i++)
        {
            byte[] temp = new byte[4];
            Array.Copy(expandedKey, (i - 1) * 4, temp, 0, 4);

            if (i % 4 == 0)
            {
                temp = SubWord(RotWord(temp));
                temp[0] ^= RoundConstants[i / 4 - 1];
            }

            for (int j = 0; j < 4; j++)
            {
                expandedKey[i * 4 + j] = (byte)(expandedKey[(i - 4) * 4 + j] ^ temp[j]);
            }
        }
    }

    private static byte[] SubWord(byte[] word)
    {
        byte[] result = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            result[i] = SBox[word[i]];
        }
        return result;
    }

    private static byte[] RotWord(byte[] word)
    {
        byte[] result = new byte[4];
        result[0] = word[1];
        result[1] = word[2];
        result[2] = word[3];
        result[3] = word[0];
        return result;
    }

    private static void EncryptBlock(byte[] block, byte[] expandedKey)
    {
        AddRoundKey(block, expandedKey);

        for (int round = 1; round < 10; round++)
        {
            SubBytes(block);
            ShiftRows(block);
            MixColumns(block);
            AddRoundKey(block, expandedKey, round * 16);
        }

        SubBytes(block);
        ShiftRows(block);
        AddRoundKey(block, expandedKey, 160);
    }

    private static void DecryptBlock(byte[] block, byte[] expandedKey)
    {
        AddRoundKey(block, expandedKey, 160);

        for (int round = 9; round > 0; round--)
        {
            InvShiftRows(block);
            InvSubBytes(block);
            AddRoundKey(block, expandedKey, round * 16);
            InvMixColumns(block);
        }

        InvShiftRows(block);
        InvSubBytes(block);
        AddRoundKey(block, expandedKey);
    }

    private static void AddRoundKey(byte[] block, byte[] roundKey, int offset = 0)
    {
        for (int i = 0; i < 16; i++)
        {
            block[i] ^= roundKey[offset + i];
        }
    }

    private static void InvSubBytes(byte[] state)
    {
        for (int i = 0; i < 16; i++)
        {
            state[i] = InvSBox[state[i]];
        }
    }

    private static void InvShiftRows(byte[] state)
    {
        byte tmp;

        // Row 1
        tmp = state[13];
        state[13] = state[9];
        state[9] = state[5];
        state[5] = state[1];
        state[1] = tmp;

        // Row 2
        tmp = state[2];
        state[2] = state[10];
        state[10] = tmp;
        tmp = state[6];
        state[6] = state[14];
        state[14] = tmp;

        // Row 3
        tmp = state[3];
        state[3] = state[7];
        state[7] = state[11];
        state[11] = state[15];
        state[15] = tmp;
    }

    private static void InvMixColumns(byte[] state)
    {
        for (int i = 0; i < 4; i++)
        {
            byte s0 = state[i];
            byte s1 = state[i + 4];
            byte s2 = state[i + 8];
            byte s3 = state[i + 12];

            state[i] = (byte)(GFMultiply(0x0E, s0) ^ GFMultiply(0x0B, s1) ^ GFMultiply(0x0D, s2) ^ GFMultiply(0x09, s3));
            state[i + 4] = (byte)(GFMultiply(0x09, s0) ^ GFMultiply(0x0E, s1) ^ GFMultiply(0x0B, s2) ^ GFMultiply(0x0D, s3));
            state[i + 8] = (byte)(GFMultiply(0x0D, s0) ^ GFMultiply(0x09, s1) ^ GFMultiply(0x0E, s2) ^ GFMultiply(0x0B, s3));
            state[i + 12] = (byte)(GFMultiply(0x0B, s0) ^ GFMultiply(0x0D, s1) ^ GFMultiply(0x09, s2) ^ GFMultiply(0x0E, s3));
        }
    }

    public static byte[] Encrypt(byte[] plaintext, byte[] key)
    {
        byte[] ciphertext = new byte[plaintext.Length];

        byte[] expandedKey = new byte[176];
        KeyExpansion(key, expandedKey);

        for (int i = 0; i < plaintext.Length; i += 16)
        {
            byte[] block = new byte[16];
            Array.Copy(plaintext, i, block, 0, 16);
            EncryptBlock(block, expandedKey);
            Array.Copy(block, 0, ciphertext, i, 16);
        }                                                                                                                                                                                   // Aes aes = Aes.Create(); aes.DecryptCbc();

        return ciphertext;
    }

    public static byte[] Decrypt(byte[] ciphertext, byte[] key)
    {
        byte[] plaintext = new byte[ciphertext.Length];

        byte[] expandedKey = new byte[176];
        KeyExpansion(key, expandedKey);

        for (int i = 0; i < ciphertext.Length; i += 16)
        {
            byte[] block = new byte[16];
            Array.Copy(ciphertext, i, block, 0, 16);
            DecryptBlock(block, expandedKey);
            Array.Copy(block, 0, plaintext, i, 16);
        }

        return plaintext;
    }
}