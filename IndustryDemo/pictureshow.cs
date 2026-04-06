using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using MySqlHelper = IndustryDemo.Controllerui.MySqlHelper;
namespace IndustryDemo
{
    public partial class pictureshow : Form
    {
        // 创建用于绘制文本的字体对象
        Font font0 = new Font("宋体", 30, FontStyle.Bold);

        // 创建用于绘制文本的画笔对象
        Brush brush0 = Brushes.Red;

        // 创建用于绘制文本的字体对象
        Font font = new Font("宋体", 25);

        // 创建用于绘制文本的画笔对象
        Brush brush = Brushes.White;

        //虚拟滤光片放大后的宽高
        private int circlewidth; //宽度
        private int circleheight;//高度
        private int rectanglewidth; //宽度
        private int rectangleheight;//高度
        private static double optdiameter1;
        private string err;

        public pictureshow()
        {
            InitializeComponent();
        }

        public void showpicture(string path, string picname, int ShowRow, int ShowCol)
        {
            this.Text = path;
            // 连接 MySQL 数据库
            MySqlConnection conn1 = new MySqlConnection(Global.conString);//数据库连接参数放在Global.cs的全局变量conString中
            conn1.Open();
            // 读取包含坐标和瑕疵文本信息的表格
            MySqlCommand cmd1 = conn1.CreateCommand();
            cmd1.CommandText = "select picX,picY,posX,posY,defectionType from defection where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and picName='" + picname + "' and posX='" + ShowRow + "' and posY='" + ShowCol + "'";
            MySqlDataReader dr1 = cmd1.ExecuteReader();

            List<double> xList = new List<double>();
            List<double> yList = new List<double>();
            List<string> textList = new List<string>();

            while (dr1.Read())
            {
                double col1 = dr1.GetDouble(0);
                double col2 = dr1.GetDouble(1);
                string col3 = dr1.GetString(4);

                xList.Add(col1);
                yList.Add(col2);
                textList.Add(col3);
            }

            double[] xArray = xList.ToArray();
            double[] yArray = yList.ToArray();
            string[] textArray = textList.ToArray();


            for (int i = 0; i < xArray.Length; i++)
            {
                xArray[i] = xArray[i] / 512 * 2048;
                yArray[i] = yArray[i] / 512 * 2592;
            }

            //计算图片所在料盘的位置
            int POS_X = dr1.GetInt16(2);
            int POS_Y = dr1.GetInt16(3);
            POS_X = Global.optRow - POS_X;
            POS_Y = Global.optLine - POS_Y;

            string x_y = "位置：" + POS_X.ToString() + "行" + POS_Y.ToString() + "列";

            dr1.Dispose();
            conn1.Close();

            //this.pictureBox1.Load(path);
            // 从指定路径加载图像文件，并在 pictureBox1 中显示
            this.pictureBox1.Load(path);

            // 将图片转换为 Bitmap 对象
            Bitmap bitmap = new Bitmap(pictureBox1.Image);

            // 将图片的像素格式转换为直接 32 位 ARGB 格式
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(newBitmap))
            {
                graphics.DrawImage(bitmap, new Rectangle(0, 0, newBitmap.Width, newBitmap.Height));
            }

            // 在转换后的图片上绘制文本信息
            using (Graphics graphics = Graphics.FromImage(newBitmap))
            {
                // 设置文本背景颜色
                Color backgroundColor = Color.Yellow;
                // 计算文本的大小
                SizeF textSize = TextRenderer.MeasureText(x_y, font0);

                // 绘制文本背景
                graphics.FillRectangle(new SolidBrush(backgroundColor), 20, 20, textSize.Width, textSize.Height);

                graphics.DrawString(x_y, font0, brush0, 20, 20);

                for (int i = 0; i < textArray.Length; i++)
                {
                    string text = textArray[i];
                    double x = xArray[i];
                    double y = yArray[i];
                    graphics.DrawString(text, font, brush, (float)y, (float)x);
                }
                pictureBox1.Image = newBitmap;
                
                //保存带瑕疵标注的图片到DefectDetail文件夹
                string saveFolder = "D://" + Global.qrCode + "/" + Global.detectiontime + "/DefectDetail";
                if (!System.IO.Directory.Exists(saveFolder))
                {
                    System.IO.Directory.CreateDirectory(saveFolder);
                }
                string fileName = saveFolder + "/" + picname + ".bmp";
                newBitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);

