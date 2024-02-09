using System.Windows;

namespace ValthrunHelper
{
    public partial class RestartInfoWindow : Window
    {
        public RestartInfoWindow()
        {
            InitializeComponent();

            Topmost = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            new RestartInfoWindow().Show();
        }
    }
}
