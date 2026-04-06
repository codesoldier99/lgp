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
using MySql.Data.MySqlClient;
using System.IO;
using System.Configuration;

namespace IndustryDemo
{
    public partial class report : DevExpress.XtraEditors.XtraForm
    {

        string type;
        string begin;
        string end;
        string defectionType;


        public static string ConnString = "";

        public static string Conn_Config_Str_Name = string.Empty;

        public static string Conn_Server = "127.0.0.1";
        public static string Conn_DBName = "filterdetectiondatabase";
        public static string Conn_Uid = "root";
        public static string Conn_Pwd = "123456";
        public static string _ConnString
        {
            get
            {
                if (!string.IsNullOrEmpty(ConnString))
                    return ConnString;

                object oConn = ConfigurationManager.ConnectionStrings[Conn_Config_Str_Name];
                if (oConn != null && oConn.ToString() != "")
                    return oConn.ToString();

                return string.Format(@"server={0};database={1};userid={2};password={3}", Conn_Server, Conn_DBName, Conn_Uid, Conn_Pwd);
            }
        }


        public report(string type, string begin, string end, string defectionType)
        {
            InitializeComponent();
            this.type = type;
            this.begin = begin;
            this.end = end;
            this.defectionType = defectionType;
            reportShow();
        }

        void reportShow()
        {

            try
            {
                string sql = "";
                DataSet ds = null;

                MySqlConnection myConn = null;
                if (type == "时间")
                {
                    ds = null;

                    string error = "";
                    string beginTime = begin.ToString();
                    beginTime = beginTime.Replace("/", "-");
                    string endTime = end.ToString();
                    endTime = endTime.Replace("/", "-");


                    DataTable dt = Controllerui.MySqlHelper.GetDataTable(out error, "select DISTINCT diameter,thickness from tray where creationtime >= '" + beginTime + "' and  creationtime <= '" + endTime + "'");
                    if (dt.Rows.Count == 0)
                    {

                        MessageBox.Show("无结果！");
                    }
                    else
                    {
                        this.reportViewer1.LocalReport.ReportEmbeddedResource = "IndustryDemo.selectByTime.rdlc";
                        LocalReport localReport = reportViewer1.LocalReport;
                        localReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "selectByTime.rdlc");
                        ReportParameter rp = new ReportParameter("begin", begin.ToString());
                        ReportParameter rp2 = new ReportParameter("end", end.ToString());
                        reportViewer1.LocalReport.SetParameters(rp);
                        reportViewer1.LocalReport.SetParameters(rp2);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow dr = dt.Rows[i];
                            sql += "select DISTINCT diameter,thickness," +
                                "(select count(*) from tray a join defection b on a.qrcode = b.qrcode where defectionType = 'swab' and diameter = " + dr[0] + "  and thickness = " + dr[1] + ") as swabNum," +
                                "(select count(*) from tray a join defection b on a.qrcode = b.qrcode where defectionType = 'chipping' and diameter = " + dr[0] + "  and thickness = " + dr[1] + ") as chippingNum," +
                                "(select count(*) from tray a join defection b on a.qrcode = b.qrcode where defectionType = 'break' and diameter = " + dr[0] + "  and thickness = " + dr[1] + ") as breakNum," +
                                "(select count(*) from tray a join defection b on a.qrcode = b.qrcode where defectionType = 'scratch' and diameter = " + dr[0] + "  and thickness = " + dr[1] + ") as scratchNum," +
                                "(select count(*) from tray a join defection b on a.qrcode = b.qrcode where defectionType = 'pinhole' and diameter = " + dr[0] + "  and thickness = " + dr[1] + ") as pinholeNum," +
                                "(select count(*) from tray a join defection b on a.qrcode = b.qrcode where defectionType = 'bubble' and diameter = " + dr[0] + "  and thickness = " + dr[1] + ") as bubbleNum," +
                                "(select count(*) from tray a join defection b on a.qrcode = b.qrcode where defectionType = 'pitting' and diameter = " + dr[0] + "  and thickness = " + dr[1] + ") as pittingNum," +
                                "(select count(*) from tray a join defection b on a.qrcode = b.qrcode where defectionType = 'splash' and diameter = " + dr[0] + "  and thickness = " + dr[1] + ") as splashNum," +
                                "(select count(*) from tray a join defection b on a.qrcode = b.qrcode where defectionType = 'fingerprint' and diameter = " + dr[0] + "  and thickness = " + dr[1] + ") as fingerprintNum," +
                                "(select count(*) from tray a join defection b on a.qrcode = b.qrcode where defectionType = 'corrosion' and diameter = " + dr[0] + "  and thickness = " + dr[1] + ") as corrosionNum " +
                                "from tray a join defection b on a.qrcode = b.qrcode where diameter = " + dr[0] + "  and thickness = " + dr[1] + "";
                            if (i != dt.Rows.Count - 1)
                                sql += " union all ";
                        }
                        myConn = new MySqlConnection(_ConnString);
                        MySqlCommand myCmd = new MySqlCommand(sql, myConn);
                        myConn.Open();
                        MySqlDataAdapter adapter = new MySqlDataAdapter(myCmd);
                        ds = new DataSet();
                        adapter.Fill(ds, "selectByTime");
                        myConn.Close();

                        ReportDataSource dsPurchRec = new ReportDataSource();
                        dsPurchRec.Name = "selectByTime";//记得对应报告上面数据源的名字
                        dsPurchRec.Value = ds.Tables["selectByTime"];

                        localReport.DataSources.Add(dsPurchRec);
                        reportViewer1.RefreshReport();
                    }
                }
                else if (type == "瑕疵类型")
                {
                    ds = null;
                    ReportParameter rp = null;
                    this.reportViewer1.LocalReport.ReportEmbeddedResource = "IndustryDemo.selectByDefection.rdlc";
                    LocalReport localReport = reportViewer1.LocalReport;
                    localReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "selectByDefection.rdlc");

