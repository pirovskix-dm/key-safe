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

    private StorageService _storageService;

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

        while (!await _storageService.ValidatePasswordAsync(oldPassword))
        {
            (oldPassword, newPassword, cancel) = await _dialogWindowsService.ShowPasswordChangeWindowAsync("Invalid old password");
        }

        await _storageService.ChangePasswordAsync(newPassword);
    }

    private async Task OnInitialize()
    {
        var (file, password, loginAction) = await _dialogWindowsService.ShowLoginWindowAsync(_settingsService.Settings.LoginFile);
        while (true)
        {
            var storageService = new StorageService(file, password);
            switch (loginAction)
            {
                case LoginAction.Login when !storageService.Exists():
                    (file, password, loginAction) = await _dialogWindowsService.ShowLoginWindowAsync(_settingsService.Settings.LoginFile, "File not found");
                    continue;
                case LoginAction.Register when storageService.Exists():
                    (file, password, loginAction) = await _dialogWindowsService.ShowLoginWindowAsync(_settingsService.Settings.LoginFile, "File already exists");
                    continue;
                case LoginAction.Login when !await storageService.ValidatePasswordAsync():
                    await _settingsService.UpdateLoginFileAsync(file);
                    (file, password, loginAction) = await _dialogWindowsService.ShowLoginWindowAsync(_settingsService.Settings.LoginFile, "Invalid password");
                    continue;
                case LoginAction.Login:
                    SafeItems = await GetSafeItemsAsync(storageService);
                    await _settingsService.UpdateLoginFileAsync(file);
                    _storageService = storageService;
                    return;
                case LoginAction.Register:
                    await storageService.CreateAsync();
                    await _settingsService.UpdateLoginFileAsync(file);
                    SafeItems = new ObservableCollection<SafeItemViewModel>();
                    _storageService = storageService;
                    return;
            }
        }
    }

    private async Task<ObservableCollection<SafeItemViewModel>> GetSafeItemsAsync(StorageService storageService)
    {
        var safeItems = new ObservableCollection<SafeItemViewModel>();
        var storageItems = await storageService.GetAsync();
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
        return _storageService.SetAsync(SafeItems.Select(item => item.ToStorageItem()).ToList());
    }
}
