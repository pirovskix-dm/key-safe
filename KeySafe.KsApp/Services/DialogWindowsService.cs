using Avalonia.Controls.ApplicationLifetimes;
using KeySafe.KsApp.Views;
using KeySafe.KsApp.Views.Dialogs;
using KeySafe.ViewModels.Dependencies;
using KeySafe.ViewModels.ViewModels;

namespace KeySafe.KsApp.Services;

public class DialogWindowsService : IDialogWindowsService
{
    private Window _mainWindow => ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
    
    public async Task<SafeItemEditResult> ShowSafeItemWindowAsync(SafeItemViewModel safeItem = null, string errorMessage = null)
    {
        var result = await SourceItemWindow.ShowAsync(
            _mainWindow, 
            safeItem?.Name, 
            safeItem?.Login, 
            safeItem?.Password, 
            safeItem?.Web, 
            safeItem?.Note,
            errorMessage);
        
        _mainWindow.Activate();
        return result;
    }

    public async Task<LoginResult> ShowLoginWindowAsync(string loginFile, string errorMessage = null)
    {
        var result = await LoginWindow.ShowAsync(_mainWindow, loginFile, errorMessage);
        _mainWindow.Activate();
        return result;
    }

    public async Task<PasswordChangeResult> ShowPasswordChangeWindowAsync(string errorMessage = null)
    {
        var result = await PasswordChangeWindow.ShowAsync(_mainWindow, errorMessage);
        _mainWindow.Activate();
        return result;
    }

    public async Task<PasswordConfirmationResult> ShowPasswordConfirmationWindowAsync()
    {
        var result = await PasswordConfirmationWindow.ShowAsync(_mainWindow);
        _mainWindow.Activate();
        return result;
    }

    public async Task<ExportResult> ShowExportWindowAsync()
    {
        var file = await _mainWindow.SaveFileDialogAsync();
        return new ExportResult(file);
    }

    public async Task<ImportResult> ShowImportWindowAsync()
    {
        var file = await _mainWindow.OpenFileDialogAsync();
        return new ImportResult(file);
    }

    public void ShutdownApplication()
    {
        var desktop = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
        desktop.Shutdown();
    }
}