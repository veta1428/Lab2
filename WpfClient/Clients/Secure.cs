using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace WpfClient.Clients;

// https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.protecteddata?view=dotnet-plat-ext-7.0&redirectedfrom=MSDN
public static class Secure
{
    public static byte[] Protect(byte[] data)
    {
        byte[] entropy = Encoding.ASCII.GetBytes(Assembly.GetExecutingAssembly().FullName ?? string.Empty);
        return ProtectedData.Protect(data, entropy, DataProtectionScope.CurrentUser);
    }

    public static byte[] Unprotect(byte[] protectedData)
    {
        byte[] entropy = Encoding.ASCII.GetBytes(Assembly.GetExecutingAssembly().FullName ?? string.Empty);
        return ProtectedData.Unprotect(protectedData, entropy, DataProtectionScope.CurrentUser);
    }

    public static string Protect(string str)
    {
        byte[] protectedData = Protect(Encoding.ASCII.GetBytes(str));
        return Convert.ToBase64String(protectedData);
    }

    public static string Unprotect(string str)
    {
        byte[] protectedData = Convert.FromBase64String(str);
        byte[] data = Unprotect(protectedData);
        return Encoding.ASCII.GetString(data);
    }
}
