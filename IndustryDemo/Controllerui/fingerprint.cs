using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IndustryDemo.Controllerui
{
    /// <summary>
    /// 手印缺陷显示控件
    /// 在虚拟盘上显示为橙色圆圈
    /// </summary>
    public partial class fingerprint : DevExpress.XtraEditors.XtraUserControl
    {
        public fingerprint()
        {
            InitializeComponent();
        }

        private void fingerprint_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 绘制橙色圆圈边框（直径10像素）
            Pen pen = new Pen(Color.Orange, 2);
            Rectangle rectangle = new Rectangle(0, 0, 10, 10);
            g.DrawEllipse(pen, rectangle);

            // 绘制中心橙色填充点
            Brush brush = new SolidBrush(Color.Orange);
            Rectangle centerRect = new Rectangle(4, 4, 2, 2);
            g.FillEllipse(brush, centerRect);

            pen.Dispose();
            brush.Dispose();
        }
    }
}
