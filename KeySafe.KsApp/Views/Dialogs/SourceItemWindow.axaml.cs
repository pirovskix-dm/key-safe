using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using KeySafe.KsApp.UserControls;
using KeySafe.ViewModels.Dependencies;

namespace KeySafe.KsApp.Views
{
    public class SourceItemWindow : Window
    {
        public SourceItemWindow()
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
        
        public static Task<SafeItemEditResult> ShowAsync(Window parent, string name, string login, string password)
        {
            var sourceItemWindow = new SourceItemWindow();

            var nameField = sourceItemWindow.FindControl<KsTextField>("NameField");
            var loginField = sourceItemWindow.FindControl<KsTextField>("LoginField");
            var passwordField = sourceItemWindow.FindControl<KsPasswordField>("PasswordField");
            var errorField = sourceItemWindow.FindControl<ErrorField>("ErrorField");
            var saveButton = sourceItemWindow.FindControl<Button>("SaveButton");
            var cancelButton = sourceItemWindow.FindControl<Button>("CancelButton");
            
            nameField.Text = name ?? string.Empty;
            loginField.Text = login ?? string.Empty;
            passwordField.Password = password ?? string.Empty;
            
            var tcs = new TaskCompletionSource<SafeItemEditResult>();
            
            saveButton.Click += delegate
            {
                if (string.IsNullOrWhiteSpace(nameField.Text))
                {
                    errorField.Show("Name field is required");
                    return;
                }
                
                tcs.TrySetResult(new SafeItemEditResult(nameField.Text, loginField.Text, passwordField.Password, false));
                sourceItemWindow.Close();
            };
            
            cancelButton.Click += delegate
            {
                tcs.TrySetResult(new SafeItemEditResult(null, null, null, true));
                sourceItemWindow.Close();
            };
            
            sourceItemWindow.Closed += delegate
            {
                tcs.TrySetResult(new SafeItemEditResult(null, null, null, true)); 
            };
            
            sourceItemWindow.ShowDialog(parent);
            return tcs.Task;
        }
    }
}
