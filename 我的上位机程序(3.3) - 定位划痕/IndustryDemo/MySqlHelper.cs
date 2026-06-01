using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Windows.Forms;

namespace IndustryDemo.Controllerui
{
    public class MySqlHelper
    {

        public static string ConnString = "";

        public static string Conn_Config_Str_Name = string.Empty;

        public static string Conn_Server = "127.0.0.1";
        public static string Conn_DBName = "filterdetectiondatabase";
        public static string Conn_Uid = "root";
        public static string Conn_Pwd = "123456";

        public static string err_2;

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

        // 数据库连接
        public static void ConnectToMysql()
        {
            MySqlConnection myConn = null;
            try
            {
                myConn = new MySqlConnection(_ConnString);
                myConn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        // 读取数据 datatable
        public static DataTable GetDataTable(out string sError, string sSQL)
        {
            DataTable dt = null;
            sError = string.Empty;

            MySqlConnection myConn = null;
            try
            {
                myConn = new MySqlConnection(_ConnString);
                MySqlCommand myCommand = new MySqlCommand(sSQL, myConn);
                myConn.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(myCommand);
                dt = new DataTable();
                adapter.Fill(dt);
                myConn.Close();
            }
            catch (Exception ex)
            {
                sError = ex.Message;
            }
            return dt;
        }

        // 读取数据 dataset
        public static DataSet GetDataSet(out string sError, string sSQL)
        {
            DataSet ds = null;
            sError = string.Empty;

            MySqlConnection myConn = null;
            try
            {
                myConn = new MySqlConnection(_ConnString);
                MySqlCommand myCmd = new MySqlCommand(sSQL, myConn);
                myConn.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(myCmd);
                ds = new DataSet();
                adapter.Fill(ds);
                myConn.Close();
            }
            catch (Exception ex)
            {
                sError = ex.Message;
            }
            return ds;
        }

        // 取最大的ID
        public static Int32 GetMaxID(out string sError, string sKeyField, string sTableName)
        {
            DataTable dt = GetDataTable(out sError, "select IFNULL(max(" + sKeyField + "),0) as MaxID from " + sTableName);
            if (dt != null && dt.Rows.Count > 0)
            {
                return Convert.ToInt32(dt.Rows[0][0].ToString());
            }

            return 0;
        }

        // 插入，修改，删除，是否使用事务
        public static bool UpdateData(out string sError, string sSQL, bool bUseTransaction = false)
        {
            int iResult = 0;
            sError = string.Empty;

            MySqlConnection myConn = null;

            if (!bUseTransaction)
            {
                try
                {
                    myConn = new MySqlConnection(_ConnString);
                    MySqlCommand myCmd = new MySqlCommand(sSQL, myConn);
                    myConn.Open();
                    iResult = myCmd.ExecuteNonQuery();
                    myConn.Close();
                }
                catch (Exception ex)
                {
                    sError = ex.Message;
                    iResult = -1;
                }
            }
            else // 使用事务
            {
                MySqlTransaction myTrans = null;
                try
                {
                    myConn = new MySqlConnection(_ConnString);
                    myConn.Open();
                    myTrans = myConn.BeginTransaction();
                    MySqlCommand myCmd = new MySqlCommand(sSQL, myConn);
                    myCmd.Transaction = myTrans;
                    iResult = myCmd.ExecuteNonQuery();
                    myTrans.Commit();
                    myConn.Close();
                }
                catch (Exception ex)
                {
                    sError = ex.Message;
                    iResult = -1;
                    myTrans.Rollback();
                }
            }

            return iResult > 0;
        }

        // 统计指定范围内的行数
        public static int CountRowsInRange(DataTable dataTable, string columnName, double minValue, double maxValue)
        {
            int count = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                if (row[columnName] != DBNull.Value) // 检查列值是否为 null
                {
                    double value = Convert.ToDouble(row[columnName]);
                    if (value >= minValue && value <= maxValue)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        // 统计符合条件瑕疵数
        public static int CountMatchingCriteria(DataTable dataTable, string column1, string column2, double minValue, double maxValue, string targetString)
        {
            int count = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                if (row[column1] != DBNull.Value && row[column2] != DBNull.Value)
                {
                    double value = Convert.ToDouble(row[column2]);
                    string stringValue = row[column1].ToString();
                    if (value > minValue && value <= maxValue && stringValue == targetString)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        public static double SumMatchingCriteria(DataTable dataTable, string column1, string column2, double minValue, double maxValue, string targetString)
        {
            double sum = 0.0;

            foreach (DataRow row in dataTable.Rows)
            {
                if (row[column1] != DBNull.Value && row[column2] != DBNull.Value)
                {
                    double value = Convert.ToDouble(row[column2]);
                    string stringValue = row[column1].ToString();
                    if (value > minValue && value <= maxValue && stringValue == targetString)
                    {
                        sum += value;
                    }
                }
            }
            return sum;
        }
        // 筛选符合条件的行并获取最大值
        public static double GetMaxValueMatchingCriteria(DataTable dataTable, string column1, string column2, string targetString)
        {
            double maxColumnValue = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                if (row[column1] != DBNull.Value && row[column2] != DBNull.Value)
                {
                    double value = Convert.ToDouble(row[column2]); 
                    string stringValue = row[column1].ToString();
                    if (stringValue == targetString && value > maxColumnValue)
                    {
                        maxColumnValue = value;
                    }
                }
            }

            if (maxColumnValue == int.MinValue)
            {
                // 如果没有符合条件的行，返回一个合适的默认值或抛出异常
                throw new Exception("没有符合条件的行");
            }

            return maxColumnValue;
        }

        // 比较坐标值与两列值差的绝对值是否在1以内
        public static bool IsCoordinateWithinOne(DataTable dataTable, string columnX, string columnY, double x, double y)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                if (row[columnX] != DBNull.Value && row[columnY] != DBNull.Value)
                {
                    double valueX = Convert.ToDouble(row[columnX]);
                    double valueY = Convert.ToDouble(row[columnY]);

                    double deltaX = Math.Abs(valueX - x);
                    double deltaY = Math.Abs(valueY - y);

                    if (deltaX <= 1.0 && deltaY <= 1.0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // 比较坐标并更新 MySQL 数据
        public static bool CompareCoordinatesAndUpMySQL(DataTable dataTable, string columnX, string columnY, double Xx, double Yy, int pos_x, int pos_y)
        {
            int closestRowIndex = -1;
            double minDistance = 1;

            foreach (DataRow row in dataTable.Rows)
            {
                if (row[columnX] != DBNull.Value && row[columnY] != DBNull.Value)
                {
                    double valueX = Convert.ToDouble(row[columnX]);
                    double valueY = Convert.ToDouble(row[columnY]);

                    // 计算绝对值差
                    double deltaX = Math.Abs(valueX - Xx);
                    double deltaY = Math.Abs(valueY - Yy);

                    if (deltaX < minDistance && deltaY < minDistance)
                    {
                        closestRowIndex = dataTable.Rows.IndexOf(row);
                    }
                }
            }

            if (closestRowIndex != -1)
            {
                double tray_X = Convert.ToDouble(dataTable.Rows[closestRowIndex][0]);
                double tray_Y = Convert.ToDouble(dataTable.Rows[closestRowIndex][1]);
                // 执行更新操作
                string newData = "UPDATE defection SET defectionType = '针孔' WHERE qrcode='" + Global.qrCode + "' and detectiontime='" + Global.detectiontime + "' and posX='" + pos_x + "' and posY='" + pos_y + "' and trayX='" + tray_X + "' and trayY='" + tray_Y + "' and upordown='" + 1 + "'"; // 替换为要更新的新数据
                UpdateData(out err_2, newData);
                
                return true;
            }
            return false;
        }

    }
}