                    switch (defectionType)
                    {
                        case "布毛":
                            rp = new ReportParameter("type", "布毛");
                            sql = "call selectBydefection('Swabs')";
                            break;
                        case "划伤":
                            rp = new ReportParameter("type", "划伤");
                            sql = "call selectBydefection('scratch')";
                            break;
                        case "针孔":
                            rp = new ReportParameter("type", "针孔");
                            sql = "call selectBydefection('pinhole')";
                            break;
                        case "膜破":
                            rp = new ReportParameter("type", "膜破");
                            sql = "call selectBydefection('bubble')";
                            break;
                        case "麻点":
                            rp = new ReportParameter("type", "麻点");
                            sql = "call selectBydefection('pitting')";
                            break;
                        case "气泡":
                            rp = new ReportParameter("type", "气泡");
                            sql = "call selectBydefection('scratch')";
                            break;
                        case "喷溅点":
                            rp = new ReportParameter("type", "喷溅点");
                            sql = "call selectBydefection('splash')";
                            break;
                        case "指纹印":
                            rp = new ReportParameter("type", "指纹印");
                            sql = "call selectBydefection('fingerprint')";
                            break;
                        case "腐蚀印":
                            rp = new ReportParameter("type", "腐蚀印");
                            sql = "call selectBydefection('corrosion')";
                            break;
                        case "崩边":
                            rp = new ReportParameter("type", "崩边");
                            sql = "call selectBydefection('chipping')";
                            break;
                    }
                    reportViewer1.LocalReport.SetParameters(rp);
                    myConn = new MySqlConnection(_ConnString);
                    MySqlCommand myCmd = new MySqlCommand(sql, myConn);
                    myConn.Open();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(myCmd);
                    ds = new DataSet();
                    adapter.Fill(ds, "selectByDefection");

                    myConn.Close();

                    ReportDataSource dsPurchRec = new ReportDataSource();
                    dsPurchRec.Name = "selectByDefection";//记得对应报告上面数据源的名字
                    dsPurchRec.Value = ds.Tables["selectByDefection"];

                    localReport.DataSources.Add(dsPurchRec);
                    reportViewer1.RefreshReport();

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }

        }

        private void report_Load(object sender, EventArgs e)
        {
            this.reportViewer1.RefreshReport();
        }
    }
}
