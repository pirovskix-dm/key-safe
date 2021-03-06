using KeySafe.ViewModels.Exceptions;
using KeySafe.ViewModels.Extensions;

namespace KeySafe.ViewModels.Service;

public readonly record struct StorageItem(string Name, string Login, string Password, string Web, string Note);

public class StorageService
{
    private const string SECRET = "hpYo2FFZoGj133LHn7ei";
    
    private readonly string _file;
    
    private string _key;

    public StorageService(string file, string password)
    {
        _file = file;
        _key = GetKey(password);
    }

    public bool Exists()
    {
        return File.Exists(_file);
    }
    
    public async Task CreateAsync()
    {
        if (!File.Exists(_file))
        {
            await using var fs = File.Create(_file);
            await fs.EncryptAsync(Array.Empty<StorageItem>(), _key);
        }
    }

    public async Task ChangePasswordAsync(string password)
    {
        var data = await GetAsync();
        _key = GetKey(password);
        await SetAsync(data);
    }
    
    public async Task<bool> ValidatePasswordAsync(string password)
    {
        await using var fs = File.OpenRead(_file);
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

    public async Task<bool> ValidatePasswordAsync()
    {
        await using var fs = File.OpenRead(_file);
        try
        {
            await fs.DecryptAsync<dynamic>(_key);
            return true;
        }
        catch (KsInvalidKeyException)
        {
            return false;
        }
    }

    public async Task<IReadOnlyCollection<StorageItem>> GetAsync()
    {
        await using var fs = File.OpenRead(_file);
        return await fs.DecryptAsync<List<StorageItem>>(_key);
    }

    public async Task SetAsync(IReadOnlyCollection<StorageItem> data)
    {
        await using var fs = new FileStream(_file, FileMode.Truncate, FileAccess.Write, FileShare.None);
        await fs.EncryptAsync(data, _key);
    }
    
    private string GetKey(string password)
    {
        return $"{password.Trim().Replace(" ", "+")}_{SECRET}";
    }
}
