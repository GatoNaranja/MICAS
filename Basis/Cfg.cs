using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obt
{
    public class Cfg
    {
        public const int QueryRnds = 48;
        public const int pxW = 240;
        public const int pxH = 240;
        public const int Zoom = 14;
        public static int AreaCode = 4401;   //广州
        public static bool AutoQueryDetails = true;
        public static string[] Downtown = { "G3344", "G3345", "G9489", "G9491", "G3402", "G1045", "G3224", "G1053", "G9515", "G3222", "G3249", "G1044", "G9539", "G9726", "G9576",
            "G3236", "G3223", "G9472", "G9474", "G3157", "G3309", "G9478", "G3102", "G9498", "G3451", "G9476", "G3447", "G9470", "G3410", "G9477",
            "G1093", "G3101", "G9590", "G1039", "G3450", "G9499", "G9475", "G3221", "G3362", "G3316", "G3241", "G1099", "G1001", "G9494",
            "G3219", "G1094", "G1009", "G9577", "G1032", "G9714", "G9527", "G1003", "G9526", "G9722", "G9708", "G1037",
            "G9709", "G1034", "G9771", "G9778", "G9587", "G9588", "G9791", "G9792", "G9793", "G9794", "G9788", "G9806", "G9795", "G9801", "G9804", "G9796", "G9797", "G9772", "9802", "G9803", "G9805", "G9798", "G9799", "G9800"}; //前4个为楼顶高海拔站点
        public static string[] GCJ02Station = { "G3444", "G3345", "G3442", "G9482", "G9483", "G9484", "G9486" };
        public static string[] BD09Station = { "G9472", "G9474", "G9478", "G3410", "G9470", "G3102", "G9475", "G1093", "G9477", "G9489", "G9491" };
    }
}
