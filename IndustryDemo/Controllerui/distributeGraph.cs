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
using System.Drawing.Drawing2D;
using DevExpress.Printing.Core.PdfExport.Metafile;
using System.Threading;

namespace IndustryDemo.Controllerui
{
    public partial class distributeGraph : DevExpress.XtraEditors.XtraUserControl
    {
        cameraDevices cameraLoc;
        Graphics dc;
        Random r1 = new Random();
        Random r2 = new Random();
        Pen area;
        List<int[]> defectsLocation = new List<int[]>();
        List<float[]> defectsRelativeLocation = new List<float[]>();
        public bool defectsShow;
        int widthStep, heightStep;
        List<bubble> bubbles = new List<bubble>();
        nap[] naps;
        pinhole[] pinholes;
        List<scratch> scratches = new List<scratch>();
        spot[] spots;
        public distributeGraph()
        {
            dc = CreateGraphics();
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            // rectangles = new Rectangle[5, 5];
        }
        
        public void DrawRectangle()
        {
            dc = CreateGraphics();
            Color areaColor = Color.Aqua;
            area = new Pen(areaColor, 2);
            area.DashStyle = DashStyle.Dash;
            area.LineJoin = LineJoin.Miter;
            Point p1, p2;
            int width = this.Size.Width;
            int height = this.Size.Height;
            widthStep = (width - 50) / 11;
            heightStep = (height - 50) / 10;
            for(int i = 0; i <= 11; ++i)
            {
                p1 = new Point { X = 25 + i * widthStep, Y = 25 };
                p2 = new Point { X = 25 + i * widthStep, Y = 25 + heightStep * 10 };
                dc.DrawLine(area, p1, p2);
            }
            for (int i = 0; i <= 10; ++i)
            {
                p1 = new Point { X = 25, Y = 25 + i * heightStep };
                p2 = new Point { X = 25 + 11 * widthStep, Y = 25 + i * heightStep };
                dc.DrawLine(area, p1, p2);
            }


        }

        void DrawScratch(int defectWidth, int defectHeight)
        {
            Graphics g = CreateGraphics();
            Pen pen = new Pen(Color.Red, 1);
            Brush brush = Brushes.Red;
            g.DrawRectangle(pen, defectWidth - 2, defectHeight - 2, 4, 4);
            g.FillRectangle(brush, defectWidth - 2, defectHeight - 2, 4, 4);
        }

        void DrawSpot(int defectWidth, int defectHeight)
        {
            Graphics g = CreateGraphics();
            Pen pen = new Pen(Color.Gray, 1);
            Brush brush = Brushes.Gray;
            Rectangle rectangle = new Rectangle(defectWidth - 2, defectHeight - 2, 4, 4);
            g.FillEllipse(brush, rectangle);
        }

        void DrawBubble(int defectWidth, int defectHeight)
        {
            Graphics g = CreateGraphics();
            Pen pen = new Pen(Color.Orange, 1);
            Brush brush = Brushes.Orange;
            Rectangle rectangle = new Rectangle(defectWidth - 2, defectHeight - 2, 4, 4);
            g.FillEllipse(brush, rectangle);
        }

        void DrawNap(int defectWidth, int defectHeight)
        {
            Graphics g = CreateGraphics();
            Pen pen = new Pen(Color.Green, 1);
            Brush brush = Brushes.Green;
            Rectangle rectangle = new Rectangle(defectWidth - 2, defectHeight - 2, 4, 4);
            g.FillEllipse(brush, rectangle);
        }

        void DrawPinhole(int defectWidth, int defectHeight)
        {
            Graphics g = CreateGraphics();
            Pen pen = new Pen(Color.Blue, 1);
            Brush brush = Brushes.Blue;
            g.DrawRectangle(pen, defectWidth - 2, defectHeight - 2, 4, 4);
            g.FillRectangle(brush, defectWidth - 2, defectHeight - 2, 4, 4);
        }

        void DrawFingerprint(int defectWidth, int defectHeight)
        {
            Graphics g = CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Pen pen = new Pen(Color.Orange, 2);
            Brush brush = new SolidBrush(Color.Orange);
            
            // 绘制橙色圆圈
            Rectangle rectangle = new Rectangle(defectWidth - 5, defectHeight - 5, 10, 10);
            g.DrawEllipse(pen, rectangle);
            
            // 绘制中心橙色点
            Rectangle centerRect = new Rectangle(defectWidth - 1, defectHeight - 1, 2, 2);
            g.FillEllipse(brush, centerRect);
        }

