namespace Server.Services;

public static class SessionKeyGenerator
{
    public static byte[] GenerateSessionKey(int byteLength = 32)
    {
        var key = new byte[byteLength];
        Random rn = new Random();

        for (int i = 0; i < byteLength; i++)
            key[i] = (byte)(rn.Next() % 256);
        return key;
    }
}
