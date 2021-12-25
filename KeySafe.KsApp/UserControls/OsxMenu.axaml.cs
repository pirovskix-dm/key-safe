using Avalonia.Platform;

namespace KeySafe.KsApp.UserControls;

public class OsxMenu : UserControl
{
    public OsxMenu()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        var runtimeInfo = AvaloniaLocator.Current.GetService<IRuntimePlatform>().GetRuntimeInfo();
        IsVisible = runtimeInfo.IsDesktop && runtimeInfo.OperatingSystem == OperatingSystemType.OSX;
    }
}