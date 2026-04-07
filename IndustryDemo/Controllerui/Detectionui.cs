using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.Threading;
using HalconDotNet;
using System.IO.Ports;//串口通信编程
using MySql.Data.MySqlClient;//mysql数据库
//using static IndustryDemo.Controllerui.DetectionWith;
//using IndustryDemo;

namespace IndustryDemo.Controllerui
{
    public partial class Detectionui : DevExpress.XtraEditors.XtraUserControl
    {
        static bool _continue;//QRCode继续扫描的判断
       // static SerialPort _serialPort;//  二维码扫码串口对象
        SerialPort _serialPort1 = new SerialPort();
        SerialPort _serialPort2 = new SerialPort();

        #region 获取当前时间
        private string currentTime
        {
            get
            {
                return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            }
        }
        DateTime datetime1;
        #endregion

        #region 电机M1运动的距离
        int currentDistance;
        //private int distance
        //{
        //    get
        //    {
        //        return control.GetDistance();
        //    }
        //}
        #endregion

        #region 其他变量
        DataTable dt = new DataTable();  //日志信息表
        MyCamera.MV_CC_DEVICE_INFO_LIST m_stDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();  //相机列表
        Camera camera1, camera2, camera3, camera4, camera5, camera6, camera7, camera8;  // 4台相机
        Thread detectionThread;
        //IntPtr[] m_hDisplayHandle;
        Thread ToDiskThread;
        Thread DefectThread;
        Thread filterThread;
        Thread FinishDetectionThread;
        Thread getDefLocThread;
        bool disk = true;
        static bool STMisConnected = false;
        Thread DetectionImage;
        STM32Control control;
        SerialPort serialPort1 = new SerialPort();
        bool RingLight;
        bool DetectionThreadStat = false;
        bool ToDiskThreadStat = false;
        bool ShowDefectsStat = false;
        bool FinishDetectionThreadStat = false;
        int[] defectType = new int[6];        
        public bool upcamerasOpenStat;
        public bool downcamerasOpenStat;
        bool DisplayDefectStat;
        int distance;
        List<int[]> defects = new List<int[]>();
        List<int[]> defectsLocation = new List<int[]>();
        List<float[]> defectsRelativeLocation = new List<float[]>();

        double M1loc;
        double M2loc;
        double M3loc;
        double M4loc;
        double M5loc;
        double M6loc;
        double M7loc;
        static int defnumberflag = 0;

        private static string opttype1;                //滤光片类型
        private static string optshape1;               //滤光片外形
        private static double optdiameter1;            //滤光片直径
        private static double optthickness1;           //滤光片厚度
        private static int defRowNum = 10;
        private static int defLineNum = 7;
        private static int[,] optArray;

        private static int downflag = 0;
        private static bool GetDefectOptFlag = false;

        //public event EventHandler eh = new EventHandler(hhh);//定义一个委托，是两个关系form间的自定义桥梁，当然必须为public 了

        private string err;

        //虚拟盘变量
        private int BlockWidth;     //虚拟盘里圆的宽
        private int BlockHeight;    //虚拟盘里圆的高
        private Graphics Graphics_G;    //
        private int lastRow;    //鼠标点击的圆的行数
        private int lastCol;    //鼠标点击的圆的列数
        private int startRow;
        //虚拟滤光片放大后的宽高
        private int circlewidth; //宽度
        private int circleheight;//高度
        private int rectanglewidth; //宽度
        private int rectangleheight;//高度

        public mainForm mainForm1;
        #endregion

        #region 构造函数
        public Detectionui()
        {
            InitializeComponent();
            InitializeDrawResources();
            //m_hDisplayHandle = new IntPtr[4];
            this.Dock = DockStyle.Fill;
            //splitContainerControl1.Dock = DockStyle.Fill;
            //splitContainerControl2.Dock = DockStyle.Fill;
            //groupControl2.Dock = DockStyle.Bottom;
            //listBoxControl1.Dock = DockStyle.Fill;
            //distributeGraph1.GetCount(8, 8);
            textEdit1.Enabled = false;
            textEdit2.Enabled = false;
            textEdit3.Enabled = false;
            textEdit4.Enabled = false;
            //groupControl1.Dock = DockStyle.Right;
            dt.Columns.Add("时间");
            dt.Columns.Add("信息");
            gridControl1.DataSource = dt;
            gridView1.PopulateColumns();
            //FormChanged();
            

        }

        public void Detectionuih(mainForm mainForm1)
        {
            this.mainForm1 =  mainForm1;
            
        }
        #endregion

        #region 显示错误信息
        private void ShowErrorMsg(string csMessage, int nErrorNum)
        {
            string errorMsg;
            if (nErrorNum == 0)
            {
                errorMsg = csMessage;
            }
            else
            {
                errorMsg = csMessage + ": Error =" + String.Format("{0:X}", nErrorNum);
            }

            switch (nErrorNum)
            {
                case MyCamera.MV_E_HANDLE: errorMsg += " Error or invalid handle "; break;
                case MyCamera.MV_E_SUPPORT: errorMsg += " Not supported function "; break;
                case MyCamera.MV_E_BUFOVER: errorMsg += " Cache is full "; break;
                case MyCamera.MV_E_CALLORDER: errorMsg += " Function calling order error "; break;
                case MyCamera.MV_E_PARAMETER: errorMsg += " Incorrect parameter "; break;
                case MyCamera.MV_E_RESOURCE: errorMsg += " Applying resource failed "; break;
                case MyCamera.MV_E_NODATA: errorMsg += " No data "; break;
                case MyCamera.MV_E_PRECONDITION: errorMsg += " Precondition error, or running environment changed "; break;
                case MyCamera.MV_E_VERSION: errorMsg += " Version mismatches "; break;
                case MyCamera.MV_E_NOENOUGH_BUF: errorMsg += " Insufficient memory "; break;
                case MyCamera.MV_E_UNKNOW: errorMsg += " Unknown error "; break;
                case MyCamera.MV_E_GC_GENERIC: errorMsg += " General error "; break;
                case MyCamera.MV_E_GC_ACCESS: errorMsg += " Node accessing condition error "; break;
                case MyCamera.MV_E_ACCESS_DENIED: errorMsg += " No permission "; break;
                case MyCamera.MV_E_BUSY: errorMsg += " Device is busy, or network disconnected "; break;
                case MyCamera.MV_E_NETER: errorMsg += " Network error "; break;
            }

            MessageBox.Show(errorMsg, "PROMPT");
        }
        #endregion

        #region 相机相关参数

        #region 查找设备
        private void DeviceListAcq_up()  //查找设备
        {
            // ch:创建设备列表 | en:Create Device List
            GC.Collect();
            m_stDeviceList.nDeviceNum = 0;
            int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_stDeviceList);
            if (0 != nRet)
            {
                ShowErrorMsg("Enumerate devices fail!", 0);
                return;
            }
            camera1 = new Camera(pictureEdit1.Handle, m_stDeviceList.pDeviceInfo[2], "camera1", pictureEdit1.Width, pictureEdit1.Height, 9876, 19884);
            camera2 = new Camera(pictureEdit2.Handle, m_stDeviceList.pDeviceInfo[0], "camera2", pictureEdit2.Width, pictureEdit2.Height, 8631, 18660);
            camera3 = new Camera(pictureEdit3.Handle, m_stDeviceList.pDeviceInfo[1], "camera3", pictureEdit3.Width, pictureEdit3.Height, 7900, 17835);
            camera4 = new Camera(pictureEdit4.Handle, m_stDeviceList.pDeviceInfo[3], "camera4", pictureEdit4.Width, pictureEdit4.Height, 6672, 16497);
        }

