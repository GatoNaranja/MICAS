//#define BingSrcMap
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.UserDataTasks;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;

#region C
using Obt;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.Security.Cryptography;
using System.Reflection.Metadata.Ecma335;
using static Basis.Process;
using Windows.UI.Xaml.Input;
using Windows.UI.Core;
using System.Threading;
#endregion

namespace Basis
{
    public class Process
    {
        private static DateTimeOffset now;
        private static PointD BeijingRd = new PointD(23.122392145071757, 113.26414372867545);   //WGS84
        private static MapControl mapControl;
        private static MapElementsLayer landmarksLayer;
        private static bool _mapReconstruct;


        public void UpdateTiles(string[] jsons)
        {
            now = DateTimeOffset.Now;
            try
            {
                Spotlights spotlights = new Spotlights(jsons);
                Spotlights.Extremes events = spotlights.Distinguish();
                spotlights.UpdateTiles(events);
            }
            catch { }
        }

        public class GroupInfoCollection<T> : OrganizableCollection<T>
        {
            public object Key { get; set; }

            public new IEnumerator<T> GetEnumerator()
            {
                return (IEnumerator<T>)base.GetEnumerator();
            }
        }

        public interface IOrganizable<T>
        {
            void OrganizeStatisticData(string json);
        }

        public class OrganizableCollection<T> : ObservableCollection<T>, IOrganizable<T>
        {
            protected OrganizableCollection<T> Statistic;
            protected OrganizableCollection<T> NewStatistic;
            public OrganizableCollection() { }

            public OrganizableCollection(OrganizableCollection<T> Statistic)
            {
                this.Statistic = Statistic;
            }

            public virtual void OrganizeStatisticData(string json) { }
        }


        public class DiffPresCollection<T> : OrganizableCollection<T> where T : StationData, new()
        {
            public DiffPresCollection(OrganizableCollection<T> Statistic) : base(Statistic) { }

            public override void OrganizeStatisticData(string json)
            {
                if (json == null) return;

                NewStatistic = JsonConvert.DeserializeObject<OrganizableCollection<T>>(json);

                for (int i = 0; i < Statistic.Count; i++)
                {
                    foreach (T data in NewStatistic)
                    {
                        if (data.PrimaryInfo.StationID == Statistic[i].PrimaryInfo.StationID)
                        {
                            Statistic[i].StatisticData.DiffPressure = data.StatisticData.Items[0].Velocity;
                            NewStatistic.Remove(data);
                            break;
                        }

                        Statistic[i].StatisticData.DiffPressure = -9999;   //无数据情况
                    }
                }
            }
        }

        public static OrganizableCollection<StationData> OrganizeStatisticData(string[] jsons)
        {
            JArray MaxTJArr = JArray.Parse(jsons[1]);  //jsons里的值都是有可能请求失败导致空值的，但这个问题暂未处理，因为太好彩了还没遇到
            JArray MinTJArr = JArray.Parse(jsons[2]);
            JArray MaxRJArr = JArray.Parse(jsons[3]);
            JArray MaxWJArr = JArray.Parse(jsons[4]);
            JArray MaxGJArr = JArray.Parse(jsons[5]);
            OrganizableCollection<StationData> MaxTStatistic = JsonConvert.DeserializeObject<OrganizableCollection<StationData>>(jsons[1]);
            OrganizableCollection<StationData> MinTStatistic = JsonConvert.DeserializeObject<OrganizableCollection<StationData>>(jsons[2]);
            OrganizableCollection<StationData> MaxRStatistic = JsonConvert.DeserializeObject<OrganizableCollection<StationData>>(jsons[3]);
            OrganizableCollection<StationData> MaxWStatistic = JsonConvert.DeserializeObject<OrganizableCollection<StationData>>(jsons[4]);
            OrganizableCollection<StationData> MaxGStatistic = JsonConvert.DeserializeObject<OrganizableCollection<StationData>>(jsons[5]);

            //已验证：Parallel.ForEach<T> 无论如何也无法比越遍历越少的 for / foreach 更快；
            //且会造成线程安全问题；
            for (int i = 0; i < MaxTJArr.Count; i++)
            {
                MaxTStatistic[i].StatisticData.MaxTemperature = MaxTStatistic[i].StatisticData.Items[0].Velocity;
                foreach (StationData data in MinTStatistic)
                {
                    if (data.PrimaryInfo.StationID == MaxTStatistic[i].PrimaryInfo.StationID)
                    {
                        MaxTStatistic[i].StatisticData.MinTemperature = data.StatisticData.Items[0].Velocity;
                        if (Math.Min(MaxTStatistic[i].StatisticData.MaxTemperature, MaxTStatistic[i].StatisticData.MinTemperature) == -9999)
                            MaxTStatistic[i].StatisticData.DifTemperature = -9999;
                        else
                            MaxTStatistic[i].StatisticData.DifTemperature = MaxTStatistic[i].StatisticData.MaxTemperature - MaxTStatistic[i].StatisticData.MinTemperature;
                        MinTStatistic.Remove(data);
                        break;
                    }
                }
                foreach (StationData data in MaxRStatistic)
                {
                    if (data.PrimaryInfo.StationID == MaxTStatistic[i].PrimaryInfo.StationID)
                    {
                        MaxTStatistic[i].StatisticData.Precip0808 = data.StatisticData.Items[0].Velocity;
                        MaxRStatistic.Remove(data);
                        break;
                    }
                }
                foreach (StationData data in MaxWStatistic)
                {
                    if (data.PrimaryInfo.StationID == MaxTStatistic[i].PrimaryInfo.StationID)
                    {
                        MaxTStatistic[i].StatisticData.MaxWindSpeed = data.StatisticData.Items[0].Velocity;
                        MaxWStatistic.Remove(data);
                        break;
                    }
                }
                foreach (StationData data in MaxGStatistic)
                {
                    if (data.PrimaryInfo.StationID == MaxTStatistic[i].PrimaryInfo.StationID)
                    {
                        MaxTStatistic[i].StatisticData.MaxGustSpeed = data.StatisticData.Items[0].Velocity;
                        MaxGStatistic.Remove(data);
                        break;
                    }
                }
            }
            return MaxTStatistic;
        }

        public static async Task InitMap()
        {
            mapControl = new MapControl
            {
                MapServiceToken = Obt.DataSet.GenAccessToken(),
            };
            #region HdWrn
            //为了干掉2025年6月30日MapControl弃用后导致弹出的警告水印，也是拼了老命了；
            //窗口最小化还原后 MapControl 的 Visual Tree 会被重建，但节省资源起见，不能每次更新布局都遍历视觉树；
            mapControl.Loaded += (o, e) =>
            {
                HideMapWarning(mapControl);
            };
            mapControl.LoadingStatusChanged += (o, e) =>
            {
                if (mapControl.LoadingStatus == MapLoadingStatus.Loading)   //重建视觉树导致地图重新载入
                    _mapReconstruct = true;
            };
            mapControl.LayoutUpdated += (o, e) =>
            {
                if (_mapReconstruct)
                {
                    HideMapWarning(mapControl);
                    _mapReconstruct = false;
                }
            };

            await Task.Delay(55);

            #endregion HdWrn
        }

        public static void AddMapTileSource()
        {
#if (BingSrcMap)
            //因为DataGrid的RowDetails在滚动过程中会涉及GC主动释放资源，导致很多时候IRandomAccessStreamReference被释放掉导致莫名其妙的卡死或崩溃；
            //因此，在一幅地图下自行申请Bing地图服务，专门做地图软件，不见得会有什么问题，但要是对表格每一行动态地图申请，就需要结合UnloadingRowDetails
            //去掉typedEventHandler，再在LoadingRowDetails重新处理行选择；复杂倒不算复杂，不过我本人很懒，仍然实在是懒得处理；况且ArcGIS地图好像信息还多点；
            //那么这个问题就留给勤快的人吧；ArcGIS地图服务也还算新（一年以内），勉强够用；我们还是来祈求ArcGIS地图服务尽快更新为好；
            CustomMapTileDataSource BingDataSrc = new CustomMapTileDataSource();
            // Attach a handler for the BitmapRequested event.
            typedEventHandler = ReqBingSrcAsync;

            BingDataSrc.BitmapRequested += typedEventHandler;
            MapTileSource BingSrc = new MapTileSource(BingDataSrc);
            BingSrc.IsFadingEnabled = false;
#else
            //https://map.geoq.cn/ArcGIS/rest/services/ChinaOnlineCommunity/MapServer/tile/{zoomlevel}/{y}/{x}      //ArcGIS地图
            //https://mapnew.geoq.cn/ArcGIS/rest/services/ChinaOnlineCommunity/MapServer/tile/{zoomlevel}/{y}/{x}
            //https://mapnew.geoq.cn/ArcGIS/rest/services/ChinaOnlineCommunity_Mobile/MapServer/tile/{zoomlevel}/{y}/{x}
            //http://wprd04.is.autonavi.com/appmaptile?x={x}&y={y}&z={zoomlevel}&lang=zh_cn&size=1&scl=1&style=7    //高德地图2017版
            HttpMapTileDataSource dataSource = new HttpMapTileDataSource(
                "https://mapnew.geoq.cn/ArcGIS/rest/services/ChinaOnlineCommunity_Mobile/MapServer/tile/{zoomlevel}/{y}/{x}");

            // Create a tile source and add it to the Map control.
            MapTileSource tileSource = new MapTileSource(dataSource) { IsFadingEnabled = false };
#endif
#if (BingSrcMap)
                mapControl.TileSources.Add(BingSrc);
#else
            mapControl.TileSources.Add(tileSource);
#endif
        }

