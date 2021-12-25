using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KeySafe.KsApp.UserControls
{
    public class SourceItemViewControl : UserControl
    {
        public SourceItemViewControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}