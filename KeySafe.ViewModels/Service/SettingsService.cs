namespace KeySafe.ViewModels.Service;

public readonly record struct SettingsItem(string LoginFile);

public class SettingsService
{
    public SettingsItem Settings { get; private set; }
    
    private readonly string _file;
    
    public SettingsService()
    {
        var directory = InitDirectory("KeySafe");
        _file = InitFile(directory, "settings.json");
        Settings = GetSettings();
    }

    public async Task UpdateLoginFileAsync(string file)
    {
        var newSettings = new SettingsItem(file);
        await SetSettingsAsync(newSettings);
        Settings = newSettings;
    }
    
    private SettingsItem GetSettings()
    {
        var json = File.ReadAllText(_file);
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }
        return json.Deserialize<SettingsItem>();
    }

    private Task SetSettingsAsync(SettingsItem settingsItem)
    {
        var json = settingsItem.Serialize();
        return File.WriteAllTextAsync(_file, json);
    }
    
    private string InitDirectory(string name)
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var baseDirectory = Path.Combine(localAppData, name);
        Directory.CreateDirectory(baseDirectory);
        return baseDirectory;
    }
    
    private string InitFile(string directory, string name)
    {
        var file = Path.Combine(directory, name);
        if (!File.Exists(file))
        {
            var def = new SettingsItem(string.Empty);
            var json = def.Serialize();
            using var sw = File.CreateText(file!);
            sw.WriteLine(json);
        }

        return file;
    }
}