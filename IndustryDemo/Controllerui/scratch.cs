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
    public partial class scratch : DevExpress.XtraEditors.XtraUserControl
    {
        //划伤
        public scratch()
        {
            InitializeComponent();
        }

        private void scratch_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Red, 1);
            Brush brush = Brushes.Red;
            g.DrawRectangle(pen, 0, 0, this.Width, this.Height);
            g.FillRectangle(brush, 1, 1, this.Width, this.Height);
        }
    }
}
