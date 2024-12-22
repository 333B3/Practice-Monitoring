using System;
using System.Windows;

namespace Monitoring
{
    public partial class SettingsWindow : Window
    {
        public int Threshold { get; private set; } 
        public int UpdateInterval { get; private set; } 
        public SettingsWindow(int currentThreshold, int currentInterval)
        {
            InitializeComponent();
            ThresholdTextBox.Text = currentThreshold.ToString();
            IntervalTextBox.Text = currentInterval.ToString();
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Top = 200; 
            this.Left = 450; 
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ThresholdTextBox.Text, out int threshold) && int.TryParse(IntervalTextBox.Text, out int interval))
            {
                Threshold = threshold;
                UpdateInterval = interval;
                DialogResult = true; 
                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter valid numbers for threshold and interval.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        internal void SaveButton_Click(object value1, object value2)
        {
            throw new NotImplementedException();
        }
    }
}