        private void DeviceListAcq_down()  //查找设备
        {
            // ch:创建设备列表 | en:Create Device List
            GC.Collect();
            m_stDeviceList.nDeviceNum = 0;
            int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_stDeviceList);
            if (0 != nRet)
            {
                ShowErrorMsg("Enumerate devices fail!", 0);
                return;
            }
            camera5 = new Camera(pictureEdit13.Handle, m_stDeviceList.pDeviceInfo[6], "camera5", pictureEdit13.Width, pictureEdit13.Height, 9876, 19884);
            camera6 = new Camera(pictureEdit14.Handle, m_stDeviceList.pDeviceInfo[4], "camera6", pictureEdit14.Width, pictureEdit14.Height, 8631, 18660);
            camera7 = new Camera(pictureEdit15.Handle, m_stDeviceList.pDeviceInfo[5], "camera7", pictureEdit15.Width, pictureEdit15.Height, 7900, 17835);
            camera8 = new Camera(pictureEdit16.Handle, m_stDeviceList.pDeviceInfo[7], "camera8", pictureEdit16.Width, pictureEdit16.Height, 6672, 16497);
        }
        #endregion

        #region 打开相机
        private void bnOpen_Click_up()
        {
            Addlog("正在打开相机1...");
            camera1.Open_click();
            camera1.rotate = true;
            camera1.cameraData.rotate = true;
            camera1.Start_Grabbing(); //开始采集            

            Addlog("正在打开相机2...");
            camera2.Open_click();
            camera2.rotate = true;
            camera2.cameraData.rotate = true;
            camera2.Start_Grabbing();

            Addlog("正在打开相机3...");
            camera3.Open_click();
            camera3.Start_Grabbing();
            //camera3.rotate = true;

            Addlog("正在打开相机4...");
            camera4.Open_click();
            camera4.Start_Grabbing();
            //camera4.rotate = true;
            Addlog("已打开所有上层相机！");

            upcamerasOpenStat = true;
        }
        private void bnOpen_Click_down()
        {
            Addlog("正在打开相机5...");
            camera5.Open_click();
            camera5.rotate = true;
            camera5.cameraData.rotate = true;
            camera5.Start_Grabbing(); //开始采集            

            Addlog("正在打开相机6...");
            camera6.Open_click();
            camera6.rotate = true;
            camera6.cameraData.rotate = true;
            camera6.Start_Grabbing();

            Addlog("正在打开相机7...");
            camera7.Open_click();
            camera7.Start_Grabbing();
            //camera3.rotate = true;

            Addlog("正在打开相机8...");
            camera8.Open_click();
            camera8.Start_Grabbing();
            //camera4.rotate = true;
            Addlog("已打开所有下层相机！");

            downcamerasOpenStat = true;
        }
        #endregion

        #region 设置相机帧率，曝光，增益
        public void SetValue()
        {
        }
        #endregion

        #region 关闭上层相机连接
        public void bnClose_up_Click()
        {
            if (camera1.isOpened)
            {
                camera1.Close();
            }

            if (camera2.isOpened)
            {
                camera2.Close();
            }
            if (camera3.isOpened)
            {
                camera3.Close();
            }
            if (camera4.isOpened)
            {
                camera4.Close();
            }
            
            upcamerasOpenStat = false;
        }
        #endregion

        #region 关闭下层相机连接
        public void bnClose_down_Click()
        {
            if (camera5.isOpened)
            {
                camera5.Close();
            }

            if (camera6.isOpened)
            {
                camera6.Close();
            }
            if (camera7.isOpened)
            {
                camera7.Close();
            }
            if (camera8.isOpened)
            {
                camera8.Close();
            }
            downcamerasOpenStat = false;
        }
        #endregion

        #region 保存图片
        private void ToDisk()
        {
            camera1.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
            camera2.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
            camera3.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
            camera4.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
            camera5.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
            camera6.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
            camera7.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
            camera8.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
        }
       

        private void ToDiskDown()
        {
            camera5.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
            camera6.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
            camera7.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
            camera8.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
        }
        #endregion

        #region 直接保存图片到硬盘
        private void SaveToDisk()
        {
            camera1.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
            camera2.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
            camera3.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
            camera4.ToDisk(defectType, defects, defectsLocation, defectsRelativeLocation);
        }
        #endregion

        #endregion

        #region 单片机相关操作
        void STMInitial_out()
        {
            STM32Start();   //复位
            //STM32Basement();    //到基准点

        }
        void STMInitial()
        {
            STM32Start();   //复位
            STM32Basement();    //到基准点

        }

        private void STM32Connect()  //建立连接
        {
            control = new STM32Control(serialPort1, 115200, "COM1");
            control.Connect();
            STMisConnected = true;
        }

        private void STM32Start()  //启动
        {
            control.Std_btn_Click();
        }

        private void STM32Basement()  //基准点
        {
            control.M3Basement_Click();  //上移基准点
            //control.up1m_Click();
            control.down1m_Click();  //下降1mm，M34 65.3mm
            control.M2RightReset_Click();  //右端基准点
            control.motor_run(2, 67);       //,M2 67mm
            control.M1fstBasement_Click();  //前后基准点,M1 183mm
        }


        private void MoveForward()  //电机向前运动
        {
            control.fwd6_Click();
        }
        private void MoveBackward()  //电机向后运动
        {
            control.bwd6_Click();
        }


        private void MoveLeft()  //电机左移59mm
        {
            control.lft32_Click();
            //Thread.Sleep(2000);

        }

        private void SaveToCameraData_up(int i, int j, string dir)   //图片保存到cameraData
        {
            currentDistance = control.GetDistance();      //获取位置
            camera1.ReceiveImageWorkThread(1, i, j, dir, currentDistance);
            camera2.ReceiveImageWorkThread(2, i, j, dir, currentDistance);
            camera3.ReceiveImageWorkThread(3, i, j, dir, currentDistance);
            camera4.ReceiveImageWorkThread(4, i, j, dir, currentDistance);
        }

        private void SaveToCameraData_down(int i, int j, string dir)   //图片保存到cameraData
        {
            currentDistance = control.GetDistance();      //获取位置
            camera5.ReceiveImageWorkThread(5, i, j, dir, currentDistance);
            camera6.ReceiveImageWorkThread(6, i, j, dir, currentDistance);
            camera7.ReceiveImageWorkThread(7, i, j, dir, currentDistance);
            camera8.ReceiveImageWorkThread(8, i, j, dir, currentDistance);
        }

        #endregion

        #region 到上层拍照参考点
        private void ToUpRefPoint()
        {
            
            control.motor_run(1, 183);       //,M1 183mm
            control.motor_run(2, 75);       //,M2 67mm
            control.motor_run(2, 67);       //,M2 67mm
            control.motor_run(34, 72 - Global.thickness + Global.focaldistance);       //,M34 72.7mm
            control.motor_run(34, 73 - Global.thickness + Global.focaldistance);       //,M34 72.7mm
            
        }
        #endregion

        #region 到下层拍照参考点
        private void ToDownRefPoint()
        {

            control.motor_run(1, 183);       //,M1 183mm
            control.motor_run(2, 68);       //,M2 67mm
            control.motor_run(2, 67);       //,M2 67mm
            control.motor_run(34, 3.6);       //,M34 2.6mm
            control.motor_run(34, 1.8 - Global.focaldistance);       //,M34 2.6mm
        }
        #endregion

        #region 连接上层相机
        private void Connect_UpCamera()
        {
            
            if (!upcamerasOpenStat)  //如果相机没有打开，打开相机
            {
                DeviceListAcq_up();  //查找设备
                bnOpen_Click_up();  //打开相机
            }
            
        }
        #endregion

        #region 上层图像获取部分
        private void getUpImage()
        {
            #region 上层环光
            camera1.SetValue(500, 0, 22);
            camera2.SetValue(500, 0, 22);
            camera3.SetValue(500, 0, 22);
            camera4.SetValue(500, 0, 22);
            RingLight = false;
            control.OnlyWritepushRegHoldingBuf(40019, 0);   //上层
            control.OnlyWritepushRegHoldingBuf(40015, 1);   //环光
            int j = 0;
            int k = 0;
            int M2flag = 67;
            for (int i = 0; i < 6; i++)
            {
                k++;
                bool flag = true;
                MoveForward();  //M1前进
                j = 0;
                while (flag)
                {
                    if (control.GetpushRegHoldingBuf(40014) == 1)
                    {
                        //保存图片到CameraData
                        SaveToCameraData_up(i, j, "ring");
                        //保存图片到硬盘
                        //SaveToDisk();
                        control.OnlyWritepushRegHoldingBuf(40014, 98);  //保存完毕
                        j++;
                    }
                    if (control.GetpushRegHoldingBuf(40016) == 99)
                    {

                        flag = false;  //一列照片读取完毕退出循环
                    }
                }
                /*if(i == (Global.scanRange - 1))
                {
                    break;
                }*/
                M2flag += 59;
                control.motor_run(2, M2flag);//M2左移59毫米
                
                i++;
                //MoveLeft();         //M2左移59毫米
                MoveBackward();     //M1后退
                flag = true;
                j = 0;
                while (flag)
                {
                    if (control.GetpushRegHoldingBuf(40014) == 1)
                    {
                        //保存图片到CameraData
                        SaveToCameraData_up(i, j, "ring");
                        //保存图片到硬盘
                        control.OnlyWritepushRegHoldingBuf(40014, 98);  //保存完毕
                        j++;
                    }
                    if (control.GetpushRegHoldingBuf(40016) == 99)
                    {
                        flag = false;  //一列照片读取完毕退出循环
                    }
                }
                
                if (i != 5)
                {
                    if (k == Global.scanRange)
                    {
                        k = 0;
                        M2flag = 67;
                        break;
                    }
                    else
                    {
                        M2flag += 59;
                        control.motor_run(2, M2flag);//M2左移59毫米
                    }
                    
                }
                else
                {
                    M2flag = 67;
                }
                /*if (i == (Global.scanRange - 1))
                {
                    break;
                }*/
                
            }
            #endregion

            #region 上层点光
            /*
            camera1.SetValue(120, 0, 22);
            camera2.SetValue(120, 0, 22);
            camera3.SetValue(120, 0, 22);
            camera4.SetValue(120, 0, 22);
            RingLight = true;
            control.OnlyWritepushRegHoldingBuf(40019, 0);   //上层
            control.OnlyWritepushRegHoldingBuf(40015, 0);   //点光
            //control.M2RightReset_Click();
            ToUpRefPoint();
            //control.motor_run(2, 67);
            k = 0;
            for (int i = 0; i < 6; i++)
            {
                k++;
                bool flag = true;
                
                MoveForward();  //M1前进
                j = 0;
                while (flag)
                {
                    if (control.GetpushRegHoldingBuf(40014) == 1)
                    {
                        //保存图片到CameraData
                        SaveToCameraData_up(i, j, "spot");
                        //保存图片到硬盘
                        //SaveToDisk();
                        control.OnlyWritepushRegHoldingBuf(40014, 98);  //保存完毕
                        j++;
                        
                    }
                    if (control.GetpushRegHoldingBuf(40016) == 99)
                    {
                        flag = false;  //一列照片读取完毕退出循环
                    }
                }
                *//*if (i == (Global.scanRange - 1))
                {
                    break;
                }*//*
                M2flag += 59;
                control.motor_run(2, M2flag);//M2左移59毫米
                
                i++;
                //MoveLeft();         //M2左移59毫米
                MoveBackward();     //M1后退
                flag = true;
                j = 0;
                while (flag)
                {
                    if (control.GetpushRegHoldingBuf(40014) == 1)
                    {
                        //保存图片到CameraData
                        SaveToCameraData_up(i, j, "spot");
                        //保存图片到硬盘
                        
                        control.OnlyWritepushRegHoldingBuf(40014, 98);  //保存完毕
                        j++;
                        
                    }
                    if (control.GetpushRegHoldingBuf(40016) == 99)
                    {
                        flag = false;  //一列照片读取完毕退出循环
                    }
                }
                
                if (i != 5)
                {
                    if (k == Global.scanRange)
                    {
                        k = 0;
                        M2flag = 67;
                        break;
                    }
                    else
                    {
                        M2flag += 59;
                        control.motor_run(2, M2flag);//M2左移59毫米
                    }
                    
                }
                else
                {
                    M2flag = 67;
                }
                *//*if (i == (Global.scanRange - 1))
                {
                    break;
                }*//*
            }*/
            #endregion
        }
        #endregion

        #region 连接下层相机
        private void Connect_DownCamera()
        {

            if (!downcamerasOpenStat)  //如果相机没有打开，打开相机
            {
                DeviceListAcq_down();  //查找设备
                bnOpen_Click_down();  //打开相机
            }

        }
        #endregion

         #region 下层图像获取部分
        private void getDownImage()
        {
            #region 下层环光

            camera5.SetValue(500, 0, 22);
            camera6.SetValue(500, 0, 22);
            camera7.SetValue(500, 0, 22);
            camera8.SetValue(500, 0, 22);
            RingLight = false;
            control.OnlyWritepushRegHoldingBuf(40019, 1);   //下层
            control.OnlyWritepushRegHoldingBuf(40015, 1);   //环光
            int j = 0;
            int k = 0;
            int M2flag = 67;
            for (int i = 0; i < 6; i++)
            {
                k++;
                bool flag = true;
                MoveForward();  //M1前进
                j = 0;
                while (flag)
                {
                    if (control.GetpushRegHoldingBuf(40014) == 1)
                    {
                        //保存图片到CameraData
                        SaveToCameraData_down(i, j, "ring");
                        //保存图片到硬盘
                        //SaveToDisk();
                        control.OnlyWritepushRegHoldingBuf(40014, 98);  //保存完毕
                        j++;
                    }
                    if (control.GetpushRegHoldingBuf(40016) == 99)
                    {

                        flag = false;  //一列照片读取完毕退出循环
                    }
                }
                /*if (i / 2 == (Global.scanRange - 1))
                {
                    break;
                }*/
                M2flag += 59;
                control.motor_run(2, M2flag);//M2左移59毫米

                i++;
                //MoveLeft();         //M2左移59毫米
                MoveBackward();     //M1后退
                flag = true;
                j = 0;
                while (flag)
                {
                    if (control.GetpushRegHoldingBuf(40014) == 1)
                    {
                        //保存图片到CameraData
                        SaveToCameraData_down(i, j, "ring");
                        //保存图片到硬盘
                        control.OnlyWritepushRegHoldingBuf(40014, 98);  //保存完毕
                        j++;
                    }
                    if (control.GetpushRegHoldingBuf(40016) == 99)
                    {
                        flag = false;  //一列照片读取完毕退出循环
                    }
                }

                if (i != 5)
                {
                    if (k == Global.scanRange)
                    {
                        M2flag = 67;
                        k = 0;
                        break;
                    }
                    else
                    {
                        M2flag += 59;
                        control.motor_run(2, M2flag);//M2左移59毫米
                    }

                }
                else
                {
                    M2flag = 67;
                }
                /*if (i == (Global.scanRange - 1))
                {
                    break;
                }*/
            }
            #endregion

            #region 下层点光


            // camera5.SetValue(120, 0, 22);
            // camera6.SetValue(120, 0, 22);
            // camera7.SetValue(120, 0, 22);
            // camera8.SetValue(120, 0, 22);
            // RingLight = true;
            // control.OnlyWritepushRegHoldingBuf(40019, 1);   //下层
            // control.OnlyWritepushRegHoldingBuf(40015, 0);   //点光
            // //control.M2RightReset_Click();
            // //control.motor_run(2, 67);
            // ToDownRefPoint();
            // int M2flag = 67;

            // int k = 0;
            // for (int i = 0; i < 6; i++)
            // {
            //     k++;
            //     bool flag = true;

            //     MoveForward();  //M1前进
            //     int j = 0;
            //     while (flag)
            //     {
            //         if (control.GetpushRegHoldingBuf(40014) == 1)
            //         {
            //             //保存图片到CameraData
            //             SaveToCameraData_down(i, j, "spot");
            //             //保存图片到硬盘
            //             //SaveToDisk();
            //                 control.OnlyWritepushRegHoldingBuf(40014, 98);  //保存完毕
            //                 j++;

            //         }
            //         if (control.GetpushRegHoldingBuf(40016) == 99)
            //         {
            //             flag = false;  //一列照片读取完毕退出循环
            //         }
            //     }
            //     /*if (i == (Global.scanRange - 1))
            //     {
            //         break;
            //     }*/
            //     M2flag += 59;
            //     control.motor_run(2, M2flag);//M2左移59毫米

            //     i++;
            //     MoveBackward();     //M1后退
            //     flag = true;
            //     j = 0;
            //     while (flag)
            //     {
            //         if (control.GetpushRegHoldingBuf(40014) == 1)
            //         {
            //             //保存图片到CameraData
            //             SaveToCameraData_down(i, j, "spot");
            //             //保存图片到硬盘

            //                 control.OnlyWritepushRegHoldingBuf(40014, 98);  //保存完毕
            //                 j++;

            //         }
            //         if (control.GetpushRegHoldingBuf(40016) == 99)
            //         {
            //             flag = false;  //一列照片读取完毕退出循环
            //         }
            //     }

            //     if (i != 5)
            //     {
            //         if (k == Global.scanRange)
            //         {
            //             k = 0;
            //             M2flag = 67;
            //             break;
            //         }
            //         else
            //         {
            //             M2flag += 59;
            //             control.motor_run(2, M2flag);//M2左移59毫米
            //         }

            //     }
            //     else
            //     {
            //         M2flag = 67;
            //     }
            //     /*if (i == (Global.scanRange - 1))
            //     {
            //         break;
            //     }*/
            // }
             #endregion
        }
        #endregion

        #region 图像获取部分
        void DetectionBegin()
        {
            if(Global.dsface == "双面" || Global.dsface == "上面")
            {
                ToUpRefPoint();         //到上层拍照参考点
                getUpImage();           //获取上层图像
            }
            DetectionWithDL2 position4 = new DetectionWithDL2();
            List<Task> IPTaskList1 = new List<Task>();
            for (int j = 1; j <= 5; j++)
            {
                int s = j;
                IPTaskList1.Add(Task.Run(() =>
                {
                    position4.ImageProcess1("G:/" + Global.qrCode + "/" + Global.detectiontime + "/camera" + s + "/ring", s);
                }));
            }

            Task.WaitAll(IPTaskList1.ToArray());
            IPTaskList1.Clear();

            //downflag = 1;
            //if (Global.dsface == "双面" || Global.dsface == "下面")
            //{
            //    ToDownRefPoint();         //到下层拍照参考点
            //    getDownImage();         //获取下层图像
            //}
            DetectionWithDL2 detectionWithDL0 = new DetectionWithDL2();
            HObject DL = new HObject();
            List<Task> IPTaskList0 = new List<Task>();
            IPTaskList0.Add(Task.Run(() =>
            {
                downflag = 1;
                if (Global.dsface == "双面" || Global.dsface == "下面")
                {
                    ToDownRefPoint();         //到下层拍照参考点
                    getDownImage();         //获取下层图像
                }
            }));
            for (int i = 1; i < 5; i++)
            {
                int k = i;
                IPTaskList0.Add(Task.Run(() =>
                {
                    detectionWithDL0.ImageProcess("G:/" + Global.qrCode + "/" + Global.detectiontime + "/camera" + k + "/ring");//
                }));
                //Task.WaitAll(IPTaskList.ToArray());
            }
            Task.WaitAll(IPTaskList0.ToArray());
        }
        #endregion

        #region 取次品
        void GetDefectOpt(int[,] optArray)
        {
            if(GetDefectOptFlag)
            {
                //扫描次品盘二维码
                control.motor_run(7, 263);   //位置待确定
                #region  扫描次品盘二维码
                _serialPort2.PortName = "com3";
                _serialPort2.Open();
                defTrayDataReceivedHandler();
                #endregion

                control.motor_run(2, 0);
                //M34复位
                //MessageBox.Show("8");
                control.motor_run(34, 0);

                for (int i = 0; i < Global.optRow; i++)
                {
                    for (int j = 0; j < Global.optLine; j++)
                    {
                        if (optArray[i, j] == 1)
                        {
                            
                            defnumberflag++;    //次品数，从1开始
                            #region 取次品动作
                            //M1位置
                            M1loc = (339 - optdiameter1 / 2 - (340 - optdiameter1 - 0.2) / (Global.optRow - 1) * i);
                            control.motor_run(1, M1loc);
                            //M5左移
                            M5loc = (368 + optdiameter1 / 2 + (350 - optdiameter1 - 0.2) / (Global.optLine - 1) * j);
                            control.motor_run(5, M5loc);
                            //M6下移
                            M6loc = (50 - Global.thickness);
                            control.motor_run(6, M6loc);
                            //吸气
                            control.btnGrabbing_Click();
                            //M6上移
                            control.motor_run(6, 0);
                            //M5右移
                            M5loc = (20 + optdiameter1 / 2 + 1 + (237 - optdiameter1 - 2) / (defLineNum - 1) * ((defnumberflag - 1) % defLineNum));
                            control.motor_run(5, M5loc);
                            //M7位置
                            M7loc = (256 + optdiameter1 / 2 + 1 + (350 - optdiameter1 - 2) / (defRowNum - 1) * ((defnumberflag - 1) / defLineNum));
                            control.motor_run(7, M7loc);

                            M7loc = (252 + optdiameter1 / 2 + 1 + (350 - optdiameter1 - 2) / (defRowNum - 1) * ((defnumberflag - 1) / defLineNum));
                            control.motor_run(7, M7loc);
                            //M6下移
                            M6loc = (84 - Global.thickness);
                            control.motor_run(6, M6loc);
                            //放气
                            control.btnRelease_Click();
                            //M6复位
                            control.motor_run(6, 0);

                            #endregion

                            //判断次品盘是否装满
                            if(defnumberflag == (defRowNum * defLineNum))
                            {
                                //control.OnlyWritepushRegHoldingBuf(40001, 2);   //次品盘装满，相当于停止键按下，亮黄灯
                                Global.allDeviceSta = false;
                                mainForm1.changecontinueButton(null,null);   //继续按钮使能
                                while (Global.allDeviceSta == false) ;  //等待更换次品盘，并按下继续按钮
                                defnumberflag = 0;
                            }

                            #region 次品数据存数据库
                            defDataToDB(i,j);
                            #endregion
                        }
                    }
                }
                defnumberflag = 0;
                GetDefectOptFlag = false;
            }
            
        }
        void GetDefectFilt()
        {
            divideScale();//分级


            DataTable dt2 = MySqlHelper.GetDataTable(out err, "select count(*) from defection WHERE qrcode ='"+Global.qrCode + "' and detectiontime='" + Global.detectiontime + "'");
            //MessageBox.Show(Global.detectiontime);
            Global.defNumber = Convert.ToInt32(dt2.Rows[0][0]);      //获取总瑕疵数
            DataTable dt3 = MySqlHelper.GetDataTable(out err, "select posX,posY from defection WHERE qrcode='"+Global.qrCode + "' and detectiontime='" + Global.detectiontime + "'");
            //获取瑕疵的位置
            DataTable dt4 = MySqlHelper.GetDataTable(out err, "select trayX,trayY from defection WHERE qrcode='"+Global.qrCode + "' and detectiontime='" + Global.detectiontime + "'");

            for (int i = 0; i < Global.defNumber; i++)
            {
                optArray[Convert.ToInt32(dt3.Rows[i][0]), Convert.ToInt32(dt3.Rows[i][1])] += 1;
            }

            Image backgroundBorder = new Bitmap(panelOpticalfilter.Width, panelOpticalfilter.Height);

            Graphics gra = Graphics.FromImage(backgroundBorder);
            gra.Clear(this.BackColor);
            gra.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Brush bushgreen = new SolidBrush(Color.Green);//填充的颜色
            Brush bushgray = new SolidBrush(Color.Gray);//填充的颜色
            Brush bushred = new SolidBrush(Color.Red);//填充的颜色
            Font myFont = new Font("宋体", 8, FontStyle.Bold);
            Brush bushblack = new SolidBrush(Color.Black);//填充的颜色
            Brush bushyellow = new SolidBrush(Color.Yellow);//黄色
            Brush bushorange = new SolidBrush(Color.Orange);//橙色


            int width = this.panelOpticalfilter.Width; //宽度
            int height = this.panelOpticalfilter.Height;//高度
            for (int i = 0; i < Global.optRow; i++)
            {
                for (int j = 0; j < Global.optLine; j++)
                {
                    //获取滤光片等级
                    string order = "select level from filterlevel where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and posX='" + i + "' and posY='" + j + "'";
                    DataTable dt_now = MySqlHelper.GetDataTable(out err, order);
                    int level = 3;
                    if (dt_now != null && dt_now.Rows != null && dt_now.Rows.Count > 0)
                    {
                        level = Convert.ToInt32(dt_now.Rows[0][0]);
                    }
                    //if (optArray[i, j] != 0)
                    {
                        if (level == 6)
                        {
                            if (Global.optshape == "圆形")
                            {
                                gra.FillEllipse(bushred, Convert.ToInt32(((350 - Global.diameter) / (Global.optLine - 1)) * j), Convert.ToInt32(((340 - Global.diameter) / (Global.optRow - 1)) * i), Convert.ToInt32(Global.diameter), Convert.ToInt32(Global.diameter - 2));//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                                if (optArray[i, j] != 0)
                                    gra.DrawString(Convert.ToString(optArray[i, j]), myFont, bushblack, Convert.ToInt32(((350 - Global.diameter) / (Global.optLine - 1)) * j + Global.diameter / 3), Convert.ToInt32(((340 - Global.diameter) / (Global.optRow - 1)) * i + Global.diameter / 3));
                            }

                            else if (Global.optshape == "方形")
                            {
                                gra.FillRectangle(bushred, Convert.ToInt32(BlockWidth * j) + 5, (int)BlockHeight * i + 5, BlockWidth - 10, BlockHeight - 10);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                                //显示每个滤光片瑕疵总数
                                if (optArray[i, j] != 0)
                                    gra.DrawString(Convert.ToString(optArray[i, j]), myFont, bushblack, Convert.ToInt32(BlockWidth * j) + 5, Convert.ToInt32(BlockHeight * i + 5));
                            }

                        }
                        else if (level == 5)
                        {
                            if (Global.optshape == "圆形")
                            {
                                gra.FillEllipse(bushorange, Convert.ToInt32(((350 - Global.diameter) / (Global.optLine - 1)) * j), Convert.ToInt32(((340 - Global.diameter) / (Global.optRow - 1)) * i), Convert.ToInt32(Global.diameter), Convert.ToInt32(Global.diameter - 2));//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                                if (optArray[i, j] != 0)
                                    gra.DrawString(Convert.ToString(optArray[i, j]), myFont, bushblack, Convert.ToInt32(((350 - Global.diameter) / (Global.optLine - 1)) * j + Global.diameter / 3), Convert.ToInt32(((340 - Global.diameter) / (Global.optRow - 1)) * i + Global.diameter / 3));
                            }
                            else if (Global.optshape == "方形")
                            {
                                gra.FillRectangle(bushorange, Convert.ToInt32(BlockWidth * j) + 5, (int)BlockHeight * i + 5, BlockWidth - 10, BlockHeight - 10);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                                if (optArray[i, j] != 0)
                                    gra.DrawString(Convert.ToString(optArray[i, j]), myFont, bushblack, Convert.ToInt32(BlockWidth * j) + 5, Convert.ToInt32(BlockHeight * i + 5));
                            }

                        }
                        else if (level == 4)
                        {
                            if (Global.optshape == "圆形")
                            {
                                gra.FillEllipse(bushyellow, Convert.ToInt32(((350 - Global.diameter) / (Global.optLine - 1)) * j), Convert.ToInt32(((340 - Global.diameter) / (Global.optRow - 1)) * i), Convert.ToInt32(Global.diameter), Convert.ToInt32(Global.diameter - 2));//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                                if (optArray[i, j] != 0)
                                    gra.DrawString(Convert.ToString(optArray[i, j]), myFont, bushblack, Convert.ToInt32(((350 - Global.diameter) / (Global.optLine - 1)) * j + Global.diameter / 3), Convert.ToInt32(((340 - Global.diameter) / (Global.optRow - 1)) * i + Global.diameter / 3));
                            }
                            else if (Global.optshape == "方形")
                            {
                                gra.FillRectangle(bushyellow, Convert.ToInt32(BlockWidth * j) + 5, (int)BlockHeight * i + 5, BlockWidth - 10, BlockHeight - 10);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                                if (optArray[i, j] != 0)
                                    gra.DrawString(Convert.ToString(optArray[i, j]), myFont, bushblack, Convert.ToInt32(BlockWidth * j) + 5, Convert.ToInt32(BlockHeight * i + 5));
                            }

                        }
                        else if (level == 3)
                        {
                            if (Global.optshape == "圆形")
                            {
                                gra.FillEllipse(bushgreen, Convert.ToInt32(((350 - Global.diameter) / (Global.optLine - 1)) * j), Convert.ToInt32(((340 - Global.diameter) / (Global.optRow - 1)) * i), Convert.ToInt32(Global.diameter), Convert.ToInt32(Global.diameter - 2));//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                                if (optArray[i, j] != 0)
                                    gra.DrawString(Convert.ToString(optArray[i, j]), myFont, bushblack, Convert.ToInt32(((350 - Global.diameter) / (Global.optLine - 1)) * j + Global.diameter / 3), Convert.ToInt32(((340 - Global.diameter) / (Global.optRow - 1)) * i + Global.diameter / 3));
                            }
                            else if (Global.optshape == "方形")
                            {
                                gra.FillRectangle(bushgreen, Convert.ToInt32(BlockWidth * j) + 5, (int)BlockHeight * i + 5, BlockWidth - 10, BlockHeight - 10);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                                if (optArray[i, j] != 0)
                                    gra.DrawString(Convert.ToString(optArray[i, j]), myFont, bushblack, Convert.ToInt32(BlockWidth * j) + 5, Convert.ToInt32(BlockHeight * i + 5));
                            }

                        }
                    }
                    
                }
            }
            
            //在虚拟盘中心绘制细黑色十字线，便于定位
            float centerX = width / 2f;
            float centerY = height / 2f;
            Pen crossPen = new Pen(Color.Black, 1);
            gra.DrawLine(crossPen, centerX, 0, centerX, height);
            gra.DrawLine(crossPen, 0, centerY, width, centerY);
            crossPen.Dispose();
            
            panelOpticalfilter.BackgroundImage = backgroundBorder;
            panelOpticalfilter.BackgroundImageLayout = ImageLayout.Stretch;



            if (Global.getdefopot == "是")
            {
                if (GetDefectOptFlag)
                {
                    //扫描次品盘二维码
                    control.motor_run(7, 262);   //位置待确定
                    #region  扫描次品盘二维码
                    _serialPort2.PortName = "com3";
                    _serialPort2.Open();
                    defTrayDataReceivedHandler();
                    #endregion
                    //M34复位
                    //MessageBox.Show("8");
                    control.motor_run(2, 0);
                    control.motor_run(34, 0);
                    control.motor_run(1, 339);
                    


                    for (int i = 0; i < Global.optRow; i++)
                    {
                        for (int j = 0; j < Global.optLine; j++)
                        {
                            if (optArray[i, j] != 0)
                            {

                                defnumberflag++;    //次品数，从1开始
                                if (Global.optshape == "圆形")
                                {
                                    #region 取次品动作
                                    //M1位置
                                    M1loc = (371 - Global.diameter / 2 - (340 - Global.diameter - 0.2) / (Global.optRow - 1) * i);
                                    control.motor_run(1, M1loc);
                                    //M5左移
                                    M5loc = (372 + Global.diameter / 2 + (350 - Global.diameter - 0.2) / (Global.optLine - 1) * j);
                                    control.motor_run(5, M5loc);
                                    //M6下移
                                    M6loc = (85 - Global.thickness);
                                    control.motor_run(6, M6loc);
                                    //吸气
                                    control.btnGrabbing_Click();
                                    //M6上移
                                    control.motor_run(6, 0);
                                    //M5右移,+10缩进10mm
                                    M5loc = (24 + 10 + Global.diameter / 2  + (217 - Global.diameter - 2) / (defLineNum - 1) * ((defnumberflag - 1) % defLineNum));
                                    control.motor_run(5, M5loc);
                                    //M7位置,+10缩进10mm
                                    M7loc = (223 + 10 + Global.diameter / 2 + 1 + (330 - Global.diameter - 2) / (defRowNum - 1) * ((defnumberflag - 1) / defLineNum));
                                    control.motor_run(7, M7loc);

                                    M7loc = (220 + 10 + Global.diameter / 2 + 1 + (330 - Global.diameter - 2) / (defRowNum - 1) * ((defnumberflag - 1) / defLineNum));
                                    control.motor_run(7, M7loc);
                                    //M6下移
                                    M6loc = (118 - Global.thickness);
                                    control.motor_run(6, M6loc);
                                    //放气
                                    control.btnRelease_Click();
                                    //M6复位
                                    control.motor_run(6, 0);
                                    #endregion
                                }
                                else if (Global.optshape == "方形")
                                {
                                    #region 取次品动作
                                    //M1位置
                                    M1loc = (371 - Global.width / 2 - (340 - Global.width - 0.2) / (Global.optRow - 1) * i);
                                    control.motor_run(1, M1loc);
                                    //M5左移
                                    M5loc = (372 + Global.length / 2 + (350 - Global.length - 0.2) / (Global.optLine - 1) * j);
                                    control.motor_run(5, M5loc);
                                    //M6下移
                                    M6loc = (85 - Global.thickness);
                                    control.motor_run(6, M6loc);
                                    //吸气
                                    control.btnGrabbing_Click();
                                    //M6上移
                                    control.motor_run(6, 0);
                                    //M5右移
                                    M5loc = (24 + 10 + Global.length / 2 + 1 + (217 - Global.length - 2) / (defLineNum - 1) * ((defnumberflag - 1) % defLineNum));
                                    control.motor_run(5, M5loc);
                                    //M7位置
                                    M7loc = (223 + 10 + Global.width / 2 + 1 + (330 - Global.width - 2) / (defRowNum - 1) * ((defnumberflag - 1) / defLineNum));
                                    control.motor_run(7, M7loc);

                                    M7loc = (220 + 10 + Global.width / 2 + 1 + (330 - Global.width - 2) / (defRowNum - 1) * ((defnumberflag - 1) / defLineNum));
                                    control.motor_run(7, M7loc);
                                    //M6下移
                                    M6loc = (118 - Global.thickness);
                                    control.motor_run(6, M6loc);
                                    //放气
                                    control.btnRelease_Click();
                                    //M6复位
                                    control.motor_run(6, 0);

                                    #endregion
                                }


                                //判断次品盘是否装满
                                if (defnumberflag == (defRowNum * defLineNum))//
                                {
                                    control.motor_run(7, 0);    //次品盘装满，次品盘复位，等待更换
                                    control.OnlyWritepushRegHoldingBuf(40001, 3);   //次品盘装满，相当于停止键按下，亮黄灯，蜂鸣器响
                                    Global.allDeviceSta = false;
                                    //mainForm1.changecontinueButton(null, null);   //继续按钮使能
                                    while (Global.allDeviceSta == false) ;  //等待更换次品盘，并按下继续按钮
                                    control.OnlyWritepushRegHoldingBuf(40001, 5);   //绿灯
                                    defnumberflag = 0;
                                }

                                #region 次品数据存数据库
                                //defDataToDB(i, j);
                                #endregion
                            }
                        }
                    }

                    defnumberflag = 0;
                    GetDefectOptFlag = false;
                }
            }
        }

        void GetDefectFilt_TEST()
        {
            DataTable dt2 = MySqlHelper.GetDataTable(out err, "select count(*) from defection WHERE qrcode ='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "'");
            MessageBox.Show(Global.detectiontime);
            Global.defNumber = Convert.ToInt32(dt2.Rows[0][0]);      //获取总瑕疵数
            DataTable dt3 = MySqlHelper.GetDataTable(out err, "select posX,posY from defection WHERE qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "'");
            //获取瑕疵的位置
            DataTable dt4 = MySqlHelper.GetDataTable(out err, "select count(*) from defection WHERE qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "'");

            for (int i = 0; i < Global.defNumber; i++)
            {
                optArray[Convert.ToInt32(dt3.Rows[i][0]), Convert.ToInt32(dt3.Rows[i][1])] += 1;//统计每个滤光片瑕疵总数
            }

            Graphics gra = this.panelOpticalfilter.CreateGraphics();
            gra.Clear(this.BackColor);
            gra.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Brush bushgreen = new SolidBrush(Color.Green);//填充的颜色
            Brush bushgray = new SolidBrush(Color.Gray);//填充的颜色
            Brush bushred = new SolidBrush(Color.Red);//填充的颜色
            Font myFont = new Font("宋体", 8, FontStyle.Bold);
            Brush bushblack = new SolidBrush(Color.Black);//填充的颜色
            int width = this.panelOpticalfilter.Width; //宽度
            int height = this.panelOpticalfilter.Height;//高度
            for (int i = 0; i < Global.optRow; i++)
            {
                for (int j = 0; j < Global.optLine; j++)
                {
                    if (optArray[i, j] != 0)
                    {
                        if (Global.optshape == "圆形")
                        {
                            gra.FillEllipse(bushred, Convert.ToInt32(((350 - Global.diameter) / (Global.optLine - 1)) * j), Convert.ToInt32(((340 - Global.diameter) / (Global.optRow - 1)) * i), Convert.ToInt32(Global.diameter), Convert.ToInt32(Global.diameter - 2));//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                            //显示每个滤光片瑕疵总数
                            gra.DrawString(Convert.ToString(optArray[i, j]), myFont, bushblack, Convert.ToInt32(((350 - Global.diameter) / (Global.optLine - 1)) * j + Global.diameter / 3), Convert.ToInt32(((340 - Global.diameter) / (Global.optRow - 1)) * i + Global.diameter / 3));
                        }
                        else if (Global.optshape == "方形")
                        {
                            gra.FillRectangle(bushred, Convert.ToInt32(BlockWidth * j) + 5, (int)BlockHeight * i + 5, BlockWidth - 10, BlockHeight - 10);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                            //显示每个滤光片瑕疵总数
                            gra.DrawString(Convert.ToString(optArray[i, j]), myFont, bushblack, Convert.ToInt32(BlockWidth * j) + 5, Convert.ToInt32(BlockHeight * i + 5));
                        }

                    }
                    else
                    {
                        if (Global.optshape == "圆形")
                            gra.FillEllipse(bushgreen, Convert.ToInt32((350 - Global.diameter - 0.2) / (Global.optLine - 1) * j), (int)(340 - Global.diameter - 0.2) / (Global.optRow - 1) * i, Convert.ToInt32(Global.diameter), Convert.ToInt32(Global.diameter - 2));//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                        else if (Global.optshape == "方形")
                        {
                            gra.FillRectangle(bushgreen, Convert.ToInt32(BlockWidth * j) + 5, (int)BlockHeight * i + 5, BlockWidth - 10, BlockHeight - 10);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                        }

                    }
                }
            }

            MessageBox.Show(Convert.ToString(this.panelOpticalfilter.Width));
            MessageBox.Show(Convert.ToString(this.panelOpticalfilter.Height));

            

        }

        #endregion

        #region 启动
        public void Start_out()
        {

            #region 连接单片机
            if (!STMisConnected)
            {
                Addlog("STM32初始化...");
                STM32Connect();
                Addlog("STM32初始化完成");
            }
            #endregion
            while (control.GetpushRegHoldingBuf(40020) != 1) { MessageBox.Show("请确认按下设备启动键，等待电机复位完毕"); }

            #region 托盘伸出
            if (STMisConnected)
            {
                if (control.GetpushRegHoldingBuf(40008) == 1)   //设备开机键按下
                {
                    //改变电机运行顺序
                    control.OnlyWritepushRegHoldingBuf(40018, 2);
                    //托盘伸出
                    //M2左移295mm
                    control.motor_run(2, 355);
                    //M3M4上移45mm
                    control.motor_run(34, 51);
                    //M1前进1010mm
                    control.motor_run(1, 1032);
                    //改变电机运行顺序
                    control.OnlyWritepushRegHoldingBuf(40018, 1);
                    control.OnlyWritepushRegHoldingBuf(40006, 4);   //料盘释放
                }
            }
            #endregion
        }
        #endregion

        #region 检测

        //声明回调
        private setTextValueCallBack setCallBack;
        public void Start_Detection()
        {
            #region  连接扫码枪 并创建数据接收事件
          //  _serialPort1.PortName = "com4";
          //  _serialPort1.Open();
            DataReceivedHandler();      //二维码数据接收处理函数
            #endregion

           // control.OnlyWritepushRegHoldingBuf(40006, 5);   //料盘固定
            //构造滤光片矩阵
            optArray = new int[Global.optRow, Global.optLine];
            for (int i = 0; i < Global.optRow; i++)
            {
                for (int j = 0; j < Global.optLine; j++)
                {
                    optArray[i, j] = 0;                             //矩阵初值全为0
                }
            }

            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            string year = currentTime.Year.ToString();
            string month = currentTime.Month.ToString();
            string day = currentTime.Day.ToString();
            string hour = currentTime.Hour.ToString();
            string minute = currentTime.Minute.ToString();
            string second = currentTime.Second.ToString();
            Global.detectiontime = year + "-" + month + "-" + day + "-" + hour + "-" + minute; //不显示秒
            creatVirtualFilt();     //创建虚拟盘



           // Connect_UpCamera();     //连接上层相机
           // Connect_DownCamera();     //连接下层相机

            //复位
            //STMInitial_out();

            //Action takephoto = DetectionBegin;
            //AsyncCallback callback = ar =>
            //{
                DetectionWithDL2 detectionWithDL = new DetectionWithDL2();
                DetectionWithDL2 position4 = new DetectionWithDL2();
                HObject DL = new HObject();
                List<Task> IPTaskList = new List<Task>();
                Global.detectiontime_test = "2026-3-20-18-34";
                for (int i = 1; i < 9; i++)
                {

                    int s = i;


                    IPTaskList.Add(Task.Run(() =>
                    {

                        position4.ImageProcess1("D:/" + Global.qrCode + "/" + Global.detectiontime_test + "/camera" + s + "/ring", s);//
                    }));
                    //Task.WaitAll(IPTaskList.ToArray());

                }
                Task.WaitAll(IPTaskList.ToArray());
                IPTaskList.Clear();
                for (int i = 1;i < 9; i++)
                {
                    int k = i;
                    IPTaskList.Add(Task.Run(() =>
                    {
                        detectionWithDL.ImageProcess("D:/" + Global.qrCode + "/" + Global.detectiontime_test + "/camera" + k + "/ring");//
                    }));
                    //Task.WaitAll(IPTaskList.ToArray());
                }
                Task.WaitAll(IPTaskList.ToArray());

                //MessageBox.Show(detectionWithDL.getZx() + "," + detectionWithDL.getZy());
                int bshow = 0;
                setCallBack = new setTextValueCallBack(showDefection);
                textEdit5.Invoke(setCallBack,bshow);    //显示瑕疵数量
                //getDefInfo(); //从数据库获取瑕疵信息

                //defToOpt();//瑕疵位置映射到哪块滤光片
                GetDefectOptFlag = true;


                //IPTaskList.Add(Task.Run(() =>
                //{
                    GetDefectFilt();     //取瑕疵滤光片
                //}));
                //Task.WaitAll(IPTaskList.ToArray());



               //control.motor_run(5, 1);
               //control.motor_run(6, 1);
               //control.motor_run(7, 1);
               //control.motor_run(5, 0);
                //control.motor_run(6, 0);
                //control.motor_run(7, 0);
               // Start_out();    //检测取次品结束，托盘伸出，进行下一轮
                //camera1.DetectFinished();
               // camera2.DetectFinished();
               // camera3.DetectFinished();
               // camera4.DetectFinished();
               // camera5.DetectFinished();
               // camera6.DetectFinished();
              //  camera7.DetectFinished();
              //  camera8.DetectFinished();
               // bnClose_up_Click(); //上层相机关闭链接
               // bnClose_down_Click();   //下层相机关闭链接
           // };
            //takephoto.BeginInvoke(callback,null);

           // ToDiskThread = new Thread(ToDisk);  //图像检测与保存线程
           // ToDiskThread.Start();
          //  ToDiskThreadStat = true;

            //MessageBox.Show("test2");


        }
        #endregion

        #region   扫码枪数据接收结束处理函数
        //private void DataReceivedHandler(object sender,SerialDataReceivedEventArgs e)
        private void DataReceivedHandler()
        {
            try
            {
               // _serialPort1.DiscardInBuffer();  //丢弃来自串行驱动程序的接收缓冲区的数据。
               //string serialText = "RDC030";
               // _serialPort1.Write(serialText);
               // Global.qrCode = _serialPort1.ReadLine().Substring(1, 4);
               // while (Global.qrCode == "") Global.qrCode = _serialPort1.ReadLine();
                //Global.cs文件定义的Global类中，存放了静态全局通用的变量 qrCode 等
                Global.qrCode = "0004";
                //MessageBox.Show(Global.qrCode);
                if (Global.qrCode != "")
                {
                    _continue = false;
                    //Uint32 k;
                    //根据读到的qr码到数据库表中读取该tray的相关数据，如thickness，以便控制z轴的高度
                    MySqlConnection conn = new MySqlConnection(Global.conString);//数据库连接参数放在Global.cs的全局变量conString中
                    conn.Open();
                    MySqlCommand comm = new MySqlCommand();
                    comm.Connection = conn;
                    // comm.CommandText = "select idtray,thickness from tray where qrcode= '" + Global.qrCode + "'";
                    comm.CommandText = "select optshape,diameter,length,width from tray where qrcode='" + Global.qrCode + "'";
                    //MessageBox.Show(comm.CommandText);
                    MySqlDataReader trayTableReader;
                    trayTableReader = comm.ExecuteReader();
                    trayTableReader.Read();
                    //Global.idtray = Convert.ToInt32(trayTableReader.GetValue(0));
                    Global.optshape = Convert.ToString(trayTableReader.GetValue(0));
                    Global.diameter = Convert.ToDouble(trayTableReader.GetValue(1));//读取tray表中滤光片的直径
                    Global.length = Convert.ToDouble(trayTableReader.GetValue(2));//读取tray表中滤光片的直径
                    Global.width = Convert.ToDouble(trayTableReader.GetValue(3));//读取tray表中滤光片的直径
                    optdiameter1 = Global.diameter;
                    //Global.thickness = Convert.ToDouble(trayTableReader.GetValue(4));//读取tray表中滤光片的厚度
                    //optthickness1 = Global.thickness;

                    

                    if (Global.optshape == "圆形")
                    {
                        //滤光片矩阵
                        Global.optRow = (int)Math.Floor(((340 - optdiameter1 - 0.2) / (optdiameter1 + 0.2 + 1)) + 1);
                        Global.optLine = (int)Math.Floor(((350 - optdiameter1 - 0.2) / (optdiameter1 + 0.2 + 1)) + 1);

                        //次品盘矩阵
                        defRowNum = (int)Math.Floor(((350 - optdiameter1 - 2) / (optdiameter1 + 2.0 + 1)) + 1);
                        defLineNum = (int)Math.Floor(((237 - optdiameter1 - 2) / (optdiameter1 + 2.0 + 1)) + 1);
                    }
                    else if(Global.optshape == "方形")
                    {
                        //滤光片矩阵
                        Global.optRow = (int)Math.Floor(((340 - Global.width - 0.2) / (Global.width + 0.2 + 5)) + 1);
                        Global.optLine = (int)Math.Floor(((350 - Global.length - 0.2) / (Global.length + 0.2 + 5)) + 1);

                        //次品盘矩阵
                        defRowNum = (int)Math.Floor(((350 - Global.width - 2) / (Global.width + 2.0 + 5)) + 1);
                        defLineNum = (int)Math.Floor(((237 - Global.length - 2) / (Global.length + 2.0 + 5)) + 1);
                    }


                    comm.Dispose();
                    conn.Close();
                    //MessageBox.Show("该滤光片盘的信息如下：" + "QR码" + Global.qrCode);
                    textBox2.Text = Global.qrCode;
                    _serialPort1.Close();
                }
                else { MessageBox.Show("未读到二维码数值"); }

            }
                catch (TimeoutException) { }
        }

        private void defTrayDataReceivedHandler()
        {
            try
            {
                _serialPort2.DiscardInBuffer();  //丢弃来自串行驱动程序的接收缓冲区的数据。
                string serialText = "RDC030";
                _serialPort2.Write(serialText);
                Global.defTrayqrCode = _serialPort2.ReadLine().Substring(1, 4);
                while (Global.defTrayqrCode == "") { Global.defTrayqrCode = _serialPort2.ReadLine().Substring(1, 4); }
                //Global.cs文件定义的Global类中，存放了静态全局通用的变量 qrCode 等

                //MessageBox.Show(Global.defTrayqrCode);

                if (Global.defTrayqrCode != "")
                {
                    
                    //MessageBox.Show("该次品盘的信息如下：" + "QR码" + Global.defTrayqrCode);
                    _serialPort2.Close();
                }
                else { MessageBox.Show("未读到二维码数值"); }

            }
            catch (TimeoutException) { }
        }
        #endregion

        #region 停止检测
        public void Stop_Detection()
        {

                control.OnlyWritepushRegHoldingBuf(40001, 2);   //下位机停止
                if (DisplayDefectStat)
                {
                    DisplayDefectStat = false;
                    DefectThread.Join();
                }
           

                if (DetectionThreadStat)
                {
                    detectionThread.Join();
                    DetectionThreadStat = false;

                }
                if (ToDiskThreadStat)
                {
                    while (true)
                    {
                        int all = 0;
                        all += camera1.cameraData.Length;
                        all += camera2.cameraData.Length;
                        all += camera3.cameraData.Length;
                        all += camera4.cameraData.Length;
                        all += camera5.cameraData.Length;
                        all += camera6.cameraData.Length;
                        all += camera7.cameraData.Length;
                        all += camera8.cameraData.Length;
                        if (all == 0)
                        {
                            camera1.DetectFinished();
                            camera2.DetectFinished();
                            camera3.DetectFinished();
                            camera4.DetectFinished();
                            camera5.DetectFinished();
                            camera6.DetectFinished();
                            camera7.DetectFinished();
                            camera8.DetectFinished();
                            disk = false;
                            break;
                        }
                        Thread.Sleep(10);
                    }
                    camera1.cameraData.Release();
                    camera2.cameraData.Release();
                    camera3.cameraData.Release();
                    camera4.cameraData.Release();
                    camera5.cameraData.Release();
                    camera6.cameraData.Release();
                    camera7.cameraData.Release();
                    camera8.cameraData.Release();
                    ToDiskThread.Join();
                    ShowDefectsStat = false;
                    //filterThread.Join();
                }

                if (STMisConnected)
                {
                    control.DisConnect();
                    STMisConnected = false;
                }

                //开始计时，用于计算整个工作耗时
                DateTime time2 = DateTime.Now;
                TimeSpan ts = time2 - datetime1;
                
                
                //bnStopGrab_Click(null, null);
                //bnClose_Click
        }
        #endregion 

        #region 日志信息
        private void Addlog(string info)
        {
            DataRow dataRow;
            dataRow = dt.NewRow();
            dataRow["时间"] = currentTime;
            dataRow["信息"] = info.ToString();
            dt.Rows.InsertAt(dataRow, 0);
            gridView1.FocusedRowHandle = 0;
            Application.DoEvents();
            
        }
        #endregion

        #region 数据库操作
        private void defDataToDB(int i,int j)
        {
            DataTable dt = MySqlHelper.GetDataTable(out err, "select count(*) from defection where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and posX='" + i + "' and posY='" + j + "'");
            Global.defNumberOnSameFilt = Convert.ToInt32(dt.Rows[0][0]);

            DataTable dt1 = MySqlHelper.GetDataTable(out err, "select defectionType,area from defection where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and posX='" + i + "' and posY='" + j + "'");

            
            
            for (int k = 0; k < Global.defNumberOnSameFilt; k++)
            {
                string Query3 = "insert into deftrayinfo(defTrayqrcode,defFiltID,defType,defArea) values('" + Global.defTrayqrCode + "'," + defnumberflag + ",'" + dt1.Rows[k][0].ToString() + "'," + Convert.ToDouble(dt1.Rows[k][1]) + ")";  //将i行j列个滤光片上的瑕疵信息，插入到次品盘信息表中
                //cmd2 = new MySqlCommand(Query3, conn);
                //cmd2.ExecuteNonQuery();
                MySqlHelper.UpdateData(out err, Query3);
            }
            //comm.Dispose();
            //conn.Close();
            
            
        }
        #endregion

        #region 显示瑕疵数量
        void DisplayDefect()
        {
            while (DisplayDefectStat)
            {
                if (textEdit5.Text != defectType[0].ToString())
                {
                    textEdit5.Text = defectType[0].ToString();
                }
                if (textEdit6.Text != defectType[1].ToString())
                {
                    textEdit6.Text = defectType[1].ToString();
                }
                if (textEdit7.Text != defectType[2].ToString())
                {
                    textEdit7.Text = defectType[2].ToString();
                }
                if (textEdit8.Text != defectType[3].ToString())
                {
                    textEdit8.Text = defectType[3].ToString();
                }
                if (textEdit9.Text != defectType[4].ToString())
                {
                    textEdit9.Text = defectType[4].ToString();
                }
                if (textEdit10.Text != defectType[5].ToString())
                {
                    textEdit10.Text = defectType[5].ToString();
                }
            }
        }
        #endregion
     
        #region 完成检测
        //void FinishDetection()
        //{
            
        //        GetDefectOpt(optArray);
        //}
        #endregion
        
        private void button1_Click(object sender, EventArgs e)
        {
            //构造滤光片矩阵
            optArray = new int[Global.optRow, Global.optLine];
            for (int i = 0; i < Global.optRow; i++)
            {
                for (int j = 0; j < Global.optLine; j++)
                {
                    optArray[i, j] = 0;                             //矩阵初值全为0
                }
            }
            DetectionWithDL2 detectionWithDL = new DetectionWithDL2();
            detectionWithDL.ImageProcess("c:/" + Global.qrCode + "/2021-6-29-13-25" + "/camera1" + "/ring");
            detectionWithDL.ImageProcess("c:/" + Global.qrCode + "/2021-6-29-13-25" + "/camera2" + "/ring");
            detectionWithDL.ImageProcess("c:/" + Global.qrCode + "/2021-6-29-13-25" + "/camera3" + "/ring");
            detectionWithDL.ImageProcess("c:/" + Global.qrCode + "/2021-6-29-13-25" + "/camera4" + "/ring");
            detectionWithDL.ImageProcess("c:/" + Global.qrCode + "/2021-6-29-13-25" + "/camera5" + "/ring");
            detectionWithDL.ImageProcess("c:/" + Global.qrCode + "/2021-6-29-13-25" + "/camera6" + "/ring");
            detectionWithDL.ImageProcess("c:/" + Global.qrCode + "/2021-6-29-13-25" + "/camera7" + "/ring");
            detectionWithDL.ImageProcess("c:/" + Global.qrCode + "/2021-6-29-13-25" + "/camera8" + "/ring");
            List<Task> IPTaskList = new List<Task>();
            IPTaskList.Add(Task.Run(() =>
            {
                GetDefectFilt_TEST();     //取瑕疵滤光片
            }));
            Task.WaitAll(IPTaskList.ToArray());

        }
        

        #region 下层相机扫描时更改窗体控件属性
        //定义回调
        private delegate void setTextValueCallBack(int value);
        //声明回调
        private setTextValueCallBack setCallBack_up;
        //private void changepictureEditname_Up()
        //{
        //    //实例化回调
        //    setCallBack_up = new setTextValueCallBack(SetValue);
        //    //创建一个线程去执行这个方法:创建的线程默认是前台线程
        //    Thread thread = new Thread(new ThreadStart(Test));
        //    //Start方法标记这个线程就绪了，可以随时被执行，具体什么时候执行这个线程，由CPU决定
        //    //将线程设置为后台线程
        //    thread.IsBackground = true;
        //    thread.Start();
        //}

        private void Test()
       {
            //使用回调
            textEdit1.Invoke(setCallBack_up,1);
           
       }

        private void pictureEdit9_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void pictureEdit1_EditValueChanged(object sender, EventArgs e)
        {

        }

        //private void button3_Click(object sender, EventArgs e)
        //{
        //    Global.qrCode = ""+Global.qrCode+"";
        //    int k = 1;
        //    DetectionWith detectionWithDL = new DetectionWith();
        //    detectionWithDL.ImageProcess("c:/" + Global.qrCode + "/camera" + k + "/ring");//
        //}

        /// <summary>
        /// 定义回调使用的方法
        /// </summary>
        /// <param name="value"></param>
        private void SetValue(int value)
       {
            this.textEdit1.EditValue = "相机5";
            this.textEdit2.EditValue = "相机6";
            this.textEdit3.EditValue = "相机7";
            this.textEdit4.EditValue = "相机8";
        }
        #endregion





        #region 从数据库获取瑕疵信息
        public void getDefInfo()
        {
            //连接数据库
            MySqlConnection conn = new MySqlConnection(Global.conString);//数据库连接参数放在Global.cs的全局变量conString中

            conn.Open();

            MySqlCommand comm = new MySqlCommand();
            comm.Connection = conn;

            comm.CommandText = "select count(*) from defection WHERE qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "'";     //拼接sql语句
            //select idtray,count(idtray) from defection WHERE idtray=1 GROUP BY idtray HAVING countidtray > 1;


            MySqlDataReader trayTableReader;
            trayTableReader = comm.ExecuteReader();
            trayTableReader.Read();
            // Global.height = double.Parse(trayTableReader.GetValue(0));

            Global.defNumber = Convert.ToInt32(trayTableReader.GetValue(0));  //读取defection表中idtray相同的数据的数量，即一个物料盘上瑕疵总数
            comm.Dispose();
            Global.defInfoArr = new Global.defInfo[Global.defNumber];   //实例化瑕疵数组，用于存储各个瑕疵的信息
            String CommandText = "select picName,picX,picY from defection WHERE qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "'";
            // MessageBox.Show(comm.CommandText);
            MySqlDataAdapter adapter = new MySqlDataAdapter(CommandText, conn);
       
            DataTable dt = new DataTable();
            adapter.Fill(dt) ;
            for (int i = 0; i < Global.defNumber;i ++)
            {
                //拼接sql语句,获取当前盘第几个瑕疵的信息
                //comm.CommandText = "select picName,picX,picY from defection WHERE defectionId=" + i + " and qrcode="+Global.qrCode+"";
                String[] str1 = dt.Rows[i][0].ToString().Split('-');     //分隔字符串
                //MessageBox.Show(dt.Rows[i][0].ToString());
                Global.defInfoArr[i ].cameraId = Convert.ToInt32(str1[0]);   //获得瑕疵所在的照片的相机号
                Global.defInfoArr[i ].row = Convert.ToInt32(str1[1]);        //获得瑕疵所在的照片的行号
                Global.defInfoArr[i ].col = Convert.ToInt32(str1[2]);        //获得瑕疵所在的照片的列号
                Global.defInfoArr[i ].defX = Convert.ToDouble(dt.Rows[i][1]);      //获得瑕疵在照片上的像素横坐标
                Global.defInfoArr[i ].defY = Convert.ToDouble(dt.Rows[i][2]);      //获得瑕疵在照片上的像素横坐标
                comm.Dispose();
                trayTableReader.Close();
            }
            
            conn.Close();
        }
        #endregion

        #region 识别瑕疵滤光片位置
        public void defToOpt()
        {

            getDefInfo(); //从数据库获取瑕疵信息
            //将所有瑕疵对应到滤光片上
            for (int i = 0; i < Global.defNumber; i++)
                getDefLoc(Global.defInfoArr[i].cameraId, Global.defInfoArr[i].row, Global.defInfoArr[i].col, Global.defInfoArr[i].defX, Global.defInfoArr[i].defY);
            
            GetDefectOptFlag = true;
            GetDefectOpt(optArray);     //取瑕疵滤光片
        }
        public void getDefLoc(int cameraId,int row,int col,double defX,double defY)
        {
            double YLoc = 0;    //照片Y轴位置
            double XLoc = 0;    //照片X轴位置
            int defRow = 0;
            int defCol = 0;

            //计算瑕疵的实际位置
            if (row % 2 == 0)
            {
                switch (cameraId)
                {
                    case 3:
                        YLoc = (col - 6) * 11.993436 + defX * (12.30 / 512) - 9.42;    //修改后3
                        XLoc = row * 59 + defY * (15.80 / 512) - 1.47;          //修改后3  
                        break;
                    case 1:
                        YLoc = (col - 12) * 11.993436 + defX * (12.30 / 512) - 3.92;  //修改后3
                        XLoc = row * 59 + defY * (15.80 / 512) + 13.20;      //修改后3
                        break;
                    case 4:
                        YLoc = (col - 3) * 11.993436 + defX * (12.30 / 512) - 2.71;   //修改后3
                        XLoc = row * 59 + defY * (15.80 / 512) + 27.72;      //修改后3
                        break;
                    case 2:
                        YLoc = (col - 8) * 11.993436 + defX * (12.30 / 512) - 9.33;    //修改后3
                        XLoc = row * 59 + defY * (15.80 / 512) + 42.66;      //修改后3
                        break;

                    case 5:
                        YLoc = (col - 8) * 11.993436 + defX * (12.30 / 512) - 6.43;    //修改后
                        XLoc = row * 59 + defY * (15.80 / 512) - 2.13;              //修改后
                        break;
                    case 7:
                        YLoc = (col - 3) * 11.993436 + defX * (12.30 / 512) - 5.24;    //修改后
                        XLoc = row * 59 + defY * (15.80 / 512) + 14.51;             //修改后
                        break;
                    case 6:
                        YLoc = (col - 12) * 11.993436 + defX * (12.30 / 512) - 2.25;   //修改后
                        XLoc = row * 59 + defY * (15.80 / 512) + 28.86;             //修改后
                        break;
                    case 8:
                        YLoc = (col - 6) * 11.993436 + defX * (12.30 / 512) - 8.75;   //修改后
                        XLoc = row * 59 + defY * (15.80 / 512) + 43.86;             //修改后
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (cameraId)
                {
                    case 3:
                        YLoc = (36 - col) * 11.993436 + defX * (12.30 / 512) - 2.34; //修改后3
                        XLoc = row * 59 + defY * (15.80 / 512) - 1.56;          //修改后3
                        break;
                    case 1:
                        YLoc = (31 - col) * 11.993436 + defX * (12.30 / 512) - 10.13;    //修改后3
                        XLoc = row * 59 + defY * (15.80 / 512) + 13.39;           //修改后3
                        break;
                    case 4:
                        YLoc = (40 - col) * 11.993436 + defX * (12.30 / 512) - 6.17; //修改后3
                        XLoc = row * 59 + defY * (15.80 / 512) + 27.67;      //修改后3
                        break;
                    case 2:
                        YLoc = (34 - col) * 11.993436 + defX * (12.30 / 512) - 3.32;    //修改后3
                        XLoc = row * 59 + defY * (15.80 / 512) + 42.65;      //修改后3
                        break;

                    case 5:
                        YLoc = (35 - col) * 11.993436 + defX * (12.30 / 512) - 12.11; //修改后
                        XLoc = row * 59 + defY * (15.80 / 512) - 1.01;              //修改后
                        break;
                    case 7:
                        YLoc = (40 - col) * 11.993436 + defX * (12.30 / 512) - 7.56;    //修改后
                        XLoc = row * 59 + defY * (15.80 / 512) + 13.63;              //修改后
                        break;
                    case 6:
                        YLoc = (31 - col) * 11.993436 + defX * (12.30 / 512) - 8.43;   //修改后
                        XLoc = row * 59 + defY * (15.80 / 512) + 29.03;              //修改后
                        break;
                    case 8:
                        YLoc = (35 - col) * 11.993436 + defX * (12.30 / 512) - 1.38;   //修改后
                        XLoc = row * 59 + defY * (15.80 / 512) + 43.69;
                        break;
                    default:
                        break;
                }
            }

            //将瑕疵位置转化为对用的滤光片矩阵位置
            double dx = (((350 - optdiameter1 - 0.2) / (Global.optLine - 1)) - optdiameter1 - 0.2);
            double dy = (((340 - optdiameter1 - 0.2) / (Global.optRow - 1)) - optdiameter1 - 0.2);
            defCol = (int)Math.Floor(XLoc / (optdiameter1 + 0.2 + dx));
            defRow = (int)Math.Floor( YLoc / ( optdiameter1 + 0.2 + dy));
            //MessageBox.Show(optRow.ToString()+","+defRow + ","+defCol + ","+optLine);
            //将有瑕疵的滤光片标记为1
            if ((defRow > 0 && defRow < Global.optRow)&&(defCol > 0 && defCol < Global.optLine))
            {
                optArray[Global.optRow - defRow - 1, Global.optLine - defCol - 1] = 1;
            }
        }
        #endregion


        //创建虚拟盘
        private void creatVirtualFilt()
        {
            DrawBackground();
            DrawMargin();
        }

        #region 各种笔刷资源

        /// <summary>
        /// 画标题的方框的灰刷子
        /// </summary>
        private Brush GrayBrush_G = null;

        /// <summary>
        /// 标题方框的橙色刷子(当有信号重叠时，所在的行和列所使用的)
        /// </summary>
        private Brush OrangeBrush_G = null;

        /// <summary>
        /// 写标题数字的刷子
        /// </summary>
        private Brush BlackBrush_G = null;

        /// <summary>
        /// 写所有0-63网格数字的灰刷子
        /// </summary>
        private Brush DeepGrayBrush_G = null;

        /// <summary>
        /// 标题数字的字体
        /// </summary>
        private Font BigFont_G = null;

        /// <summary>
        /// 0-63网格数字的字体
        /// </summary>
        private Font SmallFont_G = null;

        /// <summary>
        /// 重叠网格
        /// 【用于叹号】
        /// </summary>
        private Font OverLayFont_G = null;

        /// <summary>
        /// 划7条行线和列线的笔
        /// </summary>
        private Pen GrayPen_G = null;

        /// <summary>
        /// 画外边框的笔
        /// </summary>
        private Pen BlackPen_G = null;

        /// <summary>
        /// 居中对齐
        /// </summary>
        private StringFormat CenterSF_G = null;

        /// <summary>
        /// 右上角对齐
        /// </summary>
        private StringFormat TopRightSF_G = null;

        private void panelOpticalfilter_MouseDown(object sender, MouseEventArgs e)
        {
            if (BlockHeight != 0 && BlockWidth != 0)
            {
                //创建Bitmap用于绘制和保存
                Bitmap detailBitmap = new Bitmap(this.detailpanel.Width, this.detailpanel.Height);
                Graphics gra = Graphics.FromImage(detailBitmap);
                gra.Clear(this.BackColor);
                gra.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Brush bushwhite = new SolidBrush(Color.Black);//填充的颜色
                Brush bushred = new SolidBrush(Color.Red);//填充的颜色
                Brush bushgreen = new SolidBrush(Color.Green);//填充的颜色
                Font myFont = new Font("宋体", 8, FontStyle.Bold);
                Brush bushyellow = new SolidBrush(Color.Yellow);//黄色
                Brush bushorange = new SolidBrush(Color.Orange);//橙色


                ////获取鼠标落下时候的单元格坐标
                if (Global.optshape == "圆形")
                {
                    lastRow = e.Y / Convert.ToInt32((340 - optdiameter1 - 0.2) / (Global.optRow - 1));//确定行的位置
                    lastCol = e.X / Convert.ToInt32((350 - Global.diameter - 0.2) / (Global.optLine - 1));//列数
                }
                else if (Global.optshape == "方形")
                {
                    lastRow = e.Y / Convert.ToInt32(BlockHeight);//确定行的位置
                    lastCol = e.X / Convert.ToInt32(BlockWidth);//列数
                }

                //获取滤光片等级
                string order = "select level from filterlevel where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and posX='" + lastRow + "' and posY='" + lastCol + "'";
                DataTable dt_now = MySqlHelper.GetDataTable(out err, order);
                int level = 3;
                if (dt_now != null && dt_now.Rows != null && dt_now.Rows.Count > 0)
                {
                    level = Convert.ToInt32(dt_now.Rows[0][0]);
                }
                //if (optArray[i, j] != 0)
                if (level == 6)
                {
                    if (Global.optshape == "圆形")
                    {
                        circlewidth = this.detailpanel.Width; //宽度
                        circleheight = this.detailpanel.Height;//高度
                        gra.FillEllipse(bushred, 0, 0, circlewidth, circleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    else if (Global.optshape == "方形")
                    {
                        rectanglewidth = this.detailpanel.Width; //宽度
                        rectangleheight = Convert.ToInt32(Global.width * (this.detailpanel.Width / Global.length));//高度
                        gra.FillRectangle(bushred, 0, 0, rectanglewidth, rectangleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                }
                else if (level == 5)
                {
                    if (Global.optshape == "圆形")
                    {
                        circlewidth = this.detailpanel.Width; //宽度
                        circleheight = this.detailpanel.Height;//高度
                        gra.FillEllipse(bushorange, 0, 0, circlewidth, circleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    else if (Global.optshape == "方形")
                    {
                        rectanglewidth = this.detailpanel.Width; //宽度
                        rectangleheight = Convert.ToInt32(Global.width * (this.detailpanel.Width / Global.length));//高度
                        gra.FillRectangle(bushorange, 0, 0, rectanglewidth, rectangleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                }
                else if (level == 4)
                {
                    if (Global.optshape == "圆形")
                    {
                        circlewidth = this.detailpanel.Width; //宽度
                        circleheight = this.detailpanel.Height;//高度
                        gra.FillEllipse(bushyellow, 0, 0, circlewidth, circleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    else if (Global.optshape == "方形")
                    {
                        rectanglewidth = this.detailpanel.Width; //宽度
                        rectangleheight = Convert.ToInt32(Global.width * (this.detailpanel.Width / Global.length));//高度
                        gra.FillRectangle(bushyellow, 0, 0, rectanglewidth, rectangleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                }
                else if (level == 3)
                {
                    if (Global.optshape == "圆形")
                    {
                        circlewidth = this.detailpanel.Width; //宽度
                        circleheight = this.detailpanel.Height;//高度
                        gra.FillEllipse(bushgreen, 0, 0, circlewidth, circleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    else if (Global.optshape == "方形")
                    {
                        rectanglewidth = this.detailpanel.Width; //宽度
                        rectangleheight = Convert.ToInt32(Global.width * (this.detailpanel.Width / Global.length));//高度
                        gra.FillRectangle(bushgreen, 0, 0, rectanglewidth, rectangleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                }


                //MessageBox.Show("点击" + lastRow.ToString() + "," + lastCol.ToString());
                string query = "select * from defection where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and posX='" + lastRow + "' and posY='" + lastCol + "'";
                string ab;
                ////query = "select * from defection";
                DataTable dt = MySqlHelper.GetDataTable(out ab, query);
                DataTable dt2 = MySqlHelper.GetDataTable(out err, "select count(*) from defection where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and posX='" + lastRow + "' and posY='" + lastCol + "'");//获取该片上的瑕疵数
                double diametermultiple = 250 / Global.diameter;
                double lengthmultiple = 250 / Global.length;
                double widthmultiple = lengthmultiple;

                int num1 = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                int num5 = 0;
                int num6 = 0;
                int num7 = 0;
                int num8 = 0;
                int num9 = 0;
                int num10 = 0;
                if (Convert.ToInt32(dt2.Rows[0][0]) != 0)
                {
                    //画叉
                    if (Global.optshape == "圆形")
                    {
                        for (int i = 0; i < Convert.ToInt32(dt2.Rows[0][0]); i++)
                        {
                            //相对于单个滤光片的坐标
                            int bigNewX = Convert.ToInt32(350 - Convert.ToInt32(dt.Rows[i][5]) - (Global.diameter + ((350 - Global.diameter) / (Global.optLine - 1)) - Global.diameter) * lastCol);
                            int bigNewY = Convert.ToInt32(340 - Convert.ToInt32(dt.Rows[i][6]) - (Global.diameter + ((340 - Global.diameter) / (Global.optRow - 1)) - Global.diameter) * lastRow);
                            //MessageBox.Show(bigNewX.ToString() + "," + bigNewY.ToString());
                            switch (dt.Rows[i][9])
                            {
                                case "划痕":     //划痕
                                    gra.DrawString("1", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                    num1++;
                                    break;
                                case "内布毛":     //布毛
                                    gra.DrawString("2", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                    num2++;
                                    break;
                                case "外布毛":     //膜破
                                    gra.DrawString("3", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                    num3++;
                                    break;
                                case "水印":     //气泡
                                    gra.DrawString("4", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                    num4++;
                                    break;
                                case "内点子":   //内点子
                                    gra.DrawString("5", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                    num5++;
                                    break;
                                case "腐蚀印":     //腐蚀印
                                    gra.DrawString("6", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                    num6++;
                                    break;
                                case "点子":     //点子
                                    gra.DrawString("7", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                    num7++;
                                    break;
                                case "手印":     //手印
                                    gra.DrawString("8", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                    num8++;
                                    break;
                                case "气泡":     //水印
                                    gra.DrawString("9", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                    num9++;
                                    break;
                                case "膜破":     //膜外布毛
                                    gra.DrawString("10", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                    num10++;
                                    break;
                                //case "水印":     //水印
                                //    gra.DrawString("11", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                //    num11++;
                                //    break;
                                //case "内点子":     //针孔
                                //    gra.DrawString("7", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                //    num12++;
                                //    break;
                                default:
                                    break;

                            }
                        }
                    }
                    else if (Global.optshape == "方形")
                    {
                        for (int i = 0; i < Convert.ToInt32(dt2.Rows[0][0]); i++)
                        {
                            //相对于单个滤光片的坐标
                            int bigNewX = Convert.ToInt32(350 - Convert.ToInt32(dt.Rows[i][5]) - (Global.length + ((350 - Global.length) / (Global.optLine - 1)) - Global.length) * lastCol);
                            int bigNewY = Convert.ToInt32(340 - Convert.ToInt32(dt.Rows[i][6]) - (Global.width + ((340 - Global.width) / (Global.optRow - 1)) - Global.width) * lastRow);
                            //MessageBox.Show(bigNewX.ToString() + "," + bigNewY.ToString());
                            switch (dt.Rows[i][9])
                            {
                                case "划痕":     //划痕
                                    gra.DrawString("1", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                    num1++;
                                    break;
                                case "内布毛":     //内布毛
                                    gra.DrawString("2", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                    num2++;
                                    break;
                                case "外布毛":     //外布毛
                                    gra.DrawString("3", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                    num3++;
                                    break;
                                case "水印":     //水印
                                    gra.DrawString("4", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                    num4++;
                                    break;
                                case "内点子":     //内点子
                                    gra.DrawString("5", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                    num5++;
                                    break;
                                case "腐蚀印":     //腐蚀印
                                    gra.DrawString("6", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                    num6++;
                                    break;
                                case "点子":     //点子
                                    gra.DrawString("7", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                    num7++;
                                    break;
                                case "手印":     //手印
                                    gra.DrawString("8", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                    num8++;
                                    break;
                                case "气泡":     //气泡
                                    gra.DrawString("9", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                    num9++;
                                    break;
                                case "膜破":     //膜破
                                    gra.DrawString("10", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                    num10++;
                                    break;
                                //case "水印":     //水印
                                //    gra.DrawString("11", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                //    num11++;
                                //    break;
                                //case "内点子":     //针孔
                                //    gra.DrawString("12", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                //    num12++;
                                //    break;
                                default:
                                    break;
                            }
                        }
                    }
                    //电机红色滤光片ListView显示相应照片名
                    getPictureFileName();
                }
                else
                {
                    if (Global.optshape == "圆形")
                    {
                        gra.FillEllipse(bushgreen, 0, 0, circlewidth, circleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    else if (Global.optshape == "方形")
                    {
                        gra.FillRectangle(bushgreen, 0, 0, rectanglewidth, rectangleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                }
                
                //在detailpanel中心绘制细黑色十字线，根据实际绘制的图形尺寸
                float actualWidth, actualHeight;
                if (Global.optshape == "圆形")
                {
                    actualWidth = circlewidth;
                    actualHeight = circleheight;
                }
                else
                {
                    actualWidth = rectanglewidth;
                    actualHeight = rectangleheight;
                }
                
                float centerX = actualWidth / 2f;
                float centerY = actualHeight / 2f;
                using (Pen crossPen = new Pen(Color.Black, 1))
                {
                    gra.DrawLine(crossPen, centerX, 0, centerX, actualHeight);
                    gra.DrawLine(crossPen, 0, centerY, actualWidth, centerY);
                }
                
                textEdit5.Text = num1.ToString();   //划痕
                textEdit6.Text = num2.ToString();   //内布毛
                textEdit7.Text = num3.ToString();   //膜破
                textEdit8.Text = num4.ToString();   //气泡
                textEdit9.Text = num5.ToString();   //崩边
                textEdit10.Text = num6.ToString();  //腐蚀印
                textEdit15.Text = num7.ToString(); //针孔
                textEdit16.Text = num8.ToString();  //手印
                textEdit17.Text = num9.ToString();   //麻点
                textEdit18.Text = num10.ToString();  //喷溅点
                
                //将绘制的图像显示到detailpanel并保存到文件
                this.detailpanel.BackgroundImage = detailBitmap;
                
                //保存图片到指定文件夹
                string saveFolder = "D://" + Global.qrCode + "/" + Global.detectiontime + "/DefectDetail";
                if (!System.IO.Directory.Exists(saveFolder))
                {
                    System.IO.Directory.CreateDirectory(saveFolder);
                }
                string fileName = saveFolder + "/FilterDetail_" + lastRow + "-" + lastCol + ".bmp";
                detailBitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
                
                gra.Dispose();
            }
        }

        /// <summary>
        /// 左下角对齐
        /// </summary>
        private StringFormat BottomLeftSF_G = null;

        private void panelOpticalfilter_MouseHover(object sender, EventArgs e)
        {
            //Point p = this.PointToClient(Control.MousePosition);
            //toolTip1.Show(p.Y + "," + p.X, panelOpticalfilter);
            ////获取鼠标落下时候的单元格坐标
            //if (BlockHeight != 0 && BlockWidth != 0)
            //{
            //    lastRow = (p.Y) / BlockHeight + startRow;//确定行的位置
            //    lastCol = (p.X) / BlockWidth;//列数
            //                                 //MessageBox.Show( lastRow.ToString()+","+lastCol.ToString() );
            //    string query = "select DISTINCT (select count(*) from defection where defectionType='Prints' and  qrcode='" + Global.qrCode + "' and posY='" + lastRow + "' and posX='" + lastCol + "' ) as 印痕" +
            //                ",(select count(*) from defection where defectionType = 'Swabs' and  qrcode = '" + Global.qrCode + "' and posY='" + lastRow + "' and posX='" + lastCol + "' ) as 布毛" +
            //                ",(select count(*) from defection where defectionType = 'Scratches' and qrcode = '" + Global.qrCode + "' and posY='" + lastRow + "' and posX='" + lastCol + "' ) as 划伤" +
            //                ",(select count(*) from defection where defectionType = 'Corrosions' and qrcode = '" + Global.qrCode + "' and posY='" + lastRow + "' and posX='" + lastCol + "' ) as 腐蚀印" +
            //                ",(select count(*) from defection where defectionType = 'Digs' and qrcode = '" + Global.qrCode + "' and posY='" + lastRow + "' and posX='" + lastCol + "' ) as 针孔 from defection";
            //    string ab;
            //    //query = "select * from defection";
            //    DataTable dt = MySqlHelper.GetDataTable(out ab, query);
            //    int count = 0;
            //    string show = "";
            //    //MessageBox.Show(dt.Rows.Count.ToString());
            //    DataRow dr = dt.Rows[0];
            //    for (int i = 0; i < dt.Columns.Count; i++)
            //    {
            //        if (Convert.ToInt32(dr[i]) != 0)
            //        {
            //            count++;
            //            show += dt.Columns[i].ColumnName.ToString();
            //            show += ":" + dr[i].ToString() + "\r";
            //        }
            //    }
            //    if (count == 0)
            //    {
            //        toolTip1.Show("合格", panelOpticalfilter);
            //        //没有瑕疵
            //    }
            //    else
            //    {
            //        show = "共有" + count + "类瑕疵\r" + show;
            //        toolTip1.Show(show, panelOpticalfilter);
            //    }
            //}
        }

        /// <summary>
        /// 右下角对齐
        /// </summary>
        private StringFormat BottomRightSF_G = null;

        private void button2_Click(object sender, EventArgs e)
        {
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            string year = currentTime.Year.ToString();
            string month = currentTime.Month.ToString();
            string day = currentTime.Day.ToString();
            string hour = currentTime.Hour.ToString();
            string minute = currentTime.Minute.ToString();
            string second = currentTime.Second.ToString();
            Global.detectiontime = year + "-" + month + "-" + day + "-" + hour + "-" + minute; //不显示秒
            MessageBox.Show(Global.detectiontime);
        }


        /// <summary>
        /// 左上角对齐
        /// </summary>
        private StringFormat TopLeftSF_G = null;

        /// <summary>
        /// 信息提示控件
        /// </summary>
        private ToolTip ToolTip_W = new ToolTip();

        #endregion

        #region 初始化画笔、对齐方式、悬浮窗


        private void InitializeDrawResources()
        {
            //初始化画笔
            GrayBrush_G = new SolidBrush(Color.FromArgb(200, 200, 200));
            OrangeBrush_G = new SolidBrush(Color.FromArgb(190, 165, 210));
            DeepGrayBrush_G = new SolidBrush(Color.FromArgb(50, 50, 50));
            BlackBrush_G = new SolidBrush(Color.Black);
            BigFont_G = new Font("宋体", 12, FontStyle.Bold);
            SmallFont_G = new Font("宋体", 10);
            OverLayFont_G = new Font("宋体", 10, FontStyle.Bold);
            GrayPen_G = new Pen(Color.FromArgb(200, 200, 200), 1);
            BlackPen_G = new Pen(Color.FromArgb(0, 0, 0), 1);
            //对齐方式
            CenterSF_G = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap, Trimming = StringTrimming.Character };
            TopRightSF_G = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Far };
            BottomLeftSF_G = new StringFormat() { LineAlignment = StringAlignment.Far, Alignment = StringAlignment.Near };
            BottomRightSF_G = new StringFormat() { LineAlignment = StringAlignment.Far, Alignment = StringAlignment.Far };
            TopLeftSF_G = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };

            //初始化悬浮提示框
            ToolTip_W.InitialDelay = 200;
            ToolTip_W.AutoPopDelay = 500;
            ToolTip_W.ReshowDelay = 200;
            //ToolTip_W.ShowAlways = true;
            //ToolTip_W.IsBalloon = true;
            ToolTip_W.Tag = string.Empty;
        }
        #endregion

        /// <summary>
        /// 画背景框
        /// </summary>
        private void DrawBackground()
        {
            if (panelOpticalfilter.BackgroundImage != null)
            {
                panelOpticalfilter.BackgroundImage.Dispose();
            }
            GC.Collect();

            if (panelOpticalfilter.Width <= 0 || panelOpticalfilter.Height <= 0)
            {
                return;
            }
            Image backgroundBorder = new Bitmap(panelOpticalfilter.Width, panelOpticalfilter.Height);

            BlockWidth = panelOpticalfilter.Width / (Global.optLine);
            BlockHeight = panelOpticalfilter.Height / (Global.optRow);

            Graphics buffGraphics = Graphics.FromImage(backgroundBorder);
            Brush bushGray = new SolidBrush(Color.Gray);//填充的颜色
            Brush bushRed = new SolidBrush(Color.Red);//填充的颜色

            //划11条行线
            for (int i = 0; i < Global.optRow+1; i++)
            {
                buffGraphics.DrawLine(GrayPen_G, 0, i * BlockHeight, BlockWidth * (Global.optLine), i * BlockHeight);
            }

            //划12条列线
            for (int i = 0; i < Global.optLine+1; i++)
            {
                buffGraphics.DrawLine(GrayPen_G, i * BlockWidth, 0, i * BlockWidth, BlockHeight * (Global.optRow));
            }

            //画网格圆
            for (int row = 0; row < Global.optRow; row++)
            {
                for (int col = 0; col < Global.optLine; col++)
                {
                    if(Global.optshape == "圆形")
                    {
                        RectangleF rf = new RectangleF(col * BlockWidth, row * BlockHeight, BlockWidth, BlockHeight);
                        buffGraphics.FillEllipse(bushGray, rf);
                    }
                    else if(Global.optshape == "方形")
                    {
                        buffGraphics.FillRectangle(bushGray, col * BlockWidth+5, row * BlockHeight+5, BlockWidth - 10, BlockHeight - 10);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    
                }
            }
            //RectangleF rftest = new RectangleF(60, 38, BlockWidth, BlockHeight);
            //buffGraphics.FillEllipse(bushRed, rftest);

            //在虚拟盘中心绘制细黑色十字线，便于定位
            float centerX = BlockWidth * Global.optLine / 2f;
            float centerY = BlockHeight * Global.optRow / 2f;
            using (Pen crossPen = new Pen(Color.Black, 1))
            {
                buffGraphics.DrawLine(crossPen, centerX, 0, centerX, BlockHeight * Global.optRow);
                buffGraphics.DrawLine(crossPen, 0, centerY, BlockWidth * Global.optLine, centerY);
            }

            panelOpticalfilter.BackgroundImage = backgroundBorder;
            panelOpticalfilter.BackgroundImageLayout = ImageLayout.Stretch;
            Graphics_G = Graphics.FromImage(backgroundBorder);
        }

        private void DrawMargin()
        {
            Graphics_G.DrawLine(BlackPen_G, 0, 0, BlockWidth * 11 - 1, 0);//Top
            Graphics_G.DrawLine(BlackPen_G, 0, BlockHeight * 10 - 1, BlockWidth * 11 - 1, BlockHeight * 10 - 1);//Bottom
            Graphics_G.DrawLine(BlackPen_G, 0, 0, 0, BlockHeight * 10 - 1);//Left
            Graphics_G.DrawLine(BlackPen_G, BlockWidth * 11 - 1, 0, BlockWidth * 11 - 1, BlockHeight * 10);//Right
        }

        /// <summary>
        /// 画出不合格红色表示，以及画出标记点
        /// </summary>
        //private void DrawSignals(int unqualifiedX, int unqualifiedY)
        //{


        //    Graphics gra = this.panelOpticalfilter.CreateGraphics();
        //    gra.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        //    Brush bushwhite = new SolidBrush(Color.Black);//填充的颜色
        //    Brush bushred = new SolidBrush(Color.Red);//填充的颜色
        //    Brush bushgreen = new SolidBrush(Color.Green);//填充的颜色
        //    //gra.FillEllipse(bushred, BlockWidth * unqualifiedX, BlockHeight * unqualifiedY, BlockWidth, BlockHeight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
        //    for (int i = 0; i < Global.optRow; i++)
        //    {
        //        for (int j = 0; j < Global.optLine; j++)
        //        {
        //            if (optArray[i, j] == 1)
        //                gra.FillEllipse(bushred, BlockWidth * i, BlockHeight * j, BlockWidth, BlockHeight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
        //            else
        //                gra.FillEllipse(bushgreen, BlockWidth * i, BlockHeight * j, BlockWidth, BlockHeight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50

        //        }
        //    }
        //}

        public void showDefection(int value)
        {
            
            DataTable dt4 = MySqlHelper.GetDataTable(out err, "select count(*)  from defection where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and defectionType='Swabs'");
            DataTable dt5 = MySqlHelper.GetDataTable(out err, "select count(*)  from defection where qrcode = '" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and defectionType = 'Digs'");
            DataTable dt6 = MySqlHelper.GetDataTable(out err, "select count(*)  from defection where qrcode = '" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and defectionType = 'Scratches'");
            DataTable dt7 = MySqlHelper.GetDataTable(out err, "select count(*)  from defection where qrcode = '" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and defectionType = 'Prints'");
            DataTable dt8 = MySqlHelper.GetDataTable(out err, "select count(*)  from defection where qrcode = '" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and defectionType = 'Corrosions'");

        }

        //电机红色滤光片ListView显示相应照片名
        public void getPictureFileName()
        {
            //在这里面就可以进行ListView控件的构建以及数据更新等
            listView1.Clear();
            //构建表头
            listView1.Columns.Add("二维码");
            listView1.Columns.Add("时间");
            listView1.Columns.Add("照片名");

            // 设置OwnerDraw属性为true，启用自定义绘制
            listView1.OwnerDraw = true;
            // 添加DrawColumnHeader事件处理程序，用于绘制表头
            listView1.DrawColumnHeader += (sender, e) =>
            {
                e.DrawBackground(); // 绘制背景

                // 设置文本对齐方式为居中
                TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, e.ForeColor, flags);
            };
            // 添加DrawSubItem事件处理程序，用于绘制子项文本
            listView1.DrawSubItem += (sender, e) =>
            {
                e.DrawBackground(); // 绘制背景

                // 设置文本对齐方式为居中
                e.SubItem.ForeColor = listView1.ForeColor; // 使用与ListView控件相同的前景色
                TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, e.SubItem.ForeColor, flags);
            };
            listView1.Columns[0].Width = 70;
            listView1.Columns[1].Width = 100;
            listView1.Columns[2].Width = 80;

            //创建数据库连接类的对象
            MySqlConnection conn = new MySqlConnection(Global.conString);
            conn.Open();
            //执行con对象的函数，返回一个SqlCommand类型的对象
            MySqlCommand cmd = conn.CreateCommand();
            //把输入的数据拼接成sql语句，并交给cmd对象
            cmd.CommandText = "select picName from defection where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and posX='" + lastRow + "' and posY='" + lastCol + "' order by picName asc";

            //用cmd的函数执行语句，返回SqlDataReader类型的结果dr,dr就是返回的结果集（也就是数据库中查询到的表数据）
            MySqlDataReader dr = cmd.ExecuteReader();
            //用dr的read函数，每执行一次，返回一个包含下一行数据的集合dr
            string lastPicName = "";
            Global.stop = false;
            while (dr.Read())
            {
                if (lastPicName == dr["picName"].ToString())
                    continue;
                lastPicName = dr["picName"].ToString();

                //构建一个ListView的数据，存入数据库数据，以便添加到listView1的行数据中
                ListViewItem lt = new ListViewItem();
                //将数据库数据转变成ListView类型的一行数据
                lt.Text = Global.qrCode.ToString();
                lt.SubItems.Add(Global.detectiontime.ToString());
                lt.SubItems.Add(lastPicName);
                //将lt数据添加到listView1控件中
                listView1.Items.Add(lt);

                string path = "D:/" + Global.qrCode + "/" + Global.detectiontime_test + "/camera" + lastPicName[0] + "/ring/" + lastPicName + ".bmp";
                pictureshow NewForm = new pictureshow();
                NewForm.showpicture(path, lastPicName, lastRow, lastCol); //将图像显示到From2窗体上
                //NewForm.MdiParent = this;
                NewForm.ShowDialog();
                if (Global.stop == true)
                {
                    break;
                }
            }
            cmd.Dispose();
            conn.Close();
        }

        // 返回值: 1=中心区, 0=边缘区(含边界缓冲), -1=滤光片外
        private int GetDefectRegion(double trayX, double trayY, int rowIndex, int colIndex, double circleCenterRatio, double squareCenterRatio)
        {
            const double boundaryBandMm = 0.2;

            if (Global.optshape == "圆形")
            {
                double x = Math.Abs(350 - ((Global.diameter / 2 + 0.1) + ((350 - Global.diameter - 0.2) / (Global.optLine - 1)) * colIndex));
                double y = Math.Abs(340 - ((Global.diameter / 2 + 0.1) + ((340 - Global.diameter - 0.2) / (Global.optRow - 1)) * rowIndex));
                double distance = Math.Sqrt((trayX - x) * (trayX - x) + (trayY - y) * (trayY - y));

                double radius = Global.diameter / 2.0;
                double centerRadius = radius * circleCenterRatio;

                if (distance <= centerRadius)
                {
                    return 1;
                }
                if (distance <= radius + boundaryBandMm)
                {
                    return 0;
                }
                return -1;
            }

            if (Global.optshape == "方形")
            {
                double x = Math.Abs(350 - ((Global.length / 2 + 0.1) + ((350 - Global.length - 0.2) / (Global.optLine - 1)) * colIndex));
                double y = Math.Abs(340 - ((Global.width / 2 + 0.1) + ((340 - Global.width - 0.2) / (Global.optRow - 1)) * rowIndex));
                double distanceX = Math.Abs(trayX - x);
                double distanceY = Math.Abs(trayY - y);

                double halfLength = Global.length / 2.0;
                double halfWidth = Global.width / 2.0;

                if (distanceX <= halfLength * squareCenterRatio && distanceY <= halfWidth * squareCenterRatio)
                {
                    return 1;
                }
                if (distanceX <= halfLength + boundaryBandMm && distanceY <= halfWidth + boundaryBandMm)
                {
                    return 0;
                }
                return -1;
            }

            return -1;
        }

        //分级
        public void divideScale()
        {
            // 防止同一批次重复分级时生成重复记录
            MySqlHelper.UpdateData(out err, "delete from filterlevel where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "'");

            //定义中间划痕长度
            double centreScchLength = 0;
            //定义边缘划痕长度
            double edgeScchLength = 0;
            //定义中间针孔个数
            double centrePhLength = 0;
            //定义边缘针孔个数
            double edgePhLength = 0;

            bool isNOOK = false;

            string[] NOKArray = new string[] { "膜内布毛", "腐蚀印", "手印"}; // 直接导致滤光片不合格的瑕疵（添加手印）

            for (int i = 0; i < Global.optRow; i++)
            {
                for (int j = 0; j < Global.optLine; j++)
                {
                    //定义中间划痕长度=0
                    centreScchLength = 0;
                    //定义边缘划痕长度=0
                    edgeScchLength = 0;
                    //定义中间针孔个数=0
                    centrePhLength = 0;
                    //定义边缘针孔个数=0
                    edgePhLength = 0;

                    isNOOK = false;

                    //从数据库获取当前盘的第i行第j列滤光片的瑕疵、总数
                    string order = "select trayX,trayY,defectionType,scratchLength,pinholeRadius from defection where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and posX='" + i + "' and posY='" + j + "'";
                    DataTable dt_now = MySqlHelper.GetDataTable(out err, order);

                    // 空值检查
                    if (dt_now == null || dt_now.Rows == null)
                    {
                        continue; // 跳过这个滤光片
                    }

                    int DefectsNum = dt_now.Rows.Count;

                    if (DefectsNum == 0)//瑕疵总数=0
                    {
                        //数据库等级表设为3
                        string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 3 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                        MySqlHelper.UpdateData(out err, setLevelOrder);
                        continue;  //跳出当前滤光片,继续下一个滤光片
                    }

                    for (int k = 0; k < DefectsNum; k++)//int k = 0; k < 瑕疵个数; i++
                    {
                        if (NOKArray.Contains(Convert.ToString(dt_now.Rows[k][2])))
                        {
                            //数据库等级表设为6，不合格
                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 6 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                            MySqlHelper.UpdateData(out err, setLevelOrder);
                            isNOOK = true;
                            break;  //跳出当前滤光片的瑕疵循环
                        }
                        else if (Convert.ToString(dt_now.Rows[k][2]) == "划痕")//该瑕疵 == 划痕
                        {
                            int scratchRegion = GetDefectRegion(
                                Convert.ToDouble(dt_now.Rows[k][0]),
                                Convert.ToDouble(dt_now.Rows[k][1]),
                                i,
                                j,
                                0.7,
                                0.9);

                            if (scratchRegion == 1)
                            {
                                //中间划痕长度++
                                centreScchLength += Convert.ToDouble(dt_now.Rows[k][3]);
                            }
                            else if (scratchRegion == 0)
                            {
                                //边缘长度++
                                edgeScchLength += Convert.ToDouble(dt_now.Rows[k][3]);
                            }

                        }
                        else if (Convert.ToString(dt_now.Rows[k][2]) == "点子" || Convert.ToString(dt_now.Rows[k][2]) == "内点子")//该瑕疵 == 点子/内点子
                        {
                            if (Convert.ToDouble(dt_now.Rows[k][4]) > 0.45)//针孔直径>0.2mm
                            {
                                //数据库等级表设为6，不合格
                                string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 6 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                MySqlHelper.UpdateData(out err, setLevelOrder);
                                isNOOK = true;
                                break;  //跳出当前滤光片的瑕疵循环
                            }

                            if (Convert.ToDouble(dt_now.Rows[k][4]) > 0.10)
                            {
                                int spotRegion = GetDefectRegion(
                                    Convert.ToDouble(dt_now.Rows[k][0]),
                                    Convert.ToDouble(dt_now.Rows[k][1]),
                                    i,
                                    j,
                                    0.9,
                                    0.9);

                                if (spotRegion == 1)
                                {
                                    //中间个数++
                                    centrePhLength++;
                                }
                                else if (spotRegion == 0)
                                {
                                    //边缘个数++
                                    edgePhLength++;
                                }
                            }
                        }
                    }

                    if (isNOOK != true)
                    {
                        if (Global.standard == "标准1")
                        {
                            if (Global.dsface == "上面" || Global.dsface == "下面")
                            {
                                switch (Global.optshape)
                                {
                                    case "方形":
                                        if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 2.5 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 2.5, "划痕") > 15 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 1.5, 2.5, "划痕") > 5
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.4 || (MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.015, 0.1, "内点子") * 0.1 + MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.4, "内点子")) > (Global.length + Global.width) / 2
                                             || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.2, 0.4, "内点子") > ((Global.length + Global.width) / 2) * 0.1 || (centrePhLength + edgePhLength) > 30 || centrePhLength > 10 || edgePhLength > 20 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.2, 0.4, "点子") > ((Global.length + Global.width) / 2) * 0.1)//总长度 = 中间长度 + 边缘长度 > 0.8mm
                                        {
                                            //数据库等级表设为6，不合格
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 6 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 1.5 || centreScchLength > 0 || edgeScchLength >= 2.5
                                             || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "内点子") > (Global.length + Global.width) / 2 * 0.1
                                             || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "点子") > (Global.length + Global.width) / 2 * 0.1 || centrePhLength > 2 || edgePhLength > 6)//中间长度>0，5级
                                        {
                                            //数据库等级表设为5
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 5 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (edgeScchLength > 0 || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "内点子") > (Global.length + Global.width) / 2 * 0.1
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "点子") > (Global.length + Global.width) / 2 * 0.1 || centrePhLength > 0 || edgePhLength > 3)//边缘长度>0,4级
                                        {
                                            //数据库等级表设为4
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 4 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else
                                        {
                                            //数据库等级表设为3
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 3 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                    case "圆形":
                                        if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 2.5 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 2.5, "划痕") > 15 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 1.5, 2.5, "划痕") > 5
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.4 || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.4 || (MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.015, 0.1, "内点子") * 0.1 + MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.4, "内点子")) > Global.diameter
                                            || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.2, 0.4, "内点子") > Global.diameter * 0.1 || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.4 || (centrePhLength + edgePhLength) > 30 || centrePhLength > 10 || edgePhLength > 20)//总长度 = 中间长度 + 边缘长度 > 0.8mm
                                        {
                                            //数据库等级表设为6，不合格
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 6 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 1.5 || centreScchLength > 0 || edgeScchLength >= 2.5
                                             || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "内点子") > Global.diameter * 0.1
                                             || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "点子") > Global.diameter * 0.1 || centrePhLength > 2 || edgePhLength > 6)//中间长度>0，5级
                                        {
                                            //数据库等级表设为5
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 5 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (edgeScchLength > 0 || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "内点子") > Global.diameter * 0.1
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "点子") > Global.diameter * 0.1)//边缘长度>0,4级
                                        {
                                            //数据库等级表设为4
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 4 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else
                                        {
                                            //数据库等级表设为3
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 3 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                }
                            }
                            else if (Global.dsface == "双面")
                                switch (Global.optshape)
                                {
                                    case "方形":
                                        if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 2.5 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 2.5, "划痕") > 15 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 1.5, 2.5, "划痕") > 5
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.4 || (MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.015, 0.1, "内点子") * 0.1 + MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.4, "内点子")) > (Global.length + Global.width)
                                             || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.2, 0.4, "内点子") > ((Global.length + Global.width) / 2) * 0.2 || (centrePhLength + edgePhLength) > 60 || centrePhLength > 20 || edgePhLength > 40 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.2, 0.4, "点子") > ((Global.length + Global.width) / 2) * 0.2)//总长度 = 中间长度 + 边缘长度 > 0.8mm
                                        {
                                            //double a = MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕");
                                            //double b = MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 2.5, "划痕");
                                            //double c = MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 1.5, 2.5, "划痕");
                                            //double d = MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子");
                                            //double e = MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.015, 0.1, "内点子");
                                            //double f = MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.4, "内点子");
                                            //double g = MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.2, 0.4, "内点子");
                                            //double h = (centrePhLength + edgePhLength);
                                            //double k = centrePhLength;
                                            //double l = edgePhLength;
                                            //double m = MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.2, 0.4, "点子");



                                            //数据库等级表设为6，不合格
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 6 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 1.5 || centreScchLength > 0 || edgeScchLength >= 2.5
                                             || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "内点子") > (Global.length + Global.width) / 2 * 0.2
                                             || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "点子") > (Global.length + Global.width) / 2 * 0.2 || centrePhLength > 4 || edgePhLength > 12)//中间长度>0，5级
                                        {
                                            //数据库等级表设为5
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 5 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (edgeScchLength > 0 || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "内点子") > (Global.length + Global.width) / 2 * 0.2
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "点子") > (Global.length + Global.width) / 2 * 0.2 || centrePhLength > 0 || edgePhLength > 6)//边缘长度>0,4级
                                        {
                                            //数据库等级表设为4
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 4 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else
                                        {
                                            //数据库等级表设为3
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 3 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                    case "圆形":
                                        if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 2.5 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 2.5, "划痕") > 15 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 1.5, 2.5, "划痕") > 5
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.4 || (MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.015, 0.1, "内点子") * 0.1 + MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.4, "内点子")) > Global.diameter * 2
                                            || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.2, 0.4, "内点子") > Global.diameter * 0.2 || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.4 || (centrePhLength + edgePhLength) > 60 || centrePhLength > 20 || edgePhLength > 40)//总长度 = 中间长度 + 边缘长度 > 0.8mm
                                        {
                                            //数据库等级表设为6，不合格
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 6 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 1.5 || centreScchLength > 0 || edgeScchLength >= 2.5
                                             || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "内点子") > Global.diameter * 0.2
                                             || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "点子") > Global.diameter * 0.2 || centrePhLength > 4 || edgePhLength > 12)//中间长度>0，5级
                                        {
                                            //数据库等级表设为5
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 5 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (edgeScchLength > 0 || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "内点子") > Global.diameter * 0.2
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "点子") > Global.diameter * 0.2)//边缘长度>0,4级
                                        {
                                            //数据库等级表设为4
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 4 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else
                                        {
                                            //数据库等级表设为3
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 3 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                }
                        }
                        else if (Global.standard == "标准2")
                        {
                            if (Global.dsface == "上面" || Global.dsface == "下面")
                            {
                                switch (Global.optshape)
                                {
                                    case "方形":
                                        if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 2.5 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 2.5, "划痕") > 15 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 1.5, 2.5, "划痕") > 5
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.4 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.015, 0.4, "内点子") > (Global.length + Global.width) / 2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.2, 0.4, "内点子") > (Global.length + Global.width) / 2 * 0.1
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.4 || (centrePhLength + edgePhLength) > 30 || centrePhLength > 10 || edgePhLength > 20)//总长度 = 中间长度 + 边缘长度 > 0.8mm
                                        {
                                            //数据库等级表设为6，不合格
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 6 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 1.5 || centreScchLength > 0 || edgeScchLength >= 2.5
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "内点子") > (Global.length + Global.width) / 2 * 0.1
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "点子") > (Global.length + Global.width) / 2 * 0.1 || centrePhLength > 2 || edgePhLength > 6)//中间长度>0，5级
                                        {
                                            //数据库等级表设为5
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 5 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (edgeScchLength > 0 || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "内点子") > (Global.length + Global.width) / 2 * 0.1
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "点子") > (Global.length + Global.width) / 2 * 0.1 || centrePhLength > 0 || edgePhLength > 3)//边缘长度>0,4级
                                        {
                                            //数据库等级表设为4
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 4 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else
                                        {
                                            //数据库等级表设为3
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 3 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                    case "圆形":
                                        if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 2.5 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 2.5, "划痕") > 15 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 1.5, 2.5, "划痕") > 5
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.4 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.015, 0.4, "内点子") > Global.diameter || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.2, 0.4, "内点子") > Global.diameter * 0.1
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.4 || (centrePhLength + edgePhLength) > 30 || centrePhLength > 10 || edgePhLength > 20)//总长度 = 中间长度 + 边缘长度 > 0.8mm
                                        {
                                            //数据库等级表设为6，不合格
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 6 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 1.5 || centreScchLength > 0 || edgeScchLength >= 2.5
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "内点子") > Global.diameter * 0.1
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "点子") > Global.diameter * 0.1 || centrePhLength > 2 || edgePhLength > 6)//中间长度>0，5级
                                        {
                                            //数据库等级表设为5
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 5 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (edgeScchLength > 0 || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "内点子") > Global.diameter * 0.1
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "点子") > Global.diameter * 0.1)//边缘长度>0,4级
                                        {
                                            //数据库等级表设为4
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 4 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else
                                        {
                                            //数据库等级表设为3
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 3 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                }
                            }
                            else if (Global.dsface == "双面")
                                switch (Global.optshape)
                                {
                                    case "方形":
                                        if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 2.5 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 2.5, "划痕") > 15 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 1.5, 2.5, "划痕") > 5
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.4 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.015, 0.4, "内点子") > (Global.length + Global.width) || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.2, 0.4, "内点子") > (Global.length + Global.width) / 2 * 0.2
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.4 || (centrePhLength + edgePhLength) > 60 || centrePhLength > 20 || edgePhLength > 40)//总长度 = 中间长度 + 边缘长度 > 0.8mm
                                        {
                                            //数据库等级表设为6，不合格
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 6 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 1.5 || centreScchLength > 0 || edgeScchLength >= 2.5
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "内点子") > (Global.length + Global.width) / 2 * 0.2
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "点子") > (Global.length + Global.width) / 2 * 0.2 || centrePhLength > 4 || edgePhLength > 12)//中间长度>0，5级
                                        {
                                            //数据库等级表设为5
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 5 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (edgeScchLength > 0 || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "内点子") > (Global.length + Global.width) / 2 * 0.2
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "点子") > (Global.length + Global.width) / 2 * 0.2 || centrePhLength > 0 || edgePhLength > 6)//边缘长度>0,4级
                                        {
                                            //数据库等级表设为4
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 4 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else
                                        {
                                            //数据库等级表设为3
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 3 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                    case "圆形":
                                        if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 2.5 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 2.5, "划痕") > 15 || MySqlHelper.SumMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 1.5, 2.5, "划痕") > 5
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.4 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.015, 0.4, "内点子") > Global.diameter * 2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.2, 0.4, "内点子") > Global.diameter * 0.2
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.4 || (centrePhLength + edgePhLength) > 60 || centrePhLength > 20 || edgePhLength > 40)//总长度 = 中间长度 + 边缘长度 > 0.8mm
                                        {
                                            //数据库等级表设为6，不合格
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 6 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "scratchLength", "划痕") > 1.5 || centreScchLength > 0 || edgeScchLength >= 2.5
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "内点子") > Global.diameter * 0.2
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.2 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.1, 0.2, "点子") > Global.diameter * 0.2 || centrePhLength > 4 || edgePhLength > 12)//中间长度>0，5级
                                        {
                                            //数据库等级表设为5
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 5 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else if (edgeScchLength > 0 || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "内点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "内点子") > Global.diameter * 0.2
                                            || MySqlHelper.GetMaxValueMatchingCriteria(dt_now, "defectionType", "pinholeRadius", "点子") > 0.1 || MySqlHelper.CountMatchingCriteria(dt_now, "defectionType", "pinholeRadius", 0.05, 0.1, "点子") > Global.diameter * 0.2)//边缘长度>0,4级
                                        {
                                            //数据库等级表设为4
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 4 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                        else
                                        {
                                            //数据库等级表设为3
                                            string setLevelOrder = "insert into filterlevel(detectiontime,qrcode,posX,posY,level) value('" + Global.detectiontime + "','" + Global.qrCode + "','" + i + "','" + j + "','" + 3 + "')";                                                                                                                                                                                                               //cmd2.ExecuteNonQuery();
                                            MySqlHelper.UpdateData(out err, setLevelOrder);
                                            continue;  //跳出当前滤光片的瑕疵循环
                                        }
                                }
                        }
                    }
                }
            }
        }
    }

}

