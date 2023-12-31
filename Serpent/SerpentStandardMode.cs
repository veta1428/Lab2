﻿using System;

namespace Serpent;

public class SerpentStandardMode : SerpentAlgorithm
{
    public override int BlockSize { get; set; }
    public override int Rounds { get; set; }

    public override object MakeKey(byte[] key)
    {
        var w = new int[4 * (Rounds + 1)]; // Initialize an array to hold the key schedule
        var offset = 0;
        var limit = key.Length / 4; // Determine the number of 32-bit words in the key
        int i, j;

        // Convert the key bytes into 32-bit words
        for (i = 0; i < limit; i++)
            w[i] = (key[offset++] & 0xFF) |
                   ((key[offset++] & 0xFF) << 8) |
                   ((key[offset++] & 0xFF) << 16) |
                   ((key[offset++] & 0xFF) << 24);

        if (i < 8)
            w[i] = 1; // If the key is less than 256 bits, set the remaining words to 1

        int t;
        for (i = 8, j = 0; i < 16; i++)
        {
            t = (int)(w[j] ^ w[i - 5] ^ w[i - 3] ^ w[i - 1] ^ PHI ^ j++);
            w[i] = (t << 11) | (int)((uint)t >> 21); // Key expansion using a nonlinear function and bit shifting
        }

        // Expand the key using a linear function
        for (i = 0, j = 8; i < 8;)
            w[i++] = w[j++];

        limit = 4 * (Rounds + 1);

        for (; i < limit; i++)
        {
            t = (int)(w[i - 8] ^ w[i - 5] ^ w[i - 3] ^ w[i - 1] ^ PHI ^ i);
            w[i] = (t << 11) | (int)((uint)t >> 21); // Key expansion using a nonlinear function and bit shifting
        }

        var k = new int[limit]; // Create a new array to store the expanded key
        for (i = 0; i < Rounds + 1; i++)
        {
            var box = (Rounds + 3 - i) % Rounds;
            var a = w[4 * i];
            var b = w[4 * i + 1];
            var c = w[4 * i + 2];
            var d = w[4 * i + 3];

            for (j = 0; j < 32; j++)
            {
                var inV =
                    GetBit(a, j) |
                    (GetBit(b, j) << 1) |
                    (GetBit(c, j) << 2) |
                    (GetBit(d, j) << 3);
                var outV = S(box, inV); // Apply the S-box substitution
                k[4 * i] |= GetBit(outV, 0) << j;
                k[4 * i + 1] |= GetBit(outV, 1) << j;
                k[4 * i + 2] |= GetBit(outV, 2) << j;
                k[4 * i + 3] |= GetBit(outV, 3) << j;
            }
        }

        var K = new int[Rounds + 1][]; // Create a new array to hold the round keys
        for (var kn = 0; kn < K.Length; kn++)
            K[kn] = new int[4];

        // Split the expanded key into round keys
        for (i = 0, offset = 0; i < Rounds + 1; i++)
        {
            K[i][0] = k[offset++];
            K[i][1] = k[offset++];
            K[i][2] = k[offset++];
            K[i][3] = k[offset++];
        }

        // Apply an initial permutation to each round key
        for (i = 0; i < Rounds + 1; i++)
            K[i] = IP(K[i]);

        return K; // Return the generated round keys
    }

    public override byte[] BlockEncrypt(byte[] inV, int inOffset, object sessionKey)
    {
        var Khat = (int[][]) sessionKey;
        int[] x =
        {
            (inV[inOffset++] & 0xFF) | ((inV[inOffset++] & 0xFF) << 8) |
            ((inV[inOffset++] & 0xFF) << 16) | ((inV[inOffset++] & 0xFF) << 24),
            (inV[inOffset++] & 0xFF) | ((inV[inOffset++] & 0xFF) << 8) |
            ((inV[inOffset++] & 0xFF) << 16) | ((inV[inOffset++] & 0xFF) << 24),
            (inV[inOffset++] & 0xFF) | ((inV[inOffset++] & 0xFF) << 8) |
            ((inV[inOffset++] & 0xFF) << 16) | ((inV[inOffset++] & 0xFF) << 24),
            (inV[inOffset++] & 0xFF) | ((inV[inOffset++] & 0xFF) << 8) |
            ((inV[inOffset++] & 0xFF) << 16) | ((inV[inOffset++] & 0xFF) << 24)
        };
        var Bhat = IP(x);

        for (var i = 0; i < Rounds; i++)
            Bhat = R(i, Bhat, Khat);

        x = FP(Bhat);

        int a = x[0], b = x[1], c = x[2], d = x[3];
        byte[] result =
        {
            (byte) a, (byte) (int) ((uint) a >> 8), (byte) (int) ((uint) a >> 16), (byte) (int) ((uint) a >> 24),
            (byte) b, (byte) (int) ((uint) b >> 8), (byte) (int) ((uint) b >> 16), (byte) (int) ((uint) b >> 24),
            (byte) c, (byte) (int) ((uint) c >> 8), (byte) (int) ((uint) c >> 16), (byte) (int) ((uint) c >> 24),
            (byte) d, (byte) (int) ((uint) d >> 8), (byte) (int) ((uint) d >> 16), (byte) (int) ((uint) d >> 24)
        };
        return result;
    }

