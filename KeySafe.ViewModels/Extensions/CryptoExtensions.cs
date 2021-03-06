using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KeySafe.ViewModels.Exceptions;

namespace KeySafe.ViewModels.Extensions;

public static class CryptoExtensions
{
    public static async Task EncryptAsync(this FileStream fileStream, object data, string key)
    {
        using var aes = GetAes(key);
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        await using var cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);
        await cryptoStream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(data));
        await cryptoStream.FlushFinalBlockAsync();
    }

    public static async Task<T> DecryptAsync<T>(this FileStream fileStream, string key)
    {
        using var aes = GetAes(key);
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        try
        {
            await using var cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read);
            return await JsonSerializer.DeserializeAsync<T>(cryptoStream);
        }
        catch (CryptographicException ex)
        {
            throw new KsInvalidKeyException(ex);
        }
    }

    private static Aes GetAes(string key)
    {
        var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Key = GenerateKey(key);
        aes.IV = new byte[16];
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }

    private static byte[] GenerateKey(string key)
    {
        using var hash = MD5.Create();
        return hash.ComputeHash(Encoding.UTF8.GetBytes(key));
    }
}
