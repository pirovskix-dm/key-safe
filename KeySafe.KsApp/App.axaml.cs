using Avalonia.Controls.ApplicationLifetimes;
using KeySafe.KsApp.Services;
using KeySafe.KsApp.Views;
using KeySafe.ViewModels.ViewModels;

namespace KeySafe.KsApp;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dialogWindowsService = new DialogWindowsService();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(dialogWindowsService),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}