                // 释放资源
                bitmap.Dispose();
                graphics.Dispose();
            }
            panelOpticalfilter2(picname, ShowRow, ShowCol);
        }
        public void panelOpticalfilter2(string picname, int FRow, int FCol)
        {
            if (panelControl1.BackgroundImage != null)
            {
                panelControl1.BackgroundImage.Dispose();
            }
            GC.Collect();

            if (panelControl1.Width <= 0 || panelControl1.Height <= 0)
            {
                return;
            }
            Image backgroundBorder = new Bitmap(panelControl1.Width, panelControl1.Height);

            //BlockWidth = panelOpticalfilter.Width / (Global.optLine);
            //BlockHeight = panelOpticalfilter.Height / (Global.optRow);

            //Graphics buffGraphics = Graphics.FromImage(backgroundBorder);
            //Brush bushGray = new SolidBrush(Color.Gray);//填充的颜色
            //Brush bushRed = new SolidBrush(Color.Red);//填充的颜色
            //panelControl1.BackgroundImage = backgroundBorder;
            //Image backgroundBorder = new Bitmap(panelOpticalfilter.Width, panelOpticalfilter.Height);

            Graphics gra1 = Graphics.FromImage(backgroundBorder);
            gra1.Clear(panelControl1.BackColor);
            gra1.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Brush bushwhite = new SolidBrush(Color.Black);//填充的颜色
            Brush bushred = new SolidBrush(Color.Red);//填充的颜色
            Brush bushgreen = new SolidBrush(Color.Green);//填充的颜色
            Font myFont = new Font("宋体", 8, FontStyle.Bold);
            Brush bushyellow = new SolidBrush(Color.Yellow);//黄色
            Brush bushorange = new SolidBrush(Color.Orange);//橙色


            //获取滤光片等级
            string order = "select level from filterlevel where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and posX='" + FRow + "' and posY='" + FCol + "'";
            DataTable dt_now = MySqlHelper.GetDataTable(out err, order);
            //if (optArray[i, j] != 0)
            switch (Convert.ToInt32(dt_now.Rows[0][0]))
            {
                case 6:
                    if (Global.optshape == "圆形")
                    {
                        circlewidth = panelControl1.Width; //宽度
                        circleheight = panelControl1.Height;//高度
                        gra1.FillEllipse(bushred, 0, 0, circlewidth, circleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    else if (Global.optshape == "方形")
                    {
                        rectanglewidth = panelControl1.Width; //宽度
                        rectangleheight = Convert.ToInt32(Global.width * (panelControl1.Width / Global.length));//高度
                        gra1.FillRectangle(bushred, 0, 0, rectanglewidth, rectangleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    break;
                case 5:
                    if (Global.optshape == "圆形")
                    {
                        circlewidth = panelControl1.Width; //宽度
                        circleheight = panelControl1.Height;//高度
                        gra1.FillEllipse(bushorange, 0, 0, circlewidth, circleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    else if (Global.optshape == "方形")
                    {
                        rectanglewidth = panelControl1.Width; //宽度
                        rectangleheight = Convert.ToInt32(Global.width * (panelControl1.Width / Global.length));//高度
                        gra1.FillRectangle(bushorange, 0, 0, rectanglewidth, rectangleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    break;
                case 4:
                    if (Global.optshape == "圆形")
                    {
                        circlewidth = panelControl1.Width; //宽度
                        circleheight = panelControl1.Height;//高度
                        gra1.FillEllipse(bushyellow, 0, 0, circlewidth, circleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    else if (Global.optshape == "方形")
                    {
                        rectanglewidth = panelControl1.Width; //宽度
                        rectangleheight = Convert.ToInt32(Global.width * (panelControl1.Width / Global.length));//高度
                        gra1.FillRectangle(bushyellow, 0, 0, rectanglewidth, rectangleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    break;
                default:
                    if (Global.optshape == "圆形")
                    {
                        circlewidth = panelControl1.Width; //宽度
                        circleheight = panelControl1.Height;//高度
                        gra1.FillEllipse(bushgreen, 0, 0, circlewidth, circleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    else if (Global.optshape == "方形")
                    {
                        rectanglewidth = panelControl1.Width; //宽度
                        rectangleheight = Convert.ToInt32(Global.width * (panelControl1.Width / Global.length));//高度
                        gra1.FillRectangle(bushgreen, 0, 0, rectanglewidth, rectangleheight);//画填充椭圆的方法，x坐标、y坐标、宽、高，如果是100，则半径为50
                    }
                    break;
            }

            //MessageBox.Show("点击" + lastRow.ToString() + "," + lastCol.ToString());
            string query = "select * from defection where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and picName='" + picname + "' and posX='" + FRow + "' and posY='" + FCol + "'";
            string ab;
            ////query = "select * from defection";
            DataTable dt = MySqlHelper.GetDataTable(out ab, query);
            DataTable dt2 = MySqlHelper.GetDataTable(out err, "select count(*) from defection where qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and picName='" + picname + "' and posX='" + FRow + "' and posY='" + FCol + "'");//获取该片上的瑕疵数
            double diametermultiple = 300 / Global.diameter;
            double lengthmultiple = 300 / Global.length;
            double widthmultiple = lengthmultiple;

            if (Convert.ToInt32(dt2.Rows[0][0]) != 0)
            {
                //画叉
                if (Global.optshape == "圆形")
                {
                    for (int i = 0; i < Convert.ToInt32(dt2.Rows[0][0]); i++)
                    {
                        //相对于单个滤光片的坐标
                        int bigNewX = Convert.ToInt32(350 - Convert.ToInt32(dt.Rows[i][5]) - (Global.diameter + ((350 - Global.diameter) / (Global.optLine - 1)) - Global.diameter) * FCol);
                        int bigNewY = Convert.ToInt32(340 - Convert.ToInt32(dt.Rows[i][6]) - (Global.diameter + ((340 - Global.diameter) / (Global.optRow - 1)) - Global.diameter) * FRow);
                        //MessageBox.Show(bigNewX.ToString() + "," + bigNewY.ToString());
                        switch (dt.Rows[i][9])
                        {
                            case "划痕":     //划痕
                                gra1.DrawString("1", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                break;
                            case "内布毛":     //布毛
                                gra1.DrawString("2", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                break;
                            case "外布毛":     //膜破
                                gra1.DrawString("3", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                break;
                            case "水印":     //气泡
                                gra1.DrawString("4", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                break;
                            case "内点子":   //内点子
                                gra1.DrawString("5", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                break;
                            case "腐蚀印":     //腐蚀印
                                gra1.DrawString("6", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                break;
                            case "点子":     //点子
                                gra1.DrawString("7", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                break;
                            case "手印":     //手印
                                gra1.DrawString("8", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                break;
                            case "气泡":     //水印
                                gra1.DrawString("9", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                break;
                            case "膜破":     //膜外布毛
                                gra1.DrawString("10", myFont, bushwhite, Convert.ToInt32(bigNewX * diametermultiple), Convert.ToInt32(bigNewY * diametermultiple));
                                break;
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
                        int bigNewX = Convert.ToInt32(350 - Convert.ToInt32(dt.Rows[i][5]) - (Global.length + ((350 - Global.length) / (Global.optLine - 1)) - Global.length) * FCol);
                        int bigNewY = Convert.ToInt32(340 - Convert.ToInt32(dt.Rows[i][6]) - (Global.width + ((340 - Global.width) / (Global.optRow - 1)) - Global.width) * FRow);
                        //MessageBox.Show(bigNewX.ToString() + "," + bigNewY.ToString());
                        switch (dt.Rows[i][9])
                        {
                            case "划痕":     //划痕
                                gra1.DrawString("1", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                break;
                            case "内布毛":     //内布毛
                                gra1.DrawString("2", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                break;
                            case "外布毛":     //外布毛
                                gra1.DrawString("3", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                break;
                            case "水印":     //水印
                                gra1.DrawString("4", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                break;
                            case "内点子":     //内点子
                                gra1.DrawString("5", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                break;
                            case "腐蚀印":     //腐蚀印
                                gra1.DrawString("6", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                break;
                            case "点子":     //点子
                                gra1.DrawString("7", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                break;
                            case "手印":     //手印
                                gra1.DrawString("8", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                break;
                            case "气泡":     //气泡
                                gra1.DrawString("9", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                break;
                            case "膜破":     //膜破
                                gra1.DrawString("10", myFont, bushwhite, Convert.ToInt32(bigNewX * lengthmultiple), Convert.ToInt32(bigNewY * widthmultiple));
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            
            //在放大的滤光片图中心绘制细黑色十字线
            float centerX = panelControl1.Width / 2f;
            float centerY = panelControl1.Height / 2f;
            using (Pen crossPen = new Pen(Color.Black, 1))
            {
                gra1.DrawLine(crossPen, centerX, 0, centerX, panelControl1.Height);
                gra1.DrawLine(crossPen, 0, centerY, panelControl1.Width, centerY);
            }
            
            panelControl1.Image = backgroundBorder;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Global.stop = true;
            //this.Close;
        }
    }
}
