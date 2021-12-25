using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using KeySafe.KsApp.UserControls;
using KeySafe.ViewModels.Dependencies;

namespace KeySafe.KsApp.Views
{
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

        public static Task<LoginResult> ShowAsync(Window parent, string errorMessage)
        {
            var loginWindow = new LoginWindow();
            
            var loginField = loginWindow.FindControl<TextBox>("KsLoginBox");
            var passwordField = loginWindow.FindControl<TextBox>("KsPasswordBox");
            var errorField = loginWindow.FindControl<ErrorField>("ErrorField");
            var loginButton = loginWindow.FindControl<Button>("KsLoginButton");
            var registerButton = loginWindow.FindControl<Button>("KsRegisterButton");

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                errorField.Show(errorMessage);
            }
            
            var tcs = new TaskCompletionSource<LoginResult>();
            
            registerButton.Click += delegate
            {
                if (string.IsNullOrWhiteSpace(loginField.Text) || string.IsNullOrWhiteSpace(loginField.Text))
                {
                    errorField.Show("Please, provide login and password");
                    return;
                }
                
                tcs.TrySetResult(new LoginResult(loginField.Text.Trim(), passwordField.Text.Trim(), LoginAction.Register));
                loginWindow.Close();
            };
            
            loginButton.Click += delegate
            {
                tcs.TrySetResult(new LoginResult(loginField.Text.Trim(), passwordField.Text.Trim(), LoginAction.Login));
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
}