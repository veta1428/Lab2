using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient.Clients;

public class SecureClient
{
    private readonly HttpClient _httpClient;
    private Guid? _sessionId;
    private byte[]? _rsaPrivateKey;
    private string? _sessionKey;

    public SecureClient()
    {
        _httpClient = new HttpClient();
    }

    public async Task InitializeAsync()
    {
        var rsa = RSA.Create();
        var rsaPublicKey = rsa.ExportRSAPublicKey();
        _rsaPrivateKey = rsa.ExportRSAPrivateKey();

        HttpContent httpContent = new ByteArrayContent(rsaPublicKey);

        var response = await _httpClient.PostAsJsonAsync("https://localhost:7164/api/session", rsaPublicKey);

        CookieContainer cookies = new CookieContainer();
        foreach (var cookieHeader in response.Headers.GetValues("Set-Cookie"))
            cookies.SetCookies(new Uri("https://localhost:7164"), cookieHeader);

        byte[] sessionKeyCyfered = Convert.FromBase64String(await response.Content.ReadAsStringAsync());
        byte[] sessionKeyBytes = rsa.Decrypt(sessionKeyCyfered, RSAEncryptionPadding.Pkcs1);
        _sessionKey = Encoding.Unicode.GetString(sessionKeyBytes);
        Cookie? sessionCookie = cookies.GetCookies(new Uri("https://localhost:7164")).First(c => c.Name == "SessionId");
        Guid sessionId;
        Guid.TryParse(sessionCookie!.Value, out sessionId);
        _sessionId = sessionId;
    }


}
