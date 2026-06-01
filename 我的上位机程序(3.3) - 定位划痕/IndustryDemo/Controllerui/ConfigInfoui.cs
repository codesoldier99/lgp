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
using MySql.Data.MySqlClient;

namespace IndustryDemo.Controllerui
{ 
    public partial class ConfigInfoui : DevExpress.XtraEditors.XtraUserControl
    {
        //public int a;
        //public Camera camera1, camera2, camera3, camera4;
        public ConfigInfoui()
        {
            InitializeComponent();
            xtraTabControl1.Dock = DockStyle.Fill;
        }

        private void trackBarControl2_EditValueChanged(object sender, EventArgs e)
        {
            numericUpDown2.Value = trackBarControl2.Value;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            trackBarControl1.Value = (int)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            trackBarControl2.Value = (int)numericUpDown2.Value;
        }

        private void trackBarControl4_EditValueChanged(object sender, EventArgs e)
        {
            numericUpDown4.Value = trackBarControl4.Value;
        }

        private void trackBarControl3_EditValueChanged(object sender, EventArgs e)
        {
            numericUpDown3.Value = trackBarControl3.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            trackBarControl4.Value = (int)numericUpDown4.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            trackBarControl3.Value = (int)numericUpDown3.Value;
        }

        private void trackBarControl6_EditValueChanged(object sender, EventArgs e)
        {
            numericUpDown6.Value = (int)trackBarControl6.Value;
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            trackBarControl6.Value = (int)numericUpDown6.Value;
        }

        private void trackBarControl5_EditValueChanged(object sender, EventArgs e)
        {
            numericUpDown5.Value = trackBarControl5.Value;
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            trackBarControl5.Value = (int)numericUpDown5.Value;
        }

        private void trackBarControl8_EditValueChanged(object sender, EventArgs e)
        {
            numericUpDown8.Value = trackBarControl8.Value;
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            trackBarControl8.Value = (int)numericUpDown8.Value;
        }

        private void trackBarControl7_EditValueChanged(object sender, EventArgs e)
        {
            numericUpDown7.Value = trackBarControl7.Value;
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            trackBarControl7.Value = (int)numericUpDown7.Value;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            mainForm.frm.SetCameraParam();
        }

        public void Show_Camera_Info()
        {
            xtraTabPage1.Show();
            xtraTabPage2.Hide();
            xtraTabPage3.Hide();
        }

        public void Show_STM32_Info()
        {
            xtraTabPage2.Show();
            xtraTabPage1.Hide();
            xtraTabPage3.Hide();
        }

        public void Show_trayconfig_Info()
        {
            xtraTabPage3.Show();
            xtraTabPage1.Hide();
            xtraTabPage2.Hide();
        }

        private void trackBarControl1_EditValueChanged(object sender, EventArgs e)
        {
            numericUpDown1.Value = trackBarControl1.Value;
        }

        private void comboBoxEdit2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            MySqlConnection conn = new MySqlConnection(Global.conString);
            conn.Open();
            string sql = "delete from tray where qrcode=" + textBox3.Text + "; ";

            MySqlCommand comm = new MySqlCommand(sql, conn);
            MySqlDataReader myReader1;
            myReader1 = comm.ExecuteReader();

            MessageBox.Show("删除成功！");
            comm.Dispose();
            conn.Close();
            button3_Click(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //MySqlConnection conn = new MySqlConnection(@"Server=127.0.0.1;Port=3306;Database=filterdetectiondatabase;Uid=root;Pwd=12345");
            MySqlConnection conn = new MySqlConnection(Global.conString);
            conn.Open();
            string qrCode =textBox1.Text.Trim();
            string shape = comboBox1.Text;
            double d = double.Parse(textBox2.Text);
            double length = double.Parse(maskedTextBox1.Text);
            double width = double.Parse(maskedTextBox2.Text);
            double thickness = double.Parse(maskedTextBox3.Text);
            //double h = double.Parse(textBox3.Text);
            //DateTime createDT = DateTime.Now;  
            string createT = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");//当前输入的时间

            // string sql = "insert into defection(idtray,picName,picX,picY,trayX,trayY,defectionType,area) value('" + idtray + "','" + picName + "','" + picX + "','" + picY + "','" + trayX + "','" + trayY + "','" + defectionType + "','" + area + "');";
            string sql = "insert into tray(qrcode,optshape,diameter,length,width,thickness,creationtime) value('" + qrCode + "','" + shape + "','" +d +"','" + length + "','" + width + "','" + thickness + "','" + createT + "');";

            MySqlCommand comm = new MySqlCommand(sql, conn);
            MySqlDataReader myReader1;
            myReader1 = comm.ExecuteReader();
           
            MessageBox.Show("添加成功！");
            textBox1.Text = "";
            comboBox1.Text = "";
            textBox2.Text = "";
            maskedTextBox1.Text = "";
            maskedTextBox2.Text = "";
            maskedTextBox3.Text = "";
            //textBox3.Text = "";
            comm.Dispose();
            conn.Close();
            button3_Click(sender,e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //在这里面就可以进行ListView控件的构建以及数据更新等
            listView1.Clear();
            //构建表头
            listView1.Columns.Add("二维码");
            listView1.Columns.Add("形状");
            listView1.Columns.Add("直径");
            listView1.Columns.Add("长度");
            listView1.Columns.Add("宽度");
            //创建数据库连接类的对象
            MySqlConnection conn = new MySqlConnection(Global.conString);
            conn.Open();
            //执行con对象的函数，返回一个SqlCommand类型的对象
            MySqlCommand cmd = conn.CreateCommand();
            //把输入的数据拼接成sql语句，并交给cmd对象
            cmd.CommandText = "select qrcode,optshape,diameter,length,width from tray order by qrcode asc";

            //用cmd的函数执行语句，返回SqlDataReader类型的结果dr,dr就是返回的结果集（也就是数据库中查询到的表数据）
            MySqlDataReader dr = cmd.ExecuteReader();
            //用dr的read函数，每执行一次，返回一个包含下一行数据的集合dr
            while (dr.Read())
            {
                //构建一个ListView的数据，存入数据库数据，以便添加到listView1的行数据中
                ListViewItem lt = new ListViewItem();
                //将数据库数据转变成ListView类型的一行数据
                lt.Text = dr["qrcode"].ToString();
                lt.SubItems.Add(dr["optshape"].ToString());
                lt.SubItems.Add(dr["diameter"].ToString());
                lt.SubItems.Add(dr["length"].ToString());
                lt.SubItems.Add(dr["width"].ToString());
                //将lt数据添加到listView1控件中
                listView1.Items.Add(lt);
            }
            cmd.Dispose();
            conn.Close();
        }
    }
}
