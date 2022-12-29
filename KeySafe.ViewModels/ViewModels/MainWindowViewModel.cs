using System.Collections.ObjectModel;
using KeySafe.ViewModels.Commands;
using KeySafe.ViewModels.Dependencies;
using KeySafe.ViewModels.Mappers;
using KeySafe.ViewModels.Service;

namespace KeySafe.ViewModels.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<SafeItemViewModel> SafeItems { get; set; } = new();
    
    public SafeItemViewModel SelectedSafeItem { get; set; }

    public bool HasSelection => SelectedSafeItem != null;
    
    public AsyncDelegateCommand AddSourceCommand { get; set; }
    
    public AsyncDelegateCommand<SafeItemViewModel> EditSourceCommand { get; set; }
    
    public AsyncDelegateCommand<SafeItemViewModel> RemoveSourceCommand { get; set; }
    
    public AsyncDelegateCommand ChangePasswordCommand { get; set; }
    
    public AsyncDelegateCommand ExportCommand { get; set; }
    
    public AsyncDelegateCommand ImportCommand { get; set; }
    
    public AsyncDelegateCommand InitializeCommand { get; set; }
    
    private readonly IDialogWindowsService _dialogWindowsService;

    private KsStorage _ksStorage;

    private readonly SettingsService _settingsService;

    public MainWindowViewModel(IDialogWindowsService dialogWindowsService)
    {
        _dialogWindowsService = dialogWindowsService;
        _settingsService = new SettingsService();

        InitializeCommand = new AsyncDelegateCommand(OnInitialize);
        AddSourceCommand = new AsyncDelegateCommand(OnAddSource);
        ChangePasswordCommand = new AsyncDelegateCommand(OnChangePassword);
        EditSourceCommand = new AsyncDelegateCommand<SafeItemViewModel>(OnEditSource);
        RemoveSourceCommand = new AsyncDelegateCommand<SafeItemViewModel>(OnRemoveSource);
        ExportCommand = new AsyncDelegateCommand(OnExport);
        ImportCommand = new AsyncDelegateCommand(OnImport);
    }

    private async Task OnChangePassword()
    {
        var (oldPassword, newPassword, cancel) = await _dialogWindowsService.ShowPasswordChangeWindowAsync();
        if (cancel)
        {
            return;
        }

        if (!await _ksStorage.ChangePasswordAsync(oldPassword, newPassword))
        {
            // TODO: fallback to login window
            _dialogWindowsService.ShutdownApplication();
        }
    }

    private async Task OnInitialize()
    {
        var (file, password, loginAction) = await _dialogWindowsService.ShowLoginWindowAsync(_settingsService.Settings.LoginFile);

        var hi = new Dictionary<LoginAction, Func<string, string, Task<StorageGetResult>>>
        {
            { LoginAction.Login, KsStorage.GetAsync },
            { LoginAction.Register, KsStorage.CreateAsync },
        };
        
        while (true)
        {
            var (success, errorMessage, storage) = await hi[loginAction].Invoke(file, password);
            if (!success)
            {
                (file, password, loginAction) = await _dialogWindowsService
                    .ShowLoginWindowAsync(_settingsService.Settings.LoginFile, errorMessage);
                continue;
            }

            _ksStorage = storage;
            SafeItems = ToSafeItems(await storage.GetItemsAsync());
            await _settingsService.UpdateLoginFileAsync(file);
            return;
        }
    }

    private ObservableCollection<SafeItemViewModel> ToSafeItems(IReadOnlyCollection<StorageItem> storageItems)
    {
        var safeItems = new ObservableCollection<SafeItemViewModel>();
        foreach (var storageItem in storageItems)
        {
            safeItems.Add(storageItem.ToSafeItemViewModel());
        }
        return safeItems;
    }

    private async Task OnEditSource(SafeItemViewModel safeItem)
    {
        var result = await _dialogWindowsService.ShowSafeItemWindowAsync(safeItem);
        if (result.Cancel)
        {
            return;
        }

        result.ToSafeItemViewModel(ref safeItem);
        await UpdateStorageAsync();
    }

    private async Task OnRemoveSource(SafeItemViewModel safeItem)
    {
        SafeItems.Remove(safeItem);
        await UpdateStorageAsync();
    }

    private async Task OnAddSource()
    {
        var result = await _dialogWindowsService.ShowSafeItemWindowAsync();
        if (result.Cancel)
        {
            return;
        }
        
        SafeItems.Add(result.ToSafeItemViewModel());
        await UpdateStorageAsync();
    }

    private Task UpdateStorageAsync()
    {
        var storageItems = SafeItems.Select(item => item.ToStorageItem()).ToList();
        return _ksStorage.SetItemsAsync(storageItems);
    }

    private async Task OnExport()
    {
        var (password, cancel) = await _dialogWindowsService.ShowPasswordConfirmationWindowAsync();
        if (cancel)
        {
            return;
        }

        var validationResult = await _ksStorage.ValidatePasswordAsync(password);
        if (!validationResult)
        {
            // TODO: fallback to login window
            _dialogWindowsService.ShutdownApplication();
        }

        var file = (await _dialogWindowsService.ShowExportWindowAsync()).File;
        await using var fo = File.Exists(file) ? File.OpenWrite(file) : File.Create(file!);
        await _ksStorage.ExportDataAsync(fo);
    }

    private async Task OnImport()
    {
        var result = await _dialogWindowsService.ShowImportWindowAsync();
        
        await using var fr = File.OpenRead(result.File);
        
        var itemsToAdd = await fr.DeserializeAsync<List<StorageItem>>();
        var newItems = SafeItems
            .Select(item => item.ToStorageItem())
            .Concat(itemsToAdd)
            .ToList();
        
        await _ksStorage.SetItemsAsync(newItems);
        SafeItems = ToSafeItems(newItems);
    }
}
