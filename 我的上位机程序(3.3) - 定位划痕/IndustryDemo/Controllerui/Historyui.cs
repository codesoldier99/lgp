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
using System.Data.OleDb;

namespace IndustryDemo.Controllerui
{
    public partial class Historyui : DevExpress.XtraEditors.XtraUserControl
    {
        //OleDbConnection Connection = null;
        //string tableName = "table1";
        //string strMdb = "databases/Database1.mdb";//mdb路径

        public Historyui()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            gridControl1.Dock = DockStyle.Fill;
            //setModelList();
            //Connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + strMdb + "");
            //Connection.Open();
        }

        //private void setModelList()
        //{
        //    try
        //    {
        //        DataTable schemaTable = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
        //        foreach (DataRow dr in schemaTable.Rows)
        //        {
        //            //表名  
        //            tableName = dr["TABLE_NAME"].ToString();

        //            //字段名  
        //            //DataTable columnTable = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, dr["TABLE_NAME"].ToString(), null });
        //            //foreach (DataRow dr2 in columnTable.Rows)
        //            //{
        //            //    Console.WriteLine("{0}", dr2["COLUMN_NAME"]);
        //            //}
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        return;
        //    }

        //    DataTable dt = new DataTable();
        //    string sSql = "select * from " + tableName + "";
        //    OleDbDataAdapter da = new OleDbDataAdapter(sSql, Connection);
        //    da.Fill(dt);

        //    this.gridControl1.DataSource = dt;

        //}

        //public void Insert(string sql)
        //{
        //    OleDbCommand myCommand = new OleDbCommand(sql, Connection);//执行命令
        //    myCommand.ExecuteNonQuery();
        //}
    }
}
