using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using KeySafe.KsApp.Views;
using KeySafe.KsApp.Views.Dialogs;
using KeySafe.ViewModels.Dependencies;
using KeySafe.ViewModels.ViewModels;

namespace KeySafe.KsApp.Services
{
    public class DialogWindowsService : IDialogWindowsService
    {
        public async Task<SafeItemEditResult> ShowSafeItemWindowAsync(SafeItemViewModel safeItem = null)
        {
            var desktop = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            var result = await SourceItemWindow.ShowAsync(desktop.MainWindow, safeItem?.Name, safeItem?.Login, safeItem?.Password);
            desktop.MainWindow.Activate();
            return result;
        }

        public async Task<LoginResult> ShowLoginWindowAsync(string errorMessage = null)
        {
            var desktop = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            var result = await LoginWindow.ShowAsync(desktop.MainWindow, errorMessage);
            desktop.MainWindow.Activate();
            return result;
        }

        public async Task<PasswordChangeResult> ShowPasswordChangeWindowAsync(string errorMessage = null)
        {
            var desktop = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            var result = await PasswordChangeWindow.ShowAsync(desktop.MainWindow, errorMessage);
            desktop.MainWindow.Activate();
            return result;
        }
    }
}