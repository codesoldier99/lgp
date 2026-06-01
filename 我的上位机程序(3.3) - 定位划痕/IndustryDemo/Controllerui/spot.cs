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
    public partial class spot : DevExpress.XtraEditors.XtraUserControl
    {
        public spot()
        {
            InitializeComponent();
        }

        private void spot_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Gray, 1);
            Brush brush = Brushes.Gray;
            Rectangle rectangle = new Rectangle(0, 0, Width, Height);
            g.FillEllipse(brush, rectangle);
        }
    }
}
