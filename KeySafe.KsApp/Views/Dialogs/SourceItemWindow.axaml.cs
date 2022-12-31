using KeySafe.KsApp.UserControls;
using KeySafe.ViewModels.Dependencies;

namespace KeySafe.KsApp.Views;

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

    public static Task<SafeItemEditResult> ShowAsync(Window parent, string name, string login, string password, string web, string note, string errorMessage)
    {
        var sourceItemWindow = new SourceItemWindow();

        var nameField = sourceItemWindow.FindControl<KsTextField>("NameField");
        var loginField = sourceItemWindow.FindControl<KsTextField>("LoginField");
        var webField = sourceItemWindow.FindControl<KsTextField>("WebField");
        var noteField = sourceItemWindow.FindControl<KsTextField>("NoteField");
        var passwordField = sourceItemWindow.FindControl<KsPasswordField>("PasswordField");
        var errorField = sourceItemWindow.FindControl<ErrorField>("ErrorField");
        var saveButton = sourceItemWindow.FindControl<Button>("SaveButton");
        var cancelButton = sourceItemWindow.FindControl<Button>("CancelButton");

        nameField.Text = name ?? string.Empty;
        loginField.Text = login ?? string.Empty;
        passwordField.Password = password ?? string.Empty;
        webField.Text = web ?? string.Empty;
        webField.Text = web ?? string.Empty;
        noteField.Text = note ?? string.Empty;

        var tcs = new TaskCompletionSource<SafeItemEditResult>();

        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            errorField.Show(errorMessage);
        }

        saveButton.Click += delegate
        {
            if (string.IsNullOrWhiteSpace(nameField.Text))
            {
                errorField.Show("Name field is required");
                return;
            }

            tcs.TrySetResult(new SafeItemEditResult()
            {
                Name = nameField.Text,
                Login = loginField.Text,
                Password = passwordField.Password,
                Web = webField.Text,
                Note = noteField.Text,
            });
            sourceItemWindow.Close();
        };

        cancelButton.Click += delegate
        {
            tcs.TrySetResult(new SafeItemEditResult(null, null, null, null, null, true));
            sourceItemWindow.Close();
        };

        sourceItemWindow.Closed += delegate { tcs.TrySetResult(new SafeItemEditResult(null, null, null, null, null, true)); };

        sourceItemWindow.ShowDialog(parent);
        return tcs.Task;
    }
}
