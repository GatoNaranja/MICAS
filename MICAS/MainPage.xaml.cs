using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Navigation;

#region C
using Basis;
using Obt;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using static System.Collections.Specialized.BitVector32;
using Windows.UI.Xaml.Data;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Collections;
using Newtonsoft.Json.Linq;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Foundation;
using Windows.Storage.Streams;
using Telerik.Core;
using Windows.Storage;
using Windows.UI.Popups;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.UI.StartScreen;
using static Basis.Process;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Media;
#endregion

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MICAS
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DateTimeOffset DetailsCutoff;
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private OrganizableCollection<StationData> StationsData = new OrganizableCollection<StationData>();
        private OrganizableCollection<StationData> StationsStatistic = new OrganizableCollection<StationData>();
        private OrganizableCollection<StationData>[] Details = new OrganizableCollection<StationData>[Cfg.QueryRnds];
        private bool isDetailsCanceled = false;
        private static object reqLock = new object();

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationViewTitleBar formattableTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            formattableTitleBar.ButtonBackgroundColor = Colors.Transparent;
            formattableTitleBar.ButtonForegroundColor = Colors.Black;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
        }

        private async void Loaded(object sender, RoutedEventArgs e)
        {
            DetailsCutoff = DateTimeOffset.Now;
            TimePicker.SelectedTime = DetailsCutoff.TimeOfDay;
            currentCutoff.Text = DetailsCutoff.ToString();
            InitMap();
            region.DataContext = new CityModel();

            object area = localSettings.Values["area"];
            object auto = localSettings.Values["autoQueryDetails"];
            if (area != null)
            {
                region.SelectedValue = (int)area;
                Cfg.AreaCode = (int)area;
            }
            if (auto != null)
            {
                autoQueryDetails.IsOn = (bool)auto;
                Cfg.AutoQueryDetails = (bool)auto;
            }

            await AutoQuery(DetailsCutoff);

            rtDataGrid.RowDetailsVisibilityMode = Microsoft.Toolkit.Uwp.UI.Controls.DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
        }


        private async Task AutoQuery(DateTimeOffset Dt)
        {
            await Task.Run(async () =>
            {
                await Task.Delay(53); //等待界面加载完成
                string rtJson = await QueryRealtimeData(Dt);
                Stopwatch sw = Stopwatch.StartNew();
                IEnumerator<dynamic> enumerator = QueryStatisticData(Dt).GetEnumerator();

                enumerator.MoveNext();
                string[] jsons = enumerator.Current;
                jsons[0] = rtJson;
                if (Cfg.AreaCode == 4401)   //只针对广州做磁贴更新
                {
                    new Thread(() =>
                    {
                        new Basis.Process().UpdateTiles(jsons);
                    }).Start();
                }
                enumerator.MoveNext();
                StationsStatistic = enumerator.Current;

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    pkDataGrid.ItemsSource = StationsStatistic;
                });
                enumerator.Dispose();

                if (Cfg.AutoQueryDetails)
                {
                    await QueryDetails(Dt);
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        notifyText.Text = $"自动站实况序列加载完成，用时{sw.Elapsed}";
                        notification.IsOpen = true;
                        notifyBd.Width = Window.Current.Bounds.Width - 533;
                        notification.HorizontalOffset = -notifyBd.Width;
                    });
                }
                sw.Stop();
                await Task.Delay(8000);
            });
            notification.IsOpen = false;
        }

        private async Task<string> QueryRealtimeData(DateTimeOffset Dt)
        {
            string rtJson = null;
            try
            {
                lock (reqLock)
                {
                    rtJson = DataSet.Get(DataSet.Categories.Realtime | (Cfg.AreaCode << 16), Dt);
                    if (string.IsNullOrEmpty(rtJson)) throw new Exception("No data returns");
                }
            }
            catch { return "[]"; }
            //StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            //StorageFile sampleFile = await storageFolder.GetFileAsync("rt.txt");
            //rtJson = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);

            StationsData = JsonConvert.DeserializeObject<OrganizableCollection<StationData>>(rtJson);
            if (StationsData.Count > 0)
            {
                string appTitle = $"广州市分区自动站数据 - {StationsData.Count}站数据（{StationsData[0].ObserveData.ObserveTime:F}更新）";
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    header.Text = appTitle;
                    ApplicationView appView = ApplicationView.GetForCurrentView();
                    appView.Title = appTitle;
                    rtDataGrid.ItemsSource = StationsData;
                    rt_loading.Visibility = Visibility.Collapsed;
                });
            }
            return rtJson;
        }

        private IEnumerable<dynamic> QueryStatisticData(DateTimeOffset Dt)
        {
            string[] jsons = new string[DataSet.Categories.BasicArray.Length];

            Parallel.For(1, DataSet.Categories.BasicArray.Length, (i, parallelLoopState) =>   //从1开始为了去掉已经请求的实时监测数据
            {
                lock (reqLock)
                {
                    string pkJson;
                    try
                    {
                        int ReqCode = (1 << i) & DataSet.Categories.StatisticReqCode;
                        if (ReqCode == 0) { parallelLoopState.Break(); }
                        pkJson = DataSet.Get(ReqCode | (Cfg.AreaCode << 16), Dt);
                    }
                    catch { pkJson = "[]"; }
                    jsons[i] = pkJson;
                }
            });
            yield return jsons;
            OrganizableCollection<StationData> Statistic = OrganizeStatisticData(jsons);

            new Task(async () =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    pk_loading.Visibility = Visibility.Collapsed;
                });
            }).Start();

            yield return Statistic;
        }

        private OrganizableCollection<StationData> QueryPresStatisticData(DateTimeOffset Dt)
        {
            string json = null;
            Parallel.For(0, DataSet.Categories.BasicArray.Length, (i, parallelLoopState) =>
            {
                lock (reqLock)
                {
                    string pkJson;
                    try
                    {
                        int ReqCode = (1 << i) & DataSet.Categories.PresReqCode;
                        if (ReqCode == 0)
                        {
                            parallelLoopState.Break();
                        }
                        pkJson = DataSet.Get(DataSet.Categories.PresReqCode | (Cfg.AreaCode << 16), Dt);
                    }
                    catch { pkJson = "[]"; }
                    json = pkJson;
                }

            });

            OrganizableCollection<StationData> Statistic = new DiffPresCollection<StationData>(StationsStatistic);// OrganizeStatisticData(jsons);
            Statistic.OrganizeStatisticData(json);  //数据处理到StationsStatistic

            new Task(async () =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        pk_loading.Visibility = Visibility.Collapsed;
                    });
            }).Start();

            return StationsStatistic;
        }


        private async Task QueryDetails(DateTimeOffset Dt)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            () =>
            {
                if (Cfg.AutoQueryDetails)
                {
                    CancelHint.Text = $"终止查询（1/{Cfg.QueryRnds}）";
                    cancelSp.Visibility = Visibility.Visible;
                }
            });

            Dt = Dt.AddHours(-12).AddMinutes(15);

            string[] ObsWindow = new string[Cfg.QueryRnds];

            queryCnt = 1;

            Parallel.For(0, Cfg.QueryRnds - 1, new ParallelOptions { MaxDegreeOfParallelism = 12 }, async i =>
            {
                lock (reqLock)
                {
                    if (!isDetailsCanceled)
                    {
                        try
                        {
                            string response = DataSet.Get(DataSet.Categories.Realtime | (Cfg.AreaCode << 16), Dt.AddMinutes(i * 15));
                            ObsWindow[i] = response;
                        }
                        catch { }
                    }
                    else ObsWindow[i] = "[]";
                }

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => CancelHint.Text = $"终止查询（{++queryCnt}/{Cfg.QueryRnds}）");

            });

            //ThreadPool.SetMinThreads(9, 9);
            //ThreadPool.SetMaxThreads(16, 20);
            //for (int i = 0; i < Cfg.QueryRnds - 1; i++)
            //{
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(getDetails), new object[] { isDetailsCanceled, Dt, ObsWindow, i });
            //}

            //wait4TriggerEvent.WaitOne();

            //for (int i = 0; i < Cfg.QueryRnds - 1; i++)
            //{
            //    lock (reqLock)
            //    {
            //        if (!isDetailsCanceled)
            //        {
            //            string response = DataSet.Get(DataSet.Categories.Realtime | (Cfg.AreaCode << 16), Dt.AddMinutes(i * 15));
            //            ObsWindow[i] = response;
            //        }
            //        else ObsWindow[i] = "[]";
            //    }
            //    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            //    () => CancelHint.Text = $"终止查询（{++queryCnt}/{Cfg.QueryRnds}）");
            //}

            isDetailsCanceled = false;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            () =>
            {
                if (Cfg.AutoQueryDetails)
                {
                    cancelSp.Visibility = Visibility.Collapsed;
                }
            });

            Parallel.For(0, Cfg.QueryRnds - 1, i =>
            {
                Details[i] = JsonConvert.DeserializeObject<OrganizableCollection<StationData>>(ObsWindow[i]);
            });
            Details[Cfg.QueryRnds - 1] = StationsData;
            ObsWindow = null;
            GC.Collect();
        }

        int queryCnt = 1;
        //AutoResetEvent wait4TriggerEvent = new AutoResetEvent(false);

        //public void getDetails(object obj)
        //{
        //    bool isDetailsCanceled = (bool)(obj as object[])[0];
        //    DateTimeOffset Dt = (DateTimeOffset)(obj as object[])[1];
        //    string[] ObsWindow = (string[])(obj as object[])[2];
        //    int i = (int)(obj as object[])[3];
        //    if (!isDetailsCanceled)
        //    {
        //        string response = DataSet.Get(DataSet.Categories.Realtime | (Cfg.AreaCode << 16), Dt.AddMinutes(i * 15));
        //        ObsWindow[i] = response;
        //    }
        //    else ObsWindow[i] = "[]";
        //    if (++queryCnt >= Cfg.QueryRnds - 1)
        //        wait4TriggerEvent.Set();
        //    Debug.WriteLine(queryCnt);
        //}

        private async Task AutoQueryRealtime(DateTimeOffset Dt) => await Task.Run(() => QueryRealtimeData(Dt));

        private async Task AutoQueryStatistic(DateTimeOffset Dt)
        {
            await Task.Run(async () =>
            {
                IEnumerator<dynamic> enumerator = QueryStatisticData(Dt).GetEnumerator();
                enumerator.MoveNext();
                enumerator.MoveNext();
                StationsStatistic = enumerator.Current;

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    pkDataGrid.ItemsSource = StationsStatistic;
                });
                enumerator.Dispose();
            });
        }

        private async Task AutoQueryPresStatistic(DateTimeOffset Dt)
        {
            await Task.Run(async () =>
            {
                StationsStatistic = QueryPresStatisticData(Dt);

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        pkDataGrid.ItemsSource = StationsStatistic;
                    });
            });
        }


        private async Task AutoQueryDetails(DateTimeOffset Dt) => await Task.Run(() => QueryDetails(Dt));

        private async void rtRefresh(object sender, RoutedEventArgs e)
        {
            rt_loading.Visibility = Visibility.Visible;
            StationsData.Clear();

            rtDataGrid.ItemsSource = null;
            rtDataGrid.ContextFlyout.Hide();
            rtGroup.IsChecked = false;
            rtDowntown.IsChecked = false;

            await AutoQueryRealtime(DetailsCutoff);
            foreach (var dgColumn in rtDataGrid.Columns)
            {
                dgColumn.SortDirection = null;
            }

            rt_loading.Visibility = Visibility.Collapsed;
        }

        private async void pkRefresh(object sender, RoutedEventArgs e)
        {
            pk_loading.Visibility = Visibility.Visible;
            StationsStatistic.Clear();

            pkDataGrid.ItemsSource = null;
            pkDataGrid.ContextFlyout.Hide();
            pkGroup.IsChecked = false;
            pkDowntown.IsChecked = false;

            await AutoQueryStatistic(DetailsCutoff);
            foreach (var dgColumn in pkDataGrid.Columns)
            {
                dgColumn.SortDirection = null;
            }

            pk_loading.Visibility = Visibility.Collapsed;
        }

        private async void DetailsRefresh(object sender, RoutedEventArgs e)
        {
            //DetailsCutoff = DateTimeOffset.Now;
            await AutoQueryDetails(DetailsCutoff);
        }

        private async void Sync(object sender, RoutedEventArgs e)
        {
            rt_loading.Visibility = Visibility.Visible;
            pk_loading.Visibility = Visibility.Visible;
            StationsData.Clear();
            StationsStatistic.Clear();

            rtDataGrid.ItemsSource = null;
            rtDataGrid.ContextFlyout.Hide();
            rtGroup.IsChecked = false;
            rtDowntown.IsChecked = false;
            pkDataGrid.ItemsSource = null;
            pkDataGrid.ContextFlyout.Hide();
            pkGroup.IsChecked = false;
            pkDowntown.IsChecked = false;

            //DetailsCutoff = DateTimeOffset.Now;
            await AutoQuery(DetailsCutoff);
            foreach (var dgColumn in rtDataGrid.Columns)
            {
                dgColumn.SortDirection = null;
            }
            foreach (var dgColumn in pkDataGrid.Columns)
            {
                dgColumn.SortDirection = null;
            }

            rt_loading.Visibility = Visibility.Collapsed;
            pk_loading.Visibility = Visibility.Collapsed;
        }

        private async void Pres_Sync(object sender, RoutedEventArgs e)
        {
            pk_loading.Visibility = Visibility.Visible;
            //StationsStatistic.Clear();

            pkDataGrid.ItemsSource = null;
            pkDataGrid.ContextFlyout.Hide();
            pkGroup.IsChecked = false;
            pkDowntown.IsChecked = false;

            await AutoQueryPresStatistic(DetailsCutoff);
            foreach (var dgColumn in pkDataGrid.Columns)
            {
                dgColumn.SortDirection = null;
            }

            pk_loading.Visibility = Visibility.Collapsed;
        }


        private async void rtdg_Group(object sender, RoutedEventArgs e)
        {
            rtDataGrid.ContextFlyout.Hide();
            rt_loading.Visibility = Visibility.Visible;

            AppBarToggleButton group = sender as AppBarToggleButton;
            if ((bool)group.IsChecked)
            {
                CollectionViewSource groupedItems = new CollectionViewSource();
                await Task.Run(async () =>
                {
                    OrganizableCollection<GroupInfoCollection<StationData>> groups = new OrganizableCollection<GroupInfoCollection<StationData>>();

                    var query = from item in StationsData
                                group item by item.PrimaryInfo.County into g
                                select new { GroupName = g.Key, Items = g };

                    foreach (var g in query)
                    {
                        GroupInfoCollection<StationData> info = new GroupInfoCollection<StationData>();
                        info.Key = g.GroupName;
                        foreach (var item in g.Items)
                        {
                            info.Add(item);
                        }
                        groups.Add(info);
                    }

                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        groupedItems.IsSourceGrouped = true;
                        groupedItems.Source = groups;
                        rtDataGrid.ItemsSource = groupedItems.View;
                        rtDataGrid.SelectedIndex = -1;
                    });
                });
            }
            else
            {
                await Task.Run(async () =>
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        rtDataGrid.ItemsSource = StationsData;
                    });
                });
            }
            rt_loading.Visibility = Visibility.Collapsed;
        }

        private async void pkdg_Group(object sender, RoutedEventArgs e)
        {
            pkDataGrid.ContextFlyout.Hide();
            pk_loading.Visibility = Visibility.Visible;

            AppBarToggleButton group = sender as AppBarToggleButton;
            if ((bool)group.IsChecked)
            {
                CollectionViewSource groupedItems = new CollectionViewSource();
                await Task.Run(async () =>
                {
                    OrganizableCollection<GroupInfoCollection<StationData>> groups = new OrganizableCollection<GroupInfoCollection<StationData>>();

                    var query = from item in StationsStatistic
                                group item by item.PrimaryInfo.County into g
                                select new { GroupName = g.Key, Items = g };

                    foreach (var g in query)
                    {
                        GroupInfoCollection<StationData> info = new GroupInfoCollection<StationData>();
                        info.Key = g.GroupName;
                        foreach (var item in g.Items)
                        {
                            info.Add(item);
                        }
                        groups.Add(info);
                    }

                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        groupedItems.IsSourceGrouped = true;
                        groupedItems.Source = groups;
                        pkDataGrid.ItemsSource = groupedItems.View;
                        pkDataGrid.SelectedIndex = -1;
                    });
                });
            }
            else
            {
                await Task.Run(async () =>
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        pkDataGrid.ItemsSource = StationsStatistic;
                    });
                });
            }
            pk_loading.Visibility = Visibility.Collapsed;

        }

        private async void rtDowntown_Select(object sender, RoutedEventArgs e)
        {
            rtDataGrid.ContextFlyout.Hide();
            rt_loading.Visibility = Visibility.Visible;

            AppBarToggleButton group = sender as AppBarToggleButton;
            if ((bool)group.IsChecked)
            {
                CollectionViewSource groupedItems = new CollectionViewSource();
                await Task.Run(async () =>
                {
                    OrganizableCollection<GroupInfoCollection<StationData>> groups = new OrganizableCollection<GroupInfoCollection<StationData>>();

                    var query = from item in StationsData
                                join downtown in Cfg.Downtown on item.PrimaryInfo.StationID equals downtown
                                group item by item.PrimaryInfo.County into g
                                select new { GroupName = g.Key, Items = g };

                    foreach (var g in query)
                    {
                        GroupInfoCollection<StationData> info = new GroupInfoCollection<StationData>();
                        info.Key = g.GroupName;
                        foreach (var item in g.Items)
                        {
                            info.Add(item);
                        }
                        groups.Add(info);
                    }

                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        groupedItems.IsSourceGrouped = true;
                        groupedItems.Source = groups;
                        rtDataGrid.ItemsSource = groupedItems.View;
                        rtDataGrid.SelectedIndex = -1;
                    });
                });
            }
            else
            {
                await Task.Run(async () =>
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        rtDataGrid.ItemsSource = StationsData;
                    });
                });
            }

            //取消排序
            foreach (var dgColumn in rtDataGrid.Columns)
            {
                dgColumn.SortDirection = null;
            }
            //取消分组
            rtGroup.IsChecked = false;
            rt_loading.Visibility = Visibility.Collapsed;
        }

        private async void pkDowntown_Select(object sender, RoutedEventArgs e)
        {
            pkDataGrid.ContextFlyout.Hide();
            pk_loading.Visibility = Visibility.Visible;

            AppBarToggleButton group = sender as AppBarToggleButton;
            if ((bool)group.IsChecked)
            {
                CollectionViewSource groupedItems = new CollectionViewSource();
                await Task.Run(async () =>
                {
                    OrganizableCollection<GroupInfoCollection<StationData>> groups = new OrganizableCollection<GroupInfoCollection<StationData>>();

                    var query = from item in StationsStatistic
                                join downtown in Cfg.Downtown on item.PrimaryInfo.StationID equals downtown
                                group item by item.PrimaryInfo.County into g
                                select new { GroupName = g.Key, Items = g };

                    foreach (var g in query)
                    {
                        GroupInfoCollection<StationData> info = new GroupInfoCollection<StationData>();
                        info.Key = g.GroupName;
                        foreach (var item in g.Items)
                        {
                            info.Add(item);
                        }
                        groups.Add(info);
                    }

                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        groupedItems.IsSourceGrouped = true;
                        groupedItems.Source = groups;
                        pkDataGrid.ItemsSource = groupedItems.View;
                        pkDataGrid.SelectedIndex = -1;
                    });
                });
            }
            else
            {
                await Task.Run(async () =>
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        pkDataGrid.ItemsSource = StationsStatistic;
                    });
                });
            }

            //取消排序
            foreach (var dgColumn in pkDataGrid.Columns)
            {
                dgColumn.SortDirection = null;
            }
            //取消分组
            pkGroup.IsChecked = false;
            pk_loading.Visibility = Visibility.Collapsed;
        }


        private void dg_LoadingRowGroup(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridRowGroupHeaderEventArgs e)
        {
            ICollectionViewGroup group = e.RowGroupHeader.CollectionViewGroup;
            StationData item = group.GroupItems[0] as StationData;
            e.RowGroupHeader.PropertyValue = $"{item.PrimaryInfo.County}区";
        }

        private void paneBtn_C(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = !splitView.IsPaneOpen;
        }

        private void Now_C(object sender, RoutedEventArgs e)
        {
            DateTimeOffset Now = DateTimeOffset.Now;
            Calendar.SelectedDates.Clear();
            Calendar.SelectedDates.Add(Now);
            TimePicker.SelectedTime = Now.TimeOfDay;
        }

        private void Submit(object sender, RoutedEventArgs e)
        {
            DateTimeOffset date;
            if (Calendar.SelectedDates.Count != 0)  date = Calendar.SelectedDates[0];
            else date = DateTimeOffset.Now;
            TimeSpan time = (TimeSpan)TimePicker.SelectedTime;
            DetailsCutoff = new DateTimeOffset(
                date.Year,
                date.Month,
                date.Day,
                time.Hours,
                time.Minutes,
                time.Seconds,
                TimeSpan.FromHours(8)
                );
            currentCutoff.Text = DetailsCutoff.ToString();
        }

        private void CB_SC(object sender, SelectionChangedEventArgs e)
        {
            if ((bool)region.Tag)
            { 
                localSettings.Values["area"] = region.SelectedValue;
                Cfg.AreaCode = (int)region.SelectedValue;
            }
            region.Tag = true;
        }

        private void autoQueryDetails_Toggled(object sender, RoutedEventArgs e)
        {
            if (region.IsLoaded)
            {
                localSettings.Values["autoQueryDetails"] = autoQueryDetails.IsOn;
                Cfg.AutoQueryDetails = autoQueryDetails.IsOn;
            }
        }

        private void Cancel_C(object sender, RoutedEventArgs e) => isDetailsCanceled = true;

        private void notifyBd_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            notification.IsOpen = false;
        }

        private void rtdg_Copy(object sender, RoutedEventArgs e)
        {
            //DataPackage dataPackage = new DataPackage();
            //dataPackage.SetText(rtDataGrid.ItemsSource.ToString());
            //Clipboard.SetContent(dataPackage);
        }

        private void pkdg_Copy(object sender, RoutedEventArgs e)
        {

        }

        private async void Pin(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.StartScreen.StartScreenManager"))
            {
                // Primary tile API's supported!

                // Get your own app list entry
                // (which is always the first app list entry assuming you are not a multi-app package)
                AppListEntry entry = (await Package.Current.GetAppListEntriesAsync())[0];

                // Check if Start supports your app
                bool isSupported = StartScreenManager.GetDefault().SupportsAppListEntry(entry);
                if (!isSupported) return;

                // Check if your app is currently pinned
                bool wasPinned = await StartScreenManager.GetDefault().ContainsAppListEntryAsync(entry);
                if (wasPinned) return;

                // And pin it to Start
                bool isPinned = await StartScreenManager.GetDefault().RequestAddAppListEntryAsync(entry);
                if (!isPinned) return;

                //await AutoQuery(DetailsCutoff);
            }
        }


        StationData Last = null;

        private async void dg_SelC(object sender, SelectionChangedEventArgs e)
        {
            if (Last != null)
            {
                Last.PrimaryInfo.Map = null;
                Last.History = null;
                Last.UIBundle = null;
                GC.Collect();
            }
            if (e.AddedItems.Count <= 0) return;

            StationData data = e.AddedItems[0] as StationData;
            Last = data;


            ProcessDetailMap(data);
            await Task.Run(async () =>
            {
                DateTimeOffset Dt = new DateTimeOffset(DetailsCutoff.Year, DetailsCutoff.Month, DetailsCutoff.Day, DetailsCutoff.Hour, DetailsCutoff.Minute / 5 * 5, 0, TimeSpan.FromHours(8));
                //DateTimeOffset Dt = new DateTimeOffset(2023, 5, 24, 11, 40, 0, TimeSpan.FromHours(8));
                //DetailsCutoff = Dt;
                string json = DataSet.GetTimesData(Cfg.AreaCode, data.PrimaryInfo.StationID, Dt);

                if (string.IsNullOrEmpty(json)) json = "";
                StationData PrecipHistory = JsonConvert.DeserializeObject<StationData>(json);
                List<Moment> History = ProcessDetailChart(data.PrimaryInfo.StationID, PrecipHistory, Details, StationsStatistic, DetailsCutoff, out UIBundle uIBundle);
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    data.History = History;
                    data.UIBundle = uIBundle;
                });
            });
        }
    }
}
