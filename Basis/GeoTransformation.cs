using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basis
{
    public static class GeoTransformation
    {
        //参考文献
        //[1]程鹏飞, 文汉江, 成英燕, 等. 2000国家大地坐标系椭球参数与GRS80和WGS84的比较[J]. 测绘学报, 2009, 38(3).

        private const double PI = 3.1415926535897932384626433832795;
        private const double ER = 6371008.771415/*6378245//克拉索夫斯基椭球长半轴*//*6378137WGS-84//椭球体长半轴*/;
        private const double EE = 0.00669437999014131699613723354004/*0.00669342162296594323*/;
        private const double BD_PI = 3.1415926535897932384626433832795 * 3000.0 / 180.0;

        static bool outOfChina(double lat, double lon)
        {
            if (lon < 72.004 || lon > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }

        static double transformY(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y
                    + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * PI) + 40.0 * Math.Sin(y / 3.0 * PI)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * PI) + 320 * Math.Sin(y * PI / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        static double transformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1
                    * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * PI) + 40.0 * Math.Sin(x / 3.0 * PI)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * PI) + 300.0 * Math.Sin(x / 30.0
                    * PI)) * 2.0 / 3.0;
            return ret;
        }



        public static PointD WGS84_2_GCJ02(PointD Gpoint)
        {
            if (outOfChina(Gpoint.Y, Gpoint.X))
            {
                return new PointD(0d, 0d);
            }
            double dY = transformY(Gpoint.X - 105.0, Gpoint.Y - 35.0);
            double dLon = transformLon(Gpoint.X - 105.0, Gpoint.Y - 35.0);
            double radY = Gpoint.Y / 180.0 * PI;
            double magic = Math.Sin(radY);
            magic = 1 - EE * magic * magic;
            double SqrtMagic = Math.Sqrt(magic);
            dY = (dY * 180.0) / ((ER * (1 - EE)) / (magic * SqrtMagic) * PI);
            dLon = (dLon * 180.0) / (ER / SqrtMagic * Math.Cos(radY) * PI);
            double mgY = Gpoint.Y + dY;
            double mgLon = Gpoint.X + dLon;
            return new PointD(mgY, mgLon);
        }

        static PointD transform(PointD Gpoint)
        {
            if (outOfChina(Gpoint.Y, Gpoint.X))
            {
                return new PointD(Gpoint.Y, Gpoint.X);
            }
            double dY = transformY(Gpoint.X - 105.0, Gpoint.Y - 35.0);
            double dLon = transformLon(Gpoint.X - 105.0, Gpoint.Y - 35.0);
            double radY = Gpoint.Y / 180.0 * PI;
            double magic = Math.Sin(radY);
            magic = 1 - EE * magic * magic;
            double SqrtMagic = Math.Sqrt(magic);
            dY = (dY * 180.0) / ((ER * (1 - EE)) / (magic * SqrtMagic) * PI);
            dLon = (dLon * 180.0) / (ER / SqrtMagic * Math.Cos(radY) * PI);
            double mgY = Gpoint.Y + dY;
            double mgLon = Gpoint.X + dLon;
            return new PointD(mgLon, mgY);
        }

        /**
         * * 火星坐标系 (GCJ-02) to 84 * * @param lon * @param lat * @return 
         * */
        public static PointD GCJ02_2_WGS84(PointD Gpoint)
        {
            PointD WGS = transform(Gpoint);
            double lontitude = Gpoint.X * 2 - WGS.X;
            double latitude = Gpoint.Y * 2 - WGS.Y;
            return new PointD((float)lontitude, (float)latitude);
        }

        public static PointD GCJ02_2_BD09(PointD Gpoint)
        {
            double x = Gpoint.X, y = Gpoint.Y;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * BD_PI);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * BD_PI);
            double BD_lon = z * Math.Cos(theta) + 0.0065;
            double BD_lat = z * Math.Sin(theta) + 0.006;
            return new PointD((float)BD_lon, (float)BD_lat);
        }

        /**
         * * 火星坐标系 (GCJ-02) 与百度坐标系 (BD-09) 的转换算法 * * 将 BD-09 坐标转换成GCJ-02 坐标 * * @param 
         * BD_lat * @param BD_lon * @return 
         */
        public static PointD BD09_2_GCJ02(PointD BDPoint)
        {
            double x = BDPoint.Y - 0.0065, y = BDPoint.X - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * BD_PI);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * BD_PI);
            double gg_lon = z * Math.Cos(theta);
            double gg_lat = z * Math.Sin(theta);
            return new PointD(gg_lat, gg_lon);
        }

        public static PointD BD09_2_WGS84(PointD BDPoint)
        {

            PointD GCJ02 = BD09_2_GCJ02(BDPoint);
            PointD map84 = GCJ02_2_WGS84(GCJ02);
            //map84.Y += 0.0038173;
            //map84.X -= 0.0056408;

            return map84;

        }

        public static PointD WGS84_2_BD09(PointD WGSPoint)
        {
            PointD GCJ02 = WGS84_2_GCJ02(WGSPoint);
            PointD BD09 = GCJ02_2_BD09(GCJ02);
            return BD09;

        }

        /// <summary>
        /// 给定的经度1，纬度1；经度2，纬度2. 计算2个经纬度之间的距离。
        /// </summary>
        /// <param name="lat1">经度1</param>
        /// <param name="lon1">纬度1</param>
        /// <param name="lat2">经度2</param>
        /// <param name="lon2">纬度2</param>
        public static double Range(double lat1, double lon1, double lat2, double lon2)
        {
            //用haversine公式计算球面两点间的距离。
            //经纬度转换成弧度
            lat1 = C_DegRad(lat1);
            lon1 = C_DegRad(lon1);
            lat2 = C_DegRad(lat2);
            lon2 = C_DegRad(lon2);

            //差值
            var vLon = Math.Abs(lon1 - lon2);
            var vLat = Math.Abs(lat1 - lat2);

            //h is the great circle distance in radians, great circle就是一个球体上的切面，它的圆心即是球心的一个周长最大的圆。
            var h = HaverSin(vLat) + Math.Cos(lat1) * Math.Cos(lat2) * HaverSin(vLon);

            var distance = 2 * ER * Math.Asin(Math.Sqrt(h)) / 1000d;

            return distance;
        }
        /// <summary>
        /// 将角度换算为弧度。
        /// </summary>
        /// <param name="degrees">角度</param>
        /// <returns>弧度</returns>
        private static double C_DegRad(double degrees)
        {
            return degrees * PI / 180;
        }

        private static double C_RadDeg(double radian)
        {
            return radian * 180.0 / PI;
        }

        private static double HaverSin(double theta)
        {
            var v = Math.Sin(theta / 2);
            return v * v;
        }
    }

    public struct PointD
    {
        public double X;
        public double Y;

        public PointD(double a, double b)
        {
            X = a;
            Y = b;
        }
    }
}