    public override byte[] BlockDecrypt(byte[] inV, int inOffset, object sessionKey)
    {
        var Khat = (int[][]) sessionKey;
        int[] x =
        {
            (inV[inOffset++] & 0xFF) | ((inV[inOffset++] & 0xFF) << 8) |
            ((inV[inOffset++] & 0xFF) << 16) | ((inV[inOffset++] & 0xFF) << 24),
            (inV[inOffset++] & 0xFF) | ((inV[inOffset++] & 0xFF) << 8) |
            ((inV[inOffset++] & 0xFF) << 16) | ((inV[inOffset++] & 0xFF) << 24),
            (inV[inOffset++] & 0xFF) | ((inV[inOffset++] & 0xFF) << 8) |
            ((inV[inOffset++] & 0xFF) << 16) | ((inV[inOffset++] & 0xFF) << 24),
            (inV[inOffset++] & 0xFF) | ((inV[inOffset++] & 0xFF) << 8) |
            ((inV[inOffset++] & 0xFF) << 16) | ((inV[inOffset] & 0xFF) << 24)
        };
        var Bhat = FPinverse(x);

        for (var i = Rounds - 1; i >= 0; i--)
            Bhat = Rinverse(i, Bhat, Khat);

        x = IPinverse(Bhat);

        int a = x[0], b = x[1], c = x[2], d = x[3];
        byte[] result =
        {
            (byte) a, (byte) (int) ((uint) a >> 8), (byte) (int) ((uint) a >> 16), (byte) (int) ((uint) a >> 24),
            (byte) b, (byte) (int) ((uint) b >> 8), (byte) (int) ((uint) b >> 16), (byte) (int) ((uint) b >> 24),
            (byte) c, (byte) (int) ((uint) c >> 8), (byte) (int) ((uint) c >> 16), (byte) (int) ((uint) c >> 24),
            (byte) d, (byte) (int) ((uint) d >> 8), (byte) (int) ((uint) d >> 16), (byte) (int) ((uint) d >> 24)
        };

        return result;
    }

    public byte[] BlockDecryptGetP(int inV, int val, int inOffset, object sessionKey)
    {
        var Khat = (int[][]) sessionKey;
        int[] x = { 0, 0, 0, 0 };
        var Bhat = FPinverse(x);
        for (var i = Rounds - 1; i >= 0; i--)
            Bhat = Rinverse(i, Bhat, Khat, inV, val);
        x = IPinverse(Bhat);

        int a = x[0], b = x[1], c = x[2], d = x[3];
        byte[] result =
        {
            (byte) a, (byte) (int) ((uint) a >> 8), (byte) (int) ((uint) a >> 16), (byte) (int) ((uint) a >> 24),
            (byte) b, (byte) (int) ((uint) b >> 8), (byte) (int) ((uint) b >> 16), (byte) (int) ((uint) b >> 24),
            (byte) c, (byte) (int) ((uint) c >> 8), (byte) (int) ((uint) c >> 16), (byte) (int) ((uint) c >> 24),
            (byte) d, (byte) (int) ((uint) d >> 8), (byte) (int) ((uint) d >> 16), (byte) (int) ((uint) d >> 24)
        };

        return result;
    }

    /// <summary>
    /// Extracts the i-th bit from the integer x.
    /// </summary>
    /// <param name="x">Integer to extract from</param>
    /// <param name="i">Index</param>
    /// <returns></returns>
    private int GetBit(int x, int i)
    {
        return (int) ((uint) x >> i) & 0x01;
    }

    /// <summary>
    /// Extracts the i-th bit from the array x.
    /// </summary>
    /// <param name="x">Array of integers to extract from</param>
    /// <param name="i">Index</param>
    /// <returns></returns>
    private int GetBit(int[] x, int i)
    {
        return (int) ((uint) x[i / 32] >> (i % 32)) & 0x01;
    }

    /// <summary>
    /// Sets or clears the i-th bit in the array x based on the value v (1 for set, 0 for clear).
    /// </summary>
    /// <param name="x"></param>
    /// <param name="i"></param>
    /// <param name="v"></param>
    private void SetBit(int[] x, int i, int v)
    {
        if ((v & 0x01) == 1)
            x[i / 32] |= 1 << (i % 32); // set it
        else
            x[i / 32] &= ~(1 << (i % 32)); // clear it
    }

