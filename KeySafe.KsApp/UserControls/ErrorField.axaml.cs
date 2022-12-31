namespace KeySafe.KsApp.UserControls;

public class ErrorField : UserControl
{
    public ErrorField()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void Show(string text)
    {
        var textBlock = this.FindControl<TextBlock>("ErrorTextBlock");
        textBlock.IsVisible = true;
        textBlock.Text = text;
    }
}
