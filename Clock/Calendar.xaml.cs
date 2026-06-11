using System.Windows;
using System.Windows.Input;

namespace Clock
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Calendar : Window
    {
        public Calendar()
        {
            InitializeComponent();            
        }

        private void Calendar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { 
            e.Handled = true;
            DragMove();
        }

        private void Calendar_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {            
            Close();
        }
    }
}
