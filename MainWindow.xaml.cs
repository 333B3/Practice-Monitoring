using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Monitoring
{
    public partial class MainWindow : Window
    {
        private readonly string[] ServerUrls = { "http://temperature2.local/", "http://temperature1.local/" };
        private int temperatureThreshold = 30;
        private int updateInterval = 10;
        private DispatcherTimer _timer;

        private readonly Dictionary<string, string> RoomTranslations = new Dictionary<string, string>
        {
            { "living room", "Вітальня" },
            { "kitchen", "Кухня" },
            { "bedroom", "Спальня" },
            { "bathroom", "Ванна кімната" },
            { "garage", "Гараж" },
            { "temperature", "Температура" }
        };
        public MainWindow()
        {
            InitializeComponent();
            LoadServerContent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(updateInterval);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Top = 200;
            this.Left = 450;
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            await LoadServerContent();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadServerContent();
        }

        private async Task LoadServerContent()
        {
            try
            {
                var allData = new List<SensorData>();

                using (HttpClient client = new HttpClient())
                {
                    foreach (var url in ServerUrls)
                    {
                        string jsonResponse = await client.GetStringAsync(url);
                        var data = ParseJson(jsonResponse);
                        allData.AddRange(data);
                    }
                }

                foreach (var sensor in allData)
                {
                    sensor.Status = sensor.Value > temperatureThreshold ? "Небезпека!" : "Норма";
                    sensor.StatusColor = new SolidColorBrush(sensor.Value > temperatureThreshold ? Colors.Red : Colors.Green);
                }

                DataListView.ItemsSource = allData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private List<SensorData> ParseJson(string json)
        {
            try
            {
                var data = JsonDocument.Parse(json);
                var result = new List<SensorData>();

                foreach (var item in data.RootElement.EnumerateArray())
                {
                    if (!item.TryGetProperty("name", out var nameProp) || nameProp.ValueKind != JsonValueKind.String)
                        continue;

                    string name = nameProp.GetString();

                    if (!item.TryGetProperty("sensors", out var sensorsProp) || sensorsProp.ValueKind != JsonValueKind.Array)
                        continue;

                    if (!string.IsNullOrEmpty(name) && RoomTranslations.ContainsKey(name.ToLower()))
                    {
                        name = RoomTranslations[name.ToLower()];
                    }

                    foreach (var sensor in sensorsProp.EnumerateArray())
                    {
                        if (!sensor.TryGetProperty("type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String)
                            continue;

                        if (!sensor.TryGetProperty("value", out var valueProp) || valueProp.ValueKind != JsonValueKind.Number)
                            continue;

                        string sensorType = typeProp.GetString();

                        if (RoomTranslations.ContainsKey(sensorType.ToLower()))
                        {
                            sensorType = RoomTranslations[sensorType.ToLower()];
                        }

                        var sensorData = new SensorData
                        {
                            Id = result.Count + 1,
                            Name = name,
                            Type = sensorType,
                            Value = valueProp.GetDouble()
                        };
                        result.Add(sensorData);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<SensorData>();
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(temperatureThreshold, updateInterval);

            if (settingsWindow.ShowDialog() == true)
            {
                temperatureThreshold = settingsWindow.Threshold;
                updateInterval = settingsWindow.UpdateInterval;

                _timer.Interval = TimeSpan.FromSeconds(updateInterval);
                LoadServerContent();
            }
        }

        private void DataListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedData = DataListView.SelectedItem as SensorData;
            if (selectedData == null) return;

            var sensorsForRoom = DataListView.ItemsSource
                .Cast<SensorData>()
                .Where(sensor => sensor.Name == selectedData.Name)
                .ToList();

            var roomWindow = new RoomWindow(selectedData.Name, sensorsForRoom, temperatureThreshold);
            roomWindow.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
            "Версія: 0.0.1\nРозробив: Кіндінов Іван\nГрупа 44",
            "Про програму",
            MessageBoxButton.OK,
            MessageBoxImage.Information
            );
        }
    }

    public class SensorData : System.ComponentModel.INotifyPropertyChanged
    {
        private double _value;
        private string _status;
        private SolidColorBrush _statusColor;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public double Value
        {
            get => _value;
            set
            {
                double roundedValue = Math.Round(value, 1);
                if (_value != roundedValue)
                {
                    _value = roundedValue;
                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set
            {
                if (_statusColor != value)
                {
                    _statusColor = value;
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
       protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
