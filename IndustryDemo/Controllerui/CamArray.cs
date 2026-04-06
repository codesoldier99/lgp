using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using IndustryDemo;
using System.Windows.Forms;

namespace IndustryDemo.Controllerui
{
    public class CamArray
    {
        public struct ImageInfo 
        {
            public int cameraId;
            public string LightSource;
            public int row;
            public int column;
            public int distance;
        }

        static int[] relatedX = new int[] { 14, 44, 0, 29, 14, 44, 0, 29 };
        int[] locate = new int[2];
        //创建一个byte集合
        int cameraID;
        List<byte[]> list = new List<byte[]>();
        List<ImageInfo> Location = new List<ImageInfo>();
        DetectionUnderSpotLight SpotLight;
        DetectionUnderRingLight RingLight;
        //DataBaseOperate dbo;  // 未使用的变量，已注释
        public bool rotate;
        List<float[]> basedistance = new List<float[]>();
        
        //List<IntPtr> m_BufForDrivers = new List<IntPtr>();
        int forwardStart, forwardEnd, backwardStart, backwardEnd;
        int forwardFirst, backwardFirst;
        int forward, backward;
        int AddX;
        public CamArray(int cameraID, int forward, int backward)
        {
            this.cameraID = cameraID;
            SpotLight = new DetectionUnderSpotLight();
            RingLight = new DetectionUnderRingLight();
            //dbo = new DataBaseOperate();    //显示照片
            this.forwardStart = forward - 360;
            this.forwardEnd = forward + 1032 * 10 + 360;
            this.backwardStart = backward + 360;
            this.backwardEnd = backward - 1032 * 10 - 360;
            this.forward = forward;
            this.backward = backward;
            AddX = relatedX[cameraID - 1];
        }
        //添加单个字节
        public void Add(byte[] item, string lightSource, int cameraId ,int column, int row, int distance)     //i,j
        {
            if (rotate)
            {
                Array.Reverse(item);
            }
            list.Add(item);
            ImageInfo imageInfo = new ImageInfo();
            imageInfo.LightSource = lightSource;
            imageInfo.cameraId = cameraId;
            imageInfo.column = column;
            imageInfo.row = row;
            imageInfo.distance = distance;
            Location.Add(imageInfo);
            //m_BufForDrivers.Add(m_BufForDriver);
        }

        public void Detection(string dir, int[] defectsNumber, List<int[]> defects,
            List<int[]> defectsLocation, List<float[]> defectsRelativeLocation)
        {
            ImageInfo imageInfo = new ImageInfo();
            byte[] stSaveFileParam = Pop(ref imageInfo);
            int row = 0, column = 0;
            float fHeight = 0, fWidth = 0;
            if (stSaveFileParam is null)
            {
                return;
            }
            //column = (int)((AddX + imageInfo.row * 32 - 1.5) / 32);
            //fWidth = (column * 32 + 1.5f - imageInfo.row * 32 - AddX) / 32f;
            switch (imageInfo.column % 2)
            {
                case 0:

                    if (imageInfo.cameraId == 3 && (imageInfo.row < 6 || imageInfo.row > 34))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 1 && (imageInfo.row < 12 || imageInfo.row > 40))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 4 && (imageInfo.row < 3 || imageInfo.row > 31))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 2 && (imageInfo.row < 8 || imageInfo.row > 37))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 5 && (imageInfo.row < 8 || imageInfo.row > 36))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 7 && (imageInfo.row < 3 || imageInfo.row > 31))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 6 && (imageInfo.row < 12 || imageInfo.row > 40))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 8 && (imageInfo.row < 6 || imageInfo.row > 34))
                    {
                        return;
                    }
                    //row = (imageInfo.distance - forward) / 1032;
                    //fHeight = (imageInfo.distance - row * 1032 - forward) / 1032;
                    break;
                case 1:
                    if (imageInfo.cameraId == 3 && (imageInfo.row < 8 || imageInfo.row > 36))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 1 && (imageInfo.row < 3 || imageInfo.row > 31))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 4 && (imageInfo.row < 12 || imageInfo.row > 40))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 2 && (imageInfo.row < 6 || imageInfo.row > 34))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 5 && (imageInfo.row < 6 || imageInfo.row > 34))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 7 && (imageInfo.row < 12 || imageInfo.row > 40))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 6 && (imageInfo.row < 3 || imageInfo.row > 31))
                    {
                        return;
                    }
                    else if (imageInfo.cameraId == 8 && (imageInfo.row < 8 || imageInfo.row > 36))
                    {
                        return;
                    }
                    //row = (imageInfo.distance - (backward - 10 * 1032)) / 1032;
                    //fHeight = (imageInfo.distance - (backward - 10 * 1032) - 1032 * row) / 1032;
                    break;
            }

            HObject hobj = ByteToHobject(stSaveFileParam);
            
            //在图片中心绘制细黑色十字线
            HTuple imageWidth, imageHeight;
            HOperatorSet.GetImageSize(hobj, out imageWidth, out imageHeight);
            double centerX = imageWidth.D / 2.0;
            double centerY = imageHeight.D / 2.0;
            
            //创建十字线区域并叠加到图像上
            HObject crossRegion;
            HOperatorSet.GenRegionLine(out crossRegion, 0, centerX, imageHeight - 1, centerX);  //垂直线
            HObject crossRegion2;
            HOperatorSet.GenRegionLine(out crossRegion2, centerY, 0, centerY, imageWidth - 1);  //水平线
            HObject crossLines;
            HOperatorSet.Union2(crossRegion, crossRegion2, out crossLines);
            
            //将十字线绘制到图像上（用黑色0值）
            HOperatorSet.PaintRegion(crossLines, hobj, out hobj, 0, "fill");
            
            crossRegion.Dispose();
            crossRegion2.Dispose();
            crossLines.Dispose();
            
            string dir0 = imageInfo.LightSource + "/" + imageInfo.cameraId.ToString() + "-" + imageInfo.column.ToString() + "-" + imageInfo.row.ToString();
            
            HOperatorSet.WriteImage(hobj, "bmp", 0,dir+'/'+ dir0 + ".bmp");

            //路径  d:\qrcode\ring\  .bmp
            hobj.Dispose();
        }

        private HObject ByteToHobject(byte[] img)
        {
            HObject Hobj;
            unsafe
            {
                fixed (byte* pData = img)
                {
                    IntPtr ptr = new IntPtr(pData);
                    HOperatorSet.GenImage1(out Hobj, "byte", 2592, img.Length / 2592, ptr);
                }
            }

            return Hobj;
        }

        public byte[] Pop(ref ImageInfo imageLoc)
        {
            if (Length > 0) 
            {
                byte[] stSaveFileParam = list[0];
                list.RemoveAt(0);
                imageLoc = Location[0];
                Location.RemoveAt(0);
                //GC.Collect();
                //m_Buf = m_BufForDrivers[0];
                //m_BufForDrivers.RemoveAt(0);
                //list.TrimExcess();
                //Location.TrimExcess();
                return stSaveFileParam; 
            }
            else
            {
                //m_BufForDriver = IntPtr.Zero;
                return null;
            }
            
        }

        public void Release()
        {
            list.Clear();
            list = null;
            GC.Collect();
            list = new List<byte[]>();
        }


        //获取数组长度
        public int Length
        {
            get {
                    int a1 = list.Count();
                    int a2 = Location.Count();
                    //int a3 = m_BufForDrivers.Count;
                    a1 = (a1 <= a2  ? a1 : a2);
                    return a1; 
                }
        }
    }
}
