using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;

namespace Basis
{
    public class StationData : INotifyPropertyChanged
    {
        public PrimaryInfo PrimaryInfo { get; set; }
        public StatisticData StatisticData { get; set; }
        public ObserveData ObserveData { get; set; }

        private List<Moment> _History;
        public List<Moment> History
        {
            get { return _History; }
            set
            {
                _History = value;
                NotifyPropertyChanged(nameof(History));
            }
        }

        private UIBundle _UIBundle;
        public UIBundle UIBundle
        {
            get { return _UIBundle; }
            set 
            { 
                _UIBundle = value;
                NotifyPropertyChanged(nameof(UIBundle));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PrimaryInfo : INotifyPropertyChanged
    {
        public string StationID { get; set; }
        public int StationType { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Height { get; set; }
        public int ElementType { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Town { get; set; }
        public string TownCode { get; set; }
        public int IsSluice { get; set; }

        private string coordinate;
        public string Coordinate
        {
            get => coordinate;
            set
            {
                coordinate = value;
                NotifyPropertyChanged(nameof(Coordinate));
            }
        }

        private MapControl _Map;
        public MapControl Map
        {
            get { return _Map; }
            set
            {
                _Map = value;
                NotifyPropertyChanged(nameof(Map));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class StatisticData
    {
        public List<Item> Items { get; set; }
        public double MaxWindSpeed { get; set; }
        public double MaxGustSpeed { get; set; }
        public double MaxPressure { get; set; }
        public double MinPressure { get; set; }
        public double DiffPressure { get; set; }
        public double MaxTemperature { get; set; }
        public double MinTemperature { get; set; }
        public double DifTemperature { get; set; }
        public double Precip0808 { get; set; }
        public double Precip5Min { get; set; }
        public double Precip1H { get; set; }
        public double Precip3H { get; set; }
    }

    public class ObserveData
    {
        public int WindDirect { get; set; }
        public string WindDirectDescription { get; set; }
        public double WindSpeed { get; set; }
        public int MaxWindDirect { get; set; }
        public object MaxWindDirectDescription { get; set; }
        public double MaxWindSpeed { get; set; }
        public int GustDirect { get; set; }
        public string GustDirectDescription { get; set; }
        public double GustSpeed { get; set; }
        public double MaxGustSpeed { get; set; }
        public double Pressure { get; set; }
        public int RelativeHumidity { get; set; }
        public double DewPoint { get; set; }
        public double Temperature { get; set; }
        public double MaxTemperature { get; set; }
        public double MinTemperature { get; set; }
        public double Rain { get; set; }
        public double Visible { get; set; }
        public DateTimeOffset ObserveTime { get; set; }

    }

    public class Item
    {
        public string Description { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string ExtensionItem1 { get; set; }
        public string ExtensionItem2 { get; set; }
        public double Velocity { get; set; }
        public double Mean { get; set; }
        public int Direction { get; set; }
        public DateTimeOffset? AppearTime { get; set; }
    }

    public class Moment
    {
        public DateTimeOffset dt { get; set; }
        public double? t { get; set; }
        public double? wd3smaxdf { get; set; }
        public double? wd2df { get; set; }
        public double? p { get; set; }
        public double? rh { get; set; }
        public double? vis { get; set; }
        public double? precip { get; set; }
    }

    public class UIBundle
    {
        public double axisMaxT { get; set; }
        public double axisMinT { get; set; }
        public double axisMaxRH { get; set; }
        public double axisMinRH { get; set; }
        public double obsCnt { get; set; }
        public double precip72 { get; set; }
        public double precip48 { get; set; }
        public double precip24 { get; set; }
        public double precip12 { get; set; }
        public double precip6 { get; set; }
        public double precip1 { get; set; }
        public double? maxt { get; set; }
        public double? mint { get; set; }
        public double? maxw { get; set; }
        public double? maxg { get; set; }
        public double? precip0808 { get; set; }
    }

}
