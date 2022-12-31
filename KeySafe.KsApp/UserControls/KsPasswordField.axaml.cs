using Avalonia.Interactivity;
using Avalonia.Styling;

namespace KeySafe.KsApp.UserControls;

public class KsPasswordField : UserControl, IStyleable
{
    Type IStyleable.StyleKey => typeof(KsPasswordField);

    public static readonly StyledProperty<bool> RevealPasswordProperty =
        AvaloniaProperty.Register<KsPasswordField, bool>(nameof(RevealPassword), defaultValue: false);

    public static readonly StyledProperty<string> DataProperty =
        AvaloniaProperty.Register<KsPasswordField, string>(nameof(Data), defaultValue: string.Empty);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<KsPasswordField, string>(nameof(Title), defaultValue: string.Empty);

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<KsPasswordField, bool>(nameof(IsReadOnly), defaultValue: false);

    public static readonly StyledProperty<bool> ShowCopyButtonProperty =
        AvaloniaProperty.Register<KsPasswordField, bool>(nameof(ShowCopyButton), defaultValue: false);

    public string Password
    {
        get => _passwordBox.Text;
        set => _passwordBox.Text = value;
    }

    public string Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool ShowCopyButton
    {
        get => GetValue(ShowCopyButtonProperty);
        set => SetValue(ShowCopyButtonProperty, value);
    }

    public bool RevealPassword
    {
        get => GetValue(RevealPasswordProperty);
        set => SetValue(RevealPasswordProperty, value);
    }

    private TextBox _passwordBox;

    public KsPasswordField()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _passwordBox = this.FindControl<TextBox>("KsPasswordBox");
    }

    private void RevealButton_OnClick(object sender, RoutedEventArgs e)
    {
        _passwordBox.RevealPassword = true;
    }

    private void CopyButton_OnClick(object sender, RoutedEventArgs e)
    {
        var text = _passwordBox.Text;
        if (!string.IsNullOrWhiteSpace(text))
        {
            Application.Current.Clipboard.SetTextAsync(text);
        }
    }
}
