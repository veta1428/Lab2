using System.Security.Cryptography;
using System.Text;

namespace Serpent;

public class SerpentCipher
{
    private const int BlockSize = 16;
    private const int DefaultKeySize = 32;
    private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

    public EncryptionResult Encrypt(string FilePath, byte[] Key, int Rounds, EncryptionMode EncrMode)
    {
        var fm = new FileManagement();
        SerpentAlgorithm sa = new SerpentStandardMode(); ;
        var saltBytes = new byte[BlockSize];
        rng.GetNonZeroBytes(saltBytes);
        var iv = new byte[BlockSize];
        rng.GetBytes(iv);

        var position = 0;
        var destFilePath = Path.ChangeExtension(FilePath, ".serpent");
        var fi = new FileInfo(FilePath);
        var fragmentSize = 1024 * 1024;
        var tempFilePath = FilePath + ".temp";
        var tempFilePath2 = FilePath + ".temp2";
        Encoding enc = new UTF8Encoding();
        sa.Rounds = Rounds;
        sa.BlockSize = BlockSize;
        var previousBlock = Array.Empty<byte>();

        var roundsBytes = enc.GetBytes(Rounds.ToString());

        for (var i = 0; i < roundsBytes.Length; i++)
        {
            saltBytes[i] = roundsBytes[i];

            if (i >= roundsBytes.Length - 1)
                saltBytes[i + 1] = 3;
        }

        var expandedKey = sa.MakeKey(Key);

        var leadingBytes = new byte[BlockSize - fi.Length % BlockSize];
        rng.GetBytes(leadingBytes);
        leadingBytes[0] = (byte)leadingBytes.Length;

        fm.UnshiftBytesToFile(FilePath, tempFilePath, leadingBytes, out var errorCode);

        if (errorCode == ErrorCode.ExpandFileFailed)
        {
            return EncryptionResult.Fail("File couldn't be modified.");
        }

        do
        {
            if (position == 0)
            {
                var infoBytes = new List<byte>();

                infoBytes.AddRange(saltBytes);

                var extension = Path.GetExtension(FilePath);
                extension = extension.Replace(".", "");
                infoBytes.AddRange(enc.GetBytes(extension));
                infoBytes.Add(3);
                infoBytes.AddRange(enc.GetBytes(Rounds.ToString()));
                infoBytes.Add(3);

                if (BlockSize < infoBytes.Count - saltBytes.Length)
                {
                    return EncryptionResult.Fail("FIle extension is too long.");
                }

                var rndBytes = new byte[BlockSize - (infoBytes.Count - saltBytes.Length)];
                rng.GetNonZeroBytes(rndBytes);
                infoBytes.AddRange(rndBytes);

                fm.UnshiftBytesToFile(tempFilePath, tempFilePath2, infoBytes.ToArray(), out errorCode);

                if (errorCode == ErrorCode.ExpandFileFailed)
                {
                    return EncryptionResult.Fail("File couldn't be modified.");
                }

                fm.DeleteTempFile(tempFilePath, out errorCode);

                if (errorCode == ErrorCode.DeletingTempFileFailed)
                {
                    return EncryptionResult.Fail("Temporary file couldn't be deleted."); ;
                }

                File.Move(tempFilePath2, tempFilePath);
            }

            var InputFileFragment = fm.GetFileFragment(tempFilePath, position, fragmentSize, out errorCode).ToList();

            if (errorCode == ErrorCode.GetFileFailed)
            {
                return EncryptionResult.Fail("File couldn't be loaded.");
            }

            var OutputFileFragment = new List<byte>();

            for (var i = 0; i < InputFileFragment.Count; i += BlockSize)
            {
                if (EncrMode == EncryptionMode.ECB)
                {
                    OutputFileFragment.AddRange(sa.BlockEncrypt(InputFileFragment.GetRange(i, BlockSize).ToArray(), 0, expandedKey));
                }
                else if (EncrMode == EncryptionMode.CBC)
                {
                    if (position == 0 && i == 0)
                        previousBlock = iv;

                    var plainText = InputFileFragment.GetRange(i, BlockSize).ToArray();
                    var currBlock = plainText.XOR(previousBlock);
                    var cipherText = sa.BlockEncrypt(currBlock, 0, expandedKey);
                    OutputFileFragment.AddRange(cipherText);
                    previousBlock = cipherText;
                }
                else
                {
                    return EncryptionResult.Fail("Selected ciphering type is not implemented.");
                }
            }

            fm.SaveFileFragment(destFilePath, position, OutputFileFragment.ToArray(), out errorCode);

            if (errorCode == ErrorCode.SaveFileFailed)
            {
                return EncryptionResult.Fail("File couldn't be saved. ");
            }

            position += fragmentSize;
        }
        while (position <= fi.Length);

        fm.DeleteTempFile(tempFilePath, out errorCode);

        if (errorCode == ErrorCode.DeletingTempFileFailed)
        {
            return EncryptionResult.Fail("Temporary file couldn't be deleted.");
        }

        var ivAndSalt = new byte[iv.Length + saltBytes.Length];
        iv.CopyTo(ivAndSalt, 0);
        saltBytes.CopyTo(ivAndSalt, iv.Length);

        fm.UnshiftBytesToFile(destFilePath, tempFilePath, ivAndSalt, out errorCode);

        if (errorCode == ErrorCode.ExpandFileFailed)
        {
            return EncryptionResult.Fail("File couldn't be modified.");
        }

        fm.DeleteTempFile(destFilePath, out errorCode);

        if (errorCode == ErrorCode.DeletingTempFileFailed)
            return EncryptionResult.Fail("Temporary file couldn't be deleted.");

        File.Move(tempFilePath, destFilePath);

        return EncryptionResult.Success();
    }

