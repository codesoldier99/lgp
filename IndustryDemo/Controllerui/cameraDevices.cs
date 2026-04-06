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

namespace IndustryDemo.Controllerui
{
    public partial class cameraDevices : DevExpress.XtraEditors.XtraUserControl
    {
        public Brush brush = Brushes.Aqua;
        public cameraDevices()
        {
            InitializeComponent();
            brush = Brushes.Aqua;
        }

        private void cameraDevices_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Black, 1);
            Brush brush = Brushes.Aqua;
            g.DrawRectangle(pen, 0, 0, this.Width - 2, this.Height - 2);
            g.FillRectangle(brush, 1, 1, this.Width - 3, this.Height - 3);
            //float rectWidth = circleWidth / 5;
            //float rectHeight = circleHeight / 5 * 3 / 4;
            //g.DrawRectangle(pen, rectWidth + 2, rectWidth + 2, rectWidth, rectHeight);
            //g.DrawRectangle(pen, rectWidth * 3 + 2, rectWidth + 2, rectWidth, rectHeight);
            //g.DrawRectangle(pen, rectWidth + 2, rectWidth * 3 + 2, rectWidth, rectHeight);
            //g.DrawRectangle(pen, rectWidth * 3 + 2, rectWidth * 3 + 2, rectWidth, rectHeight);
        }
    }
}
