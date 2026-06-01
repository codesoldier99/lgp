using MvCamCtrl.NET;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace IndustryDemo.Controllerui
{
    public class Camera
    {
        #region 变量
        IntPtr m_BufForDriver;
        UInt32 m_nBufSizeForDriver = 4096 * 3000;
        byte[] m_pBufForDriver = new byte[4096 * 3000];
        string dir;
        int cameraID;
        public CamArray cameraData;
        private MyCamera.MVCC_INTVALUE stParam;  //用于接收特定的参数
        public bool rotate = false;
        public MyCamera m_MyCamera;
        int pictureEditWidth;
        int pictureEditHeight;
        public bool isDetection;
        Thread detectionThread;
        HWindow m_Window;
        IntPtr deviceInfo;
        IntPtr m_hDisplayHandle;
        private MyCamera.MV_CC_DEVICE_INFO device = new MyCamera.MV_CC_DEVICE_INFO();
        HObject Hobj = new HObject(); //halcon图像格式
        IntPtr pTemp = IntPtr.Zero;
        MyCamera.MV_DISPLAY_FRAME_INFO stDisplayInfo;
        public bool isOpened;  //相机是否打开
        MyCamera.MV_CC_ROTATE_IMAGE_PARAM RotateParam;

        #endregion

        #region 构造函数
        public Camera(IntPtr DisplayHandle, IntPtr deviceInfo, string dir, int pictureWidth, int pictureHeight, int forward, int backward)  //pictureEdit控件的IntPtr，相机的IntPtr，图像存放路径
        {
            this.m_hDisplayHandle = DisplayHandle;
            m_MyCamera = new MyCamera();
            m_Window = new HWindow();
            //DisplayWindowsInitial();
            this.deviceInfo = deviceInfo;
            this.dir = "G://" + Global.qrCode + "/" + Global.detectiontime +"/"+dir;
            this.cameraID = Convert.ToInt32(dir[dir.Length - 1].ToString());
            this.pictureEditWidth = pictureWidth;
            this.pictureEditHeight = pictureHeight;
            DisplayWindowsInitial();
            
            stDisplayInfo = new MyCamera.MV_DISPLAY_FRAME_INFO();
            cameraData = new CamArray(cameraID, forward, backward);
            if (!Directory.Exists(this.dir))
            {
                Directory.CreateDirectory(this.dir);
                Directory.CreateDirectory(this.dir + "/spot");
                Directory.CreateDirectory(this.dir + "/ring");
            }
        }
        #endregion

        #region 初始化图像参数
        private void DisplayWindowsInitial()
        {
            // ch: 定义显示的起点和宽高 || en: Definition the width and height of the display window
            HTuple hWindowRow, hWindowColumn, hWindowWidth, hWindowHeight;

            // ch: 设置显示窗口的起点和宽高 || en: Set the width and height of the display window
            hWindowRow = 0;
            hWindowColumn = 0;
            hWindowWidth = pictureEditWidth;
            hWindowHeight = pictureEditHeight;

            try
            {
                HTuple hWindowID = (HTuple)m_hDisplayHandle;
                m_Window.OpenWindow(hWindowRow, hWindowColumn, hWindowWidth, hWindowHeight, hWindowID, "visible", "");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
        }

        public void DisplayWindowChanging(int pictureWidth, int pictureHeight)
        {
            m_Window.CloseWindow();
            // ch: 定义显示的起点和宽高 || en: Definition the width and height of the display window
            HTuple hWindowRow, hWindowColumn, hWindowWidth, hWindowHeight;

            // ch: 设置显示窗口的起点和宽高 || en: Set the width and height of the display window
            hWindowRow = 0;
            hWindowColumn = 0;
            hWindowWidth = pictureWidth;
            hWindowHeight = pictureHeight;

            try
            {
                HTuple hWindowID = (HTuple)m_hDisplayHandle;
                m_Window.OpenWindow(hWindowRow, hWindowColumn, hWindowWidth, hWindowHeight, hWindowID, "visible", "");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
        }
        #endregion

        #region 打开相机
        public void Open_click()  // 打开设备
        {
            device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(deviceInfo,
                                                        typeof(MyCamera.MV_CC_DEVICE_INFO));
            int nRet = m_MyCamera.MV_CC_CreateDevice_NET(ref device);
            nRet = m_MyCamera.MV_CC_OpenDevice_NET();
            isOpened = true;
            m_MyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
            m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);  //设置相机为触发模式
            m_MyCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0);  //触发源为线路0
            nRet = m_MyCamera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                m_MyCamera.MV_CC_DestroyDevice_NET();
                return;
            }
            if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                int nPacketSize = m_MyCamera.MV_CC_GetOptimalPacketSize_NET();
                if (nPacketSize > 0)
                {
                    nRet = m_MyCamera.MV_CC_GetOptimalPacketSize_NET();
                    if (nRet != MyCamera.MV_OK)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            
        }
        #endregion

        #region 开始采集图像
        public void Start_Grabbing()
        {
            int nRet;

            // ch:开启抓图 | en:start grab
            nRet = m_MyCamera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Start Grabbing Fail");
                return;
            }
            m_BufForDriver = Marshal.AllocHGlobal((Int32)m_nBufSizeForDriver);
            if (rotate)
            {
                m_MyCamera.MV_CC_RotateImage_NET(ref RotateParam);
            }
        }
        #endregion

        #region 设置显示图像的宽和高
        public void SetHWindowRegion(int row, int column)
        {
            //m_Window.SetPart(0, 0, row - 1, column - 1);
            //m_Window.Set
            //m_Window.SetPart(0, 0, 60000 , 60000);
        }
        #endregion

        #region 显示图像画面
        public void HalconDisplay(HTuple hWindow, HObject Hobj, HTuple hHeight, HTuple hWidth)
        {
            // ch: 显示 || display
            try
            {
                HOperatorSet.SetPart(hWindow, 0, 0, hHeight - 1, hWidth - 1);// ch: 使图像显示适应窗口大小 || en: Make the image adapt the window size
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            if (hWindow == null)
            {
                return;
            }
            try
            {
                HOperatorSet.DispObj(Hobj, hWindow);// ch 显示 || en: display
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            return;
        }
        #endregion

        #region 保存当前图像在cameraData中
        public void ReceiveImageWorkThread(int cameraID, int cnt, int cnt2, string type, int distance)
        {
            MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
            int nRet = m_MyCamera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);    //返回给调用者有关相机属性结构体指针
            MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo;
            stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
            uint nPayloadSize = stParam.nCurValue;
            m_nBufSizeForDriver = nPayloadSize;
            m_BufForDriver = Marshal.AllocHGlobal((Int32)m_nBufSizeForDriver);  //通过使用指定的字节数，从进程的非托管内存中分配内存。返回结果:指向新分配的内存的指针。
            nRet = m_MyCamera.MV_CC_GetOneFrameTimeout_NET(m_BufForDriver, nPayloadSize, ref stFrameInfo, 50);//采用超时机制获取一帧图片（图像数据接受指针，接受缓存大小，图像信息结构体，等待超时时间）
            //MessageBox.Show("nRet的值："+nRet.ToString());
            /*while (MyCamera.MV_OK != nRet)//等待读取一幅图片，解决漏掉问题
            {
                nRet = m_MyCamera.MV_CC_GetOneFrameTimeout_NET(m_BufForDriver, nPayloadSize, ref stFrameInfo, 500);//采用超时机制获取一帧图片（图像数据接受指针，接受缓存大小，图像信息结构体，等待超时时间）
            }*/
            if (MyCamera.MV_OK == nRet)
            {
                pTemp = m_BufForDriver;
                HOperatorSet.GenImage1Extern(out Hobj, "byte", stFrameInfo.nWidth, stFrameInfo.nHeight, pTemp, IntPtr.Zero);    //     Create an image from a pointer on the pixels with storage management.创建Halcon对象的图像
                if (rotate)
                {
                    HOperatorSet.RotateImage(Hobj, out Hobj, 180, "constant");  //旋转图像
                    //m_MyCamera.MV_CC_RotateImage_NET(ref RotateParam);
                }
                byte[] b = new byte[nPayloadSize];
                Marshal.Copy(pTemp, b, 0, (int)nPayloadSize);       //将数据从非托管内存指针复制到托管 8 位无符号整数数组。(source:从中进行复制的内存指针。destination:要复制到的数组。startIndex:目标数组中从零开始的索引，在此处开始复制。length:要复制的数组元素的数目。
                //HObject obj = new HObject();
                //HOperatorSet.GenImage1Extern(out obj, "byte", stFrameInfo.nWidth, stFrameInfo.nHeight, b, 0);
                HalconDisplay(m_Window, Hobj, stFrameInfo.nHeight, stFrameInfo.nWidth);
                Marshal.FreeHGlobal(pTemp);     //释放以前从进程的非托管内存中分配的内存。
                cameraData.Add(b, type, cameraID, cnt, cnt2, distance);
                //return true;
            }
            //else
            //{
            //    return false;
            //}

        }
        #endregion

        #region 关闭设备
        public void Close()  //关闭设备
        {
            if (m_BufForDriver != IntPtr.Zero)
            {
                Marshal.Release(m_BufForDriver);
            }
            m_MyCamera.MV_CC_CloseDevice_NET();
            m_MyCamera.MV_CC_DestroyDevice_NET();
            m_Window.ClearWindow();
            m_Window.CloseWindow();
            isOpened = false;
        }
        #endregion

        #region 停止图像采集
        public void StopGrabPicture()
        {
            // ch:标志位设为false | en:Set flag bit false

            // ch:停止采集 | en:Stop Grabbing
            int nRet = m_MyCamera.MV_CC_StopGrabbing_NET();
        }
        #endregion

        #region 设置相机曝光时间、增益、帧率
        public void SetValue(float exposure, float gain, float frameRate)
        {
            m_MyCamera.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
            int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("ExposureTime", exposure);
            if (nRet != MyCamera.MV_OK)
            {
                ShowErrorMsg("Set Exposure Time Fail!", nRet);
            }

            m_MyCamera.MV_CC_SetEnumValue_NET("GainAuto", 0);
            nRet = m_MyCamera.MV_CC_SetFloatValue_NET("Gain", gain);
            if (nRet != MyCamera.MV_OK)
            {
                ShowErrorMsg("Set Gain Fail!", nRet);
            }

            m_MyCamera.MV_CC_SetEnumValue_NET("AcquisitionFrameRateAuto", 0);
            nRet = m_MyCamera.MV_CC_SetFloatValue_NET("AcquisitionFrameRate", frameRate);
            if (nRet != MyCamera.MV_OK)
            {
                ShowErrorMsg("Set Frame Rate Fail!", nRet);
            }
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

        #region 从cameraData中获取图像并保存在硬盘中
        public void ToDisk(int[] defectType, List<int[]> defects,
            List<int[]> defectsLocation, List<float[]> defectsRelativeLocation)
        {
            isDetection = true;
            detectionThread = new Thread(() => StartDetect(defectType, defects, defectsLocation, defectsRelativeLocation));
            detectionThread.Start();
             
        }

        void StartDetect(int[] defectType, List<int[]> defects,
            List<int[]> defectsLocation, List<float[]> defectsRelativeLocation)
        {
            while (isDetection)
            {
                Thread.Sleep(1);
                cameraData.Detection(dir, defectType, defects, defectsLocation, defectsRelativeLocation);
            }

        }

        public void DetectFinished()
        {
            isDetection = false;
            detectionThread.Join();
        }
        #endregion
    }
}
