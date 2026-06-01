using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace IndustryDemo.Controllerui
{
    class DataBaseOperate
    {
        OleDbConnection oleDb;
        public DataBaseOperate()
        {
            oleDb = new OleDbConnection(@"Provider = Microsoft.ACE.OLEDB.12.0; Data Source = databases/Database1.mdb");
            oleDb.Open();
        }

        public string ToSql(int cameraID, string LightSource, int column, int row, int distance, 
            string defect, string defectX, string defectY)
        {
            string sql = "insert into table1";
            sql += " (相机编号,光源类型,行数,列数,距离,文件名,瑕疵类型,瑕疵位置X,瑕疵位置Y) ";
            sql += "values (";
            sql += cameraID.ToString() + ",";
            sql += "'" + LightSource.ToString() + "'" + ",";
            sql += "'" + column.ToString() + "',";
            sql += "'" + row.ToString() + "',";
            sql += "'" + distance.ToString() + "',";
            sql += "'" + column.ToString() + "-" + row.ToString() + "-" + distance.ToString() + ".bmp" + "',";
            sql += "'" + defect + "',";
            sql += defectX + ",";
            sql += defectY + ")";
            return sql;
        }

        public void Exec(int cameraID, string LightSource, int column, int row, int distance, List<string[]> defects)
        {
            foreach (var defect in defects)
            {
                string sql = ToSql(cameraID, LightSource, column, row, distance, defect[0], defect[1], defect[2]);
                //OleDbCommand oleDbCommand = new OleDbCommand(sql, oleDb);
                //    //int i = oleDbCommand.ExecuteNonQuery(); //返回被修改的数目
                //oleDbCommand.ExecuteNonQueryAsync();
            }
        }
    }
}
