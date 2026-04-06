using DevExpress.XtraEditors;
using IndustryDemo.Controllerui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HalconDotNet;
using Microsoft.Reporting.WinForms;

namespace IndustryDemo
{
    public partial class mainForm : DevExpress.XtraEditors.XtraForm
    {
        bool isOpen;
        int iter = 0;

        public string opttype = "窄带";
        public ushort optdiameter = 30;
        public string optshape = "圆形";
        public double optthickness = 5.0;

        public static mainForm frm;
        public Detectionui Detectionui1;

        public mainForm()
        {
            
            InitializeComponent();
            //detectionui1.Show();
            //detectionui1.FormChanged();
            configInfoui1.Hide();
            historyui1.Hide();
            //barButtonItem2.Enabled = true;
            frm = this;
            Detectionui Detectionui1 = new Detectionui();   //实例化Detectionui1
            Detectionui1.mainForm1 = this;
            //Detectionui1.eh += new EventHandler(changecontinueButton);//当form2的关联委托eh的各种方法触发时，form1中的showtext方法就会被调用，注意此处的+=；
            detectionui1.Show();
            ///Detectionui1.ShowDialog();
            //Detectionui1.Detectionuih(mainForm.frm);
        }

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            detectionui1.Start_Detection();
            isOpen = false;
            barButtonItem6.Enabled = true;
            barButtonItem1.Enabled = true;
            //barButtonItem2.Enabled = true;
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            detectionui1.Stop_Detection();
            if (detectionui1.upcamerasOpenStat)
            {
                detectionui1.bnClose_up_Click();  // 关闭相机连接
            }
            
        }

        

        private void btnStart_SelectedPageChanged(object sender, EventArgs e)
        {
            switch (btnStart.SelectedPage.Name)
            {
                case "ribbonPage1":
                    detectionui1.Show();
                    configInfoui1.Hide();
                    historyui1.Hide();
                    break;
                case "ribbonPage2":
                    detectionui1.Hide();
                    configInfoui1.Show();
                    historyui1.Hide();
                    break;
                case "ribbonPage3":
                    detectionui1.Hide();
                    configInfoui1.Hide();
                    historyui1.Show();
                    break;
                default:
                    break;
            }
        }

        public void SetCameraParam()
        {
            detectionui1.SetValue();
        }

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            configInfoui1.Show_Camera_Info();
        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            configInfoui1.Show_STM32_Info();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
        }

        private void mainForm_SizeChanged(object sender, EventArgs e)
        {
            //detectionui1.FormChanged();
        }

        private void mainForm_MaximumSizeChanged(object sender, EventArgs e)
        {
            //detectionui1.FormChanged();
        }

        private void mainForm_MaximizedBoundsChanged(object sender, EventArgs e)
        {
            //detectionui1.FormChanged();
        }

        private void barButtonItem6_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            detectionui1.Start_out();
            isOpen = true;
            barButtonItem6.Enabled = true;
            barButtonItem1.Enabled = true;
            //barButtonItem2.Enabled = true;
        }

        private void barButtonItem7_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //opttype = barEditItem3.EditValue.ToString();                //滤光片类型
            //optshape = barEditItem4.EditValue.ToString();               //滤光片外形
            //optdiameter = Convert.ToUInt16(barEditItem5.EditValue.ToString());            //滤光片直径
            //optthickness = Convert.ToDouble(barEditItem6.EditValue.ToString());           //滤光片厚度
            //Detectionui1 = new Detectionui();
            //Detectionui1.createMatrix(opttype, optdiameter, optshape, optthickness);
            
            if (barEditItem6.EditValue == null)
            {
                MessageBox.Show("请确认厚度！！！");
            }
            else
            {
                Global.thickness = Convert.ToDouble(barEditItem6.EditValue.ToString())+0.3;
            }

            if (barEditItem4.EditValue == null )
            {
                MessageBox.Show("请确定是否取次品！！！");
            }
            else
            { 
                Global.getdefopot = barEditItem4.EditValue.ToString();
            }
            if (barEditItem14.EditValue == null)
            {
                MessageBox.Show("请确认质检标准！！！");
            }
            else
            {
                Global.standard = barEditItem14.EditValue.ToString();
            }
        }

        private void barButtonItem8_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            configInfoui1.Show_trayconfig_Info();
        }

        private void barButtonItem9_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //DetectionWithDL detectionWithDL = new DetectionWithDL();
            //HObject DL = new HObject();
            //int r, c;
            //detectionWithDL.ImageProcess();
            // textBox1.Text =detectionWithDL.getX().ToString();
            //MessageBox.Show(detectionWithDL.getZx() + "," + detectionWithDL.getZy());
            Global.allDeviceSta = true;
            //barButtonItem9.Enabled = false;
        }

        private void detectionui1_Load(object sender, EventArgs e)
        {
            
        }

        public void changecontinueButton(object sender, EventArgs e)
        {
            this.barButtonItem9.Enabled = true;
        }

        public void changeqrcode()
        {
            barEditItem7.EditValue = Global.qrCode;
        }


        public void nextagain()
        {
            barButtonItem1.Enabled = true;
        }

        private void barButtonItem10_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void btnStart_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void barButtonItem11_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            
            if (barEditItem8.EditValue == null)
            {
                MessageBox.Show("请确认扫描范围！！！");
            }
            else
            {
                Global.scanRange = Convert.ToInt32(barEditItem8.EditValue);
            }
            
            if (barEditItem3.EditValue == null)
            {
                MessageBox.Show("请确认单双面！！！");
            }
            else
            {
                Global.dsface = Convert.ToString(barEditItem3.EditValue);
            }
            /*if(Convert.ToString(barEditItem14.EditValue) == "是")
            {
                Global.focaldistance = 0.5;
            }*/
        }

        private void barButtonItem12_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string type= "";
            string begin = "";
            string end = "";
            string defectionType ="";

            if (barEditItem9.EditValue != null)
            {
                type = barEditItem9.EditValue.ToString();
                if (type == "时间")
                {
                    if (barEditItem11.EditValue == null || barEditItem12.EditValue == null)
                    {
                        MessageBox.Show("未选择起止时间！");
                    }
                    else
                    {
                        begin = barEditItem11.EditValue.ToString();
                        end = barEditItem12.EditValue.ToString();
                        report report = new report(type, begin, end, defectionType);
                        report.Show();
                    }
                }
                else
                {
                    if (barEditItem13.EditValue == null)
                    {
                        MessageBox.Show("未选择瑕疵类型！");
                    }
                    else
                    {
                        defectionType = barEditItem13.EditValue.ToString();
                        report report = new report(type, begin, end, defectionType);
                        report.Show();
                    }
                }

            }
            else
            {
                MessageBox.Show("请选择查询类型！");
            }

        }
    }
}
