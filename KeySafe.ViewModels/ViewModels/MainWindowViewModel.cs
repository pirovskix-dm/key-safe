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
    
    public string SearchText { get; set; }

    public bool HasSelection => SelectedSafeItem != null;
    
    public AsyncDelegateCommand AddSourceCommand { get; set; }
    
    public AsyncDelegateCommand<SafeItemViewModel> EditSourceCommand { get; set; }
    
    public AsyncDelegateCommand<SafeItemViewModel> RemoveSourceCommand { get; set; }
    
    public AsyncDelegateCommand ChangePasswordCommand { get; set; }
    
    public AsyncDelegateCommand ExportCommand { get; set; }
    
    public AsyncDelegateCommand ImportCommand { get; set; }
    
    public AsyncDelegateCommand InitializeCommand { get; set; }
    
    public AsyncDelegateCommand SearchCommand { get; set; }
    
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
        SearchCommand = new AsyncDelegateCommand(OnSearch);
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
            SetSafeItems();
            await _settingsService.UpdateLoginFileAsync(file);
            return;
        }
    }

    private async Task OnEditSource(SafeItemViewModel safeItem)
    {
        var result = await _dialogWindowsService.ShowSafeItemWindowAsync(safeItem);
        if (result.Cancel)
        {
            return;
        }

        while (!await _ksStorage.UpdateAsync(safeItem.Name, result.ToStorageItem()))
        {
            result = await _dialogWindowsService.ShowSafeItemWindowAsync(result.ToSafeItemViewModel(), "Name already exists");
            if (result.Cancel)
            {
                return;
            }   
        }
        SetSafeItems();
    }

    private async Task OnRemoveSource(SafeItemViewModel safeItem)
    {
        await _ksStorage.RemoveAsync(safeItem.Name);
        SetSafeItems();
    }

    private async Task OnAddSource()
    {
        var result = await _dialogWindowsService.ShowSafeItemWindowAsync();
        if (result.Cancel)
        {
            return;
        }

        while (!await _ksStorage.AddAsync(result.ToStorageItem()))
        {
            result = await _dialogWindowsService.ShowSafeItemWindowAsync(result.ToSafeItemViewModel(), "Name already exists");
            if (result.Cancel)
            {
                return;
            }   
        }
        
        SetSafeItems();
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
        await _ksStorage.ImportDataAsync(fr);
        SetSafeItems();
    }
    
    private Task OnSearch()
    {
        SetSafeItems();
        return Task.CompletedTask;
    }

    private void SetSafeItems()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            SafeItems = ToSafeItems(_ksStorage.Items);
            return;
        }

        var newItems = _ksStorage.Items
            .Where(i => i.Name.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase))
            .ToList();
        
        SafeItems = ToSafeItems(newItems);
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
}
