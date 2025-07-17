using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Telerik.Charting;
using Telerik.UI.Xaml.Controls.Chart;
using Windows.UI.Xaml.Data;

namespace MICAS
{
    public class AccuracyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double d = (double)value;
            if (Math.Abs(d) == 9999) return "-";
            string format = "{0:f1}";
            if (parameter != null) format = parameter.ToString();
            return string.Format(format, d);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return double.Parse(value.ToString());
        }
    }

    public class DoubleValidityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double d = (double)value;
            if (Math.Abs(d) == 9999) return "-";
            else return $"{d:f1}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class Int32ValidityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int v = (int)value;
            if (Math.Abs(v) == 9999) return "-";
            else return v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class StationTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int v = (int)value;
            return v switch
            {
                1     => "国家站",
                2     => "自动站",
                8     => "浮标站",
                8192  => "平台站",
                512   => "海岛",
                1024  => "沿海",
                _     => v.ToString()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class DataPointToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DataPoint point = value as DataPoint;
            if (point == null)
            {
                return value;
            }

            var series = point.Presenter as LineSeries;
            if (point.Parent == null || series == null)
            {
                return value;
            }

            return series.Stroke;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class AxisMinimumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double d = (double)value;
            return d == 0d ? 0 : d - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double d = (double)value;
            return d + 1;
        }
    }

    public class AxisMaximumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double d = (double)value;
            return d + 0.1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double d = (double)value;
            return d - 0.1;
        }
    }

    public class PrecipValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double d = (double)value;
            return d == 0 ? "" : $"{d:F1}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