    public EncryptionResult Decrypt(string FilePath, byte[] Key, int Rounds, EncryptionMode EncrMode)
    {
        var fm = new FileManagement();
        SerpentAlgorithm sa = new SerpentStandardMode(); ;

        var position = 0;
        var destFilePath = string.Empty;
        var fi = new FileInfo(FilePath);
        const int fragmentSize = 1024 * 1024;
        Encoding enc = new UTF8Encoding();
        sa.Rounds = Rounds;
        sa.BlockSize = BlockSize;
        var tempFilePath = FilePath + ".temp";

        var iv = fm.GetFileFragment(FilePath, 0, BlockSize, out var errorCodeIv);
        var plainControlSum = fm.GetFileFragment(FilePath, BlockSize, BlockSize, out var errorCode);
        var roundBytesFromPtControlSum = Array.Empty<byte>();
        var previousBlock = Array.Empty<byte>();

        if (errorCodeIv == ErrorCode.GetFileFailed || errorCode == ErrorCode.GetFileFailed)
        {
            return EncryptionResult.Fail("File couldn't be loaded.");
        }

        var listRoundsBytesFromPtControlSum = new List<byte>();

        for (var i = 0; i < BlockSize; i++)
        {
            if (plainControlSum[i] != 3)
                listRoundsBytesFromPtControlSum.Add(plainControlSum[i]);
            else
            {
                roundBytesFromPtControlSum = listRoundsBytesFromPtControlSum.ToArray();
                break;
            }
        }

        var expandedKey = sa.MakeKey(Key);

        fm.ShiftBytesFromFile(FilePath, tempFilePath, BlockSize * 2, out errorCode);

        if (errorCode == ErrorCode.ShiftFileFailed)
        {
            return EncryptionResult.Fail("File couldn't be modified.");
        }

        do
        {
            var InputFileFragment = fm.GetFileFragment(tempFilePath, position, fragmentSize, out errorCode).ToList();

            if (errorCode == ErrorCode.GetFileFailed)
            {
                return EncryptionResult.Fail("File couldn't be loaded'.");
            }

            var OutputFileFragment = new List<byte>();

            for (var i = 0; i < InputFileFragment.Count; i += BlockSize)
            {
                switch (EncrMode)
                {
                    case EncryptionMode.ECB:
                        OutputFileFragment.AddRange(sa.BlockDecrypt(InputFileFragment.GetRange(i, BlockSize).ToArray(), 0, expandedKey));
                        break;
                    case EncryptionMode.CBC:
                        {
                            if (position == 0 && i == 0)
                                previousBlock = iv;
                            var cipherText = InputFileFragment.GetRange(i, BlockSize).ToArray();
                            var currBlock = sa.BlockDecrypt(cipherText, 0, expandedKey);
                            var plainText = currBlock.XOR(previousBlock);
                            OutputFileFragment.AddRange(plainText);
                            previousBlock = cipherText;
                            break;
                        }
                    default:
                        return EncryptionResult.Fail("Selected ciphering type is not implemented. ");
                }
            }

            // ===================

            if (position == 0)
            {
                int shiftedbytes = OutputFileFragment[BlockSize * 2];
                var decryptedControlSum = new byte[BlockSize];
                var extBytes = new byte[1];
                var roundBytes = new byte[1];
                var i = 0;

                for (; i < BlockSize; i++)
                    decryptedControlSum[i] = OutputFileFragment[i];

                var listExtbytes = new List<byte>();

                for (; i < BlockSize * 2; i++)
                {
                    if (OutputFileFragment[i] != 3)
                        listExtbytes.Add(OutputFileFragment[i]);
                    else
                    {
                        extBytes = listExtbytes.ToArray();
                        break;
                    }
                }

                i++;
                var listRoundsBytes = new List<byte>();

                for (; i < BlockSize * 2; i++)
                {
                    if (OutputFileFragment[i] != 3)
                        listRoundsBytes.Add(OutputFileFragment[i]);
                    else
                    {
                        roundBytes = listRoundsBytes.ToArray();
                        break;
                    }
                }

                var outputExtension = enc.GetString(extBytes);
                destFilePath = Path.ChangeExtension(FilePath, outputExtension);
                var areRoundsParsable = int.TryParse(enc.GetString(roundBytes), out var readRounds);

                if (!plainControlSum.SequenceEqual(decryptedControlSum) || !areRoundsParsable || shiftedbytes > 16 || readRounds != Rounds)
                {
                    fm.DeleteTempFile(tempFilePath, out errorCode);
                    if (errorCode == ErrorCode.DeletingTempFileFailed)
                        return EncryptionResult.Fail("Temporary file couldn't be deleted");

                    return EncryptionResult.Fail("Key is invalid.");
                }

                var shiftedBytesAndInfoBytes = shiftedbytes + BlockSize * 2;

                for (; shiftedBytesAndInfoBytes > 0; shiftedBytesAndInfoBytes--)
                    OutputFileFragment.RemoveAt(0);
            }

            fm.SaveFileFragment(destFilePath, position, OutputFileFragment.ToArray(), out errorCode);

            if (errorCode == ErrorCode.SaveFileFailed)
            {
                return EncryptionResult.Fail("FIle couldn't be saved. ");
            }

            position += fragmentSize;
        }
        while (position <= fi.Length);

        fm.DeleteTempFile(tempFilePath, out errorCode);

        if (errorCode == ErrorCode.DeletingTempFileFailed)
            return EncryptionResult.Fail("Temporary file couldn't be deleted.");

        return EncryptionResult.Success();
    }
}

public enum EncryptionMode
{
    ECB,
    CBC
}

public static class ExtensionMethods
{
    public static byte[] XOR(this byte[] buffer1, byte[] buffer2)
    {
        for (var i = 0; i < buffer1.Length; i++)
            buffer1[i] ^= buffer2[i];

        return buffer1;
    }
}

public class EncryptionResult
{
    public static EncryptionResult Success() => new EncryptionResult() { IsSuccessful = true };
    public static EncryptionResult Fail(string error) => new EncryptionResult() { IsSuccessful = false, Error = error };
    public bool IsSuccessful { get; set; }

    public string? Error { get; set; }
}