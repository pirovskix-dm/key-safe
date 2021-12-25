using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using KeySafe.ViewModels.Commands;
using KeySafe.ViewModels.Dependencies;
using KeySafe.ViewModels.Service;

namespace KeySafe.ViewModels.ViewModels
{
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

        public MainWindowViewModel(IDialogWindowsService dialogWindowsService)
        {
            _dialogWindowsService = dialogWindowsService;

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
            var (login, password, loginAction) = await _dialogWindowsService.ShowLoginWindowAsync();
            while (true)
            {
                var storageService = new StorageService(login, password);
                switch (loginAction)
                {
                    case LoginAction.Login when !storageService.Exists():
                        (login, password, loginAction) = await _dialogWindowsService.ShowLoginWindowAsync("Login not found");
                        continue;
                    case LoginAction.Register when storageService.Exists():
                        (login, password, loginAction) = await _dialogWindowsService.ShowLoginWindowAsync("Login already exists");
                        continue;
                    case LoginAction.Login when !await storageService.ValidatePasswordAsync():
                        (login, password, loginAction) = await _dialogWindowsService.ShowLoginWindowAsync("Invalid password");
                        continue;
                    case LoginAction.Login:
                        SafeItems = await GetSafeItemsAsync(storageService);
                        _storageService = storageService;
                        return;
                    case LoginAction.Register:
                        await storageService.CreateAsync();
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
            foreach (var (name, login, password) in storageItems)
            {
                safeItems.Add(new SafeItemViewModel
                {
                    Name = name,
                    Login = login,
                    Password = password
                });
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

            safeItem.Name = result.Name;
            safeItem.Login = result.Login;
            safeItem.Password = result.Password;
            
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
            
            SafeItems.Add(new SafeItemViewModel()
            {
                Name = result.Name,
                Login = result.Login,
                Password = result.Password
            });

            await UpdateStorageAsync();
        }

        private Task UpdateStorageAsync()
        {
            return _storageService.SetAsync(SafeItems.Select(item => new StorageItem
            {
                Name = item.Name,
                Login = item.Login,
                Password = item.Password
            }).ToList());
        }
    }
}
