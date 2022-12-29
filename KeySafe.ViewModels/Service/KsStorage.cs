using System.Text.Json;
using KeySafe.ViewModels.Exceptions;
using KeySafe.ViewModels.Extensions;

namespace KeySafe.ViewModels.Service;

public readonly record struct StorageItem(string Name, string Login, string Password, string Web, string Note);

public readonly record struct StorageGetResult(bool Success, string ErrorMessage, KsStorage Storage);

public sealed class KsStorage
{
    private const string SECRET = "hpYo2FFZoGj133LHn7ei";
    
    public string FilePath { get; }
    
    private string _key;
    
    private KsStorage(string file, string password)
    {
        FilePath = file;
        _key = GetKey(password);
    }

    public static async Task<StorageGetResult> CreateAsync(string file, string password)
    {
        if (File.Exists(file))
        {
            return new StorageGetResult(false, "File already exists", null);
        }

        await using var fs = File.Create(file!);
        await fs.EncryptAsync(Array.Empty<StorageItem>(), GetKey(password));
        
        return new StorageGetResult(true, null, new KsStorage(file, password));
    }

    public static async Task<StorageGetResult> GetAsync(string file, string password)
    {
        if (!File.Exists(file))
        {
            return new StorageGetResult(false, "File not found or Invalid password", null);
        }

        if (!await ValidatePasswordAsync(file, password))
        {
            return new StorageGetResult(false, "File not found or Invalid password", null);
        }

        return new StorageGetResult(true, null, new KsStorage(file, password));
    }
    
    public async Task<IReadOnlyCollection<StorageItem>> GetItemsAsync()
    {
        await using var fs = File.OpenRead(FilePath);
        return await fs.DecryptAsync<List<StorageItem>>(_key);
    }
    
    public async Task SetItemsAsync(IReadOnlyCollection<StorageItem> data)
    {
        await using var fs = new FileStream(FilePath, FileMode.Truncate, FileAccess.Write, FileShare.None);
        await fs.EncryptAsync(data, _key);
    }
    
    public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
    {
        if (!await ValidatePasswordAsync(FilePath, oldPassword))
        {
            return false;
        }
        
        var data = await GetItemsAsync();
        _key = GetKey(newPassword);
        await SetItemsAsync(data);
        return true;
    }

    public Task<bool> ValidatePasswordAsync(string password)
    {
        return ValidatePasswordAsync(FilePath, password);
    }

    public async Task ExportDataAsync(FileStream outputFileStream)
    {
        await using var fi = File.OpenRead(FilePath);
        await fi.DecryptAsync(outputFileStream, _key);
    }
    
    private static async Task<bool> ValidatePasswordAsync(string file, string password)
    {
        await using var fs = File.OpenRead(file);
        try
        {
            await fs.DecryptAsync<dynamic>(GetKey(password));
            return true;
        }
        catch (KsInvalidKeyException)
        {
            return false;
        }
    }
    
    private static string GetKey(string password)
    {
        return $"{password.Trim().Replace(" ", "+")}_{SECRET}";
    }
}