        public static void ProcessDetailMap(StationData data)
        {
            double Lat = data.PrimaryInfo.Latitude;
            double Lng = data.PrimaryInfo.Longitude;

            PointD Loc = new PointD();
            if (Cfg.GCJ02Station.Contains(data.PrimaryInfo.StationID))
            {
                Loc = new PointD(Lat, Lng);
                data.PrimaryInfo.                Coordinate = "GCJ02";
            }
            else if (Cfg.BD09Station.Contains(data.PrimaryInfo.StationID))
            {
                Loc = GeoTransformation.BD09_2_GCJ02(new PointD(Lat, Lng));
                data.PrimaryInfo.                Coordinate = "BD09";
            }
            else
            {
                Loc = GeoTransformation.WGS84_2_GCJ02(new PointD(Lng, Lat));
                data.PrimaryInfo.                Coordinate = "WGS84";
            }


            var landmarks = new List<MapElement>();

            BasicGeoposition snPosition = new BasicGeoposition { Latitude = Loc.X, Longitude = Loc.Y };
            Geopoint snPoint = new Geopoint(snPosition);

            var spaceNeedleIcon = new MapIcon
            {
                Location = snPoint,
                NormalizedAnchorPoint = new Point(0.5, 1.0),
                ZIndex = 0,
                Title = $"{data.PrimaryInfo.StationID} {data.PrimaryInfo.Name}"
            };

            landmarks.Add(spaceNeedleIcon);

            landmarksLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = landmarks
            };

            double distance = GeoTransformation.Range(BeijingRd.X, BeijingRd.Y, Loc.X, Loc.Y);
            double ZoomLevel;

            if (data.PrimaryInfo.City == "广州")
            {
                ZoomLevel = -0.1 * Math.Round(distance) + 14.5;
                //Debug.WriteLine($"{ZoomLevel}, {distance}");
                ZoomLevel = Math.Max(ZoomLevel, 9.4);
                ZoomLevel = Math.Min(ZoomLevel, 14);
            }
            else
            {
                ZoomLevel = -2.9 * Math.Tanh(0.043 * distance - 1.14) + 10.9 - Math.Tanh(0.02 * distance - 4);
                //Debug.WriteLine($"{ZoomLevel}, {distance}");
            }

            mapControl.ZoomLevel = ZoomLevel;
            mapControl.Center = snPoint;
            if(mapControl.Layers.Count > 0)
                mapControl.Layers.Clear();
            mapControl.Layers.Add(landmarksLayer);
            data.PrimaryInfo.Map = mapControl;
        }

