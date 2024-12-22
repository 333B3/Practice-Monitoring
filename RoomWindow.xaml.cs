using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Monitoring
{
    public partial class RoomWindow : Window
    {
        private readonly int _temperatureThreshold;
        private readonly ObservableCollection<SensorData> _sensors;

        public RoomWindow(string roomName, List<SensorData> sensors, int temperatureThreshold)
        {
            InitializeComponent();

            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Top = 200; 
            this.Left = 60; 

            RoomNameTextBlock.Text = $"Room: {roomName}";
            _sensors = new ObservableCollection<SensorData>(sensors);

            foreach (var sensor in _sensors)
            {
                sensor.Status = sensor.Value > temperatureThreshold ? "Небезпека!" : "Норма";
                sensor.StatusColor = new SolidColorBrush(sensor.Value > temperatureThreshold ? Colors.Red : Colors.Green);
            }

            SensorsListView.ItemsSource = _sensors;
        }

    }
}
