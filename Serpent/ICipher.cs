namespace Serpent;

public interface ICipher
{
    public int Rounds { get; }

    public int BlockSizeBytes { get; }

    public byte[] Encrypt(byte[] plainText, byte[] key, BlockMode blockMode, PaddingMode paddingMode);

    public byte[] Decrypt(byte[] encryptedText, byte[] key, BlockMode blockMode, PaddingMode paddingMode);
}