        public static void HideMapWarning(MapControl map)
        {
            Trace.WriteLine("HideMapWarning called");

            var watermarkElements = FindVisualChildren<Border>(map)
                .Where(bd => bd.Background is SolidColorBrush brush
                             && brush.Color == Colors.White);
            foreach (var element in watermarkElements)
            {
                element.Visibility = Visibility.Collapsed;
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }


        public static List<Moment> ProcessDetailChart(string StationID, StationData PrecipHistory, OrganizableCollection<StationData>[] StationHistory, OrganizableCollection<StationData> StationsStatistic, DateTimeOffset ObsTm, out UIBundle uIBundle)
        {
            List<Moment> Moment = new List<Moment>();
            DateTimeOffset Dt = new DateTimeOffset(ObsTm.Year, ObsTm.Month, ObsTm.Day, ObsTm.Hour, ObsTm.Minute / 5 * 5, 0, TimeSpan.FromHours(8));
            int remainder = ObsTm.Minute % 5;
            if (remainder < 4 || remainder == 0)
                Dt = Dt.AddMinutes(-5);

            DateTimeOffset _8am = new DateTimeOffset(ObsTm.Year, ObsTm.Month, ObsTm.Day, 8, 0, 0, TimeSpan.FromHours(8));

            if (Dt <= _8am)    //时间未过当天8点
                _8am = _8am.AddDays(-1);

            DateTimeOffset Precip48Cutoff = Dt.AddHours(-48);
            DateTimeOffset Precip24Cutoff = Dt.AddHours(-24);
            DateTimeOffset Precip12Cutoff = Dt.AddHours(-12);
            DateTimeOffset Precip6Cutoff = Dt.AddHours(-6);
            DateTimeOffset Precip1Cutoff = Dt.AddHours(-1);
            DateTimeOffset SlideCutoff = Precip12Cutoff;

            int precip48rnd = -1;
            int precip24rnd = -1;
            int precip12rnd = -1;
            int precip6rnd = -1;
            int precip1rnd = -1;

            double Precip72 = 0;
            double Precip48 = 0;
            double Precip24 = 0;
            double Precip12 = 0;
            double Precip6 = 0;
            double Precip1 = 0;
            double Precip0808 = 0;
            Item lastItem = null;

            if(PrecipHistory == null)
            {
                for (int i = 0; i < Cfg.QueryRnds; i++)
                {
                    SlideCutoff = SlideCutoff.AddMinutes(15);
                    Moment History = new Moment
                    {
                        dt = SlideCutoff,
                        precip = 0
                    };
                    Moment.Add(History);
                }
            }
            else
            {
                if (PrecipHistory.StatisticData.Items.Count > 0) //初始化，获取首个降水结果
                {
                    Item item = PrecipHistory.StatisticData.Items[PrecipHistory.StatisticData.Items.Count - 1];
                    if (Dt.AddHours(-72) == (DateTimeOffset)item.AppearTime)
                    {
                        lastItem = item;
                    }
                    else
                    {
                        lastItem = new Item
                        {
                            AppearTime = Dt.AddHours(-72),
                            Velocity = 0
                        };
                    }
                }

                for (int i = PrecipHistory.StatisticData.Items.Count - 1; i >= 0; i--)
                {
                    Item item = PrecipHistory.StatisticData.Items[i];
                    if (Precip48Cutoff < (DateTimeOffset)item.AppearTime)
                    {
                        precip48rnd = i;
                        break;
                    }
                    DateTimeOffset currentDt = ((DateTimeOffset)item.AppearTime);
                    DateTimeOffset lastDt = ((DateTimeOffset)lastItem.AppearTime);
                    bool isDirect = currentDt.Minute != 0 && (currentDt - lastDt).TotalMinutes >= currentDt.Minute  //明确限制1小时累加制
                        || currentDt.Minute == 0 && (currentDt - lastDt).TotalMinutes >= 60;                        //针对整点做特殊处理
                    if (isDirect)
                        Precip72 += item.Velocity;
                    else
                    {
                        Precip72 += item.Velocity - lastItem.Velocity;
                    }
                    lastItem = item;
                }

                if (precip48rnd >= 0)
                {
                    for (int i = precip48rnd; i >= 0; i--)
                    {
                        Item item = PrecipHistory.StatisticData.Items[i];
                        if (Precip24Cutoff < (DateTimeOffset)item.AppearTime)
                        {
                            precip24rnd = i;
                            break;
                        }
                        //if (((DateTimeOffset)lastItem.AppearTime).Minute == 0
                        //                || lastItem.AppearTime <= Precip48Cutoff)
                        DateTimeOffset currentDt = ((DateTimeOffset)item.AppearTime);
                        DateTimeOffset lastDt = ((DateTimeOffset)lastItem.AppearTime);
                        bool isDirect = currentDt.Minute != 0 && (currentDt - lastDt).TotalMinutes >= currentDt.Minute  //明确限制1小时累加制
                            || currentDt.Minute == 0 && (currentDt - lastDt).TotalMinutes >= 60;                        //针对整点做特殊处理
                        if (isDirect)
                            Precip48 += item.Velocity;
                        else
                        {
                            Precip48 += item.Velocity - lastItem.Velocity;
                        }
                        lastItem = item;
                    }
                }

                if (precip24rnd >= 0)
                {
                    for (int i = precip24rnd; i >= 0; i--)
                    {
                        Item item = PrecipHistory.StatisticData.Items[i];
                        if (Precip12Cutoff < (DateTimeOffset)item.AppearTime)
                        {
                            precip12rnd = i;
                            break;
                        }
                        DateTimeOffset currentDt = ((DateTimeOffset)item.AppearTime);
                        DateTimeOffset lastDt = ((DateTimeOffset)lastItem.AppearTime);
                        bool isDirect = currentDt.Minute != 0 && (currentDt - lastDt).TotalMinutes >= currentDt.Minute  //明确限制1小时累加制
                            || currentDt.Minute == 0 && (currentDt - lastDt).TotalMinutes >= 60;                        //针对整点做特殊处理
                        double diff = 0;
                        if (isDirect)
                            diff += item.Velocity;
                        else
                        {
                            diff += item.Velocity - lastItem.Velocity;
                        }
                        Precip24 += diff;
                        if (_8am < (DateTimeOffset)item.AppearTime)
                            Precip0808 += diff;
                        lastItem = item;
                    }
                }

                if (precip12rnd >= 0)     //有录得降水的情况
                {
                    for (int i = 0; i < Cfg.QueryRnds; i++)
                    {
                        double Precip15m = 0;
                        SlideCutoff = SlideCutoff.AddMinutes(15);

                        for (int j = 0; j < 3; j++, precip12rnd--)
                        {
                            if (precip12rnd < 0) break;
                            Item item = PrecipHistory.StatisticData.Items[precip12rnd];

                            double diff = 0;
                            bool InWindow = SlideCutoff >= ((DateTimeOffset)item.AppearTime);
                            if (InWindow)
                            {
                                if (precip12rnd < PrecipHistory.StatisticData.Items.Count - 1)
                                {
                                    lastItem = PrecipHistory.StatisticData.Items[precip12rnd + 1];
                                    DateTimeOffset currentDt = ((DateTimeOffset)item.AppearTime);
                                    DateTimeOffset lastDt = ((DateTimeOffset)lastItem.AppearTime);
                                    bool isDirect = currentDt.Minute != 0 && (currentDt - lastDt).TotalMinutes >= currentDt.Minute  //明确限制1小时累加制
                                        || currentDt.Minute == 0 && (currentDt - lastDt).TotalMinutes >= 60;                        //针对整点做特殊处理
                                    if (isDirect)
                                        diff += item.Velocity;
                                    else
                                    {
                                        diff += item.Velocity - lastItem.Velocity;
                                    }
                                }
                                else
                                {
                                    diff += item.Velocity;
                                }

                                Precip15m += diff < 0 ? 0 : diff;

                                if (_8am < (DateTimeOffset)item.AppearTime)
                                    Precip0808 += diff;

                            }
                            else
                            {
                                break;
                            }
                        }
                        Precip12 += Precip15m;
                        if (i >= 24) Precip6 += Precip15m;
                        if (i >= 44) Precip1 += Precip15m;

                        Moment History = new Moment
                        {
                            dt = SlideCutoff,
                            precip = Precip15m
                        };
                        Moment.Add(History);
                    }
                }
                else            //没有录得降水
                {
                    for (int i = 0; i < Cfg.QueryRnds; i++)
                    {
                        SlideCutoff = SlideCutoff.AddMinutes(15);
                        Moment History = new Moment
                        {
                            dt = SlideCutoff,
                            precip = 0
                        };
                        Moment.Add(History);
                    }
                }
            }

            Precip24 += Precip12;
            Precip48 += Precip24;
            Precip72 += Precip48;

            double axisMaxT = -99;
            double axisMinT = 99;
            double axisMaxRH = 0;
            double axisMinRH = 0;
            int obsCnt = 0;

            for(int i = 0; i < Cfg.QueryRnds; i++)
            {
                StationData recall = null;      //处理该时刻没有该自动站数据的情况
                if (StationHistory[i] != null)  //处理没有返回数据的情况
                {
                    foreach (StationData data in StationHistory[i])
                    {
                        if (data.PrimaryInfo.StationID == StationID)
                        {
                            recall = data;
                            break;
                        }
                    }

                }
                if(recall != null)
                {
                    Moment[i].t = recall.ObserveData.Temperature < -99 || recall.ObserveData.Temperature > 99 ? null : (double?)recall.ObserveData.Temperature;
                    Moment[i].wd2df = recall.ObserveData.WindSpeed < 0 || recall.ObserveData.WindSpeed > 999 ? null : (double?)recall.ObserveData.WindSpeed;
                    Moment[i].wd3smaxdf = recall.ObserveData.GustSpeed < 0 || recall.ObserveData.GustSpeed > 999 ? null : (double?)recall.ObserveData.GustSpeed;
                    Moment[i].p = recall.ObserveData.Pressure < 300 || recall.ObserveData.Pressure > 1200 ? null : (double?)recall.ObserveData.Pressure;
                    Moment[i].rh = recall.ObserveData.RelativeHumidity < 0 || recall.ObserveData.RelativeHumidity > 100 ? null : (double?)recall.ObserveData.RelativeHumidity;
                    Moment[i].vis = recall.ObserveData.Visible < 0 || recall.ObserveData.Visible > 999 ? null : (double?)recall.ObserveData.Visible;
                    if (Moment[i].t != null)
                    {
                        axisMaxT = Math.Max((double)Moment[i].t, axisMaxT);
                        axisMinT = Math.Min((double)Moment[i].t, axisMinT);
                        axisMaxRH = Moment[i].rh == null ? 0 : 100;
                    }
                    obsCnt++;
                }
            }

            double? maxt = null;
            double? mint = null;
            double? maxw = null;
            double? maxg = null;
            double? precip0808 = null;

            foreach (StationData statistic in StationsStatistic)
            {
                if(statistic.PrimaryInfo.StationID == StationID)
                {
                    maxt = statistic.StatisticData.MaxTemperature;
                    mint = statistic.StatisticData.MinTemperature;
                    maxw = statistic.StatisticData.MaxWindSpeed;
                    maxg = statistic.StatisticData.MaxGustSpeed;
                    precip0808 = statistic.StatisticData.Precip0808;
                    break;
                }
            }

            uIBundle = new UIBundle
            {
                axisMaxT = axisMaxT,
                axisMinT = axisMinT,
                axisMaxRH = axisMaxRH,
                axisMinRH = axisMinRH,
                obsCnt = obsCnt,
                precip72 = Precip72,
                precip48 = Precip48,
                precip24 = Precip24,
                precip12 = Precip12,
                precip6 = Precip6,
                precip1 = Precip1,
                maxt = maxt,
                mint = mint,
                maxw = maxw,
                maxg = maxg,
                precip0808= Precip0808  //大写Precip0808：计算得来的数据（无延迟，逐5分钟有偏差）；小写precip0808：请求给出的数据（有延迟，更准）；
            };

            return Moment;
        }

#if (BingSrcMap)        
        public static TypedEventHandler<CustomMapTileDataSource, MapTileBitmapRequestedEventArgs> typedEventHandler;

        // Handle the BitmapRequested event.
        public static async void ReqBingSrcAsync(CustomMapTileDataSource sender, MapTileBitmapRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();
            IInputStream inputStream = null;
            args.Request.PixelData = await CreateBitmapAsStreamAsync(args.X, args.Y, args.ZoomLevel, inputStream);
            if (inputStream != null) inputStream.Dispose();
            deferral.Complete();
        }

        // Create the custom tiles.
        private static async Task<RandomAccessStreamReference> CreateBitmapAsStreamAsync(int X, int Y, int Z, IInputStream inputStream)
        {
            int pixelHeight = 256;
            int pixelWidth = 256;
            int bpp = 4;

            byte[] buffer = new byte[pixelHeight * pixelWidth * bpp];

            string QuadKey = TileXYToQuadKey(X, Y, Z);
            /*IInputStream */inputStream = await Obt.DataSet.GetBingMapStream(QuadKey);
            if (inputStream == null) return null;
            //using (InMemoryRandomAccessStream IRAStream = new InMemoryRandomAccessStream())
            //{
            InMemoryRandomAccessStream IRAStream = new InMemoryRandomAccessStream();
            await RandomAccessStream.CopyAsync(inputStream, IRAStream);
            IRAStream.Seek(0);
            BitmapDecoder bd = await BitmapDecoder.CreateAsync(IRAStream);
            ///获取数据         
            PixelDataProvider imageData = await bd.GetPixelDataAsync(
                BitmapPixelFormat.Rgba8,
                BitmapAlphaMode.Premultiplied,
                new BitmapTransform(),
                ExifOrientationMode.RespectExifOrientation,
                ColorManagementMode.DoNotColorManage
                );//获取二维数组

            buffer = imageData.DetachPixelData();
            //}
            inputStream.Dispose();

            // Create RandomAccessStream from byte array.
            //InMemoryRandomAccessStream randomAccessStream =
            //    new InMemoryRandomAccessStream();
            IOutputStream outputStream = IRAStream.GetOutputStreamAt(0);
            DataWriter writer = new DataWriter(outputStream);
            writer.WriteBytes(buffer);
            await writer.StoreAsync();
            await writer.FlushAsync();
            writer.Dispose();
            return RandomAccessStreamReference.CreateFromStream(IRAStream);
        }



        /// <summary>
        /// Converts tile XY coordinates into aQuadKey at a specified level of detail.
        /// </summary>
        /// <paramname="tileX">Tile X coordinate.</param>
        /// <paramname="tileY">Tile Y coordinate.</param>
        /// <paramname="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highestdetail).</param>
        /// <returns>A string containingthe QuadKey.</returns>
        public static string TileXYToQuadKey(int X, int Y, int Z)
        {
            StringBuilder quadKey = new StringBuilder();
            for (int i = Z; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);
                if ((X & mask) != 0)
                {
                    digit++;
                }
                if ((Y & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKey.Append(digit);
            }
            return quadKey.ToString();
        }
#endif


        internal class Spotlights
        {
            internal Spotlights(string[] jsons)
            {
                RealJArr = JArray.Parse(jsons[0]);  //jsons里的值都是有可能请求失败导致空值的，但这个问题暂未处理，因为太好彩了还没遇到
                MaxTJArr = JArray.Parse(jsons[1]);
                MinTJArr = JArray.Parse(jsons[2]);
                MaxRJArr = JArray.Parse(jsons[3]);
                MaxWJArr = JArray.Parse(jsons[4]);
                MaxGJArr = JArray.Parse(jsons[5]);
                JObject MaxTJObj = (JObject)MaxTJArr[0];
                JObject MinTJObj = (JObject)MinTJArr[MinTJArr.Count - 1];
                JObject MaxRJObj = (JObject)MaxRJArr[0];
                JObject MaxGJObj = (JObject)MaxGJArr[0];
                JObject MaxWJObj = (JObject)MaxWJArr[0];
                MaxTData = JsonConvert.DeserializeObject<StationData>(MaxTJObj.ToString());
                MinTData = JsonConvert.DeserializeObject<StationData>(MinTJObj.ToString());
                MaxRData = JsonConvert.DeserializeObject<StationData>(MaxRJObj.ToString());
                MaxGData = JsonConvert.DeserializeObject<StationData>(MaxGJObj.ToString());
                MaxWData = JsonConvert.DeserializeObject<StationData>(MaxWJObj.ToString());
                maxTemp     = MaxTData.StatisticData.Items[0].Velocity;
                minTemp     = MinTData.StatisticData.Items[0].Velocity;
                maxR0808    = MaxRData.StatisticData.Items[0].Velocity;
                gust        = MaxGData.StatisticData.Items[0].Velocity;
            }

            private JArray MaxTJArr { get; }
            private JArray MinTJArr { get; }
            private JArray MaxRJArr { get; }
            private JArray MaxWJArr { get; }
            private JArray MaxGJArr { get; }
            private JArray RealJArr { get; }


            private StationData MaxTData { get; }
            private StationData MinTData { get; }
            private StationData MaxRData { get; }
            private StationData MaxGData { get; }
            private StationData MaxWData { get; }
            private StationData[] MaxTs { get; set; }
            private StationData[] HighTs { get; set; }
            private StationData[] MinTs { get; set; }
            private StationData[] LowTs { get; set; }
            private StationData[] MaxRs { get; set; }
            private StationData[] MaxGs { get; set; }
            private StationData[] MaxWs { get; set; }
            private StationData G1044_Simp { get; set; }
            private StationData G1099_Simp { get; set; }
            private StationData G3241_Simp { get; set; }
            private StationData _59287_Simp { get; set; }


            private double maxTemp = double.MinValue;
            private double minTemp = double.MaxValue;
            private double maxR0808 = double.MinValue;
            private double gust = double.MinValue;


            internal enum Extremes
            {
                none = 0,
                highT = 0b0001,
                lowT = 0b0010,
                rain = 0b0100,
                windy = 0b1000
            }

            private Dictionary<Extremes, double> scores = new Dictionary<Extremes, double>();

            internal Extremes Distinguish()
            {
                Extremes events = 0;
                if (maxTemp > 33)
                {
                    events |= Extremes.highT;
                    scores.Add(Extremes.highT, maxTemp - 33);

                    OrganizeMaxTData();
                    OrganizeHighTData();
                }
                if (minTemp < 10)
                {
                    events |= Extremes.lowT;
                    scores.Add(Extremes.lowT, -(minTemp - 10));

                    OrganizeMinTData();
                    OrganizeLowTData();
                }
                if (maxR0808 > 0)
                {
                    events |= Extremes.rain;
                    scores.Add(Extremes.rain, 0.04761904761904761904761904761905 * (maxR0808 - 10));

                    OrganizeMaxR0808Data();
                }
                if (gust > 17.2)
                {
                    events |= Extremes.windy;
                    scores.Add(Extremes.windy, 0.63291139240506329113924050632911 * (gust - 17.2));

                    OrganizeMaxGustData();
                    OrganizeMaxWindData();
                }
                return events;
            }

            private void OrganizeMaxTData()
            {
                bool T0 = false, T1 = false, T2 = false;
                MaxTs = new StationData[3];
                MaxTs[0] = MaxTData;
                if (MaxTs[0].PrimaryInfo.StationID != "G1044")
                {
                    for (int i = 1; i < MaxTJArr.Count; i++)
                    {
                        if(MaxTJArr[i]["PrimaryInfo"]["StationID"].ToString() == "G1044" && !T2)
                        {
                            MaxTs[2] = JsonConvert.DeserializeObject<StationData>(MaxTJArr[i].ToString());
                            T2 = true;
                        }
                        else if (Cfg.Downtown.Contains(MaxTJArr[i]["PrimaryInfo"]["StationID"].ToString()) && !T1)
                        {
                            MaxTs[1] = JsonConvert.DeserializeObject<StationData>(MaxTJArr[i].ToString());
                            T1 = true;
                        }
                        if (T1 && T2) break;
                    }
                }
                else
                {
                    MaxTs[1] = JsonConvert.DeserializeObject<StationData>(MaxTJArr[1].ToString());
                    for (int i = 2; i < MaxTJArr.Count; i++)
                    {
                        if (Cfg.Downtown.Contains(MaxTJArr[i]["PrimaryInfo"]["StationID"].ToString()) && !T2)
                        {
                            MaxTs[2] = JsonConvert.DeserializeObject<StationData>(MaxTJArr[i].ToString());
                            break;
                        }
                    }
                }
                T0 = false; T1 = false; T2 = false;

                for (int i = 0; i < RealJArr.Count; i++)
                {
                    if (RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == MaxTs[0].PrimaryInfo.StationID && !T0)
                    {
                        MaxTs[0].ObserveData = new ObserveData
                        {
                            Temperature = (double)RealJArr[i]["ObserveData"]["Temperature"],
                            ObserveTime = (DateTimeOffset)RealJArr[i]["ObserveData"]["ObserveTime"]
                        };
                        T0 = true;
                    }
                    else if (MaxTs[1] != null && RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == MaxTs[1].PrimaryInfo.StationID && !T1)
                    {
                        MaxTs[1].ObserveData = new ObserveData
                        {
                            Temperature = (double)RealJArr[i]["ObserveData"]["Temperature"],
                            ObserveTime = (DateTimeOffset)RealJArr[i]["ObserveData"]["ObserveTime"]
                        };
                        T1 = true;
                    }
                    else if (MaxTs[2] != null && RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == MaxTs[2].PrimaryInfo.StationID && !T2)
                    {
                        MaxTs[2].ObserveData = new ObserveData
                        {
                            Temperature = (double)RealJArr[i]["ObserveData"]["Temperature"],
                            ObserveTime = (DateTimeOffset)RealJArr[i]["ObserveData"]["ObserveTime"]
                        };
                        T2 = true;
                    }
                    if (T0 && T1 && T2) break;
                }
            }

            private void OrganizeHighTData()
            {
                bool T0 = false, T1 = false, T2 = false;
                HighTs = new StationData[3];
                HighTs[0] = JsonConvert.DeserializeObject<StationData>(RealJArr[0].ToString());
                if (HighTs[0].PrimaryInfo.StationID != "G1044")
                {
                    for (int i = 1; i < RealJArr.Count; i++)
                    {
                        if (RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == "G1044" && !T2)
                        {
                            HighTs[2] = JsonConvert.DeserializeObject<StationData>(RealJArr[i].ToString());
                            T2 = true;
                        }
                        else if (Cfg.Downtown.Contains(RealJArr[i]["PrimaryInfo"]["StationID"].ToString()) && !T1)
                        {
                            HighTs[1] = JsonConvert.DeserializeObject<StationData>(RealJArr[i].ToString());
                            T1 = true;
                        }
                        if (T1 && T2) break;
                    }
                }
                else
                {
                    HighTs[1] = JsonConvert.DeserializeObject<StationData>(RealJArr[1].ToString());
                    for (int i = 2; i < MaxTJArr.Count; i++)
                    {
                        if (Cfg.Downtown.Contains(RealJArr[i]["PrimaryInfo"]["StationID"].ToString()) && !T2)
                        {
                            HighTs[2] = JsonConvert.DeserializeObject<StationData>(RealJArr[i].ToString());
                            break;
                        }
                    }
                }
                T0 = false; T1 = false; T2 = false;

                for (int i = 0; i < MaxTJArr.Count; i++)
                {
                    if (MaxTJArr[i]["PrimaryInfo"]["StationID"].ToString() == HighTs[0].PrimaryInfo.StationID && !T0)
                    {
                        List<Item> items = new List<Item>
                            {
                                new Item { Velocity = (double)MaxTJArr[i]["StatisticData"]["Items"][0]["Velocity"] }
                            };
                        HighTs[0].StatisticData = new StatisticData { Items = items };
                        T0 = true;
                    }
                    else if (HighTs[1] != null && MaxTJArr[i]["PrimaryInfo"]["StationID"].ToString() == HighTs[1].PrimaryInfo.StationID && !T1)
                    {
                        List<Item> items = new List<Item>
                            {
                                new Item { Velocity = (double)MaxTJArr[i]["StatisticData"]["Items"][0]["Velocity"] }
                            };
                        HighTs[1].StatisticData = new StatisticData { Items = items };
                        T1 = true;
                    }
                    else if (HighTs[2] != null && MaxTJArr[i]["PrimaryInfo"]["StationID"].ToString() == HighTs[2].PrimaryInfo.StationID && !T2)
                    {
                        List<Item> items = new List<Item>
                            {
                                new Item { Velocity = (double)MaxTJArr[i]["StatisticData"]["Items"][0]["Velocity"] }
                            };
                        HighTs[2].StatisticData = new StatisticData { Items = items };
                        T2 = true;
                    }
                    if (T0 && T1 && T2) break;
                }

            }

            private void OrganizeMinTData()
            {
                bool T0 = false, T1 = false, T2 = false;
                MinTs = new StationData[3];
                MinTs[0] = MinTData;
                if (MinTs[0].PrimaryInfo.StationID != "G1044")
                {
                    for (int i = MinTJArr.Count - 1; i >= 1; i--)
                    {
                        if (MinTJArr[i]["PrimaryInfo"]["StationID"].ToString() == "G1044" && !T2)
                        {
                            MinTs[2] = JsonConvert.DeserializeObject<StationData>(MinTJArr[i].ToString());
                            T2 = true;
                        }
                        else if (Cfg.Downtown.Contains(MinTJArr[i]["PrimaryInfo"]["StationID"].ToString())
                            && (double)MinTJArr[i]["PrimaryInfo"]["Height"] < 100   //超过100米低温作弊；
                            && !T1)
                        {
                            MinTs[1] = JsonConvert.DeserializeObject<StationData>(MinTJArr[i].ToString());
                            T1 = true;
                        }
                        if (T1 && T2) break;
                    }
                }
                else
                {
                    MinTs[1] = JsonConvert.DeserializeObject<StationData>(MinTJArr[1].ToString());
                    for (int i = MinTJArr.Count - 1; i >= 2; i--)
                    {
                        if (Cfg.Downtown.Contains(MinTJArr[i]["PrimaryInfo"]["StationID"].ToString())
                            && (double)MinTJArr[i]["PrimaryInfo"]["Height"] < 100   //超过100米低温作弊；
                            && !T2)
                        {
                            MinTs[2] = JsonConvert.DeserializeObject<StationData>(MinTJArr[i].ToString());
                            break;
                        }
                    }
                }
                T0 = false; T1 = false; T2 = false;

                for (int i = RealJArr.Count - 1; i >= 0; i--)
                {
                    if (RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == MinTs[0].PrimaryInfo.StationID && !T0)
                    {
                        MinTs[0].ObserveData = new ObserveData
                        {
                            Temperature = (double)RealJArr[i]["ObserveData"]["Temperature"],
                            ObserveTime = (DateTimeOffset)RealJArr[i]["ObserveData"]["ObserveTime"]
                        };
                        T0 = true;
                    }
                    else if (MinTs[1] != null && RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == MinTs[1].PrimaryInfo.StationID && !T1)
                    {
                        MinTs[1].ObserveData = new ObserveData
                        {
                            Temperature = (double)RealJArr[i]["ObserveData"]["Temperature"],
                            ObserveTime = (DateTimeOffset)RealJArr[i]["ObserveData"]["ObserveTime"]
                        };
                        T1 = true;
                    }
                    else if (MinTs[2] != null && RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == MinTs[2].PrimaryInfo.StationID && !T2)
                    {
                        MinTs[2].ObserveData = new ObserveData
                        {
                            Temperature = (double)RealJArr[i]["ObserveData"]["Temperature"],
                            ObserveTime = (DateTimeOffset)RealJArr[i]["ObserveData"]["ObserveTime"]
                        };
                        T2 = true;
                    }
                    if (T0 && T1 && T2) break;
                }
            }

            private void OrganizeLowTData()
            {
                bool T0 = false, T1 = false, T2 = false;
                LowTs = new StationData[3];
                LowTs[0] = JsonConvert.DeserializeObject<StationData>(RealJArr[RealJArr.Count - 1].ToString());
                if (LowTs[0].PrimaryInfo.StationID != "G1044")
                {
                    for (int i = RealJArr.Count - 1; i >= 1; i--)
                    {
                        if (RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == LowTs[0].PrimaryInfo.StationID) continue;
                        if (RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == "G1044" && !T2)
                        {
                            LowTs[2] = JsonConvert.DeserializeObject<StationData>(RealJArr[i].ToString());
                            T2 = true;
                        }
                        else if (Cfg.Downtown.Contains(RealJArr[i]["PrimaryInfo"]["StationID"].ToString())
                            && (double)RealJArr[i]["PrimaryInfo"]["Height"] < 100   //超过100米低温作弊；
                            && !T1)
                        {
                            LowTs[1] = JsonConvert.DeserializeObject<StationData>(RealJArr[i].ToString());
                            T1 = true;
                        }
                        if (T1 && T2) break;
                    }
                }
                else
                {
                    LowTs[1] = JsonConvert.DeserializeObject<StationData>(RealJArr[RealJArr.Count - 2].ToString());
                    for (int i = MinTJArr.Count - 1; i >= 2; i--)
                    {
                        if (Cfg.Downtown.Contains(RealJArr[i]["PrimaryInfo"]["StationID"].ToString()) 
                            && (double)RealJArr[i]["PrimaryInfo"]["Height"] < 100   //超过100米低温作弊；
                            && !T2)
                        {
                            LowTs[2] = JsonConvert.DeserializeObject<StationData>(RealJArr[i].ToString());
                            break;
                        }
                    }
                }
                T0 = false; T1 = false; T2 = false;

                for (int i = MinTJArr.Count - 1; i >= 0; i--)
                {
                    if (MinTJArr[i]["PrimaryInfo"]["StationID"].ToString() == LowTs[0].PrimaryInfo.StationID && !T0)
                    {
                        List<Item> items = new List<Item>
                            {
                                new Item { Velocity = (double)MinTJArr[i]["StatisticData"]["Items"][0]["Velocity"] }
                            };
                        LowTs[0].StatisticData = new StatisticData { Items = items };
                        T0 = true;
                    }
                    else if (LowTs[1] != null && MinTJArr[i]["PrimaryInfo"]["StationID"].ToString() == LowTs[1].PrimaryInfo.StationID && !T1)
                    {
                        List<Item> items = new List<Item>
                            {
                                new Item { Velocity = (double)MinTJArr[i]["StatisticData"]["Items"][0]["Velocity"] }
                            };
                        LowTs[1].StatisticData = new StatisticData { Items = items };
                        T1 = true;
                    }
                    else if (LowTs[2] != null && MinTJArr[i]["PrimaryInfo"]["StationID"].ToString() == LowTs[2].PrimaryInfo.StationID && !T2)
                    {
                        List<Item> items = new List<Item>
                            {
                                new Item { Velocity = (double)MinTJArr[i]["StatisticData"]["Items"][0]["Velocity"] }
                            };
                        LowTs[2].StatisticData = new StatisticData { Items = items };
                        T2 = true;
                    }
                    if (T0 && T1 && T2) break;
                }

            }

            private void OrganizeMaxR0808Data()
            {
                bool T0 = false, T1 = false, T2 = false;
                StationData[] substitute = new StationData[2];
                MaxRs = new StationData[3];
                MaxRs[0] = MaxRData;
                //条件A：最大值是G1044
                //条件A：最大值是59287
                bool A = MaxRs[0].PrimaryInfo.StationID == "59287";
                bool B = MaxRs[0].PrimaryInfo.StationID == "G1044";
                if (!A && !B)
                {
                    for (int i = 1; i < MaxRJArr.Count; i++)
                    {
                        if (MaxRJArr[i]["PrimaryInfo"]["StationID"].ToString() == "59287" && !T1)
                        {
                            MaxRs[1] = JsonConvert.DeserializeObject<StationData>(MaxRJArr[i].ToString());
                            T1 = true;
                        }
                        else if (MaxRJArr[i]["PrimaryInfo"]["StationID"].ToString() == "G1044" && !T2)
                        {
                            MaxRs[2] = JsonConvert.DeserializeObject<StationData>(MaxRJArr[i].ToString());
                            T2 = true;
                        }
                        else if (!T1 || !T2)
                        {
                            if (Cfg.Downtown.Contains(MaxRJArr[i]["PrimaryInfo"]["StationID"].ToString()))
                            {
                                if (MaxRs[1] == null)
                                    MaxRs[1] = JsonConvert.DeserializeObject<StationData>(MaxRJArr[i].ToString());
                                else if (MaxRs[2] == null)
                                    MaxRs[2] = JsonConvert.DeserializeObject<StationData>(MaxRJArr[i].ToString());
                            }
                        }
                        if (T1 && T2) break;
                    }
                }
                else if(A)
                {
                    for (int i = 1; i < MaxRJArr.Count; i++)
                    {
                        if (MaxRJArr[i]["PrimaryInfo"]["StationID"].ToString() == "G1044" && !T2)
                        {
                            MaxRs[2] = JsonConvert.DeserializeObject<StationData>(MaxRJArr[i].ToString());
                            T2 = true;
                        }
                        else if (!T2)   //T2还没找到
                        {
                            if (Cfg.Downtown.Contains(MaxRJArr[i]["PrimaryInfo"]["StationID"].ToString()))
                            {
                                if (MaxRs[1] == null)   //MaxRs[1]更优先，因为 MaxRs[2]有机会被G1044覆盖
                                {
                                    MaxRs[1] = JsonConvert.DeserializeObject<StationData>(MaxRJArr[i].ToString());
                                    T1 = true;
                                }
                                else if (MaxRs[2] == null)
                                    MaxRs[2] = JsonConvert.DeserializeObject<StationData>(MaxRJArr[i].ToString());
                            }
                        }
                        if (T1 && T2) break;
                    }
                }
                else if (B)
                {
                    for (int i = 1; i < MaxRJArr.Count; i++)
                    {
                        if (MaxRJArr[i]["PrimaryInfo"]["StationID"].ToString() == "59287" && !T1)
                        {
                            MaxRs[1] = JsonConvert.DeserializeObject<StationData>(MaxRJArr[i].ToString());
                            T1 = true;
                        }
                        else if (!T1)   //T1还没找到
                        {
                            if (MaxRs[2] == null)    //MaxRs[2]更优先，因为 MaxRs[1]有机会被59287覆盖
                            {
                                MaxRs[2] = JsonConvert.DeserializeObject<StationData>(MaxRJArr[i].ToString());
                                T2 = true;
                            }
                            else if (MaxRs[1] == null)
                                MaxRs[1] = JsonConvert.DeserializeObject<StationData>(MaxRJArr[i].ToString());
                        }
                        if (T1 && T2) break;
                    }
                }
                T0 = false; T1 = false; T2 = false;

                for (int i = 0; i < RealJArr.Count; i++)
                {
                    if (RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == MaxRs[0].PrimaryInfo.StationID && !T0)
                    {
                        MaxRs[0].ObserveData = new ObserveData
                        {
                            Rain = (double)RealJArr[i]["ObserveData"]["Rain"],
                            ObserveTime = (DateTimeOffset)RealJArr[i]["ObserveData"]["ObserveTime"]
                        };
                        T0 = true;
                    }
                    else if (MaxRs[1] != null && RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == MaxRs[1].PrimaryInfo.StationID && !T1)
                    {
                        MaxRs[1].ObserveData = new ObserveData
                        {
                            Rain = (double)RealJArr[i]["ObserveData"]["Rain"],
                            ObserveTime = (DateTimeOffset)RealJArr[i]["ObserveData"]["ObserveTime"]
                        };
                        T1 = true;
                    }
                    else if (MaxRs[2] != null && RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == MaxRs[2].PrimaryInfo.StationID && !T2)
                    {
                        MaxRs[2].ObserveData = new ObserveData
                        {
                            Rain = (double)RealJArr[i]["ObserveData"]["Rain"],
                            ObserveTime = (DateTimeOffset)RealJArr[i]["ObserveData"]["ObserveTime"]
                        };
                        T2 = true;
                    }
                    if (T0 && T1 && T2) break;
                }

            }

            private void OrganizeMaxGustData()
            {
                bool T0 = false, T1 = false, T2 = false;
                MaxGs = new StationData[3];
                MaxGs[0] = MaxGData;
                if (MaxGs[0].PrimaryInfo.StationID != "G1044")
                {
                    for (int i = 1; i < MaxGJArr.Count; i++)
                    {
                        if (MaxGJArr[i]["PrimaryInfo"]["StationID"].ToString() == "G1044" && !T2)
                        {
                            MaxGs[2] = JsonConvert.DeserializeObject<StationData>(MaxGJArr[i].ToString());
                            T2 = true;
                        }
                        else if (Cfg.Downtown.Contains(MaxGJArr[i]["PrimaryInfo"]["StationID"].ToString())
                            && (double)MaxGJArr[i]["PrimaryInfo"]["Height"] < 100   //超过100米风速作弊；
                            && !T1)
                        {
                            MaxGs[1] = JsonConvert.DeserializeObject<StationData>(MaxGJArr[i].ToString());
                            T1 = true;
                        }
                        if (T1 && T2) break;
                    }
                }
                else
                {
                    MaxGs[1] = JsonConvert.DeserializeObject<StationData>(MaxGJArr[1].ToString());
                    for (int i = 2; i < MaxGJArr.Count; i++)
                    {
                        if (Cfg.Downtown.Contains(MaxGJArr[i]["PrimaryInfo"]["StationID"].ToString())
                            && (double)MaxGJArr[i]["PrimaryInfo"]["Height"] < 100   //超过100米风速作弊；
                            && !T2)
                        {
                            MaxGs[2] = JsonConvert.DeserializeObject<StationData>(MaxGJArr[i].ToString());
                            break;
                        }
                    }
                }
                T0 = false; T1 = false; T2 = false;

                for (int i = 0; i < MaxWJArr.Count; i++)
                {
                    if (MaxWJArr[i]["PrimaryInfo"]["StationID"].ToString() == MaxGs[0].PrimaryInfo.StationID && !T0)
                    {
                        MaxGs[0].StatisticData.Items[0].Mean = (double)MaxWJArr[i]["StatisticData"]["Items"][0]["Velocity"];
                        T0 = true;
                    }
                    else if (MaxGs[1] != null && MaxWJArr[i]["PrimaryInfo"]["StationID"].ToString() == MaxGs[1].PrimaryInfo.StationID && !T1)
                    {
                        MaxGs[1].StatisticData.Items[0].Mean = (double)MaxWJArr[i]["StatisticData"]["Items"][0]["Velocity"];
                        T1 = true;
                    }
                    else if (MaxGs[2] != null && MaxWJArr[i]["PrimaryInfo"]["StationID"].ToString() == MaxGs[2].PrimaryInfo.StationID && !T2)
                    {
                        MaxGs[2].StatisticData.Items[0].Mean = (double)MaxWJArr[i]["StatisticData"]["Items"][0]["Velocity"];
                        T2 = true;
                    }
                    if (T0 && T1 && T2) break;
                }
            }


            private void OrganizeMaxWindData()
            {
                bool T0 = false, T1 = false, T2 = false;
                MaxWs = new StationData[3];
                MaxWs[0] = MaxWData;
                if (MaxWs[0].PrimaryInfo.StationID != "G1044")
                {
                    for (int i = 1; i < MaxWJArr.Count; i++)
                    {
                        if (MaxWJArr[i]["PrimaryInfo"]["StationID"].ToString() == "G1044" && !T2)
                        {
                            MaxWs[2] = JsonConvert.DeserializeObject<StationData>(MaxWJArr[i].ToString());
                            T2 = true;
                        }
                        else if (Cfg.Downtown.Contains(MaxWJArr[i]["PrimaryInfo"]["StationID"].ToString())
                            && (double)MaxWJArr[i]["PrimaryInfo"]["Height"] < 100   //超过100米风速作弊；
                            && !T1)
                        {
                            MaxWs[1] = JsonConvert.DeserializeObject<StationData>(MaxWJArr[i].ToString());
                            T1 = true;
                        }
                        if (T1 && T2) break;
                    }
                }
                else
                {
                    MaxWs[1] = JsonConvert.DeserializeObject<StationData>(MaxWJArr[1].ToString());
                    for (int i = 2; i < MaxWJArr.Count; i++)
                    {
                        if (Cfg.Downtown.Contains(MaxWJArr[i]["PrimaryInfo"]["StationID"].ToString())
                            && (double)MaxWJArr[i]["PrimaryInfo"]["Height"] < 100   //超过100米风速作弊；
                            && !T2)
                        {
                            MaxWs[2] = JsonConvert.DeserializeObject<StationData>(MaxWJArr[i].ToString());
                            break;
                        }
                    }
                }
                T0 = false; T1 = false; T2 = false;

                for (int i = 0; i < MaxGJArr.Count; i++)
                {
                    if (MaxGJArr[i]["PrimaryInfo"]["StationID"].ToString() == MaxWs[0].PrimaryInfo.StationID && !T0)
                    {
                        MaxWs[0].StatisticData.Items[0].Mean = MaxWs[0].StatisticData.Items[0].Velocity;
                        MaxWs[0].StatisticData.Items[0].Velocity = (double)MaxGJArr[i]["StatisticData"]["Items"][0]["Velocity"];
                        T0 = true;
                    }
                    else if (MaxWs[1] != null && MaxGJArr[i]["PrimaryInfo"]["StationID"].ToString() == MaxWs[1].PrimaryInfo.StationID && !T1)
                    {
                        MaxWs[1].StatisticData.Items[0].Mean = MaxWs[1].StatisticData.Items[0].Velocity;
                        MaxWs[1].StatisticData.Items[0].Velocity = (double)MaxGJArr[i]["StatisticData"]["Items"][0]["Velocity"];
                        T1 = true;
                    }
                    else if (MaxWs[2] != null && MaxGJArr[i]["PrimaryInfo"]["StationID"].ToString() == MaxWs[2].PrimaryInfo.StationID && !T2)
                    {
                        MaxWs[2].StatisticData.Items[0].Mean = MaxWs[2].StatisticData.Items[0].Velocity;
                        MaxWs[2].StatisticData.Items[0].Velocity = (double)MaxGJArr[i]["StatisticData"]["Items"][0]["Velocity"];
                        T2 = true;
                    }
                    if (T0 && T1 && T2) break;
                }
            }

            private void OrganizeSimpleData()
            {
                bool G1099Completed = false, G3241Completed = false, _59287_Completed = false, G1044Completed = false;

                for (int i = 0; i < RealJArr.Count; i++)
                {
                    if (RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == "59287" && !_59287_Completed)
                    {
                        _59287_Simp = JsonConvert.DeserializeObject<StationData>(RealJArr[i].ToString());
                        _59287_Completed = true;
                    }
                    else if (RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == "G1099" && !G1099Completed)
                    {
                        G1099_Simp = JsonConvert.DeserializeObject<StationData>(RealJArr[i].ToString());
                        G1099Completed = true;
                    }
                    else if (RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == "G3241" && !G3241Completed)
                    {
                        G3241_Simp = JsonConvert.DeserializeObject<StationData>(RealJArr[i].ToString());
                        G3241Completed = true;
                    }
                    else if (RealJArr[i]["PrimaryInfo"]["StationID"].ToString() == "G1044" && !G1044Completed)
                    {
                        G1044_Simp = JsonConvert.DeserializeObject<StationData>(RealJArr[i].ToString());
                        G1044Completed = true;
                    }
                    if (_59287_Completed && G1099Completed && G3241Completed && G1044Completed) break;
                }

            }


            internal void UpdateTiles(Extremes events)
            {
                TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Clear();
                switch (Convert.ToInt32(events))
                {
                    case 0b0000:
                        OrganizeSimpleData();
                        SimpleTileHandler handler = (SimpleTileHandler)handleSimple();
                        handler.UpdatePrimaryTile(updater);
                        break;
                    case 0b0001:
                        {
                            MaxTTileHandler maxt_handler = (MaxTTileHandler)handleMaxT();
                            HighTTileHandler temp_handler = (HighTTileHandler)handleHighT();
                            if (now.Hour >= 8 && now.Hour < 18)
                            {
                                maxt_handler.UpdatePrimaryTile(updater);
                                temp_handler.UpdatePrimaryTile(updater);
                            }
                            else
                            {
                                temp_handler.UpdatePrimaryTile(updater);
                                maxt_handler.UpdatePrimaryTile(updater);
                            }
                        }
                        break;
                    case 0b0010:
                        {
                            MinTTileHandler mint_handler = (MinTTileHandler)handleMinT();
                            LowTTileHandler temp_handler = (LowTTileHandler)handleLowT();
                            if (now.Hour >= 18 || now.Hour < 8)
                            {
                                mint_handler.UpdatePrimaryTile(updater);
                                temp_handler.UpdatePrimaryTile(updater);
                            }
                            else
                            {
                                temp_handler.UpdatePrimaryTile(updater);
                                mint_handler.UpdatePrimaryTile(updater);
                            }
                        }
                        break;
                    case 0b0011:
                        scores = scores.OrderByDescending(s => s.Value).ToDictionary(s => s.Key, s => s.Value);
                        foreach (Extremes _event in scores.Keys)
                        {
                            if (_event == Extremes.highT)
                            {
                                MaxTTileHandler maxt_handler = (MaxTTileHandler)handleMaxT();
                                HighTTileHandler temp_handler = (HighTTileHandler)handleHighT();
                                if (now.Hour >= 8 && now.Hour < 18)
                                {
                                    maxt_handler.UpdatePrimaryTile(updater);
                                    temp_handler.UpdatePrimaryTile(updater);
                                }
                                else
                                {
                                    temp_handler.UpdatePrimaryTile(updater);
                                    maxt_handler.UpdatePrimaryTile(updater);
                                }
                            }
                            else
                            {
                                MinTTileHandler mint_handler = (MinTTileHandler)handleMinT();
                                LowTTileHandler temp_handler = (LowTTileHandler)handleLowT();
                                if (now.Hour >= 18 || now.Hour < 8)
                                {
                                    mint_handler.UpdatePrimaryTile(updater);
                                    temp_handler.UpdatePrimaryTile(updater);
                                }
                                else
                                {
                                    temp_handler.UpdatePrimaryTile(updater);
                                    mint_handler.UpdatePrimaryTile(updater);
                                }
                            }
                        }
                        break;
                    case 0b0100:
                        {
                            R0808TileHandler r0808_handler = (R0808TileHandler)handleR0808();
                            r0808_handler.UpdatePrimaryTile(updater);
                        }
                        break;
                    case 0b0101:
                        scores = scores.OrderByDescending(s => s.Value).ToDictionary(s => s.Key, s => s.Value);
                        foreach (Extremes _event in scores.Keys)
                        {
                            if (_event == Extremes.highT)
                            {
                                MaxTTileHandler maxt_handler = (MaxTTileHandler)handleMaxT();
                                HighTTileHandler temp_handler = (HighTTileHandler)handleHighT();
                                if (now.Hour >= 8 && now.Hour < 18)
                                {
                                    maxt_handler.UpdatePrimaryTile(updater);
                                    temp_handler.UpdatePrimaryTile(updater);
                                }
                                else
                                {
                                    temp_handler.UpdatePrimaryTile(updater);
                                    maxt_handler.UpdatePrimaryTile(updater);
                                }
                            }
                            else
                            {
                                R0808TileHandler r0808_handler = (R0808TileHandler)handleR0808();
                                r0808_handler.UpdatePrimaryTile(updater);
                            }
                        }
                        break;
                    case 0b0110:
                        scores = scores.OrderByDescending(s => s.Value).ToDictionary(s => s.Key, s => s.Value);
                        foreach (Extremes _event in scores.Keys)
                        {
                            if (_event == Extremes.lowT)
                            {
                                MinTTileHandler mint_handler = (MinTTileHandler)handleMinT();
                                LowTTileHandler temp_handler = (LowTTileHandler)handleLowT();
                                if (now.Hour >= 18 || now.Hour < 8)
                                {
                                    mint_handler.UpdatePrimaryTile(updater);
                                    temp_handler.UpdatePrimaryTile(updater);
                                }
                                else
                                {
                                    temp_handler.UpdatePrimaryTile(updater);
                                    mint_handler.UpdatePrimaryTile(updater);
                                }
                            }
                            else
                            {
                                R0808TileHandler r0808_handler = (R0808TileHandler)handleR0808();
                                r0808_handler.UpdatePrimaryTile(updater);
                            }
                        }
                        break;
                    case 0b0111:
                        scores = scores.OrderByDescending(s => s.Value).ToDictionary(s => s.Key, s => s.Value);
                        foreach (Extremes _event in scores.Keys)
                        {
                            if (_event == Extremes.highT)
                            {
                                MaxTTileHandler maxt_handler = (MaxTTileHandler)handleMaxT();
                                HighTTileHandler temp_handler = (HighTTileHandler)handleHighT();
                                if (now.Hour >= 8 && now.Hour < 18)
                                {
                                    maxt_handler.UpdatePrimaryTile(updater);
                                    temp_handler.UpdatePrimaryTile(updater);
                                }
                                else
                                {
                                    temp_handler.UpdatePrimaryTile(updater);
                                    maxt_handler.UpdatePrimaryTile(updater);
                                }
                            }
                            else if (_event == Extremes.lowT)
                            {
                                MinTTileHandler mint_handler = (MinTTileHandler)handleMinT();
                                LowTTileHandler temp_handler = (LowTTileHandler)handleLowT();
                                if (now.Hour >= 18 || now.Hour < 8)
                                {
                                    mint_handler.UpdatePrimaryTile(updater);
                                    temp_handler.UpdatePrimaryTile(updater);
                                }
                                else
                                {
                                    temp_handler.UpdatePrimaryTile(updater);
                                    mint_handler.UpdatePrimaryTile(updater);
                                }
                            }
                            else
                            {
                                R0808TileHandler r0808_handler = (R0808TileHandler)handleR0808();
                                r0808_handler.UpdatePrimaryTile(updater);
                            }
                        }
                        break;
                    case 0b1000:
                        {
                            MaxGTileHandler maxg_handler = (MaxGTileHandler)handleMaxG();
                            MaxWTileHandler maxw_handler = (MaxWTileHandler)handleMaxW();
                            maxg_handler.UpdatePrimaryTile(updater);
                            maxw_handler.UpdatePrimaryTile(updater);
                        }
                        break;
                    case 0b1001:
                        scores = scores.OrderByDescending(s => s.Value).ToDictionary(s => s.Key, s => s.Value);
                        foreach (Extremes _event in scores.Keys)
                        {
                            if (_event == Extremes.highT)
                            {
                                MaxTTileHandler maxt_handler = (MaxTTileHandler)handleMaxT();
                                HighTTileHandler temp_handler = (HighTTileHandler)handleHighT();
                                if (now.Hour >= 8 && now.Hour < 18)
                                {
                                    maxt_handler.UpdatePrimaryTile(updater);
                                    temp_handler.UpdatePrimaryTile(updater);
                                }
                                else
                                {
                                    temp_handler.UpdatePrimaryTile(updater);
                                    maxt_handler.UpdatePrimaryTile(updater);
                                }
                            }
                            else
                            {
                                MaxGTileHandler maxg_handler = (MaxGTileHandler)handleMaxG();
                                MaxWTileHandler maxw_handler = (MaxWTileHandler)handleMaxW();
                                maxg_handler.UpdatePrimaryTile(updater);
                                maxw_handler.UpdatePrimaryTile(updater);
                            }
                        }
                        break;
                    case 0b1010:
                        scores = scores.OrderByDescending(s => s.Value).ToDictionary(s => s.Key, s => s.Value);
                        foreach (Extremes _event in scores.Keys)
                        {
                            if (_event == Extremes.lowT)
                            {
                                MinTTileHandler mint_handler = (MinTTileHandler)handleMinT();
                                LowTTileHandler temp_handler = (LowTTileHandler)handleLowT();
                                if (now.Hour >= 18 || now.Hour < 8)
                                {
                                    mint_handler.UpdatePrimaryTile(updater);
                                    temp_handler.UpdatePrimaryTile(updater);
                                }
                                else
                                {
                                    temp_handler.UpdatePrimaryTile(updater);
                                    mint_handler.UpdatePrimaryTile(updater);
                                }
                            }
                            else
                            {
                                MaxGTileHandler maxg_handler = (MaxGTileHandler)handleMaxG();
                                MaxWTileHandler maxw_handler = (MaxWTileHandler)handleMaxW();
                                maxg_handler.UpdatePrimaryTile(updater);
                                maxw_handler.UpdatePrimaryTile(updater);
                            }
                        }
                        break;
                    case 0b1011:
                        scores = scores.OrderByDescending(s => s.Value).ToDictionary(s => s.Key, s => s.Value);
                        foreach (Extremes _event in scores.Keys)
                        {
                            if (_event == Extremes.highT)
                            {
                                MaxTTileHandler maxt_handler = (MaxTTileHandler)handleMaxT();
                                HighTTileHandler temp_handler = (HighTTileHandler)handleHighT();
                                if (now.Hour >= 8 && now.Hour < 18)
                                {
                                    maxt_handler.UpdatePrimaryTile(updater);
                                    temp_handler.UpdatePrimaryTile(updater);
                                }
                                else
                                {
                                    temp_handler.UpdatePrimaryTile(updater);
                                    maxt_handler.UpdatePrimaryTile(updater);
                                }
                            }
                            else if (_event == Extremes.lowT)
                            {
                                MinTTileHandler mint_handler = (MinTTileHandler)handleMinT();
                                mint_handler.UpdatePrimaryTile(updater);
                            }
                            else
                            {
                                MaxGTileHandler maxg_handler = (MaxGTileHandler)handleMaxG();
                                MaxWTileHandler maxw_handler = (MaxWTileHandler)handleMaxW();
                                maxg_handler.UpdatePrimaryTile(updater);
                                maxw_handler.UpdatePrimaryTile(updater);
                            }
                        }
                        break;
                    case 0b1100:
                        scores = scores.OrderByDescending(s => s.Value).ToDictionary(s => s.Key, s => s.Value);
                        foreach (Extremes _event in scores.Keys)
                        {
                            if (_event == Extremes.rain)
                            {
                                R0808TileHandler R0808_handler = (R0808TileHandler)handleR0808();
                                R0808_handler.UpdatePrimaryTile(updater);
                            }
                            else
                            {
                                MaxGTileHandler maxg_handler = (MaxGTileHandler)handleMaxG();
                                MaxWTileHandler maxw_handler = (MaxWTileHandler)handleMaxW();
                                maxg_handler.UpdatePrimaryTile(updater);
                                maxw_handler.UpdatePrimaryTile(updater);
                            }
                        }
                        break;
                    case 0b1101:
                        scores = scores.OrderByDescending(s => s.Value).ToDictionary(s => s.Key, s => s.Value);
                        foreach (Extremes _event in scores.Keys)
                        {
                            if (_event == Extremes.highT)
                            {
                                MaxTTileHandler maxt_handler = (MaxTTileHandler)handleMaxT();
                                HighTTileHandler temp_handler = (HighTTileHandler)handleHighT();
                                if (now.Hour >= 8 && now.Hour < 18)
                                {
                                    maxt_handler.UpdatePrimaryTile(updater);
                                    temp_handler.UpdatePrimaryTile(updater);
                                }
                                else
                                {
                                    temp_handler.UpdatePrimaryTile(updater);
                                    maxt_handler.UpdatePrimaryTile(updater);
                                }
                            }
                            else if (_event == Extremes.rain)
                            {
                                R0808TileHandler R0808_handler = (R0808TileHandler)handleR0808();
                                R0808_handler.UpdatePrimaryTile(updater);
                            }
                            else
                            {
                                MaxGTileHandler maxg_handler = (MaxGTileHandler)handleMaxG();
                                MaxWTileHandler maxw_handler = (MaxWTileHandler)handleMaxW();
                                maxg_handler.UpdatePrimaryTile(updater);
                                maxw_handler.UpdatePrimaryTile(updater);
                            }
                        }
                        break;
                    case 0b1110:
                        scores = scores.OrderByDescending(s => s.Value).ToDictionary(s => s.Key, s => s.Value);
                        foreach (Extremes _event in scores.Keys)
                        {
                            if (_event == Extremes.lowT)
                            {
                                MinTTileHandler mint_handler = (MinTTileHandler)handleMinT();
                                LowTTileHandler temp_handler = (LowTTileHandler)handleLowT();
                                if (now.Hour >= 18 || now.Hour < 8)
                                {
                                    mint_handler.UpdatePrimaryTile(updater);
                                    temp_handler.UpdatePrimaryTile(updater);
                                }
                                else
                                {
                                    temp_handler.UpdatePrimaryTile(updater);
                                    mint_handler.UpdatePrimaryTile(updater);
                                }
                            }
                            else if (_event == Extremes.rain)
                            {
                                R0808TileHandler R0808_handler = (R0808TileHandler)handleR0808();
                                R0808_handler.UpdatePrimaryTile(updater);
                            }
                            else
                            {
                                MaxGTileHandler maxg_handler = (MaxGTileHandler)handleMaxG();
                                MaxWTileHandler maxw_handler = (MaxWTileHandler)handleMaxW();
                                maxg_handler.UpdatePrimaryTile(updater);
                                maxw_handler.UpdatePrimaryTile(updater);
                            }
                        }

                        break;
                    case 0b1111:
                        scores = scores.OrderByDescending(s => s.Value).ToDictionary(s => s.Key, s => s.Value);
                        foreach (Extremes _event in scores.Keys)
                        {
                            if (_event == Extremes.highT)
                            {
                                MaxTTileHandler maxt_handler = (MaxTTileHandler)handleMaxT();
                                maxt_handler.UpdatePrimaryTile(updater);
                            }
                            else if (_event == Extremes.lowT)
                            {
                                MinTTileHandler mint_handler = (MinTTileHandler)handleMinT();
                                mint_handler.UpdatePrimaryTile(updater);
                            }
                            else if (_event == Extremes.rain)
                            {
                                R0808TileHandler R0808_handler = (R0808TileHandler)handleR0808();
                                R0808_handler.UpdatePrimaryTile(updater);
                            }
                            else
                            {
                                MaxGTileHandler maxg_handler = (MaxGTileHandler)handleMaxG();
                                maxg_handler.UpdatePrimaryTile(updater);
                            }
                        }

                        break;
                }
            }

            private TileHandler handleMaxT()
            {
                MaxTTileHandler handler = new MaxTTileHandler();
                handler.Package(MaxTs);
                return handler;
            }

            private TileHandler handleHighT()
            {
                HighTTileHandler handler = new HighTTileHandler();
                handler.Package(HighTs);
                return handler;
            }

            private TileHandler handleMinT()
            {
                MinTTileHandler handler = new MinTTileHandler();
                handler.Package(MinTs);
                return handler;
            }

            private TileHandler handleLowT()
            {
                LowTTileHandler handler = new LowTTileHandler();
                handler.Package(LowTs);
                return handler;
            }


            private TileHandler handleR0808()
            {
                R0808TileHandler handler = new R0808TileHandler();
                handler.Package(MaxRs);
                return handler;
            }

            private TileHandler handleMaxG()
            {
                MaxGTileHandler handler = new MaxGTileHandler();
                handler.Package(MaxGs);
                return handler;
            }

            private TileHandler handleMaxW()
            {
                MaxWTileHandler handler = new MaxWTileHandler();
                handler.Package(MaxWs);
                return handler;
            }

            private TileHandler handleSimple()
            {
                StationData[] datas = new StationData[3];

                if(_59287_Simp == null)
                {
                    datas[0] = G1099_Simp;
                    datas[1] = G3241_Simp;
                    datas[2] = G1044_Simp;
                }
                else if(G1099_Simp == null)
                {
                    datas[0] = _59287_Simp;
                    datas[1] = G3241_Simp;
                    datas[2] = G1044_Simp;
                }
                else
                {
                    datas[0] = _59287_Simp;
                    datas[1] = G1099_Simp;
                    datas[2] = G1044_Simp;
                }

                SimpleTileHandler handler = new SimpleTileHandler();
                handler.Package(datas);
                return handler;
            }
        }
    }
}
