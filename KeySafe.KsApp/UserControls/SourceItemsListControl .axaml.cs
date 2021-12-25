using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KeySafe.KsApp.UserControls
{
    public class SourceItemsListControl : UserControl
    {
        public SourceItemsListControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}