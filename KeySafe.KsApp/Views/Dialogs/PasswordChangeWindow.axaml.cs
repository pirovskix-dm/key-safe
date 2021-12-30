using KeySafe.KsApp.UserControls;
using KeySafe.ViewModels.Dependencies;

namespace KeySafe.KsApp.Views.Dialogs;

public class PasswordChangeWindow : Window
{
    public PasswordChangeWindow()
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

    public static Task<PasswordChangeResult> ShowAsync(Window parent, string errorMessage)
    {
        var passwordChangeWindow = new PasswordChangeWindow();
            
        var oldPasswordField = passwordChangeWindow.FindControl<KsPasswordField>("OldPasswordField");
        var newPasswordField = passwordChangeWindow.FindControl<KsPasswordField>("NewPasswordField");
        var errorField = passwordChangeWindow.FindControl<ErrorField>("ErrorField");
        var changeButton = passwordChangeWindow.FindControl<Button>("ChangeButton");
        var cancelButton = passwordChangeWindow.FindControl<Button>("CancelButton");

        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            errorField.Show(errorMessage);
        }

        var tcs = new TaskCompletionSource<PasswordChangeResult>();
            
        changeButton.Click += delegate
        {
            if (string.IsNullOrWhiteSpace(oldPasswordField.Password) || string.IsNullOrWhiteSpace(newPasswordField.Password))
            {
                errorField.Show("Please, provide an old password and a new password");
                return;
            }

            if (oldPasswordField.Password == newPasswordField.Password)
            {
                errorField.Show("old and new password should not be same");
                return;
            }
                
            tcs.TrySetResult(new PasswordChangeResult(oldPasswordField.Password.Trim(), newPasswordField.Password.Trim(), false));
            passwordChangeWindow.Close();
        };

        cancelButton.Click += delegate
        {
            tcs.TrySetResult(new PasswordChangeResult(null, null, true));
            passwordChangeWindow.Close();
        };

        passwordChangeWindow.Closed += delegate
        {
            tcs.TrySetResult(new PasswordChangeResult(null, null, true));
        };
            
        passwordChangeWindow.ShowDialog(parent);
        return tcs.Task;
    }
}