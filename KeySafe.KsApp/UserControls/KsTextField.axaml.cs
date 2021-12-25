using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace KeySafe.KsApp.UserControls
{
    public class KsTextField : UserControl, IStyleable
    {
        Type IStyleable.StyleKey => typeof(KsTextField);
        
        public static readonly StyledProperty<string> DataProperty =
            AvaloniaProperty.Register<KsTextField, string>(nameof(Data));
        
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<KsTextField, string>(nameof(Title));
        
        public static readonly StyledProperty<bool> IsReadOnlyProperty =
            AvaloniaProperty.Register<KsTextField, bool>(nameof(IsReadOnly));
        
        public static readonly StyledProperty<bool> ShowCopyButtonProperty =
            AvaloniaProperty.Register<KsTextField, bool>(nameof(ShowCopyButton));

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

        public string Text
        {
            get => _textBox.Text;
            set => _textBox.Text = value;
        }

        private TextBox _textBox;

        public KsTextField()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _textBox = this.FindControl<TextBox>("KsTextBox");
        }

        private void CopyButton_OnClick(object sender, RoutedEventArgs e)
        {
            var text = _textBox.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                Application.Current.Clipboard.SetTextAsync(text);
            }
        }
    }
}
