using KeySafe.ViewModels.Exceptions;
using KeySafe.ViewModels.Extensions;

namespace KeySafe.ViewModels.Service;

public readonly record struct StorageItem(string Name, string Login, string Password, string Web, string Note);

public readonly record struct StorageGetResult(bool Success, string ErrorMessage, KsStorage Storage);

public sealed class KsStorage
{
    public IReadOnlyCollection<StorageItem> Items { get; private set; }

    private const string SECRET = "hpYo2FFZoGj133LHn7ei";

    public string FilePath { get; }

    private string _key;

    private KsStorage(string file, string password, IReadOnlyCollection<StorageItem> items)
    {
        FilePath = file;
        _key = GetKey(password);
        Items = items;
    }

    public static async Task<StorageGetResult> CreateAsync(string file, string password)
    {
        if (File.Exists(file))
        {
            return new StorageGetResult(false, "File already exists", null);
        }

        await using var fs = File.Create(file!);
        var items = Array.Empty<StorageItem>();
        await fs.EncryptAsync(items, GetKey(password));

        return new StorageGetResult(true, null, new KsStorage(file, password, items));
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

        var items = await GetItemsAsync(file, password);
        return new StorageGetResult(true, null, new KsStorage(file, password, items));
    }

    public bool ValidateItem(StorageItem item)
    {
        return !Items.Any(i => i.Name.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase));
    }

    public async Task<bool> AddAsync(StorageItem item)
    {
        if (!ValidateItem(item))
        {
            return false;
        }

        var newItems = Items.Concat(new List<StorageItem> { item }).ToList();
        await SetItemsAsync(newItems.AsReadOnly());
        return true;
    }

    public async Task RemoveAsync(string name)
    {
        var newItems = Items.Where(i => !i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        await SetItemsAsync(newItems.AsReadOnly());
    }

    public async Task<bool> UpdateAsync(string name, StorageItem item)
    {
        if (!name.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase) && !ValidateItem(item))
        {
            return false;
        }

        var newItems = Items
            .Where(i => i.Name != name)
            .Concat(new List<StorageItem> { item })
            .ToList();
        await SetItemsAsync(newItems.AsReadOnly());
        return true;
    }

    public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
    {
        if (!await ValidatePasswordAsync(FilePath, oldPassword))
        {
            return false;
        }

        _key = GetKey(newPassword);
        await SetItemsAsync(Items);
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

    public async Task ImportDataAsync(FileStream inputFileStream)
    {
        var items = await inputFileStream.DeserializeAsync<List<StorageItem>>();
        await SetItemsAsync(items.AsReadOnly());
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

    private static async Task<IReadOnlyList<StorageItem>> GetItemsAsync(string file, string password)
    {
        await using var fs = File.OpenRead(file);
        return await fs.DecryptAsync<List<StorageItem>>(GetKey(password));
    }

    private async Task SetItemsAsync(IReadOnlyCollection<StorageItem> items)
    {
        await using var fs = new FileStream(FilePath, FileMode.Truncate, FileAccess.Write, FileShare.None);
        await fs.EncryptAsync(items, _key);
        Items = items;
    }
}
