using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VoiceChat.Desktop.Controls
{
    /// <summary>
    /// Interaction logic for MicroButton.xaml
    /// </summary>
    public partial class MicroButton : UserControl
    {
        public static DependencyProperty SwitchCommandProperty = DependencyProperty.Register(
          "SwitchCommand",
          typeof(ICommand),
          typeof(MicroButton));

        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(SwitchCommandProperty);
            }

            set
            {
                SetValue(SwitchCommandProperty, value);
            }
        }

        public MicroButton()
        {
            InitializeComponent();
        }

        private void Rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            microOnBtn.ButtonCommand?.Execute(null);
        }
    }
}