        public void DrawDot()
        {
            defectsLocation.Add(new int[] { 0, 1, 1 });
            defectsRelativeLocation.Add(new float[] { 0.6f, 0.4f });
            defectsLocation.Add(new int[] { 1, 2, 2 });
            defectsRelativeLocation.Add(new float[] { 0.6f, 0.4f });
            defectsLocation.Add(new int[] { 2, 3, 3 });
            defectsRelativeLocation.Add(new float[] { 0.6f, 0.4f });
            defectsLocation.Add(new int[] { 3, 4, 4 });
            defectsRelativeLocation.Add(new float[] { 0.6f, 0.4f });
            defectsLocation.Add(new int[] { 4, 5, 5 });
            defectsRelativeLocation.Add(new float[] { 0.6f, 0.4f });
            for (int i = 0; i < defectsLocation.Count; i++)
            {
                DisplayDefects(defectsLocation[i][0], defectsLocation[i][1], defectsLocation[i][2],
                    defectsRelativeLocation[i][0], defectsRelativeLocation[i][1]);
            }
        }

        public void DisplayDefects(int type, int row, int column, float heightRatio, float widthRatio)
        {
            defectsLocation.Add(new int[] { type, row, column });
            defectsRelativeLocation.Add(new float[] { heightRatio, widthRatio });
            int defectWidth = 25 + row * widthStep + (int)(widthStep * widthRatio) - 2;
            int defectHeight = 25 + column * heightStep + (int)(heightStep * heightRatio) - 2;
            if (r2.NextDouble() >= 0.99)
            {
                switch (type)
                {
                    case 0:
                        DrawScratch(defectWidth, defectHeight);
                        break;
                    case 1:
                        DrawPinhole(defectWidth, defectHeight);
                        break;
                    case 2:
                        DrawNap(defectWidth, defectHeight);
                        break;
                    case 3:
                        DrawBubble(defectWidth, defectHeight);
                        break;
                    case 4:
                        DrawSpot(defectWidth, defectHeight);
                        break;
                    case 5:
                        DrawFingerprint(defectWidth, defectHeight);
                        break;
                    default:
                        break;
                }
            }
            
        }


        //private void DrawDot(int type, int x, int y)
        //{
        //    Point pt = new Point { X = 25 + widthStep * x, Y = 25 + heightStep * y };
        //    switch (type)
        //    {
        //        case 0:
        //            dc.FillEllipse(Brushes.Gray, pt.X - 5, pt.Y - 5, 10, 10);
        //            break;
        //        case 1:
        //            dc.FillEllipse(Brushes.Green, pt.X - 5, pt.Y - 5, 10, 10);
        //            break;
        //        case 2:
        //            dc.FillEllipse(Brushes.YellowGreen, pt.X - 5, pt.Y - 5, 10, 10);
        //            break;
        //        case 3:
        //            dc.FillRectangle(Brushes.PowderBlue, pt.X - 5, pt.Y - 5, 10, 10);
        //            //dc.FillEllipse(Brushes.PowderBlue, X, Y, 10, 10);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        public void FormChanged()
        {
            
            dc.Clear(Color.White);
            DrawRectangle();
            for (int i = 0; i < defectsLocation.Count; i++)
            {
                DisplayDefects(defectsLocation[i][0], defectsLocation[i][1], defectsLocation[i][2],
                    defectsRelativeLocation[i][0], defectsRelativeLocation[i][1]);
            }

        }

        public void cameraLocation(int x, int y)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate { cameraLocation(x, y); }));
                return;
            }
            this.Controls.Remove(cameraLoc);
            //DrawRectangle();
            cameraLoc = new cameraDevices();
            float onecm = (this.Width - 50) / 43;
            int cameraWidth = (int)(onecm * 11.3);
            int cameraHeight = (int)(onecm * 16);
            cameraLoc.Size = new Size { Height = cameraHeight, Width = cameraWidth };
            int Locy = (int)(35 + ((y - 6150) / 300 - 14.1) * onecm * 1.13);
            int Locx = (int)(25 + (-1 + 5.9 * x) * onecm * 1.2); 
            cameraLoc.Location = new Point(Locx, Locy);
            this.Controls.Add(cameraLoc);
            Application.DoEvents();
            
        }
        #region 测试使用
        public void StartDection()
        {
            int width = this.Width / 6;
            int height = this.Height / 51;
            int locx = 25, locy = 0;
            for (int j = 0; j < 6; j++)
            {
                locy = 0;
                for (int i = 0; i <= 50; i++)
                {
                    cameraLocation(1, 2);
                    if (r1.NextDouble() > 0.6)
                    {
                        DrawDot();
                    }
                    Thread.Sleep(200);
                    locy += height;
                }
                Thread.Sleep(200);
                locx += width;
                j++;
                for (int i = 0; i <= 50; i++)
                {
                    cameraLocation(1, 2);
                    Thread.Sleep(200);
                    locy -= height;
                }
                locx += width;
            }
            
        }
        #endregion

        public void distributeGraph_Paint(object sender, PaintEventArgs e)
        {
            this.Controls.Clear();
            DrawRectangle();
            //DisplayDefects(0, 3, 3, 0.6f, 0.4f);
            //for (int i = 0; i < defectsLocation.Count; i++)
            //{
            //    DisplayDefects(defectsLocation[i][0], defectsLocation[i][1], defectsLocation[i][2],
            //        defectsRelativeLocation[i][0], defectsRelativeLocation[i][1]);
            //}
        }
    }
}
