using System.Security.Cryptography;
using KeySafe.ViewModels.Exceptions;

namespace KeySafe.ViewModels.Service;

public static class CryptoService
{
    private const int SaltLength = 8;

    public static async Task EncryptAsync(this FileStream fileStream, object data, string key)
    {
        var salt = GenerateRandomSalt();
        using var aes = CreateAes(key, salt);
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        fileStream.Write(salt, 0, salt.Length);
        await using var cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);
        await cryptoStream.WriteAsync(data.SerializeToUtf8Bytes());
        await cryptoStream.FlushFinalBlockAsync();
    }

    public static async Task<T> DecryptAsync<T>(this FileStream fileStream, string key)
    {
        T data = default;
        await fileStream.DecryptAsync(key, async cryptoStream => { data = await cryptoStream.DeserializeAsync<T>(); });
        return data;
    }

    public static async Task DecryptAsync(this FileStream inputFileStream, FileStream outputFileStream, string key)
    {
        await inputFileStream.DecryptAsync(key, cs => cs.CopyToAsync(outputFileStream));
    }

    private static async Task DecryptAsync(this FileStream fileStream, string key, Func<CryptoStream, Task> handler)
    {
        var salt = await fileStream.GetSaltAsync();

        using var aes = CreateAes(key, salt);
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        try
        {
            await using var cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read);
            await handler.Invoke(cryptoStream);
        }
        catch (CryptographicException ex)
        {
            throw new KsInvalidKeyException(ex);
        }
    }

    private static Aes CreateAes(this string key, byte[] salt)
    {
        var rfc2898 = new Rfc2898DeriveBytes(key, salt, 1000000);
        var aes = Aes.Create();
        aes.Key = rfc2898.GetBytes(16);
        aes.IV = rfc2898.GetBytes(16);
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }

    private static byte[] GenerateRandomSalt()
    {
        var salt = new byte[SaltLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    private static async Task<byte[]> GetSaltAsync(this FileStream fileStream)
    {
        var salt = new byte[SaltLength];
        await fileStream.ReadAsync(salt, 0, SaltLength);
        return salt;
    }
}
