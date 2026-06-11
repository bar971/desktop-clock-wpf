using System.Windows;
using System.Windows.Input;

namespace Clock
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Help : Window
    {
        public Help()
        {
            InitializeComponent();            
        }

        private void Window_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            HelpWindow.Close();
        }

        private void HelpWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            DragMove();
        }
    }
}
