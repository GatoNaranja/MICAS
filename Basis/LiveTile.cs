using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Basis
{
    public class MaxTTileHandler : TileHandler
    {
        public override void Package(StationData[] datas)
        {
            Init(datas);
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] == null)
                {
                    tileStations[i].obtid = "-----";
                    tileStations[i].obtName = "--无自动站数据--";
                    tileStations[i].major = "--无数据--";
                    tileStations[i].minor = "实时气温 -";
                    tileStations[i].desc = "日高温";
                    continue;
                }
                tileStations[i].obtid = datas[i].PrimaryInfo.StationID;
                tileStations[i].obtName = datas[i].PrimaryInfo.Name;
                tileStations[i].major = $"{datas[i].StatisticData.Items[0].Velocity:f1}";
                if (datas[i].ObserveData != null)
                    tileStations[i].minor = $"{datas[i].ObserveData.Temperature:f1} ({datas[i].ObserveData.ObserveTime:t})"/*$"{statistics[i].maxttime:MM-dd HH:mm:ss}"*/;
                tileStations[i].desc = "日高温";
            }
        }
    }

    public class HighTTileHandler : TileHandler
    {
        public override void Package(StationData[] datas)
        {
            Init(datas);
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] == null)
                {
                    tileStations[i].obtid = "-----";
                    tileStations[i].obtName = "--无自动站数据--";
                    tileStations[i].major = "--无数据--";
                    tileStations[i].minor = "日高温 -";
                    tileStations[i].desc = "气温";
                    continue;
                }
                tileStations[i].obtid = datas[i].PrimaryInfo.StationID;
                tileStations[i].obtName = datas[i].PrimaryInfo.Name;
                tileStations[i].major = $"{datas[i].ObserveData.Temperature:f1}";
                tileStations[i].minor = $"日高温 {datas[i].StatisticData?.Items[0].Velocity:f1}";
                tileStations[i].desc = "气温";
            }
        }
    }

    public class MinTTileHandler : TileHandler
    {
        public override void Package(StationData[] datas)
        {
            Init(datas);
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] == null)
                {
                    tileStations[i].obtid = "-----";
                    tileStations[i].obtName = "--无自动站数据--";
                    tileStations[i].major = "--无数据--";
                    tileStations[i].minor = "实时气温 -";
                    tileStations[i].desc = "日低温";
                    continue;
                }
                tileStations[i].obtid = datas[i].PrimaryInfo.StationID;
                tileStations[i].obtName = datas[i].PrimaryInfo.Name;
                tileStations[i].major = $"{datas[i].StatisticData.Items[0].Velocity:f1}";
                if (datas[i].ObserveData != null)
                    tileStations[i].minor = $"{datas[i].ObserveData.Temperature:f1} ({datas[i].ObserveData.ObserveTime:t})";
                tileStations[i].desc = "日低温";
            }
        }
    }

    public class LowTTileHandler : TileHandler
    {
        public override void Package(StationData[] datas)
        {
            Init(datas);
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] == null)
                {
                    tileStations[i].obtid = "-----";
                    tileStations[i].obtName = "--无自动站数据--";
                    tileStations[i].major = "--无数据--";
                    tileStations[i].minor = "日低温 -";
                    tileStations[i].desc = "气温";
                    continue;
                }
                tileStations[i].obtid = datas[i].PrimaryInfo.StationID;
                tileStations[i].obtName = datas[i].PrimaryInfo.Name;
                tileStations[i].major = $"{datas[i].ObserveData.Temperature:f1}";
                tileStations[i].minor = $"日低温 {datas[i].StatisticData?.Items[0].Velocity:f1}";
                tileStations[i].desc = "气温";
            }
        }
    }

    public class R0808TileHandler : TileHandler
    {
        public override void Package(StationData[] datas)
        {
            Init(datas);
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] == null)
                {
                    tileStations[i].obtid = "-----";
                    tileStations[i].obtName = "--无自动站数据--";
                    tileStations[i].major = "--无数据--";
                    tileStations[i].minor = "时雨 -";
                    tileStations[i].desc = "8-8雨";
                    continue;
                }
                tileStations[i].obtid = datas[i].PrimaryInfo.StationID;
                tileStations[i].obtName = datas[i].PrimaryInfo.Name;
                tileStations[i].major = $"{datas[i].StatisticData.Items[0].Velocity:f1}";
                tileStations[i].minor = $"{datas[i].ObserveData?.Rain:f1} ({datas[i].ObserveData?.ObserveTime:t})";
                tileStations[i].desc = "8-8雨";
            }
        }
    }

    public class MaxGTileHandler : TileHandler
    {
        public override void Package(StationData[] datas)
        {
            Init(datas);
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] == null)
                {
                    tileStations[i].obtid = "-----";
                    tileStations[i].obtName = "--无自动站数据--";
                    tileStations[i].major = "--无数据--";
                    tileStations[i].minor = "最大风速 -";
                    tileStations[i].desc = "最大阵风";
                    continue;
                }
                tileStations[i].obtid = datas[i].PrimaryInfo.StationID;
                tileStations[i].obtName = datas[i].PrimaryInfo.Name;
                tileStations[i].major = $"{datas[i].StatisticData.Items[0].Velocity:f1}";
                tileStations[i].minor = $"最大风速 {datas[i].StatisticData?.Items[0].Mean:f1}";
                tileStations[i].desc = "最大阵风";
            }
        }
    }

    public class MaxWTileHandler : TileHandler
    {
        public override void Package(StationData[] datas)
        {
            Init(datas);
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] == null)
                {
                    tileStations[i].obtid = "-----";
                    tileStations[i].obtName = "--无自动站数据--";
                    tileStations[i].major = "--无数据--";
                    tileStations[i].minor = "最大阵风 -";
                    tileStations[i].desc = "最大风速";
                    continue;
                }
                tileStations[i].obtid = datas[i].PrimaryInfo.StationID;
                tileStations[i].obtName = datas[i].PrimaryInfo.Name;
                tileStations[i].major = $"{datas[i].StatisticData.Items[0].Mean:f1}";
                tileStations[i].minor = $"最大阵风 {datas[i].StatisticData?.Items[0].Velocity:f1}";
                tileStations[i].desc = "最大风速";
            }
        }
    }

    public class SimpleTileHandler : TileHandler
    {
        public override void Package(StationData[] datas)
        {
            Init(datas);
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] == null)
                {
                    tileStations[i].obtid = "-----";
                    tileStations[i].obtName = "--无自动站数据--";
                    tileStations[i].major = "--无数据--";
                    tileStations[i].minor = "阵风 -";
                    tileStations[i].desc = "气温";
                    continue;
                }
                tileStations[i].obtid = datas[i].PrimaryInfo.StationID;
                tileStations[i].obtName = datas[i].PrimaryInfo.Name;
                tileStations[i].major = $"{datas[i].ObserveData.Temperature:f1}";
                tileStations[i].minor = $"阵风 {datas[i].ObserveData.GustSpeed:f1} ({datas[i].ObserveData.ObserveTime:t})";
                tileStations[i].desc = "气温";
            }
        }
    }



    //public class WindTileHandler : TileHandler
    //{
    //    public override void Package(Station[] stations)
    //    {
    //        Init(stations);
    //        for (int i = 0; i < stations.Length; i++)
    //        {
    //            if (stations[i] == null)
    //            {
    //                tileStations[i].obtid = "-----";
    //                tileStations[i].obtName = "--无自动站数据--";
    //                tileStations[i].major = "--无数据--";
    //                tileStations[i].minor = "最大阵风 -";
    //                tileStations[i].desc = "2min风";
    //                continue;
    //            }
    //            tileStations[i].obtid = stations[i].obtid;
    //            tileStations[i].obtName = stations[i].obtName;
    //            tileStations[i].major = $"{stations[i].wd2ds:f1}";
    //            tileStations[i].minor = $"最大阵风 {stations[i].maxwd3smaxdf:f1}";
    //            tileStations[i].desc = "2min风";
    //        }
    //    }
    //}




    public abstract class TileHandler
    {
        private const int MinimumStation = 3;
        public TileStation[] tileStations;

        public void Init(StationData[] datas)
        {
            if (datas.Length < MinimumStation) throw new StationsInvalidException(StationsInvalidException.InvalidType.EmptyArray);
            tileStations = new TileStation[datas.Length];
            if (tileStations[0] == null)
                for (int i = 0; i < tileStations.Length; i++)
                {
                    tileStations[i] = new TileStation();
                }
        }

        public abstract void Package(StationData[] datas);

        public void UpdatePrimaryTile(TileUpdater updater)
        {
            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileSmall = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[0].obtid}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintWrap = true,
                                        HintAlign = AdaptiveTextAlign.Center
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[0].major}",
                                        HintStyle = AdaptiveTextStyle.Subtitle,
                                        HintAlign = AdaptiveTextAlign.Center
                                    }
                                }
                            }
                        }
                    }
                }
                        }
                    },
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = ""
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[0].obtid}",
                                        HintStyle = AdaptiveTextStyle.BaseSubtle,
                                        HintWrap = true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[0].major}",
                                        HintStyle = AdaptiveTextStyle.Title
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[0].desc}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        }
                    }
                }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                HintWeight = $"{tileStations[0].major}".Length > 4 ? 55 : 65,
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[0].desc}：",
                                        HintStyle = AdaptiveTextStyle.BaseSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[0].obtid} {tileStations[0].obtName}",
                                        HintStyle = AdaptiveTextStyle.Caption
                                    }
                                }
                            },
                            new AdaptiveSubgroup()
                            {
                                HintWeight = $"{tileStations[0].major}".Length > 4 ? 45 : 35,
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[0].major}",
                                        HintStyle = AdaptiveTextStyle.Subheader,
                                        HintAlign = AdaptiveTextAlign.Right
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                HintWeight = $"{tileStations[1].major}".Length > 4 ? 84 : 87,
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[1].obtid} {tileStations[1].obtName}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[2].obtid} {tileStations[2].obtName}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            },
                            new AdaptiveSubgroup()
                            {
                                HintWeight = $"{tileStations[1].major}".Length > 4 ? 16 : 13,
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[1].major}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintAlign = AdaptiveTextAlign.Right
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[2].major}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintAlign = AdaptiveTextAlign.Right
                                    }
                                }
                            }
                        }
                    }
                }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = $"{tileStations[0].obtName}",
                        HintStyle = AdaptiveTextStyle.Caption
                    },
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                HintWeight = 60,
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[0].major}",
                                        HintStyle = AdaptiveTextStyle.Header
                                    }
                                }
                            },
                            new AdaptiveSubgroup()
                            {
                                HintWeight = 40,
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = "",
                                        HintStyle = AdaptiveTextStyle.Title
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = $"{tileStations[0].desc}",
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveText()
                    {
                        Text = $"{tileStations[0].obtid}     {tileStations[0].minor}",
                        HintStyle = AdaptiveTextStyle.Base
                    },
                    new AdaptiveText()
                    {
                        Text = "",
                        HintStyle = AdaptiveTextStyle.Caption
                    },
                    new AdaptiveText()
                    {
                        Text = $"{tileStations[1].obtName}",
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    },
                    new AdaptiveText()
                    {
                        Text = $"{tileStations[1].obtid} | {tileStations[1].major} | {tileStations[1].minor}",
                        HintStyle = AdaptiveTextStyle.BaseSubtle
                    },
                    new AdaptiveText()
                    {
                        Text = $"{tileStations[2].obtName}",
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    },
                    new AdaptiveText()
                    {
                        Text = $"{tileStations[2].obtid} | {tileStations[2].major} | {tileStations[2].minor}",
                        HintStyle = AdaptiveTextStyle.BaseSubtle
                    }
                }
                        }
                    }
                }
            };

            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            //// And send the notification to the primary tile
            //var updater2 = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);

            //updater.Clear();
            updater.Update(tileNotif);
        }
    }


    internal class StationsInvalidException : Exception
    {
        internal StationsInvalidException(InvalidType type)
        {
            switch (type)
            {
                case InvalidType.EmptyArray:
                    throw new Exception("Empty stations array");
                case InvalidType.EmptyStation:
                    throw new Exception("Empty station");
            }
        }

        internal enum InvalidType
        {
            EmptyStation = 0,
            EmptyArray = 1
        }
    }


    public class TileStation
    {
        public string obtid { get; set; }
        public string obtName { get; set; }
        public string major { get; set; }
        public string minor { get; set; }
        public string desc { get; set; }
    }

}
