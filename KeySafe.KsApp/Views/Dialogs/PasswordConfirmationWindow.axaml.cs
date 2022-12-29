using KeySafe.KsApp.UserControls;
using KeySafe.ViewModels.Dependencies;

namespace KeySafe.KsApp.Views.Dialogs;

public class PasswordConfirmationWindow : Window
{
    public PasswordConfirmationWindow()
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

    public static Task<PasswordConfirmationResult> ShowAsync(Window parent)
    {
        var passwordConfirmationWindow = new PasswordConfirmationWindow();
        
        var confirmField = passwordConfirmationWindow.FindControl<KsPasswordField>("ConfirmField");
        var confirmButton = passwordConfirmationWindow.FindControl<Button>("ConfirmButton");
        var cancelButton = passwordConfirmationWindow.FindControl<Button>("CancelButton");
        var errorField = passwordConfirmationWindow.FindControl<ErrorField>("ErrorField");
        
        var tcs = new TaskCompletionSource<PasswordConfirmationResult>();
        
        cancelButton.Click += delegate
        {
            tcs.TrySetResult(new PasswordConfirmationResult(string.Empty, true));
            passwordConfirmationWindow.Close();
        };

        confirmButton.Click += delegate
        {
            if (string.IsNullOrWhiteSpace(confirmField.Password))
            {
                errorField.Show("Please, provide a password");
                return;
            }
            
            tcs.TrySetResult(new PasswordConfirmationResult(confirmField.Password.Trim(), false));
            passwordConfirmationWindow.Close();
        };
        
        passwordConfirmationWindow.ShowDialog(parent);
        return tcs.Task;
    }
}