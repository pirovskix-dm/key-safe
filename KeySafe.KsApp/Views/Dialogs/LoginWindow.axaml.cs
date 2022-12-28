using System.Net.Security;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using KeySafe.KsApp.Services;
using KeySafe.KsApp.UserControls;
using KeySafe.ViewModels.Dependencies;

namespace KeySafe.KsApp.Views;

public class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static Task<LoginResult> ShowAsync(Window parent, string loginFile, string errorMessage)
    {
        var loginWindow = new LoginWindow();

        var selectedFile = string.Empty;
        
        var selectFileButton = loginWindow.FindControl<Button>("KsSelectFileButton");
        var passwordField = loginWindow.FindControl<TextBox>("KsPasswordBox");
        var errorField = loginWindow.FindControl<ErrorField>("ErrorField");
        var loginButton = loginWindow.FindControl<Button>("KsLoginButton");
        var createFileButton = loginWindow.FindControl<Button>("KsCreateFileButton");

        if (!string.IsNullOrWhiteSpace(loginFile))
        {
            var loginFileInfo = new FileInfo(loginFile);
            if (loginFileInfo.Exists)
            {
                selectFileButton.Content = loginFileInfo.Name;
                selectedFile = loginFile;
            }
        }

        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            errorField.Show(errorMessage);
        }
            
        var tcs = new TaskCompletionSource<LoginResult>();

        createFileButton.Click += async delegate
        {
            if (string.IsNullOrWhiteSpace(passwordField.Text))
            {
                errorField.Show("Please, provide a password");
                return;
            }

            var file = await loginWindow.SaveFileDialogAsync();
            if (string.IsNullOrWhiteSpace(file))
            {
                errorField.Show("Please, provide a file");
                return;
            }
            
            tcs.TrySetResult(new LoginResult(file, passwordField.Text.Trim(), LoginAction.Register));
            loginWindow.Close();
        };
        
        selectFileButton.Click += async delegate
        {
            var file = await loginWindow.OpenFileDialogAsync();
            if (string.IsNullOrWhiteSpace(file))
            {
                errorField.Show("Please, chose a file");
                return;
            }

            selectFileButton.Content = FileSystemService.GetFileName(file);
            selectedFile = file;
        };

        loginButton.Click += delegate
        {
            if (string.IsNullOrWhiteSpace(selectedFile) || string.IsNullOrWhiteSpace(passwordField.Text))
            {
                errorField.Show("Please, provide file and password");
                return;
            }
            
            tcs.TrySetResult(new LoginResult(selectedFile, passwordField.Text.Trim(), LoginAction.Login));
            loginWindow.Close();
        };
            
        loginWindow.ShowDialog(parent);
        return tcs.Task;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        this.BeginMoveDrag(e);
    }

    private void ExitButton_OnClick(object sender, RoutedEventArgs e)
    {
        var desktop = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
        desktop.Shutdown();
    }
}