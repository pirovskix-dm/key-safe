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
    }

    private async Task OnChangePassword()
    {
        var (oldPassword, newPassword, cancel) = await _dialogWindowsService.ShowPasswordChangeWindowAsync();
        if (cancel)
        {
            return;
        }

        while (!await _ksStorage.ChangePasswordAsync(oldPassword, newPassword))
        {
            (oldPassword, newPassword, cancel) = await _dialogWindowsService.ShowPasswordChangeWindowAsync("Invalid old password");
            if (cancel)
            {
                return;
            }
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
            var result = await hi[loginAction].Invoke(file, password);
            if (!result.Success)
            {
                (file, password, loginAction) = await _dialogWindowsService.ShowLoginWindowAsync(_settingsService.Settings.LoginFile, result.ErrorMessage);
                continue;
            }
            
            SafeItems = await SetSafeItemsAsync(result.Storage);
            _ksStorage = result.Storage;
            await _settingsService.UpdateLoginFileAsync(file);
            return;
        }
    }

    private async Task<ObservableCollection<SafeItemViewModel>> SetSafeItemsAsync(KsStorage storage)
    {
        var storageItems = await storage.GetItemsAsync();
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
}
