using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndustryDemo.Controllerui
{
    class DefectionClass
    {
        int pici;
        int xiangjibianhao;
        string tuxiangweizhi;
        string xiacileixing;
        float xiaciweizhiX;
        float xiaciweizhiY;

        public DefectionClass(int pici, int xiangjibianhao, string tuxiangweizhi, string xiacileixing, float xiaciweizhiX, float xiaciweizhiY )
        {
            this.pici = pici;
            this.xiangjibianhao = xiangjibianhao;
            this.tuxiangweizhi = tuxiangweizhi;
            this.xiacileixing = xiacileixing;
            this.xiaciweizhiX = xiaciweizhiX;
            this.xiaciweizhiY = xiaciweizhiY;
        }

        public string ToSql()
        {
            string sql = "insert into table1";
            sql += " (批次,相机编号,图像位置,瑕疵类型,瑕疵位置X,瑕疵位置Y) ";
            sql += "values (";
            sql += pici.ToString() + ",";
            sql += xiangjibianhao.ToString() + ",";
            sql += "'" + tuxiangweizhi + "',";
            sql += "'" + xiacileixing + "',";
            sql += xiaciweizhiX.ToString() + ",";
            sql += xiaciweizhiY.ToString() + ")";
            return sql;
        }
    }
}
