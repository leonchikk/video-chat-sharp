using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace VoiceChat.Desktop.Controls
{
    public partial class IconButton : UserControl
    {
        public PackIconKind Icon
        {
            get { return (PackIconKind)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("IconProperty", typeof(PackIconKind),
                                           typeof(IconButton));


        public IconButton()
        {
            InitializeComponent();
        }
    }
}
