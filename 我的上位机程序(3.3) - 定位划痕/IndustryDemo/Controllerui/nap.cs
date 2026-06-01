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
    public partial class nap : DevExpress.XtraEditors.XtraUserControl
    {
        //布毛
        public nap()
        {
            InitializeComponent();
        }

        private void nap_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Green, 1);
            Brush brush = Brushes.Green;
            Rectangle rectangle = new Rectangle(0, 0, Width, Height);
            g.FillEllipse(brush, rectangle);
            //g.DrawRectangle(pen, 0, 0, this.Width, this.Height);
            //g.FillRectangle(brush, 1, 1, this.Width, this.Height);
        }
    }
}
