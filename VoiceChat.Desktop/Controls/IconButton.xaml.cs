using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VoiceChat.Desktop.Controls
{
    public partial class IconButton : UserControl
    {
        public PackIconKind? Icon
        {
            get 
            { 
                return (PackIconKind?)GetValue(IconProperty);
            }
            set 
            { 
                 SetValue(IconProperty, value); 
            }
        }

        public static DependencyProperty IconProperty =
            DependencyProperty.Register("IconProperty", typeof(PackIconKind?),
                                           typeof(IconButton));


        public static DependencyProperty ButtonCommandProperty = DependencyProperty.Register(
         "ButtonCommand",
         typeof(ICommand),
         typeof(IconButton));

        public ICommand ButtonCommand
        {
            get
            {
                return (ICommand)GetValue(ButtonCommandProperty);
            }

            set
            {
                SetValue(ButtonCommandProperty, value);
            }
        }

        public IconButton()
        {
            InitializeComponent();
        }
    }
}