    private static int GetNibble(int x, int i)
    {
        return (int) ((uint) x >> (4 * i)) & 0x0F;
    }

    private int[] IP(int[] x)
    {
        return Permutate(IPtable, x);
    }

    private int[] IPinverse(int[] x)
    {
        return Permutate(FPtable, x);
    }

    private int[] FP(int[] x)
    {
        return Permutate(FPtable, x);
    }

    private int[] FPinverse(int[] x)
    {
        return Permutate(IPtable, x);
    }

    private int[] Permutate(byte[] T, int[] x)
    {
        var result = new int[4];
        for (var i = 0; i < 128; i++)
            SetBit(result, i, GetBit(x, T[i]));
        return result;
    }

    private int[] xor128(int[] x, int[] y)
    {
        return new[] { x[0] ^ y[0], x[1] ^ y[1], x[2] ^ y[2], x[3] ^ y[3] };
    }

    /// <summary>
    /// Performs an S-box substitution on x using the S-box table. The box parameter determines which S-box to use, and x is the input to the S-box.
    /// </summary>
    /// <param name="box">Number of box to use</param>
    /// <param name="x">Input to s-box</param>
    /// <returns></returns>
    private int S(int box, int x)
    {
        return Sbox[box % 32][x] & 0x0F;
    }

    private int Sinverse(int box, int x)
    {
        return SboxInverse[box % 32][x] & 0x0F;
    }

    private int[] Shat(int box, int[] x)
    {
        var result = new int[4];
        for (var i = 0; i < 4; i++)
        for (var nibble = 0; nibble < 8; nibble++)
            result[i] |= S(box, GetNibble(x[i], nibble)) << (nibble * 4);
        return result;
    }

    private int[] ShatInverse(int box, int[] x)
    {
        var result = new int[4];
        for (var i = 0; i < 4; i++)
        for (var nibble = 0; nibble < 8; nibble++)
            result[i] |= Sinverse(box, GetNibble(x[i], nibble)) << (nibble * 4);
        return result;
    }

    private int[] LT(int[] x)
    {
        return Transform(LTtable, x);
    }

    private int[] LTinverse(int[] x)
    {
        return Transform(LTtableInverse, x);
    }

    private int[] Transform(byte[][] T, int[] x)
    {
        int j;
        var result = new int[4];
        for (var i = 0; i < 128; i++)
        {
            var b = 0;
            j = 0;
            while (T[i][j] != xFF)
            {
                b ^= GetBit(x, T[i][j] & 0x7F);
                j++;
            }
            SetBit(result, i, b);
        }

        return result;
    }

    private int[] R(int i, int[] Bhati, int[][] Khat)
    {
        var xored = xor128(Bhati, Khat[i]);
        var Shati = Shat(i, xored);
        int[] BhatiPlus1;
        if (0 <= i && i <= Rounds - 2)
            BhatiPlus1 = LT(Shati);
        else if (i == Rounds - 1)
            BhatiPlus1 = xor128(Shati, Khat[Rounds]);
        else
            throw new Exception(
                "Round " + i + " is out of 0.." + (Rounds - 1) + " range");

        return BhatiPlus1;
    }

    private int[] Rinverse(int i, int[] BhatiPlus1, int[][] Khat)
    {
        int[] Shati;
        if (0 <= i && i <= Rounds - 2)
            Shati = LTinverse(BhatiPlus1);
        else if (i == Rounds - 1)
            Shati = xor128(BhatiPlus1, Khat[Rounds]);
        else
            throw new Exception(
                "Round " + i + " is out of 0.." + (Rounds - 1) + " range");

        var xored = ShatInverse(i, Shati);
        var Bhati = xor128(xored, Khat[i]);

        return Bhati;
    }

    private int[] Rinverse(int i, int[] BhatiPlus1, int[][] Khat, int inV, int val)
    {
        int[] Shati;

        if (0 <= i && i <= Rounds - 2)
            Shati = LTinverse(BhatiPlus1);
        else if (i == Rounds - 1)
            Shati = xor128(BhatiPlus1, Khat[Rounds]);
        else
            throw new Exception(
                "Round " + i + " is out of 0.." + (Rounds - 1) + " range");

        var xored = ShatInverse(i, Shati);
        if (i == inV)
        {
            xored[0] = val | (val << 4);
            xored[0] |= xored[0] << 8;
            xored[0] |= xored[0] << 16;
            xored[1] = xored[2] = xored[3] = xored[0];
        }
        var Bhati = xor128(xored, Khat[i]);

        return Bhati;
    }
}
