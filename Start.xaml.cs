using System.Windows;

namespace Monitoring
{
    public partial class Start : Window
    {
        public Start()
        {
            InitializeComponent();

            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Top = 200; 
            this.Left = 450; 
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
