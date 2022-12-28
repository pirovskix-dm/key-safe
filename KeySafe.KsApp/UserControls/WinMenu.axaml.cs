using Avalonia.Platform;

namespace KeySafe.KsApp.UserControls;

public class WinMenu : UserControl
{
    public WinMenu()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        var runtimeInfo = AvaloniaLocator.Current.GetService<IRuntimePlatform>().GetRuntimeInfo();
        IsVisible = runtimeInfo is { IsDesktop: true, OperatingSystem: OperatingSystemType.WinNT };
    }
}