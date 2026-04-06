using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndustryDemo
{
    public class Global
    {
        #region "常量定义区"
        //数据库连接字符串
        public const string conString = @"Server=127.0.0.1;Port=3306;Database=filterdetectiondatabase;Uid=root;Pwd=123456;";
        //帐套数据库名称
        //public const string DBName = "myCompany001";
        public const string DBName = "ERPClient";
        #endregion


        #region "变量定义区"
        public static string detectiontime = "2021-7-1-10-41";
        public static string detectiontime_test = "";
        public static string qrCode = "0001";
        public static string defTrayqrCode = "";
        public static double thickness = 0;
        public static string standard = "";
        public static string dir = "";
        public static string deepLearningModelDir = "c:/work";
        public static string imgLocation = "d:/testimg";
        public static int idtray = 1;
        public static int defNumber = 0;
        public static string optshape = "圆形";
        public static double length = 50;
        public static double width = 50;
        public static double diameter = 30;
        public static bool allDeviceSta = true;
        public static int optRow = 10;
        public static int optLine = 11;
        public static int defNumberOnSameFilt = 0;
        public static int scanRange = 3;
        public static string getdefopot = "否";
        public static double focaldistance = 0;
        public static double photoRadio = 4.5;
        public static string dsface = "双面";
        public static bool stop = false;
        
        // 点子聚类识别手印功能参数
        public const int ImageWidthPixels = 2592;    // 图像宽度（像素）
        public const int ImageHeightPixels = 2048;   // 图像高度（像素）
        public const double ImageWidthMm = 16.0;     // 图像宽度（毫米）
        public const double ImageHeightMm = 12.0;    // 图像高度（毫米）
        public const double PixelsPerMmX = ImageWidthPixels / ImageWidthMm;    // 水平方向：162 像素/mm
        public const double PixelsPerMmY = ImageHeightPixels / ImageHeightMm;  // 垂直方向：170.67 像素/mm
        public const double PixelsPerMmAvg = (PixelsPerMmX + PixelsPerMmY) / 2; // 平均：166.33 像素/mm
        
        // 手印识别阈值
        public const double FingerprintDistanceThresholdMm = 10;  // 点子间距阈值（毫米）
        public const int FingerprintMinSpotCount = 20;              // 形成手印的最小点子数量
        #endregion

        public struct defInfo
        {
            public int cameraId;
            public int row;
            public int col;
            public double defX;
            public double defY;
        };

        public static defInfo[] defInfoArr;

        private Global()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
    }
}





