using Serpent;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace WpfClient.Clients;

public class SecureClient
{
    private const string BaseUrl = "https://localhost:7164";
    private readonly HttpClient _httpClient;
    private byte[] _sessionKey;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SecureClient()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _httpClient = new HttpClient();
    }

    private byte[] SessionKey
    {
        get => Secure.Unprotect(_sessionKey);
        set
        {
            _sessionKey = Secure.Protect(value);
        } 
    }

    public async Task<RequestResult> InitializeAsync()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        var rsa = RSA.Create();

        var response = await _httpClient.PostAsJsonAsync(BaseUrl + "/api/session", rsa.ExportRSAPublicKey());

        if (response.StatusCode != HttpStatusCode.OK)
            return RequestResult.Fail(Error.Unknown);

        var cookies = new CookieContainer();
        foreach (var cookieHeader in response.Headers.GetValues("Set-Cookie"))
            cookies.SetCookies(new Uri(BaseUrl), cookieHeader);

        _httpClient.DefaultRequestHeaders.Add("cookie", cookies.GetCookieHeader(new Uri(BaseUrl)));
        byte[] sessionKeyCyfered = Convert.FromBase64String(await response.Content.ReadAsStringAsync());

        SessionKey = rsa.Decrypt(sessionKeyCyfered, RSAEncryptionPadding.Pkcs1);
        return RequestResult.Success();
    }

    public async Task<RequestResult<string>> GetFileAsync(string filepath)
    {
        var response = await _httpClient.GetAsync(BaseUrl + "/api/file/" + filepath);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return RequestResult<string>.Fail(Error.NotFound);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return RequestResult<string>.Fail(Error.UnAuthorized);
        if (!response.IsSuccessStatusCode)
            return RequestResult<string>.Fail(Error.Unknown);

        byte[] fileEncrypted = Convert.FromBase64String(await response.Content.ReadAsStringAsync());
        await System.IO.File.WriteAllBytesAsync(filepath, fileEncrypted);
        SerpentCipher serpentCipher = new SerpentCipher();
        serpentCipher.Decrypt(filepath, SessionKey, 32, Mode.Standard, EncryptionMode.ECB);

        return RequestResult<string>.Success(System.IO.File.ReadAllText(filepath));
    }
}

public enum Error
{
    None = 0,
    NotFound = 1,
    UnAuthorized = 2,
    Unknown
}

public class RequestResult
{
    protected RequestResult(bool isSuccessful, Error error)
    {
        IsSuccessful = isSuccessful;
        Error = error;
    }

    public static RequestResult Success()
    {
        return new RequestResult(true, Error.None);
    }

    public static RequestResult Fail(Error error)
    {
        return new RequestResult(false, error);
    }

    public bool IsSuccessful { get; set; }

    public Error Error { get; set; }
}


public class RequestResult<T> where T: class
{
    protected RequestResult(bool isSuccessful, T? result, Error error)
    {
        IsSuccessful = isSuccessful;
        Result = result;
        Error = error;
    }

    public static RequestResult<T> Success(T result)
    {
        return new RequestResult<T>(true, result, Error.None);
    }

    public static RequestResult<T> Fail(Error error)
    {
        return new RequestResult<T>(false, null, error);
    }

    public bool IsSuccessful { get; set; }

    public T? Result { get; set; }

    public Error Error { get; set; }
}
