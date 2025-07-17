using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using static System.Collections.Specialized.BitVector32;

#region C
using Basis;
using Windows.UI.Xaml;
using Obt;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Data;
#endregion

namespace MICAS
{
    public sealed partial class MainPage : Page
    {
        private void rtdg_Sorting(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumnEventArgs e)
        {
            //Use the Tag property to pass the bound column name for the sorting implementation
            if (e.Column.Tag.ToString() == "StationID")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.PrimaryInfo.StationID ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.PrimaryInfo.StationID descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
            }
            if (e.Column.Tag.ToString() == "StationType")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.PrimaryInfo.StationType descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.PrimaryInfo.StationType ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "Name")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.PrimaryInfo.Name descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.PrimaryInfo.Name ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "WindDirect")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.WindDirect == 9999, item.ObserveData.WindDirect descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.WindDirect ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "WindDirectDescription")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.WindDirectDescription descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.WindDirectDescription ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "WindSpeed")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.WindSpeed == 9999, item.ObserveData.WindSpeed descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.WindSpeed ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "GustDirect")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.GustDirect == 9999, item.ObserveData.GustDirect descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.GustDirect ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "GustDirectDescription")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.GustDirectDescription descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.GustDirectDescription ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "GustSpeed")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.GustSpeed == 9999, item.ObserveData.GustSpeed descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.GustSpeed ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "Temperature")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null)
                {
                    int doy = DateTime.Now.DayOfYear;
                    if (doy > 129 && doy < 312)//夏季：5月9日-11月8日
                    {
                        rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.Temperature == 9999, item.ObserveData.Temperature descending select item);
                        e.Column.SortDirection = DataGridSortDirection.Descending;
                    }
                    else
                    {
                        rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.Temperature ascending select item);
                        e.Column.SortDirection = DataGridSortDirection.Ascending;
                    }
                }
                else if (e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.Temperature == 9999, item.ObserveData.Temperature descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.Temperature ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "DewPoint")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.DewPoint == 9999, item.ObserveData.DewPoint descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.DewPoint ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "Pressure")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.Pressure == 9999, item.ObserveData.Pressure descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.Pressure ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "RH")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.RelativeHumidity == 9999, item.ObserveData.RelativeHumidity descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.RelativeHumidity ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "Rain")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.Rain == 9999, item.ObserveData.Rain descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.Rain ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "Visible")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.Visible == -9999, item.ObserveData.Visible ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
                else
                {
                    rtDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.ObserveData.Visible descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
            }

            //if (e.Column.Tag.ToString() == "area")
            //{
            //    //Implement sort on the column "Range" using LINQ
            //    if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
            //    {
            //        dataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.area descending select item);
            //        e.Column.SortDirection = DataGridSortDirection.Descending;
            //    }
            //    else
            //    {
            //        dataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsData orderby item.area ascending select item);
            //        e.Column.SortDirection = DataGridSortDirection.Ascending;
            //    }
            //}

            // add code to handle sorting by other columns as required

            // Remove sorting indicators from other columns
            foreach (var dgColumn in rtDataGrid.Columns)
            {
                if (dgColumn.Tag.ToString() != e.Column.Tag.ToString())
                {
                    dgColumn.SortDirection = null;
                }
            }
            rtGroup.IsChecked = false;

        }

        private void pkdg_Sorting(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridColumnEventArgs e)
        {
            if (e.Column.Tag.ToString() == "StationID")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.PrimaryInfo.StationID ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
                else
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.PrimaryInfo.StationID descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
            }
            if (e.Column.Tag.ToString() == "StationType")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.PrimaryInfo.StationType descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.PrimaryInfo.StationType ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "Name")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.PrimaryInfo.Name descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.PrimaryInfo.Name ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "MaxWindSpeed")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.MaxWindSpeed == 9999, item.StatisticData.MaxWindSpeed descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.MaxWindSpeed ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "MaxGustSpeed")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.MaxGustSpeed descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.MaxGustSpeed == -9999, item.StatisticData.MaxGustSpeed ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "MaxTemperature")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.MaxTemperature descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.MaxTemperature == -9999, item.StatisticData.MaxTemperature ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "MinTemperature")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.MinTemperature == -9999, item.StatisticData.MinTemperature ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
                else
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.MinTemperature descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
            }
            if (e.Column.Tag.ToString() == "DifTemperature")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.DifTemperature descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.DifTemperature == -9999, item.StatisticData.DifTemperature ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "Precip0808")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.Precip0808 descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.Precip0808 == -9999, item.StatisticData.Precip0808 ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            if (e.Column.Tag.ToString() == "DiffPressure")
            {
                //Implement sort on the column "Range" using LINQ
                if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Ascending)
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.DiffPressure descending select item);
                    e.Column.SortDirection = DataGridSortDirection.Descending;
                }
                else
                {
                    pkDataGrid.ItemsSource = new ObservableCollection<StationData>(from item in StationsStatistic orderby item.StatisticData.DiffPressure == -9999, item.StatisticData.DiffPressure ascending select item);
                    e.Column.SortDirection = DataGridSortDirection.Ascending;
                }
            }
            foreach (var dgColumn in pkDataGrid.Columns)
            {
                if (dgColumn.Tag.ToString() != e.Column.Tag.ToString())
                {
                    dgColumn.SortDirection = null;
                }
            }
            pkGroup.IsChecked = false;
        }
    }
}
