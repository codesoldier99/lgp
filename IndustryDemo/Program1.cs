using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.UserSkins;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using HalconDotNet;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace IndustryDemo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new mainForm());
        }
    }
    /*
    // DetectionUnderRingLightSource类：环形光源条件下，检测针孔、划痕、擦伤、道子。    

    public partial class DetectionUnderRingLightSource
    {
#if !NO_EXPORT_MAIN
        // Main procedure 
        public HObject ImageProcess(HObject ho_Image, out HObject ho_Pinhole, out string hv_DefectTypeCoordinate)
        {
            // Local iconic variables 
            HObject ho_Regions, ho_ConnectedRegions;
            HObject ho_SelectedRegions, ho_SelectedRegions1, ho_RegionFillUp;
            HObject ho_RegionDilation, ho_RegionErosion, ho_RegionUnion;
            HObject ho_ImageReduced, ho_ImageMean, ho_DarkPixels, ho_RegionDilation1;
            HObject ho_RegionUnion1, ho_RegionDifference1, ho_ImageReduced1;
            HObject ho_Edges2, ho_UnionContours, ho_SelectedContours;
            HObject ho_Bruise, ho_Region, ho_RegionDifference;
            HObject ho_ImageReduced2;
            //HObject ho_Pinhole;

            // Local control variables 
            HTuple hv_WindowHandle = new HTuple(), hv_BruiseNumber = new HTuple(), hv_PinholeNumber = new HTuple();
            HTuple hv_Area_pinhole = new HTuple(), hv_Row_pinhole = new HTuple(), hv_Column_pinhole = new HTuple();
            HTuple hv_Area_bruise = new HTuple(), hv_Row_bruise = new HTuple(), hv_Column_bruise = new HTuple(), hv_PointOrder = new HTuple();

            hv_DefectTypeCoordinate = "";

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_ImageMean);
            HOperatorSet.GenEmptyObj(out ho_DarkPixels);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation1);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion1);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference1);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced1);
            HOperatorSet.GenEmptyObj(out ho_Edges2);
            HOperatorSet.GenEmptyObj(out ho_UnionContours);
            HOperatorSet.GenEmptyObj(out ho_SelectedContours);
            HOperatorSet.GenEmptyObj(out ho_Bruise);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced2);
            HOperatorSet.GenEmptyObj(out ho_Pinhole);

            //**************** 滤光片和料盘分割 ***********************
            //灰度直方图，阈值根据滤光片实际情况进行设置
            ho_Regions.Dispose();
            HOperatorSet.Threshold(ho_Image, out ho_Regions, 0, 30);
            //打散连通域
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_Regions, out ho_ConnectedRegions);
            //使用面积特征选择感兴趣区域：阈值可考虑两帧图像滤光片区域重叠大小设置
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area", "and", 500000, 1e+007);
            //select_shape (SelectedRegions, SelectedRegions1, 'rectangularity', 'and', 0, 0.98016)
            //填充空白区域
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_SelectedRegions, out ho_RegionFillUp);
            //膨胀：填充滤光片边缘缺角
            ho_RegionDilation.Dispose();
            HOperatorSet.DilationCircle(ho_RegionFillUp, out ho_RegionDilation, 40);
            //腐蚀：去除滤光片边缘凹槽区域或边缘光照不均区域
            ho_RegionErosion.Dispose();
            HOperatorSet.ErosionCircle(ho_RegionDilation, out ho_RegionErosion, 80);
            //合并连通域
            ho_RegionUnion.Dispose();
            HOperatorSet.Union1(ho_RegionErosion, out ho_RegionUnion);
            //获得滤光片区域图像
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_Image, ho_RegionUnion, out ho_ImageReduced);

            //******************* 环光情况下的气泡提取、去除 ****************************
            //均值滤波，配合dyn_threshold做动态阈值分割
            ho_ImageMean.Dispose();
            HOperatorSet.MeanImage(ho_ImageReduced, out ho_ImageMean, 15, 15);
            ho_DarkPixels.Dispose();
            HOperatorSet.DynThreshold(ho_ImageReduced, ho_ImageMean, out ho_DarkPixels, 10, "light");
            //打散连通区域
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_DarkPixels, out ho_ConnectedRegions);
            //特征选择：使用面积、圆度、孔洞面积三个特征  （考虑outer_radius）
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, ((new HTuple("area")).TupleConcat(
                "roundness")).TupleConcat("area_holes"), "and", ((new HTuple(500)).TupleConcat(
                0.7)).TupleConcat(150), ((new HTuple(2000)).TupleConcat(1)).TupleConcat(2000));
            //将气泡孔洞填充
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_SelectedRegions, out ho_RegionFillUp);
            //膨胀，多取气泡外围一圈，避免干扰
            ho_RegionDilation1.Dispose();
            HOperatorSet.DilationCircle(ho_RegionFillUp, out ho_RegionDilation1, 20);
            //形成连通域
            ho_RegionUnion1.Dispose();
            HOperatorSet.Union1(ho_RegionDilation1, out ho_RegionUnion1);
            //做减法，ImageReduced - RegionUnion1（气泡区域）
            ho_RegionDifference1.Dispose();
            HOperatorSet.Difference(ho_ImageReduced, ho_RegionUnion1, out ho_RegionDifference1);
            //获取去除气泡的区域，用于划伤等缺陷的检测
            ho_ImageReduced1.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageReduced, ho_RegionDifference1, out ho_ImageReduced1);

            //******************* 划伤、划痕、擦伤、道子检测 ****************************
            //亚像素提取边缘 edges_sub_pix(Image : Edges : Filter, Alpha, Low, High : )
            //*** 'canny'算子能够提取十分轻微的划痕，'canny_junctions'次之
            //Alpha数值越大,平滑越强大,会减少边缘细节
            //Low 越小，越能提取浅划痕
            ho_Edges2.Dispose();
            HOperatorSet.EdgesSubPix(ho_ImageReduced1, out ho_Edges2, "canny", 0.9, 3, 5);
            //连接端点相近的轮廓
            ho_UnionContours.Dispose();
            HOperatorSet.UnionAdjacentContoursXld(ho_Edges2, out ho_UnionContours, 30, 1, "attr_keep");
            //连接近似共线轮廓
            //union_collinear_contours_xld (Edges2, UnionContours, 30, 1, 2, 0.7, 'attr_keep')
            //使用最大直径特征，去除针孔、灰尘等小点
            ho_SelectedContours.Dispose();
            HOperatorSet.SelectShapeXld(ho_UnionContours, out ho_SelectedContours, "max_diameter", "and", 30, 99999);
            //使用圆度特征，为了去除针孔瑕疵
            ho_Bruise.Dispose();
            HOperatorSet.SelectShapeXld(ho_SelectedContours, out ho_Bruise, "circularity", "and", 0, 0.4);

            //********************* 针孔检测（灰尘仍无法识别）  ****************************
            //xld 转化为 region
            ho_Region.Dispose();
            HOperatorSet.GenRegionContourXld(ho_Bruise, out ho_Region, "filled");
            //膨胀，将整块擦伤/道子/划痕区域提取出来
            //注意阈值不能太小，否则擦伤/道子/划痕区域提取不完全，会对后续其他缺陷检测造成影响
            ho_RegionDilation.Dispose();
            HOperatorSet.DilationRectangle1(ho_Region, out ho_RegionDilation, 55, 55);
            //合并，形成同一个连通域
            ho_RegionUnion.Dispose();
            HOperatorSet.Union1(ho_RegionDilation, out ho_RegionUnion);
            //区域相减：减去擦伤/道子/划痕区域，获得的区域后续进行针孔检测
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_ImageReduced1, ho_RegionUnion, out ho_RegionDifference);
            ho_ImageReduced2.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageReduced1, ho_RegionDifference, out ho_ImageReduced2);

            ho_ImageMean.Dispose();
            HOperatorSet.MeanImage(ho_ImageReduced2, out ho_ImageMean, 100, 100);
            ho_DarkPixels.Dispose();
            HOperatorSet.DynThreshold(ho_ImageReduced2, ho_ImageMean, out ho_DarkPixels, 10, "light");
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_DarkPixels, out ho_ConnectedRegions);
            //面积特征
            ho_SelectedRegions1.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions1, "area", "and", 10, 650);
            //最大直径特征去除道子
            ho_Pinhole.Dispose();
            HOperatorSet.SelectShape(ho_SelectedRegions1, out ho_Pinhole, "max_diameter", "and", 0, 50);

            HOperatorSet.CountObj(ho_Bruise, out hv_BruiseNumber);
            HOperatorSet.CountObj(ho_Pinhole, out hv_PinholeNumber);
            //获取划痕坐标
            if ((int)(new HTuple(hv_BruiseNumber.TupleGreater(0))) != 0)
            {
                HOperatorSet.AreaCenterXld(ho_Bruise, out hv_Area_bruise, out hv_Row_bruise, out hv_Column_bruise, out hv_PointOrder);
                hv_DefectTypeCoordinate = "瑕疵类型：划伤(红)" + "\r\nX轴坐标：" + hv_Row_bruise.ToString() + "\r\nY轴坐标：" + hv_Column_bruise.ToString() + "\r\n";
            }
            //获取针孔坐标
            if ((int)(new HTuple(hv_PinholeNumber.TupleGreater(0))) != 0)
            {
                HOperatorSet.AreaCenter(ho_Pinhole, out hv_Area_pinhole, out hv_Row_pinhole, out hv_Column_pinhole);
                hv_DefectTypeCoordinate = hv_DefectTypeCoordinate + "瑕疵类型：针孔（蓝）" + "\r\nX轴坐标：" + hv_Row_pinhole.ToString() + "\r\nY轴坐标：" + hv_Column_pinhole.ToString();
            }
            // 合格品
            if ((int)((new HTuple(hv_BruiseNumber.TupleEqual(0))).TupleAnd(new HTuple(hv_PinholeNumber.TupleEqual(0)))) != 0)
            {
                hv_DefectTypeCoordinate = "合格";
            }

            return ho_Bruise;
        }
#endif
    }

    // DetectionUnderSpotLightSource类：点光源条件下，检测气泡、布毛、麻点。
    public partial class DetectionUnderSpotLightSource
    {

        public HObject ImageProcess(HObject ho_Image, out HObject ho_bumaoDefects, out HObject ho_spotDefects, out string hv_DefectTypeCoordinate)
        {
            // Local iconic variables 
            HObject ho_Regions, ho_ConnectedRegions;
            HObject ho_SelectedRegions, ho_bubbleDefects, ho_RegionFillUp;
            HObject ho_RegionDilation, ho_RegionErosion, ho_RegionUnion;
            HObject ho_ImageReduced, ho_ImageMean, ho_DarkPixels, ho_RegionDifference;
            HObject ho_ImageReduced2, ho_RegionClosing, ho_ImageReduced3;
            HObject ho_Regions2, ho_ConnectedRegions3;
            HObject ho_RegionDilation1, ho_RegionDifference1, ho_SelectedRegions3;
            HObject ho_SelectedRegions4;

            // Local control variables 
            HTuple hv_WindowHandle = new HTuple(), hv_bubbleNumber = new HTuple();
            HTuple hv_bumaoNumber = new HTuple(), hv_spotNumber = new HTuple();
            HTuple hv_messages = new HTuple(), hv_Area_bubble = new HTuple();
            HTuple hv_Row_bubble = new HTuple(), hv_Column_bubble = new HTuple();
            HTuple hv_Area_bumao = new HTuple(), hv_Row_bumao = new HTuple();
            HTuple hv_Column_bumao = new HTuple(), hv_Area_spot = new HTuple();
            HTuple hv_Row_spot = new HTuple(), hv_Column_spot = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_bubbleDefects);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_ImageMean);
            HOperatorSet.GenEmptyObj(out ho_DarkPixels);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced2);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced3);
            HOperatorSet.GenEmptyObj(out ho_Regions2);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions3);
            HOperatorSet.GenEmptyObj(out ho_bumaoDefects);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation1);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions3);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions4);
            HOperatorSet.GenEmptyObj(out ho_spotDefects);

            hv_DefectTypeCoordinate = "";

            //**************** 滤光片和料盘分割 ***********************
            //灰度直方图，阈值根据滤光片实际情况进行设置
            ho_Regions.Dispose();
            HOperatorSet.Threshold(ho_Image, out ho_Regions, 60, 210);
            //打散连通域
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_Regions, out ho_ConnectedRegions);
            //使用面积特征选择感兴趣区域：阈值可考虑两帧图像滤光片区域重叠大小设置
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area", "and", 500000, 1e+007);
            //select_shape (SelectedRegions, bubbleDefects, 'rectangularity', 'and', 0, 0.98016)
            //填充空白区域
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_SelectedRegions, out ho_RegionFillUp);
            //膨胀：填充滤光片边缘缺角
            ho_RegionDilation.Dispose();
            HOperatorSet.DilationCircle(ho_RegionFillUp, out ho_RegionDilation, 40);
            //腐蚀：去除滤光片边缘凹槽区域或边缘光照不均区域。若滤光片边缘白边仍无法消除，则继续加大阈值
            ho_RegionErosion.Dispose();
            HOperatorSet.ErosionCircle(ho_RegionDilation, out ho_RegionErosion, 150);
            //合并连通域
            ho_RegionUnion.Dispose();
            HOperatorSet.Union1(ho_RegionErosion, out ho_RegionUnion);
            //获得滤光片区域图像
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_Image, ho_RegionUnion, out ho_ImageReduced);

            //********************** 先检测气泡***************************
            //均值滤波，配合dyn_threshold做动态阈值分割
            ho_ImageMean.Dispose();
            HOperatorSet.MeanImage(ho_ImageReduced, out ho_ImageMean, 10, 10);
            ho_DarkPixels.Dispose();
            HOperatorSet.DynThreshold(ho_ImageReduced, ho_ImageMean, out ho_DarkPixels, 10, "dark");
            //打散连通区域
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_DarkPixels, out ho_ConnectedRegions);
            //特征选择：使用面积'area'：环的面积、圆度'roundness'、环形特征'area_holes':
            ho_bubbleDefects.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_bubbleDefects, ((new HTuple("area")).TupleConcat(
                "roundness")).TupleConcat("area_holes"), "and", ((new HTuple(100)).TupleConcat(
                0.8)).TupleConcat(50), ((new HTuple(400)).TupleConcat(1)).TupleConcat(9999));
            //shape_trans (ConnectedRegions, RegionTrans, 'convex')
            //select_shape (ConnectedRegions, SelectedRegions, ['area','roundness'], 'and', [250,0.90], [800,1])

            //若气泡存在，先去除气泡
            //********************** 再检测布毛 ***************************
            //利用外接圆进行填充
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUpShape(ho_bubbleDefects, out ho_RegionFillUp, "outer_circle", 1, 10);
            //膨胀
            ho_RegionDilation.Dispose();
            HOperatorSet.DilationCircle(ho_RegionFillUp, out ho_RegionDilation, 20);
            //区域相减：减去气泡区域
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_ImageReduced, ho_RegionDilation, out ho_RegionDifference);
            ho_ImageReduced2.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageReduced, ho_RegionDifference, out ho_ImageReduced2);
            //通过动态阈值分割锁定布毛区域
            ho_ImageMean.Dispose();
            HOperatorSet.MeanImage(ho_ImageReduced2, out ho_ImageMean, 50, 50);
            ho_DarkPixels.Dispose();
            HOperatorSet.DynThreshold(ho_ImageReduced2, ho_ImageMean, out ho_DarkPixels, 10, "light");
            ho_RegionClosing.Dispose();
            HOperatorSet.Closing(ho_DarkPixels, ho_DarkPixels, out ho_RegionClosing);
            ho_ImageReduced3.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageReduced2, ho_RegionClosing, out ho_ImageReduced3);
            //凹布毛亮度不够，最低阈值设置150；针对凸布毛，最低阈值可以进一步提高
            ho_Regions2.Dispose();
            HOperatorSet.Threshold(ho_ImageReduced3, out ho_Regions2, 155, 255);
            ho_ConnectedRegions3.Dispose();
            HOperatorSet.Connection(ho_Regions2, out ho_ConnectedRegions3);
            ho_bumaoDefects.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions3, out ho_bumaoDefects, (new HTuple("area")).TupleConcat(
                "roundness"), "and", (new HTuple(50)).TupleConcat(-1), (new HTuple(200000)).TupleConcat(0.67));

            //************** 麻点提取（未解决：会把针孔和灰尘当做麻点）****************

            ho_RegionDilation1.Dispose();
            HOperatorSet.DilationCircle(ho_bumaoDefects, out ho_RegionDilation1, 100);
            ho_RegionDifference1.Dispose();
            HOperatorSet.Difference(ho_ImageReduced2, ho_RegionDilation1, out ho_RegionDifference1);
            ho_ImageReduced3.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageReduced2, ho_RegionDifference1, out ho_ImageReduced3);

            ho_ImageMean.Dispose();
            HOperatorSet.MeanImage(ho_ImageReduced3, out ho_ImageMean, 10, 10);
            ho_DarkPixels.Dispose();
            HOperatorSet.DynThreshold(ho_ImageReduced3, ho_ImageMean, out ho_DarkPixels, 10, "dark");
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_DarkPixels, out ho_ConnectedRegions);

            ho_SelectedRegions3.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions3, (new HTuple("area")).TupleConcat(
                "area_holes"), "and", (new HTuple(10)).TupleConcat(0), (new HTuple(130)).TupleConcat(0.0001));
            ho_SelectedRegions4.Dispose();
            HOperatorSet.SelectShape(ho_SelectedRegions3, out ho_SelectedRegions4, "max_diameter", "and", 0, 20);
            ho_spotDefects.Dispose();
            HOperatorSet.SelectShape(ho_SelectedRegions4, out ho_spotDefects, "roundness", "and", 0.5, 1);


            HOperatorSet.CountObj(ho_bubbleDefects, out hv_bubbleNumber);
            HOperatorSet.CountObj(ho_bumaoDefects, out hv_bumaoNumber);
            HOperatorSet.CountObj(ho_spotDefects, out hv_spotNumber);
            // 获取气泡坐标
            if ((int)(new HTuple(hv_bubbleNumber.TupleGreater(0))) != 0)
            {
                HOperatorSet.AreaCenter(ho_bubbleDefects, out hv_Area_bubble, out hv_Row_bubble, out hv_Column_bubble);
                hv_DefectTypeCoordinate = "瑕疵类型：气泡(红)" + "\r\nX轴坐标：" + hv_Row_bubble.ToString() + "\r\nY轴坐标：" + hv_Column_bubble.ToString() + "\r\n";

            }
            // 获取布毛坐标
            if ((int)(new HTuple(hv_bumaoNumber.TupleGreater(0))) != 0)
            {
                HOperatorSet.AreaCenter(ho_bumaoDefects, out hv_Area_bumao, out hv_Row_bumao, out hv_Column_bumao);
                hv_DefectTypeCoordinate = hv_DefectTypeCoordinate + "瑕疵类型：布毛(蓝)" + "\r\nX轴坐标：" + hv_Row_bumao.ToString() + "\r\nY轴坐标：" + hv_Column_bumao.ToString() + "\r\n";
            }
            // 获取麻点坐标
            if ((int)(new HTuple(hv_spotNumber.TupleGreater(0))) != 0)
            {
                HOperatorSet.AreaCenter(ho_spotDefects, out hv_Area_spot, out hv_Row_spot, out hv_Column_spot);
                hv_DefectTypeCoordinate = hv_DefectTypeCoordinate + "瑕疵类型：麻点(绿)" + "\r\nX轴坐标：" + hv_Row_spot.ToString() + "\r\nY轴坐标：" + hv_Column_spot.ToString() + "\r\n";
            }
            if ((int)((new HTuple((new HTuple(hv_bubbleNumber.TupleEqual(0))).TupleAnd(new HTuple(hv_bumaoNumber.TupleEqual(
                0))))).TupleAnd(new HTuple(hv_spotNumber.TupleEqual(0)))) != 0)
            {
                hv_DefectTypeCoordinate = "合格";
            }

            ho_Regions.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_RegionFillUp.Dispose();
            ho_RegionDilation.Dispose();
            ho_RegionErosion.Dispose();
            ho_RegionUnion.Dispose();
            ho_ImageReduced.Dispose();
            ho_ImageMean.Dispose();
            ho_DarkPixels.Dispose();
            ho_RegionDifference.Dispose();
            ho_ImageReduced2.Dispose();
            ho_RegionClosing.Dispose();
            ho_ImageReduced3.Dispose();
            ho_Regions2.Dispose();
            ho_ConnectedRegions3.Dispose();
            ho_RegionDilation1.Dispose();
            ho_RegionDifference1.Dispose();
            ho_SelectedRegions3.Dispose();
            ho_SelectedRegions4.Dispose();
            return ho_bubbleDefects;
        }
    }

    */


    public partial class DetectionWithDL

    {
        int x;
        int y;
        string zx, zy, fileBaseName;
        public int getX() { return x; }

        public int getY() { return y; }

        public string getZx() { return zx; }
        public string getZy() { return zy; }
        public string getFileBaseName() { return fileBaseName; }

        public void parse_filename(HTuple hv_FileName, out HTuple hv_BaseName, out HTuple hv_Extension,
     out HTuple hv_Directory)
        {



            // Local control variables 

            HTuple hv_DirectoryTmp = new HTuple(), hv_Substring = new HTuple();
            // Initialize local and output iconic variables 
            hv_BaseName = new HTuple();
            hv_Extension = new HTuple();
            hv_Directory = new HTuple();
            try
            {
                //This procedure gets a filename (with full path) as input
                //and returns the directory path, the base filename and the extension
                //in three different strings.
                //
                //In the output path the path separators will be replaced
                //by '/' in all cases.
                //
                //The procedure shows the possibilities of regular expressions in HALCON.
                //
                //Input parameters:
                //FileName: The input filename
                //
                //Output parameters:
                //BaseName: The filename without directory description and file extension
                //Extension: The file extension
                //Directory: The directory path
                //
                //Example:
                //basename('C:/images/part_01.png',...) returns
                //BaseName = 'part_01'
                //Extension = 'png'
                //Directory = 'C:\\images\\' (on Windows systems)
                //
                //Explanation of the regular expressions:
                //
                //'([^\\\\/]*?)(?:\\.[^.]*)?$':
                //To start at the end, the '$' matches the end of the string,
                //so it is best to read the expression from right to left.
                //The part in brackets (?:\\.[^.}*) denotes a non-capturing group.
                //That means, that this part is matched, but not captured
                //in contrast to the first bracketed group ([^\\\\/], see below.)
                //\\.[^.]* matches a dot '.' followed by as many non-dots as possible.
                //So (?:\\.[^.]*)? matches the file extension, if any.
                //The '?' at the end assures, that even if no extension exists,
                //a correct match is returned.
                //The first part in brackets ([^\\\\/]*?) is a capture group,
                //which means, that if a match is found, only the part in
                //brackets is returned as a result.
                //Because both HDevelop strings and regular expressions need a '\\'
                //to describe a backslash, inside regular expressions within HDevelop
                //a backslash has to be written as '\\\\'.
                //[^\\\\/] matches any character but a slash or backslash ('\\' in HDevelop)
                //[^\\\\/]*? matches a string od 0..n characters (except '/' or '\\')
                //where the '?' after the '*' switches the greediness off,
                //that means, that the shortest possible match is returned.
                //This option is necessary to cut off the extension
                //but only if (?:\\.[^.]*)? is able to match one.
                //To summarize, the regular expression matches that part of
                //the input string, that follows after the last '/' or '\\' and
                //cuts off the extension (if any) after the last '.'.
                //
                //'\\.([^.]*)$':
                //This matches everything after the last '.' of the input string.
                //Because ([^.]) is a capturing group,
                //only the part after the dot is returned.
                //
                //'.*[\\\\/]':
                //This matches the longest substring with a '/' or a '\\' at the end.
                //
                hv_DirectoryTmp.Dispose();
                HOperatorSet.TupleRegexpMatch(hv_FileName, ".*[\\\\/]", out hv_DirectoryTmp);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Substring.Dispose();
                    HOperatorSet.TupleSubstr(hv_FileName, hv_DirectoryTmp.TupleStrlen(), (hv_FileName.TupleStrlen()
                        ) - 1, out hv_Substring);
                }
                hv_BaseName.Dispose();
                HOperatorSet.TupleRegexpMatch(hv_Substring, "([^\\\\/]*?)(?:\\.[^.]*)?$", out hv_BaseName);
                hv_Extension.Dispose();
                HOperatorSet.TupleRegexpMatch(hv_Substring, "\\.([^.]*)$", out hv_Extension);
                //
                //
                //Finally all found backslashes ('\\') are converted
                //to a slash to get consistent paths
                hv_Directory.Dispose();
                HOperatorSet.TupleRegexpReplace(hv_DirectoryTmp, (new HTuple("\\\\")).TupleConcat(
                    "replace_all"), "/", out hv_Directory);

                hv_DirectoryTmp.Dispose();
                hv_Substring.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_DirectoryTmp.Dispose();
                hv_Substring.Dispose();

                throw HDevExpDefaultException;
            }
        }





        public void check_dl_preprocess_param(HTuple hv_DLPreprocessParam)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_DLModelType = new HTuple(), hv_Exception = new HTuple();
            HTuple hv_ParamNamesGeneral = new HTuple(), hv_ParamNamesSegmentation = new HTuple();
            HTuple hv_ParamNamesAll = new HTuple(), hv_ParamNames = new HTuple();
            HTuple hv_KeysExists = new HTuple(), hv_I = new HTuple();
            HTuple hv_Exists = new HTuple(), hv_InputKeys = new HTuple();
            HTuple hv_Key = new HTuple(), hv_Value = new HTuple();
            HTuple hv_Indices = new HTuple(), hv_ValidValues = new HTuple();
            HTuple hv_ValidTypes = new HTuple(), hv_V = new HTuple();
            HTuple hv_T = new HTuple(), hv_IsInt = new HTuple(), hv_ValidTypesListing = new HTuple();
            HTuple hv_Index = new HTuple(), hv_ValidValueListing = new HTuple();
            HTuple hv_SetBackgroundID = new HTuple(), hv_ClassIDsBackground = new HTuple();
            HTuple hv_Intersection = new HTuple(), hv_IgnoreClassIDs = new HTuple();
            HTuple hv_KnownClasses = new HTuple(), hv_IgnoreClassID = new HTuple();
            HTuple hv_IndexParam = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //
                //This procedure checks a dictionary with parameters for DL preprocessing.
                //
                try
                {
                    hv_DLModelType.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "model_type", out hv_DLModelType);
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    throw new HalconException(new HTuple(new HTuple("DLPreprocessParam needs the parameter: '") + "model_type") + "'");
                }
                //
                //Parameter names that are required.
                //General parameters.
                hv_ParamNamesGeneral.Dispose();
                hv_ParamNamesGeneral = new HTuple();
                hv_ParamNamesGeneral[0] = "model_type";
                hv_ParamNamesGeneral[1] = "image_width";
                hv_ParamNamesGeneral[2] = "image_height";
                hv_ParamNamesGeneral[3] = "image_num_channels";
                hv_ParamNamesGeneral[4] = "image_range_min";
                hv_ParamNamesGeneral[5] = "image_range_max";
                hv_ParamNamesGeneral[6] = "contrast_normalization";
                hv_ParamNamesGeneral[7] = "domain_handling";
                //Segmentation specific parameters.
                hv_ParamNamesSegmentation.Dispose();
                hv_ParamNamesSegmentation = new HTuple();
                hv_ParamNamesSegmentation[0] = "ignore_class_ids";
                hv_ParamNamesSegmentation[1] = "set_background_id";
                hv_ParamNamesSegmentation[2] = "class_ids_background";
                hv_ParamNamesAll.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_ParamNamesAll = new HTuple();
                    hv_ParamNamesAll = hv_ParamNamesAll.TupleConcat(hv_ParamNamesGeneral, hv_ParamNamesSegmentation);
                }
                if ((int)(new HTuple(hv_DLModelType.TupleEqual("segmentation"))) != 0)
                {
                    hv_ParamNames.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ParamNames = new HTuple();
                        hv_ParamNames = hv_ParamNames.TupleConcat(hv_ParamNamesGeneral, hv_ParamNamesSegmentation);
                    }
                }
                else
                {
                    hv_ParamNames.Dispose();
                    hv_ParamNames = new HTuple(hv_ParamNamesGeneral);
                }
                //
                //Check that all necessary parameters are included.
                hv_KeysExists.Dispose();
                HOperatorSet.GetDictParam(hv_DLPreprocessParam, "key_exists", hv_ParamNames,
                    out hv_KeysExists);
                if ((int)(new HTuple(((((hv_KeysExists.TupleEqualElem(0))).TupleSum())).TupleGreater(
                    0))) != 0)
                {
                    for (hv_I = 0; (int)hv_I <= (int)(new HTuple(hv_KeysExists.TupleLength())); hv_I = (int)hv_I + 1)
                    {
                        hv_Exists.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_Exists = hv_KeysExists.TupleSelect(
                                hv_I);
                        }
                        if ((int)(hv_Exists.TupleNot()) != 0)
                        {
                            throw new HalconException(("DLPreprocessParam needs the parameter: '" + (hv_ParamNames.TupleSelect(
                                hv_I))) + "'");
                        }
                    }
                }
                //
                //Check the keys provided.
                hv_InputKeys.Dispose();
                HOperatorSet.GetDictParam(hv_DLPreprocessParam, "keys", new HTuple(), out hv_InputKeys);
                for (hv_I = 0; (int)hv_I <= (int)((new HTuple(hv_InputKeys.TupleLength())) - 1); hv_I = (int)hv_I + 1)
                {
                    hv_Key.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Key = hv_InputKeys.TupleSelect(
                            hv_I);
                    }
                    hv_Value.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLPreprocessParam, hv_Key, out hv_Value);
                    //Check that the Key is known.
                    hv_Indices.Dispose();
                    HOperatorSet.TupleFind(hv_ParamNamesAll, hv_Key, out hv_Indices);
                    if ((int)(new HTuple(hv_Indices.TupleEqual(-1))) != 0)
                    {
                        throw new HalconException(("The key '" + (hv_InputKeys.TupleSelect(
                            hv_I))) + "' in DLPreprocessParam is unknown.");

                        hv_DLModelType.Dispose();
                        hv_Exception.Dispose();
                        hv_ParamNamesGeneral.Dispose();
                        hv_ParamNamesSegmentation.Dispose();
                        hv_ParamNamesAll.Dispose();
                        hv_ParamNames.Dispose();
                        hv_KeysExists.Dispose();
                        hv_I.Dispose();
                        hv_Exists.Dispose();
                        hv_InputKeys.Dispose();
                        hv_Key.Dispose();
                        hv_Value.Dispose();
                        hv_Indices.Dispose();
                        hv_ValidValues.Dispose();
                        hv_ValidTypes.Dispose();
                        hv_V.Dispose();
                        hv_T.Dispose();
                        hv_IsInt.Dispose();
                        hv_ValidTypesListing.Dispose();
                        hv_Index.Dispose();
                        hv_ValidValueListing.Dispose();
                        hv_SetBackgroundID.Dispose();
                        hv_ClassIDsBackground.Dispose();
                        hv_Intersection.Dispose();
                        hv_IgnoreClassIDs.Dispose();
                        hv_KnownClasses.Dispose();
                        hv_IgnoreClassID.Dispose();
                        hv_IndexParam.Dispose();

                        return;
                    }
                    //Set expected values and types.
                    hv_ValidValues.Dispose();
                    hv_ValidValues = new HTuple();
                    hv_ValidTypes.Dispose();
                    hv_ValidTypes = new HTuple();
                    if ((int)(new HTuple(hv_Key.TupleEqual("contrast_normalization"))) != 0)
                    {
                        hv_ValidValues.Dispose();
                        hv_ValidValues = new HTuple();
                        hv_ValidValues[0] = "true";
                        hv_ValidValues[1] = "false";
                    }
                    else if ((int)(new HTuple(hv_Key.TupleEqual("domain_handling"))) != 0)
                    {
                        hv_ValidValues.Dispose();
                        hv_ValidValues = new HTuple();
                        hv_ValidValues[0] = "full_domain";
                        hv_ValidValues[1] = "crop_domain";
                    }
                    else if ((int)(new HTuple(hv_Key.TupleEqual("model_type"))) != 0)
                    {
                        hv_ValidValues.Dispose();
                        hv_ValidValues = new HTuple();
                        hv_ValidValues[0] = "detection";
                        hv_ValidValues[1] = "segmentation";
                    }
                    else if ((int)(new HTuple(hv_Key.TupleEqual("set_background_id"))) != 0)
                    {
                        hv_ValidTypes.Dispose();
                        hv_ValidTypes = "int";
                    }
                    else if ((int)(new HTuple(hv_Key.TupleEqual("class_ids_background"))) != 0)
                    {
                        hv_ValidTypes.Dispose();
                        hv_ValidTypes = "int";
                    }
                    //Check that type is valid.
                    if ((int)(new HTuple((new HTuple(hv_ValidTypes.TupleLength())).TupleGreater(
                        0))) != 0)
                    {
                        for (hv_V = 0; (int)hv_V <= (int)((new HTuple(hv_ValidTypes.TupleLength())) - 1); hv_V = (int)hv_V + 1)
                        {
                            hv_T.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_T = hv_ValidTypes.TupleSelect(
                                    hv_V);
                            }
                            if ((int)(new HTuple(hv_T.TupleEqual("int"))) != 0)
                            {
                                hv_IsInt.Dispose();
                                HOperatorSet.TupleIsInt(hv_Value, out hv_IsInt);
                                if ((int)(hv_IsInt.TupleNot()) != 0)
                                {
                                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                    {
                                        {
                                            HTuple
                                              ExpTmpLocalVar_ValidTypes = ("'" + hv_ValidTypes) + "'";
                                            hv_ValidTypes.Dispose();
                                            hv_ValidTypes = ExpTmpLocalVar_ValidTypes;
                                        }
                                    }
                                    if ((int)(new HTuple((new HTuple(hv_ValidTypes.TupleLength())).TupleLess(
                                        2))) != 0)
                                    {
                                        hv_ValidTypesListing.Dispose();
                                        hv_ValidTypesListing = new HTuple(hv_ValidTypes);
                                    }
                                    else
                                    {
                                        hv_ValidTypesListing.Dispose();
                                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                        {
                                            hv_ValidTypesListing = ((((hv_ValidTypes.TupleSelectRange(
                                                0, (new HTuple(0)).TupleMax2((new HTuple(hv_ValidTypes.TupleLength()
                                                )) - 2))) + new HTuple(", ")) + (hv_ValidTypes.TupleSelect((new HTuple(hv_ValidTypes.TupleLength()
                                                )) - 1)))).TupleSum();
                                        }
                                    }
                                    throw new HalconException(((((("The value given in the key '" + hv_Key) + "' of DLPreprocessParam is invalid. Valid types are: ") + hv_ValidTypesListing) + ". The given value was '") + hv_Value) + "'.");

                                    hv_DLModelType.Dispose();
                                    hv_Exception.Dispose();
                                    hv_ParamNamesGeneral.Dispose();
                                    hv_ParamNamesSegmentation.Dispose();
                                    hv_ParamNamesAll.Dispose();
                                    hv_ParamNames.Dispose();
                                    hv_KeysExists.Dispose();
                                    hv_I.Dispose();
                                    hv_Exists.Dispose();
                                    hv_InputKeys.Dispose();
                                    hv_Key.Dispose();
                                    hv_Value.Dispose();
                                    hv_Indices.Dispose();
                                    hv_ValidValues.Dispose();
                                    hv_ValidTypes.Dispose();
                                    hv_V.Dispose();
                                    hv_T.Dispose();
                                    hv_IsInt.Dispose();
                                    hv_ValidTypesListing.Dispose();
                                    hv_Index.Dispose();
                                    hv_ValidValueListing.Dispose();
                                    hv_SetBackgroundID.Dispose();
                                    hv_ClassIDsBackground.Dispose();
                                    hv_Intersection.Dispose();
                                    hv_IgnoreClassIDs.Dispose();
                                    hv_KnownClasses.Dispose();
                                    hv_IgnoreClassID.Dispose();
                                    hv_IndexParam.Dispose();

                                    return;
                                }
                            }
                            else
                            {
                                throw new HalconException("Internal error. Unknown valid type.");
                            }
                        }
                    }
                    //Check that value is valid.
                    if ((int)(new HTuple((new HTuple(hv_ValidValues.TupleLength())).TupleGreater(
                        0))) != 0)
                    {
                        hv_Index.Dispose();
                        HOperatorSet.TupleFindFirst(hv_ValidValues, hv_Value, out hv_Index);
                        if ((int)(new HTuple(hv_Index.TupleEqual(-1))) != 0)
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_ValidValues = ("'" + hv_ValidValues) + "'";
                                    hv_ValidValues.Dispose();
                                    hv_ValidValues = ExpTmpLocalVar_ValidValues;
                                }
                            }
                            if ((int)(new HTuple((new HTuple(hv_ValidValues.TupleLength())).TupleLess(
                                2))) != 0)
                            {
                                hv_ValidValueListing.Dispose();
                                hv_ValidValueListing = new HTuple(hv_ValidValues);
                            }
                            else
                            {
                                hv_ValidValueListing.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_ValidValueListing = ((((hv_ValidValues.TupleSelectRange(
                                        0, (new HTuple(0)).TupleMax2((new HTuple(hv_ValidValues.TupleLength()
                                        )) - 2))) + new HTuple(", ")) + (hv_ValidValues.TupleSelect((new HTuple(hv_ValidValues.TupleLength()
                                        )) - 1)))).TupleSum();
                                }
                            }
                            throw new HalconException(((((("The value given in the key '" + hv_Key) + "' of DLPreprocessParam is invalid. Valid values are: ") + hv_ValidValueListing) + ". The given value was '") + hv_Value) + "'.");
                        }
                    }
                }
                //
                //Check segmentation specific parameters.
                if ((int)(new HTuple(hv_DLModelType.TupleEqual("segmentation"))) != 0)
                {
                    //Check 'set_background_id'.
                    hv_SetBackgroundID.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "set_background_id", out hv_SetBackgroundID);
                    if ((int)(new HTuple((new HTuple(hv_SetBackgroundID.TupleLength())).TupleGreater(
                        1))) != 0)
                    {
                        throw new HalconException("Only one class_id as 'set_background_id' allowed.");
                    }
                    //Check 'class_ids_background'.
                    hv_ClassIDsBackground.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "class_ids_background", out hv_ClassIDsBackground);
                    if ((int)((new HTuple((new HTuple((new HTuple(hv_SetBackgroundID.TupleLength()
                        )).TupleGreater(0))).TupleAnd((new HTuple((new HTuple(hv_ClassIDsBackground.TupleLength()
                        )).TupleGreater(0))).TupleNot()))).TupleOr((new HTuple((new HTuple(hv_ClassIDsBackground.TupleLength()
                        )).TupleGreater(0))).TupleAnd((new HTuple((new HTuple(hv_SetBackgroundID.TupleLength()
                        )).TupleGreater(0))).TupleNot()))) != 0)
                    {
                        throw new HalconException("Both keys 'set_background_id' and 'class_ids_background' are required.");
                    }
                    //Check that 'class_ids_background' and 'set_background_id' are disjoint.
                    if ((int)(new HTuple((new HTuple(hv_SetBackgroundID.TupleLength())).TupleGreater(
                        0))) != 0)
                    {
                        hv_Intersection.Dispose();
                        HOperatorSet.TupleIntersection(hv_SetBackgroundID, hv_ClassIDsBackground,
                            out hv_Intersection);
                        if ((int)(new HTuple(hv_Intersection.TupleLength())) != 0)
                        {
                            throw new HalconException("Class IDs in 'set_background_id' and 'class_ids_background' need to be disjoint.");
                        }
                    }
                    //Check 'ignore_class_ids'.
                    hv_IgnoreClassIDs.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "ignore_class_ids", out hv_IgnoreClassIDs);
                    hv_KnownClasses.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_KnownClasses = new HTuple();
                        hv_KnownClasses = hv_KnownClasses.TupleConcat(hv_SetBackgroundID, hv_ClassIDsBackground);
                    }
                    for (hv_I = 0; (int)hv_I <= (int)((new HTuple(hv_IgnoreClassIDs.TupleLength()
                        )) - 1); hv_I = (int)hv_I + 1)
                    {
                        hv_IgnoreClassID.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_IgnoreClassID = hv_IgnoreClassIDs.TupleSelect(
                                hv_I);
                        }
                        hv_Index.Dispose();
                        HOperatorSet.TupleFindFirst(hv_KnownClasses, hv_IgnoreClassID, out hv_Index);
                        if ((int)((new HTuple((new HTuple(hv_Index.TupleLength())).TupleGreater(
                            0))).TupleAnd(new HTuple(hv_Index.TupleNotEqual(-1)))) != 0)
                        {
                            throw new HalconException("The given 'ignore_class_ids' must not be included in the 'class_ids_background' or 'set_background_id'.");
                        }
                    }
                }
                else if ((int)(new HTuple(hv_DLModelType.TupleEqual("detection"))) != 0)
                {
                    //Check if segmentation specific parameters are set.
                    hv_KeysExists.Dispose();
                    HOperatorSet.GetDictParam(hv_DLPreprocessParam, "key_exists", hv_ParamNamesSegmentation,
                        out hv_KeysExists);
                    //If they are present, check that they are [].
                    for (hv_IndexParam = 0; (int)hv_IndexParam <= (int)((new HTuple(hv_ParamNamesSegmentation.TupleLength()
                        )) - 1); hv_IndexParam = (int)hv_IndexParam + 1)
                    {
                        if ((int)(hv_KeysExists.TupleSelect(hv_IndexParam)) != 0)
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_Value.Dispose();
                                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, hv_ParamNamesSegmentation.TupleSelect(
                                    hv_IndexParam), out hv_Value);
                            }
                            if ((int)(new HTuple(hv_Value.TupleNotEqual(new HTuple()))) != 0)
                            {
                                throw new HalconException(((("The preprocessing parameter '" + (hv_ParamNamesSegmentation.TupleSelect(
                                    hv_IndexParam))) + "' was set to ") + hv_Value) + new HTuple(" but for detection it should be set to [], as it is not used for this method."));
                            }
                        }
                    }
                }


                hv_DLModelType.Dispose();
                hv_Exception.Dispose();
                hv_ParamNamesGeneral.Dispose();
                hv_ParamNamesSegmentation.Dispose();
                hv_ParamNamesAll.Dispose();
                hv_ParamNames.Dispose();
                hv_KeysExists.Dispose();
                hv_I.Dispose();
                hv_Exists.Dispose();
                hv_InputKeys.Dispose();
                hv_Key.Dispose();
                hv_Value.Dispose();
                hv_Indices.Dispose();
                hv_ValidValues.Dispose();
                hv_ValidTypes.Dispose();
                hv_V.Dispose();
                hv_T.Dispose();
                hv_IsInt.Dispose();
                hv_ValidTypesListing.Dispose();
                hv_Index.Dispose();
                hv_ValidValueListing.Dispose();
                hv_SetBackgroundID.Dispose();
                hv_ClassIDsBackground.Dispose();
                hv_Intersection.Dispose();
                hv_IgnoreClassIDs.Dispose();
                hv_KnownClasses.Dispose();
                hv_IgnoreClassID.Dispose();
                hv_IndexParam.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_DLModelType.Dispose();
                hv_Exception.Dispose();
                hv_ParamNamesGeneral.Dispose();
                hv_ParamNamesSegmentation.Dispose();
                hv_ParamNamesAll.Dispose();
                hv_ParamNames.Dispose();
                hv_KeysExists.Dispose();
                hv_I.Dispose();
                hv_Exists.Dispose();
                hv_InputKeys.Dispose();
                hv_Key.Dispose();
                hv_Value.Dispose();
                hv_Indices.Dispose();
                hv_ValidValues.Dispose();
                hv_ValidTypes.Dispose();
                hv_V.Dispose();
                hv_T.Dispose();
                hv_IsInt.Dispose();
                hv_ValidTypesListing.Dispose();
                hv_Index.Dispose();
                hv_ValidValueListing.Dispose();
                hv_SetBackgroundID.Dispose();
                hv_ClassIDsBackground.Dispose();
                hv_Intersection.Dispose();
                hv_IgnoreClassIDs.Dispose();
                hv_KnownClasses.Dispose();
                hv_IgnoreClassID.Dispose();
                hv_IndexParam.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Graphics / Output
        // Short Description: Display a map of the confidences. 
        public void dev_display_confidence_regions(HObject ho_ImageConfidence, HTuple hv_DrawTransparency,
            out HTuple hv_Colors)
        {




            // Local iconic variables 

            HObject ho_Region = null;

            // Local control variables 

            HTuple hv_NumColors = new HTuple(), hv_WeightsColorsAlpha = new HTuple();
            HTuple hv_ColorIndex = new HTuple(), hv_Threshold = new HTuple();
            HTuple hv_MinGray = new HTuple(), hv_MaxGray = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Region);
            hv_Colors = new HTuple();
            try
            {
                //
                //This procedure displays a map of the confidences
                //given in ImageConfidence as regions.
                //DrawTransparency determines the alpha value of the colors.
                //The used colors are returned.
                //
                //Define colors.
                hv_NumColors.Dispose();
                hv_NumColors = 20;
                hv_Colors.Dispose();
                get_distinct_colors(hv_NumColors, 0, 0, 100, out hv_Colors);
                hv_WeightsColorsAlpha.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WeightsColorsAlpha = hv_Colors + hv_DrawTransparency;
                }
                hv_ColorIndex.Dispose();
                hv_ColorIndex = 0;
                //
                //Threshold the image according to
                //the number of colors and
                //display resulting regions.
                HTuple end_val15 = hv_NumColors - 1;
                HTuple step_val15 = 1;
                for (hv_ColorIndex = 0; hv_ColorIndex.Continue(end_val15, step_val15); hv_ColorIndex = hv_ColorIndex.TupleAdd(step_val15))
                {
                    hv_Threshold.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Threshold = hv_ColorIndex * (1.0 / hv_NumColors);
                    }
                    hv_MinGray.Dispose();
                    hv_MinGray = new HTuple(hv_Threshold);
                    hv_MaxGray.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_MaxGray = hv_Threshold + (1 / hv_NumColors);
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_Region.Dispose();
                        HOperatorSet.Threshold(ho_ImageConfidence, out ho_Region, hv_Threshold, hv_Threshold + (1.0 / hv_NumColors));
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            HOperatorSet.SetColor(HDevWindowStack.GetActive(), hv_WeightsColorsAlpha.TupleSelect(
                                hv_ColorIndex));
                        }
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispObj(ho_Region, HDevWindowStack.GetActive());
                    }
                }
                ho_Region.Dispose();

                hv_NumColors.Dispose();
                hv_WeightsColorsAlpha.Dispose();
                hv_ColorIndex.Dispose();
                hv_Threshold.Dispose();
                hv_MinGray.Dispose();
                hv_MaxGray.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Region.Dispose();

                hv_NumColors.Dispose();
                hv_WeightsColorsAlpha.Dispose();
                hv_ColorIndex.Dispose();
                hv_Threshold.Dispose();
                hv_MinGray.Dispose();
                hv_MaxGray.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Deep Learning / Model
        // Short Description: Visualize different images, annotations and inference results for a sample. 
        public void dev_display_dl_data(HTuple hv_DLSample, HTuple hv_DLResult, HTuple hv_DLDatasetInfo,
            HTuple hv_KeysForDisplay, HTuple hv_GenParam, HTuple hv_WindowHandleDict)
        {



            // Local iconic variables 

            HObject ho_Image = null, ho_ImageWeight = null;
            HObject ho_ImageConfidence = null, ho_SegmentationImagGroundTruth = null;
            HObject ho_SegmentationImageResult = null, ho_ImageAbsDiff = null;
            HObject ho_DiffRegion = null;

            // Local control variables 

            HTuple hv_ThresholdWidth = new HTuple(), hv_ScaleWindows = new HTuple();
            HTuple hv_Font = new HTuple(), hv_FontSize = new HTuple();
            HTuple hv_LineWidth = new HTuple(), hv_MapTransparency = new HTuple();
            HTuple hv_MapColorBarWidth = new HTuple(), hv_SegMaxWeight = new HTuple();
            HTuple hv_SegDraw = new HTuple(), hv_SegTransparency = new HTuple();
            HTuple hv_SegExcludeClassIDs = new HTuple(), hv_BboxLabelColor = new HTuple();
            HTuple hv_BboxDisplayConfidence = new HTuple(), hv_BboxTextColor = new HTuple();
            HTuple hv_ShowBottomDesc = new HTuple(), hv_ShowLegend = new HTuple();
            HTuple hv_ShowLabels = new HTuple(), hv_GenParamNames = new HTuple();
            HTuple hv_ParamIndex = new HTuple(), hv_GenParamName = new HTuple();
            HTuple hv_GenParamValue = new HTuple(), hv_SampleKeys = new HTuple();
            HTuple hv_ResultKeys = new HTuple(), hv_ImageIDExists = new HTuple();
            HTuple hv_ImageID = new HTuple(), hv_ImageIDString = new HTuple();
            HTuple hv_NeededKeys = new HTuple(), hv_Index = new HTuple();
            HTuple hv_DLDatasetInfoKeys = new HTuple(), hv_ClassNames = new HTuple();
            HTuple hv_ClassIDs = new HTuple(), hv_Colors = new HTuple();
            HTuple hv_ClassesLegend = new HTuple(), hv_PrevWindowCoordinates = new HTuple();
            HTuple hv_Keys = new HTuple(), hv_Exception = new HTuple();
            HTuple hv_MetaInfoIndex = new HTuple(), hv_MetaInfo = new HTuple();
            HTuple hv_FlushValues = new HTuple(), hv_WindowHandleKeys = new HTuple();
            HTuple hv_KeyIndex = new HTuple(), hv_WindowHandles = new HTuple();
            HTuple hv_WindowIndex = new HTuple(), hv_FlushValue = new HTuple();
            HTuple hv_WidthImage = new HTuple(), hv_HeightImage = new HTuple();
            HTuple hv_CurrentWindowHandle = new HTuple(), hv_WindowImageRatio = new HTuple();
            HTuple hv_BboxIDs = new HTuple(), hv_Text = new HTuple();
            HTuple hv_BboxColors = new HTuple(), hv_BboxIDsUniq = new HTuple();
            HTuple hv_BboxConfidences = new HTuple(), hv_TextConf = new HTuple();
            HTuple hv_BboxClassIndex = new HTuple(), hv_BboxColorsResults = new HTuple();
            HTuple hv_BboxClassIndexUniq = new HTuple(), hv_BboxLabelIndex = new HTuple();
            HTuple hv_BboxColorsBoth = new HTuple(), hv_BboxClassLabelIndexUniq = new HTuple();
            HTuple hv_ColorsSegmentation = new HTuple(), hv_DrawMode = new HTuple();
            HTuple hv_Width = new HTuple(), hv_ImageClassIDs = new HTuple();
            HTuple hv_ImageClassIDsUniq = new HTuple(), hv_ColorsResults = new HTuple();
            HTuple hv_GroundTruthIDs = new HTuple(), hv_ResultIDs = new HTuple();
            HTuple hv_StringSegExcludeClassIDs = new HTuple(), hv_StringIndex = new HTuple();
            HTuple hv_Min = new HTuple(), hv_Max = new HTuple(), hv_Range = new HTuple();
            HTuple hv_MinWeight = new HTuple(), hv_WeightsColors = new HTuple();
            HTuple hv_ConfidenceColors = new HTuple(), hv_WindowHandleKeysNew = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_ImageWeight);
            HOperatorSet.GenEmptyObj(out ho_ImageConfidence);
            HOperatorSet.GenEmptyObj(out ho_SegmentationImagGroundTruth);
            HOperatorSet.GenEmptyObj(out ho_SegmentationImageResult);
            HOperatorSet.GenEmptyObj(out ho_ImageAbsDiff);
            HOperatorSet.GenEmptyObj(out ho_DiffRegion);
            try
            {
                //
                //This procedure displays the content of the provided DLSample and/or DLResult
                //depending on the input string KeysForDisplay.
                //DLDatasetInfo is a dictionary containing the information about the dataset.
                //The visualization can be adapted with GenParam.
                //
                //** Set the default values: ***
                //
                //Define the screen width when a new window row is started.
                hv_ThresholdWidth.Dispose();
                hv_ThresholdWidth = 1024;
                //Since potentially a lot of windows are opened,
                //scale the windows consistently.
                hv_ScaleWindows.Dispose();
                hv_ScaleWindows = 0.8;
                //Set a font and a font size.
                hv_Font.Dispose();
                hv_Font = "mono";
                hv_FontSize.Dispose();
                hv_FontSize = 14;
                //
                hv_LineWidth.Dispose();
                hv_LineWidth = 2;
                hv_MapTransparency.Dispose();
                hv_MapTransparency = "cc";
                hv_MapColorBarWidth.Dispose();
                hv_MapColorBarWidth = 140;
                //
                //Define segmentation-specific parameter values.
                hv_SegMaxWeight.Dispose();
                hv_SegMaxWeight = 0;
                hv_SegDraw.Dispose();
                hv_SegDraw = "fill";
                hv_SegTransparency.Dispose();
                hv_SegTransparency = "aa";
                hv_SegExcludeClassIDs.Dispose();
                hv_SegExcludeClassIDs = new HTuple();
                //
                //Define bbox-specific parameter values.
                hv_BboxLabelColor.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_BboxLabelColor = new HTuple("#000000") + "99";
                }
                hv_BboxDisplayConfidence.Dispose();
                hv_BboxDisplayConfidence = 1;
                hv_BboxTextColor.Dispose();
                hv_BboxTextColor = "#eeeeee";
                //
                //By default, display a description on the bottom.
                hv_ShowBottomDesc.Dispose();
                hv_ShowBottomDesc = 1;
                //
                //By default, show a legend with class IDs.
                hv_ShowLegend.Dispose();
                hv_ShowLegend = 1;
                //
                //By default, show class labels for detection ground truth/results.
                hv_ShowLabels.Dispose();
                hv_ShowLabels = 1;
                //
                //** Set user defined values: ***
                //
                //Overwrite default values by given generic parameters.
                if ((int)(new HTuple(hv_GenParam.TupleNotEqual(new HTuple()))) != 0)
                {
                    hv_GenParamNames.Dispose();
                    HOperatorSet.GetDictParam(hv_GenParam, "keys", new HTuple(), out hv_GenParamNames);
                    for (hv_ParamIndex = 0; (int)hv_ParamIndex <= (int)((new HTuple(hv_GenParamNames.TupleLength()
                        )) - 1); hv_ParamIndex = (int)hv_ParamIndex + 1)
                    {
                        hv_GenParamName.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_GenParamName = hv_GenParamNames.TupleSelect(
                                hv_ParamIndex);
                        }
                        hv_GenParamValue.Dispose();
                        HOperatorSet.GetDictTuple(hv_GenParam, hv_GenParamName, out hv_GenParamValue);
                        if ((int)(new HTuple(hv_GenParamName.TupleEqual("threshold_width"))) != 0)
                        {
                            hv_ThresholdWidth.Dispose();
                            hv_ThresholdWidth = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("scale_windows"))) != 0)
                        {
                            hv_ScaleWindows.Dispose();
                            hv_ScaleWindows = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("font"))) != 0)
                        {
                            hv_Font.Dispose();
                            hv_Font = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("font_size"))) != 0)
                        {
                            hv_FontSize.Dispose();
                            hv_FontSize = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("line_width"))) != 0)
                        {
                            hv_LineWidth.Dispose();
                            hv_LineWidth = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("map_transparency"))) != 0)
                        {
                            hv_MapTransparency.Dispose();
                            hv_MapTransparency = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("map_color_bar_width"))) != 0)
                        {
                            hv_MapColorBarWidth.Dispose();
                            hv_MapColorBarWidth = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("segmentation_max_weight"))) != 0)
                        {
                            hv_SegMaxWeight.Dispose();
                            hv_SegMaxWeight = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("segmentation_draw"))) != 0)
                        {
                            hv_SegDraw.Dispose();
                            hv_SegDraw = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("segmentation_transparency"))) != 0)
                        {
                            hv_SegTransparency.Dispose();
                            hv_SegTransparency = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("segmentation_exclude_class_ids"))) != 0)
                        {
                            hv_SegExcludeClassIDs.Dispose();
                            hv_SegExcludeClassIDs = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("bbox_label_color"))) != 0)
                        {
                            hv_BboxLabelColor.Dispose();
                            hv_BboxLabelColor = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("bbox_display_confidence"))) != 0)
                        {
                            hv_BboxDisplayConfidence.Dispose();
                            hv_BboxDisplayConfidence = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("bbox_text_color"))) != 0)
                        {
                            hv_BboxTextColor.Dispose();
                            hv_BboxTextColor = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("display_bottom_desc"))) != 0)
                        {
                            hv_ShowBottomDesc.Dispose();
                            hv_ShowBottomDesc = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("display_legend"))) != 0)
                        {
                            hv_ShowLegend.Dispose();
                            hv_ShowLegend = new HTuple(hv_GenParamValue);
                        }
                        else if ((int)(new HTuple(hv_GenParamName.TupleEqual("display_labels"))) != 0)
                        {
                            hv_ShowLabels.Dispose();
                            hv_ShowLabels = new HTuple(hv_GenParamValue);
                        }
                        else
                        {
                            throw new HalconException(("Unknown generic parameter: " + hv_GenParamName) + ".");
                        }
                    }
                }
                //
                //Get the dictionary keys.
                hv_SampleKeys.Dispose();
                HOperatorSet.GetDictParam(hv_DLSample, "keys", new HTuple(), out hv_SampleKeys);
                if ((int)(new HTuple(hv_DLResult.TupleNotEqual(new HTuple()))) != 0)
                {
                    hv_ResultKeys.Dispose();
                    HOperatorSet.GetDictParam(hv_DLResult, "keys", new HTuple(), out hv_ResultKeys);
                }
                //
                //Get image ID if it is available.
                hv_ImageIDExists.Dispose();
                HOperatorSet.GetDictParam(hv_DLSample, "key_exists", "image_id", out hv_ImageIDExists);
                if ((int)(hv_ImageIDExists) != 0)
                {
                    hv_ImageID.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLSample, "image_id", out hv_ImageID);
                    hv_ImageIDString.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ImageIDString = (" (" + hv_ImageID) + ")";
                        hv_ImageIDString = "";
                    }
                }
                else
                {
                    hv_ImageIDString.Dispose();
                    hv_ImageIDString = "";
                }
                //
                //Check if DLDatasetInfo is valid.
                if ((int)(new HTuple(hv_DLDatasetInfo.TupleEqual(new HTuple()))) != 0)
                {
                    //If DLDatasetInfo is empty, 'image' is the only key allowed in KeysForDisplay.
                    if ((int)((new HTuple((new HTuple(hv_KeysForDisplay.TupleLength())).TupleNotEqual(
                        1))).TupleOr(new HTuple(((hv_KeysForDisplay.TupleSelect(0))).TupleNotEqual(
                        "image")))) != 0)
                    {
                        throw new HalconException("DLDatasetInfo is needed for requested keys in KeysForDisplay.");
                    }
                }
                else
                {
                    //Check if DLDatasetInfo contains necessary keys.
                    hv_NeededKeys.Dispose();
                    hv_NeededKeys = new HTuple();
                    hv_NeededKeys[0] = "class_names";
                    hv_NeededKeys[1] = "class_ids";
                    for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_NeededKeys.TupleLength()
                        )) - 1); hv_Index = (int)hv_Index + 1)
                    {
                        hv_DLDatasetInfoKeys.Dispose();
                        HOperatorSet.GetDictParam(hv_DLDatasetInfo, "keys", new HTuple(), out hv_DLDatasetInfoKeys);
                        if ((int)(new HTuple(((hv_DLDatasetInfoKeys.TupleFindFirst(hv_NeededKeys.TupleSelect(
                            hv_Index)))).TupleEqual(-1))) != 0)
                        {
                            throw new HalconException(("Key " + (hv_NeededKeys.TupleSelect(
                                hv_Index))) + " is missing in DLDatasetInfo.");
                        }
                    }
                }
                //
                //Get the general dataset information, if available.
                if ((int)(new HTuple(hv_DLDatasetInfo.TupleNotEqual(new HTuple()))) != 0)
                {
                    hv_ClassNames.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLDatasetInfo, "class_names", out hv_ClassNames);
                    hv_ClassIDs.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLDatasetInfo, "class_ids", out hv_ClassIDs);
                    //
                    //Define distinct colors for the classes.
                    hv_Colors.Dispose();
                    get_dl_class_colors(hv_ClassNames, out hv_Colors);
                    //
                    hv_ClassesLegend.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ClassesLegend = (hv_ClassIDs + " : ") + hv_ClassNames;
                    }
                }
                //
                //** Set window parameters: ***
                //
                //Set previous window coordinates.
                hv_PrevWindowCoordinates.Dispose();
                hv_PrevWindowCoordinates = new HTuple();
                hv_PrevWindowCoordinates[0] = 0;
                hv_PrevWindowCoordinates[1] = 0;
                hv_PrevWindowCoordinates[2] = 0;
                hv_PrevWindowCoordinates[3] = 0;
                hv_PrevWindowCoordinates[4] = 1;
                //
                //
                //Check that the WindowHandleDict is of type dictionary.
                try
                {
                    hv_Keys.Dispose();
                    HOperatorSet.GetDictParam(hv_WindowHandleDict, "keys", new HTuple(), out hv_Keys);
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    if ((int)(new HTuple(((hv_Exception.TupleSelect(0))).TupleEqual(1401))) != 0)
                    {
                        throw new HalconException("WindowHandleDict has to be of type dictionary. Use create_dict to create an empty dictionary.");
                    }
                    else
                    {
                        throw new HalconException(hv_Exception);
                    }
                }
                //For better usage, add meta information about the window handles in WindowHandleDict.
                hv_MetaInfoIndex.Dispose();
                HOperatorSet.TupleFind(hv_Keys, "meta_information", out hv_MetaInfoIndex);
                if ((int)((new HTuple(hv_MetaInfoIndex.TupleEqual(-1))).TupleOr(new HTuple(hv_MetaInfoIndex.TupleEqual(
                    new HTuple())))) != 0)
                {
                    hv_MetaInfo.Dispose();
                    HOperatorSet.CreateDict(out hv_MetaInfo);
                    HOperatorSet.SetDictTuple(hv_WindowHandleDict, "meta_information", hv_MetaInfo);
                }
                //
                //For each window, set 'flush' to 'false' to avoid flickering.
                hv_FlushValues.Dispose();
                hv_FlushValues = new HTuple();
                hv_WindowHandleKeys.Dispose();
                HOperatorSet.GetDictParam(hv_WindowHandleDict, "keys", new HTuple(), out hv_WindowHandleKeys);
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_WindowHandleKeys.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    //Only consider the WindowHandleKeys that are needed for the current visualization.
                    hv_KeyIndex.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_KeyIndex = hv_KeysForDisplay.TupleFind(
                            hv_WindowHandleKeys.TupleSelect(hv_Index));
                    }
                    if ((int)((new HTuple(hv_KeyIndex.TupleNotEqual(-1))).TupleAnd(new HTuple(hv_KeyIndex.TupleNotEqual(
                        new HTuple())))) != 0)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_WindowHandles.Dispose();
                            HOperatorSet.GetDictTuple(hv_WindowHandleDict, hv_WindowHandleKeys.TupleSelect(
                                hv_Index), out hv_WindowHandles);
                        }
                        for (hv_WindowIndex = 0; (int)hv_WindowIndex <= (int)((new HTuple(hv_WindowHandles.TupleLength()
                            )) - 1); hv_WindowIndex = (int)hv_WindowIndex + 1)
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_FlushValue.Dispose();
                                HOperatorSet.GetWindowParam(hv_WindowHandles.TupleSelect(hv_WindowIndex),
                                    "flush", out hv_FlushValue);
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_FlushValues = hv_FlushValues.TupleConcat(
                                        hv_FlushValue);
                                    hv_FlushValues.Dispose();
                                    hv_FlushValues = ExpTmpLocalVar_FlushValues;
                                }
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetWindowParam(hv_WindowHandles.TupleSelect(hv_WindowIndex),
                                    "flush", "false");
                            }
                        }
                    }
                }
                //
                //** Display the data: ***
                //
                //Display data dictionaries.
                for (hv_KeyIndex = 0; (int)hv_KeyIndex <= (int)((new HTuple(hv_KeysForDisplay.TupleLength()
                    )) - 1); hv_KeyIndex = (int)hv_KeyIndex + 1)
                {
                    if ((int)(new HTuple(((hv_KeysForDisplay.TupleSelect(hv_KeyIndex))).TupleEqual(
                        "image"))) != 0)
                    {
                        //
                        //Image.
                        ho_Image.Dispose();
                        get_image(out ho_Image, hv_SampleKeys, hv_DLSample);
                        //
                        //Get or open next window.
                        hv_WidthImage.Dispose(); hv_HeightImage.Dispose();
                        HOperatorSet.GetImageSize(ho_Image, out hv_WidthImage, out hv_HeightImage);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CurrentWindowHandle.Dispose(); hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                            get_next_window(hv_Font, hv_FontSize, hv_ShowBottomDesc, hv_WidthImage,
                                hv_HeightImage, 0, hv_ScaleWindows, hv_ThresholdWidth, hv_PrevWindowCoordinates,
                                hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex), out hv_CurrentWindowHandle,
                                out hv_WindowImageRatio, out ExpTmpOutVar_0);
                            hv_PrevWindowCoordinates.Dispose();
                            hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                        }
                        //
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                        }
                        if ((int)(hv_ShowBottomDesc) != 0)
                        {
                            if (HDevWindowStack.IsOpen())
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Image" + hv_ImageIDString,
                                        "window", "bottom", "left", "white", "box", "false");
                                }
                            }
                        }
                    }
                    else if ((int)(new HTuple(((hv_KeysForDisplay.TupleSelect(hv_KeyIndex))).TupleEqual(
                        "segmentation_weight"))) != 0)
                    {
                        //
                        //Weight image.
                        ho_ImageWeight.Dispose();
                        get_weight_image(out ho_ImageWeight, hv_SampleKeys, hv_DLSample);
                        //
                        //Get or open next window.
                        hv_WidthImage.Dispose(); hv_HeightImage.Dispose();
                        HOperatorSet.GetImageSize(ho_ImageWeight, out hv_WidthImage, out hv_HeightImage);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CurrentWindowHandle.Dispose(); hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                            get_next_window(hv_Font, hv_FontSize, hv_ShowBottomDesc, hv_WidthImage,
                                hv_HeightImage, 0, hv_ScaleWindows, hv_ThresholdWidth, hv_PrevWindowCoordinates,
                                hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex), out hv_CurrentWindowHandle,
                                out hv_WindowImageRatio, out ExpTmpOutVar_0);
                            hv_PrevWindowCoordinates.Dispose();
                            hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                        }
                        //
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_ImageWeight, HDevWindowStack.GetActive());
                        }
                        if ((int)(hv_ShowBottomDesc) != 0)
                        {
                            if (HDevWindowStack.IsOpen())
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Weight image" + hv_ImageIDString,
                                        "window", "bottom", "left", "white", "box", "false");
                                }
                            }
                        }
                    }
                    else if ((int)(new HTuple(((hv_KeysForDisplay.TupleSelect(hv_KeyIndex))).TupleEqual(
                        "segmentation_confidence"))) != 0)
                    {
                        //
                        //Segmentation confidences.
                        ho_ImageConfidence.Dispose();
                        get_confidence_image(out ho_ImageConfidence, hv_ResultKeys, hv_DLResult);
                        //
                        //Get or open next window.
                        hv_WidthImage.Dispose(); hv_HeightImage.Dispose();
                        HOperatorSet.GetImageSize(ho_ImageConfidence, out hv_WidthImage, out hv_HeightImage);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CurrentWindowHandle.Dispose(); hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                            get_next_window(hv_Font, hv_FontSize, hv_ShowBottomDesc, hv_WidthImage,
                                hv_HeightImage, 0, hv_ScaleWindows, hv_ThresholdWidth, hv_PrevWindowCoordinates,
                                hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex), out hv_CurrentWindowHandle,
                                out hv_WindowImageRatio, out ExpTmpOutVar_0);
                            hv_PrevWindowCoordinates.Dispose();
                            hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                        }
                        //
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_ImageConfidence, HDevWindowStack.GetActive());
                        }
                        if ((int)(hv_ShowBottomDesc) != 0)
                        {
                            if (HDevWindowStack.IsOpen())
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Confidence image" + hv_ImageIDString,
                                        "window", "bottom", "left", "white", "box", "false");
                                }
                            }
                        }
                    }
                    else if ((int)(new HTuple(((hv_KeysForDisplay.TupleSelect(hv_KeyIndex))).TupleEqual(
                        "bbox_ground_truth"))) != 0)
                    {
                        //
                        //Sample bounding boxes on image.
                        ho_Image.Dispose();
                        get_image(out ho_Image, hv_SampleKeys, hv_DLSample);
                        //
                        //Get or open next window.
                        hv_WidthImage.Dispose(); hv_HeightImage.Dispose();
                        HOperatorSet.GetImageSize(ho_Image, out hv_WidthImage, out hv_HeightImage);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CurrentWindowHandle.Dispose(); hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                            get_next_window(hv_Font, hv_FontSize, hv_ShowBottomDesc, hv_WidthImage,
                                hv_HeightImage, 0, hv_ScaleWindows, hv_ThresholdWidth, hv_PrevWindowCoordinates,
                                hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex), out hv_CurrentWindowHandle,
                                out hv_WindowImageRatio, out ExpTmpOutVar_0);
                            hv_PrevWindowCoordinates.Dispose();
                            hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                        }
                        //
                        //Visualization.
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                        }
                        //
                        hv_BboxIDs.Dispose();
                        dev_display_ground_truth_detection(hv_DLSample, hv_SampleKeys, hv_LineWidth,
                            hv_ClassIDs, hv_Colors, hv_BboxLabelColor, hv_BboxTextColor, hv_ShowLabels,
                            hv_CurrentWindowHandle, out hv_BboxIDs);
                        hv_Text.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_Text = "Ground truth bounding boxes" + hv_ImageIDString;
                        }
                        if ((int)(hv_ShowBottomDesc) != 0)
                        {
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window",
                                    "bottom", "left", "white", "box", "false");
                            }
                        }
                        //
                        //Display the legend.
                        if ((int)(hv_ShowLegend) != 0)
                        {
                            hv_BboxColors.Dispose();
                            hv_BboxColors = "white";
                            if ((int)(new HTuple(hv_BboxIDs.TupleLength())) != 0)
                            {
                                hv_BboxIDsUniq.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_BboxIDsUniq = ((hv_BboxIDs.TupleSort()
                                        )).TupleUniq();
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    {
                                        HTuple
                                          ExpTmpLocalVar_Text = hv_Text.TupleConcat(
                                            hv_ClassesLegend.TupleSelect(hv_BboxIDsUniq));
                                        hv_Text.Dispose();
                                        hv_Text = ExpTmpLocalVar_Text;
                                    }
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    {
                                        HTuple
                                          ExpTmpLocalVar_BboxColors = hv_BboxColors.TupleConcat(
                                            hv_Colors.TupleSelect(hv_BboxIDsUniq));
                                        hv_BboxColors.Dispose();
                                        hv_BboxColors = ExpTmpLocalVar_BboxColors;
                                    }
                                }
                            }
                            else
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    {
                                        HTuple
                                          ExpTmpLocalVar_Text = hv_Text.TupleConcat(
                                            "No ground truth bounding boxes present.");
                                        hv_Text.Dispose();
                                        hv_Text = ExpTmpLocalVar_Text;
                                    }
                                }
                            }
                            //
                            //Get or open next child window.
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                                get_child_window(hv_HeightImage, hv_Font, hv_FontSize, hv_Text, hv_PrevWindowCoordinates,
                                    hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex),
                                    out hv_WindowImageRatio, out ExpTmpOutVar_0);
                                hv_PrevWindowCoordinates.Dispose();
                                hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                            }
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window",
                                    "top", "left", hv_BboxColors, "box", "false");
                            }
                        }
                    }
                    else if ((int)(new HTuple(((hv_KeysForDisplay.TupleSelect(hv_KeyIndex))).TupleEqual(
                        "bbox_result"))) != 0)
                    {
                        //
                        //Result bounding boxes on image.
                        ho_Image.Dispose();
                        get_image(out ho_Image, hv_SampleKeys, hv_DLSample);
                        //
                        //Get or open next window.
                        hv_WidthImage.Dispose(); hv_HeightImage.Dispose();
                        HOperatorSet.GetImageSize(ho_Image, out hv_WidthImage, out hv_HeightImage);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CurrentWindowHandle.Dispose(); hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                            get_next_window(hv_Font, hv_FontSize, hv_ShowBottomDesc, hv_WidthImage,
                                hv_HeightImage, 0, hv_ScaleWindows, hv_ThresholdWidth, hv_PrevWindowCoordinates,
                                hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex), out hv_CurrentWindowHandle,
                                out hv_WindowImageRatio, out ExpTmpOutVar_0);
                            hv_PrevWindowCoordinates.Dispose();
                            hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                        }
                        //
                        //Visualization.
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                        }
                        //
                        if ((int)(new HTuple(((hv_ResultKeys.TupleFind("bbox_confidence"))).TupleNotEqual(
                            -1))) != 0)
                        {
                            hv_BboxConfidences.Dispose();
                            HOperatorSet.GetDictTuple(hv_DLResult, "bbox_confidence", out hv_BboxConfidences);
                        }
                        else
                        {
                            throw new HalconException("Result bounding box data could not be found in DLResult.");
                        }
                        if ((int)(hv_BboxDisplayConfidence) != 0)
                        {
                            hv_TextConf.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_TextConf = (" (" + (hv_BboxConfidences.TupleString(
                                    ".2f"))) + ")";
                            }
                        }
                        else
                        {
                            hv_TextConf.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_TextConf = HTuple.TupleGenConst(
                                    new HTuple(hv_BboxConfidences.TupleLength()), "");
                            }
                        }
                        hv_BboxClassIndex.Dispose();
                        dev_display_result_detection(hv_DLResult, hv_ResultKeys, hv_LineWidth,
                            hv_ClassIDs, hv_TextConf, hv_Colors, hv_BboxLabelColor, hv_WindowImageRatio,
                            "top", hv_BboxTextColor, hv_ShowLabels, hv_CurrentWindowHandle, out hv_BboxClassIndex);
                        hv_Text.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_Text = "Result bounding boxes" + hv_ImageIDString;
                        }
                        if ((int)(hv_ShowBottomDesc) != 0)
                        {
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window",
                                    "bottom", "left", "white", "box", "false");
                            }
                        }
                        //
                        //Display the legend.
                        if ((int)(hv_ShowLegend) != 0)
                        {
                            hv_BboxColorsResults.Dispose();
                            hv_BboxColorsResults = "white";
                            if ((int)(new HTuple((new HTuple(hv_BboxClassIndex.TupleLength())).TupleGreater(
                                0))) != 0)
                            {
                                hv_BboxClassIndexUniq.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_BboxClassIndexUniq = ((hv_BboxClassIndex.TupleSort()
                                        )).TupleUniq();
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    {
                                        HTuple
                                          ExpTmpLocalVar_Text = hv_Text.TupleConcat(
                                            hv_ClassesLegend.TupleSelect(hv_BboxClassIndexUniq));
                                        hv_Text.Dispose();
                                        hv_Text = ExpTmpLocalVar_Text;
                                    }
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    {
                                        HTuple
                                          ExpTmpLocalVar_BboxColorsResults = hv_BboxColorsResults.TupleConcat(
                                            hv_Colors.TupleSelect(hv_BboxClassIndexUniq));
                                        hv_BboxColorsResults.Dispose();
                                        hv_BboxColorsResults = ExpTmpLocalVar_BboxColorsResults;
                                    }
                                }
                            }
                            else
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    {
                                        HTuple
                                          ExpTmpLocalVar_Text = hv_Text.TupleConcat(
                                            "No result bounding boxes present.");
                                        hv_Text.Dispose();
                                        hv_Text = ExpTmpLocalVar_Text;
                                    }
                                }
                            }
                            //
                            //Get or open next child window.
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                                get_child_window(hv_HeightImage, hv_Font, hv_FontSize, hv_Text, hv_PrevWindowCoordinates,
                                    hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex),
                                    out hv_WindowImageRatio, out ExpTmpOutVar_0);
                                hv_PrevWindowCoordinates.Dispose();
                                hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                            }
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window",
                                    "top", "left", hv_BboxColorsResults, "box", "false");
                            }
                        }
                    }
                    else if ((int)(new HTuple(((hv_KeysForDisplay.TupleSelect(hv_KeyIndex))).TupleEqual(
                        "bbox_both"))) != 0)
                    {
                        //
                        //Ground truth and result bounding boxes on image.
                        ho_Image.Dispose();
                        get_image(out ho_Image, hv_SampleKeys, hv_DLSample);
                        //
                        //Get or open next window.
                        hv_WidthImage.Dispose(); hv_HeightImage.Dispose();
                        HOperatorSet.GetImageSize(ho_Image, out hv_WidthImage, out hv_HeightImage);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CurrentWindowHandle.Dispose(); hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                            get_next_window(hv_Font, hv_FontSize, hv_ShowBottomDesc, hv_WidthImage,
                                hv_HeightImage, 0, hv_ScaleWindows, hv_ThresholdWidth, hv_PrevWindowCoordinates,
                                hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex), out hv_CurrentWindowHandle,
                                out hv_WindowImageRatio, out ExpTmpOutVar_0);
                            hv_PrevWindowCoordinates.Dispose();
                            hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                        }
                        //
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                        }
                        //
                        //Visualization.
                        hv_BboxLabelIndex.Dispose();
                        dev_display_ground_truth_detection(hv_DLSample, hv_SampleKeys, hv_LineWidth,
                            hv_ClassIDs, hv_Colors, hv_BboxLabelColor, hv_BboxTextColor, hv_ShowLabels,
                            hv_CurrentWindowHandle, out hv_BboxLabelIndex);
                        if ((int)(new HTuple(((hv_ResultKeys.TupleFind("bbox_confidence"))).TupleNotEqual(
                            -1))) != 0)
                        {
                            hv_BboxConfidences.Dispose();
                            HOperatorSet.GetDictTuple(hv_DLResult, "bbox_confidence", out hv_BboxConfidences);
                        }
                        else
                        {
                            throw new HalconException("Result bounding box data could not be found in DLResult.");
                        }
                        if ((int)(hv_BboxDisplayConfidence) != 0)
                        {
                            hv_TextConf.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_TextConf = (" (" + (hv_BboxConfidences.TupleString(
                                   ".2f"))) + ")";
                            }
                        }
                        else
                        {
                            hv_TextConf.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_TextConf = HTuple.TupleGenConst(
                                    new HTuple(hv_BboxConfidences.TupleLength()), "");
                            }
                        }
                        hv_BboxClassIndex.Dispose();
                        dev_display_result_detection(hv_DLResult, hv_ResultKeys, hv_LineWidth,
                            hv_ClassIDs, hv_TextConf, hv_Colors, hv_BboxLabelColor, hv_WindowImageRatio,
                            "bottom", hv_BboxTextColor, hv_ShowLabels, hv_CurrentWindowHandle,
                            out hv_BboxClassIndex);
                        hv_Text.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_Text = "Ground truth and result bounding boxes" + hv_ImageIDString;
                        }
                        if ((int)(hv_ShowBottomDesc) != 0)
                        {
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window",
                                    "bottom", "left", "white", "box", "false");
                            }
                        }
                        hv_Text.Dispose();
                        hv_Text = "Ground truth and";
                        if (hv_Text == null)
                            hv_Text = new HTuple();
                        hv_Text[new HTuple(hv_Text.TupleLength())] = "result bounding boxes" + hv_ImageIDString;
                        //
                        //Display the legend.
                        if ((int)(hv_ShowLegend) != 0)
                        {
                            hv_BboxColorsBoth.Dispose();
                            hv_BboxColorsBoth = new HTuple();
                            hv_BboxColorsBoth[0] = "white";
                            hv_BboxColorsBoth[1] = "white";
                            if ((int)(new HTuple((new HTuple((new HTuple(hv_BboxClassIndex.TupleLength()
                                )) + (new HTuple(hv_BboxLabelIndex.TupleLength())))).TupleGreater(0))) != 0)
                            {
                                hv_BboxClassLabelIndexUniq.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_BboxClassLabelIndexUniq = ((((hv_BboxClassIndex.TupleConcat(
                                        hv_BboxLabelIndex))).TupleSort())).TupleUniq();
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    {
                                        HTuple
                                          ExpTmpLocalVar_Text = hv_Text.TupleConcat(
                                            hv_ClassesLegend.TupleSelect(hv_BboxClassLabelIndexUniq));
                                        hv_Text.Dispose();
                                        hv_Text = ExpTmpLocalVar_Text;
                                    }
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    {
                                        HTuple
                                          ExpTmpLocalVar_BboxColorsBoth = hv_BboxColorsBoth.TupleConcat(
                                            hv_Colors.TupleSelect(hv_BboxClassLabelIndexUniq));
                                        hv_BboxColorsBoth.Dispose();
                                        hv_BboxColorsBoth = ExpTmpLocalVar_BboxColorsBoth;
                                    }
                                }
                            }
                            else
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    {
                                        HTuple
                                          ExpTmpLocalVar_Text = hv_Text.TupleConcat(
                                            "No ground truth nor result bounding boxes present.");
                                        hv_Text.Dispose();
                                        hv_Text = ExpTmpLocalVar_Text;
                                    }
                                }
                            }
                            //
                            //Get or open next child window.
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                                get_child_window(hv_HeightImage, hv_Font, hv_FontSize, hv_Text, hv_PrevWindowCoordinates,
                                    hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex),
                                    out hv_WindowImageRatio, out ExpTmpOutVar_0);
                                hv_PrevWindowCoordinates.Dispose();
                                hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                            }
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window",
                                    "top", "left", hv_BboxColorsBoth, "box", "false");
                            }
                        }
                    }
                    else if ((int)(new HTuple(((hv_KeysForDisplay.TupleSelect(hv_KeyIndex))).TupleEqual(
                        "segmentation_image_ground_truth"))) != 0)
                    {
                        //
                        //Ground truth segmentation image.
                        ho_Image.Dispose();
                        get_image(out ho_Image, hv_SampleKeys, hv_DLSample);
                        ho_SegmentationImagGroundTruth.Dispose();
                        get_segmentation_image_ground_truth(out ho_SegmentationImagGroundTruth,
                            hv_SampleKeys, hv_DLSample);
                        //
                        //Get or open next window.
                        hv_WidthImage.Dispose(); hv_HeightImage.Dispose();
                        HOperatorSet.GetImageSize(ho_Image, out hv_WidthImage, out hv_HeightImage);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CurrentWindowHandle.Dispose(); hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                            get_next_window(hv_Font, hv_FontSize, hv_ShowBottomDesc, hv_WidthImage,
                                hv_HeightImage, 0, hv_ScaleWindows, hv_ThresholdWidth, hv_PrevWindowCoordinates,
                                hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex), out hv_CurrentWindowHandle,
                                out hv_WindowImageRatio, out ExpTmpOutVar_0);
                            hv_PrevWindowCoordinates.Dispose();
                            hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                        }
                        //
                        //Visualization.
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                        }
                        //
                        //Display segmentation regions.
                        hv_ColorsSegmentation.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ColorsSegmentation = hv_Colors + hv_SegTransparency;
                        }
                        hv_DrawMode.Dispose();
                        HOperatorSet.GetDraw(hv_CurrentWindowHandle, out hv_DrawMode);
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetDraw(HDevWindowStack.GetActive(), hv_SegDraw);
                        }
                        hv_Width.Dispose();
                        HOperatorSet.GetLineWidth(hv_CurrentWindowHandle, out hv_Width);
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), hv_LineWidth);
                        }
                        hv_ImageClassIDs.Dispose();
                        dev_display_segmentation_regions(ho_SegmentationImagGroundTruth, hv_ClassIDs,
                            hv_ColorsSegmentation, hv_SegExcludeClassIDs, out hv_ImageClassIDs);
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetDraw(HDevWindowStack.GetActive(), hv_DrawMode);
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), hv_Width.TupleInt()
                                    );
                            }
                        }
                        hv_Text.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_Text = "Ground truth segmentation" + hv_ImageIDString;
                        }
                        if ((int)(hv_ShowBottomDesc) != 0)
                        {
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window",
                                    "bottom", "left", "white", "box", "false");
                            }
                        }
                        //
                        //Display the legend.
                        if ((int)(hv_ShowLegend) != 0)
                        {
                            hv_ImageClassIDsUniq.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ImageClassIDsUniq = ((hv_ImageClassIDs.TupleSort()
                                    )).TupleUniq();
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_Text = hv_Text.TupleConcat(
                                        hv_ClassesLegend.TupleSelect(hv_ImageClassIDsUniq));
                                    hv_Text.Dispose();
                                    hv_Text = ExpTmpLocalVar_Text;
                                }
                            }
                            //
                            //Get or open next child window
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                                get_child_window(hv_HeightImage, hv_Font, hv_FontSize, hv_Text, hv_PrevWindowCoordinates,
                                    hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex),
                                    out hv_WindowImageRatio, out ExpTmpOutVar_0);
                                hv_PrevWindowCoordinates.Dispose();
                                hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                            }
                            if (HDevWindowStack.IsOpen())
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window",
                                        "top", "left", (new HTuple("white")).TupleConcat(hv_Colors.TupleSelect(
                                        hv_ImageClassIDsUniq)), "box", "false");
                                }
                            }
                        }
                    }
                    else if ((int)(new HTuple(((hv_KeysForDisplay.TupleSelect(hv_KeyIndex))).TupleEqual(
                        "segmentation_image_result"))) != 0)
                    {
                        //
                        //Result segmentation on image.
                        ho_Image.Dispose();
                        get_image(out ho_Image, hv_SampleKeys, hv_DLSample);
                        ho_SegmentationImageResult.Dispose();
                        get_segmentation_image_result(out ho_SegmentationImageResult, hv_ResultKeys,
                            hv_DLResult);
                        //
                        //Get or open next window.
                        hv_WidthImage.Dispose(); hv_HeightImage.Dispose();
                        HOperatorSet.GetImageSize(ho_Image, out hv_WidthImage, out hv_HeightImage);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CurrentWindowHandle.Dispose(); hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                            get_next_window(hv_Font, hv_FontSize, hv_ShowBottomDesc, hv_WidthImage,
                                hv_HeightImage, 0, hv_ScaleWindows, hv_ThresholdWidth, hv_PrevWindowCoordinates,
                                hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex), out hv_CurrentWindowHandle,
                                out hv_WindowImageRatio, out ExpTmpOutVar_0);
                            hv_PrevWindowCoordinates.Dispose();
                            hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                        }
                        //
                        //Visualization.
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                        }
                        //
                        //Display result segmentation regions.
                        hv_ColorsResults.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ColorsResults = hv_Colors + hv_SegTransparency;
                        }
                        hv_DrawMode.Dispose();
                        HOperatorSet.GetDraw(hv_CurrentWindowHandle, out hv_DrawMode);
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetDraw(HDevWindowStack.GetActive(), hv_SegDraw);
                        }
                        hv_Width.Dispose();
                        HOperatorSet.GetLineWidth(hv_CurrentWindowHandle, out hv_Width);
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), hv_LineWidth);
                        }
                        hv_ImageClassIDs.Dispose();
                        dev_display_segmentation_regions(ho_SegmentationImageResult, hv_ClassIDs,
                            hv_ColorsResults, hv_SegExcludeClassIDs, out hv_ImageClassIDs);
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetDraw(HDevWindowStack.GetActive(), hv_DrawMode);
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), hv_Width.TupleInt()
                                    );
                            }
                        }
                        hv_Text.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_Text = "Result segmentation" + hv_ImageIDString;
                        }
                        if ((int)(hv_ShowBottomDesc) != 0)
                        {
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window",
                                    "bottom", "left", "white", "box", "false");
                            }
                        }
                        //
                        //Display the legend.
                        if ((int)(hv_ShowLegend) != 0)
                        {
                            hv_ImageClassIDsUniq.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ImageClassIDsUniq = ((hv_ImageClassIDs.TupleSort()
                                    )).TupleUniq();
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_Text = hv_Text.TupleConcat(
                                        hv_ClassesLegend.TupleSelect(hv_ImageClassIDsUniq));
                                    hv_Text.Dispose();
                                    hv_Text = ExpTmpLocalVar_Text;
                                }
                            }
                            //
                            //Get or open next child window.
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                                get_child_window(hv_HeightImage, hv_Font, hv_FontSize, hv_Text, hv_PrevWindowCoordinates,
                                    hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex),
                                    out hv_WindowImageRatio, out ExpTmpOutVar_0);
                                hv_PrevWindowCoordinates.Dispose();
                                hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                            }
                            if (HDevWindowStack.IsOpen())
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window",
                                        "top", "left", (new HTuple("white")).TupleConcat(hv_Colors.TupleSelect(
                                        hv_ImageClassIDsUniq)), "box", "false");
                                }
                            }
                        }
                    }
                    else if ((int)(new HTuple(((hv_KeysForDisplay.TupleSelect(hv_KeyIndex))).TupleEqual(
                        "segmentation_image_both"))) != 0)
                    {
                        //
                        //Ground truth and result segmentation on image.
                        ho_Image.Dispose();
                        get_image(out ho_Image, hv_SampleKeys, hv_DLSample);
                        ho_SegmentationImagGroundTruth.Dispose();
                        get_segmentation_image_ground_truth(out ho_SegmentationImagGroundTruth,
                            hv_SampleKeys, hv_DLSample);
                        ho_SegmentationImageResult.Dispose();
                        get_segmentation_image_result(out ho_SegmentationImageResult, hv_ResultKeys,
                            hv_DLResult);
                        //
                        //Get or open next window.
                        hv_WidthImage.Dispose(); hv_HeightImage.Dispose();
                        HOperatorSet.GetImageSize(ho_Image, out hv_WidthImage, out hv_HeightImage);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CurrentWindowHandle.Dispose(); hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                            get_next_window(hv_Font, hv_FontSize, hv_ShowBottomDesc, hv_WidthImage,
                                hv_HeightImage, 0, hv_ScaleWindows, hv_ThresholdWidth, hv_PrevWindowCoordinates,
                                hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex), out hv_CurrentWindowHandle,
                                out hv_WindowImageRatio, out ExpTmpOutVar_0);
                            hv_PrevWindowCoordinates.Dispose();
                            hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                        }
                        //
                        //Visualization.
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                        }
                        //
                        //Display regions.
                        hv_ColorsResults.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ColorsResults = hv_Colors + hv_SegTransparency;
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetDraw(HDevWindowStack.GetActive(), "margin");
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), 2);
                        }
                        hv_GroundTruthIDs.Dispose();
                        dev_display_segmentation_regions(ho_SegmentationImagGroundTruth, hv_ClassIDs,
                            hv_ColorsResults, hv_SegExcludeClassIDs, out hv_GroundTruthIDs);
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), 6);
                        }
                        hv_ResultIDs.Dispose();
                        dev_display_segmentation_regions(ho_SegmentationImageResult, hv_ClassIDs,
                            hv_ColorsResults, hv_SegExcludeClassIDs, out hv_ResultIDs);
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetDraw(HDevWindowStack.GetActive(), "fill");
                        }
                        hv_Text.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_Text = "Ground truth and result segmentation" + hv_ImageIDString;
                        }
                        if ((int)(hv_ShowBottomDesc) != 0)
                        {
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window",
                                    "bottom", "left", "white", "box", "false");
                            }
                        }
                        //
                        //Display the legend.
                        if ((int)(hv_ShowLegend) != 0)
                        {
                            hv_ImageClassIDsUniq.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ImageClassIDsUniq = ((((hv_GroundTruthIDs.TupleConcat(
                                    hv_ResultIDs))).TupleSort())).TupleUniq();
                            }
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_Text = hv_Text.TupleConcat(
                                        hv_ClassesLegend.TupleSelect(hv_ImageClassIDsUniq));
                                    hv_Text.Dispose();
                                    hv_Text = ExpTmpLocalVar_Text;
                                }
                            }
                            if (hv_Text == null)
                                hv_Text = new HTuple();
                            hv_Text[(new HTuple(hv_Text.TupleLength())) + 1] = new HTuple("- thicker line: result, thinner lines: ground truth");
                            if (hv_Text == null)
                                hv_Text = new HTuple();
                            hv_Text[new HTuple(hv_Text.TupleLength())] = "  (you may have to zoom in for a more detailed view)";
                            hv_StringSegExcludeClassIDs.Dispose();
                            hv_StringSegExcludeClassIDs = "";
                            for (hv_StringIndex = 0; (int)hv_StringIndex <= (int)((new HTuple(hv_SegExcludeClassIDs.TupleLength()
                                )) - 1); hv_StringIndex = (int)hv_StringIndex + 1)
                            {
                                if ((int)(new HTuple(hv_StringIndex.TupleEqual((new HTuple(hv_SegExcludeClassIDs.TupleLength()
                                    )) - 1))) != 0)
                                {
                                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                    {
                                        {
                                            HTuple
                                              ExpTmpLocalVar_StringSegExcludeClassIDs = hv_StringSegExcludeClassIDs + (hv_SegExcludeClassIDs.TupleSelect(
                                                hv_StringIndex));
                                            hv_StringSegExcludeClassIDs.Dispose();
                                            hv_StringSegExcludeClassIDs = ExpTmpLocalVar_StringSegExcludeClassIDs;
                                        }
                                    }
                                }
                                else
                                {
                                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                    {
                                        {
                                            HTuple
                                              ExpTmpLocalVar_StringSegExcludeClassIDs = (hv_StringSegExcludeClassIDs + (hv_SegExcludeClassIDs.TupleSelect(
                                                hv_StringIndex))) + new HTuple(", ");
                                            hv_StringSegExcludeClassIDs.Dispose();
                                            hv_StringSegExcludeClassIDs = ExpTmpLocalVar_StringSegExcludeClassIDs;
                                        }
                                    }
                                }
                            }
                            if ((int)(new HTuple(hv_SegExcludeClassIDs.TupleNotEqual(new HTuple()))) != 0)
                            {
                                if (hv_Text == null)
                                    hv_Text = new HTuple();
                                hv_Text[new HTuple(hv_Text.TupleLength())] = ("- (excluded classID(s) " + hv_StringSegExcludeClassIDs) + " from visualization)";
                            }
                            //
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                                get_child_window(hv_HeightImage, hv_Font, hv_FontSize, hv_Text, hv_PrevWindowCoordinates,
                                    hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex),
                                    out hv_WindowImageRatio, out ExpTmpOutVar_0);
                                hv_PrevWindowCoordinates.Dispose();
                                hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                            }
                            if (HDevWindowStack.IsOpen())
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window",
                                        "top", "left", (((new HTuple("white")).TupleConcat(hv_Colors.TupleSelect(
                                        hv_ImageClassIDsUniq)))).TupleConcat(((new HTuple("white")).TupleConcat(
                                        "white")).TupleConcat("white")), "box", "false");
                                }
                            }
                        }
                    }
                    else if ((int)(new HTuple(((hv_KeysForDisplay.TupleSelect(hv_KeyIndex))).TupleEqual(
                        "segmentation_image_diff"))) != 0)
                    {
                        //
                        //Difference of ground truth and result segmentation on image.
                        ho_Image.Dispose();
                        get_image(out ho_Image, hv_SampleKeys, hv_DLSample);
                        ho_SegmentationImagGroundTruth.Dispose();
                        get_segmentation_image_ground_truth(out ho_SegmentationImagGroundTruth,
                            hv_SampleKeys, hv_DLSample);
                        ho_SegmentationImageResult.Dispose();
                        get_segmentation_image_result(out ho_SegmentationImageResult, hv_ResultKeys,
                            hv_DLResult);
                        //
                        //Get or open next window.
                        hv_WidthImage.Dispose(); hv_HeightImage.Dispose();
                        HOperatorSet.GetImageSize(ho_Image, out hv_WidthImage, out hv_HeightImage);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CurrentWindowHandle.Dispose(); hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                            get_next_window(hv_Font, hv_FontSize, hv_ShowBottomDesc, hv_WidthImage,
                                hv_HeightImage, 0, hv_ScaleWindows, hv_ThresholdWidth, hv_PrevWindowCoordinates,
                                hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(hv_KeyIndex), out hv_CurrentWindowHandle,
                                out hv_WindowImageRatio, out ExpTmpOutVar_0);
                            hv_PrevWindowCoordinates.Dispose();
                            hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                        }
                        //
                        //Visualization.
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                        }
                        ho_ImageAbsDiff.Dispose();
                        HOperatorSet.AbsDiffImage(ho_SegmentationImagGroundTruth, ho_SegmentationImageResult,
                            out ho_ImageAbsDiff, 1);
                        hv_Min.Dispose(); hv_Max.Dispose(); hv_Range.Dispose();
                        HOperatorSet.MinMaxGray(ho_SegmentationImageResult, ho_ImageAbsDiff, 0,
                            out hv_Min, out hv_Max, out hv_Range);
                        if ((int)(new HTuple(hv_Min.TupleNotEqual(hv_Max))) != 0)
                        {
                            ho_DiffRegion.Dispose();
                            HOperatorSet.Threshold(ho_ImageAbsDiff, out ho_DiffRegion, 0.00001, hv_Max);
                            if (HDevWindowStack.IsOpen())
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.SetColor(HDevWindowStack.GetActive(), "#ff0000" + hv_SegTransparency);
                                }
                            }
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispObj(ho_DiffRegion, HDevWindowStack.GetActive());
                            }
                        }
                        else
                        {
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispText(HDevWindowStack.GetActive(), "No difference found.",
                                    "window", "top", "left", "black", new HTuple(), new HTuple());
                            }
                        }
                        if ((int)(hv_ShowBottomDesc) != 0)
                        {
                            hv_Text.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_Text = "Difference of ground truth and result segmentation" + hv_ImageIDString;
                            }
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window",
                                    "bottom", "left", "white", "box", "false");
                            }
                        }
                    }
                    else if ((int)(new HTuple(((hv_KeysForDisplay.TupleSelect(hv_KeyIndex))).TupleEqual(
                        "segmentation_weight_map"))) != 0)
                    {
                        //
                        //Weight map on image.
                        ho_Image.Dispose();
                        get_image(out ho_Image, hv_SampleKeys, hv_DLSample);
                        ho_ImageWeight.Dispose();
                        get_weight_image(out ho_ImageWeight, hv_SampleKeys, hv_DLSample);
                        //
                        if ((int)(new HTuple(hv_SegMaxWeight.TupleEqual(0))) != 0)
                        {
                            //Calculate SegMaxWeight if not given in GenParam.
                            hv_MinWeight.Dispose(); hv_SegMaxWeight.Dispose(); hv_Range.Dispose();
                            HOperatorSet.MinMaxGray(ho_ImageWeight, ho_ImageWeight, 0, out hv_MinWeight,
                                out hv_SegMaxWeight, out hv_Range);
                        }
                        //
                        //Get or open next window.
                        hv_WidthImage.Dispose(); hv_HeightImage.Dispose();
                        HOperatorSet.GetImageSize(ho_Image, out hv_WidthImage, out hv_HeightImage);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CurrentWindowHandle.Dispose(); hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                            get_next_window(hv_Font, hv_FontSize, hv_ShowBottomDesc, hv_WidthImage,
                                hv_HeightImage, hv_MapColorBarWidth, hv_ScaleWindows, hv_ThresholdWidth,
                                hv_PrevWindowCoordinates, hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(
                                hv_KeyIndex), out hv_CurrentWindowHandle, out hv_WindowImageRatio,
                                out ExpTmpOutVar_0);
                            hv_PrevWindowCoordinates.Dispose();
                            hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                        }
                        //
                        //Visualization.
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                        }
                        hv_WeightsColors.Dispose();
                        dev_display_weight_regions(ho_ImageWeight, hv_MapTransparency, hv_SegMaxWeight,
                            out hv_WeightsColors);
                        dev_display_map_color_bar(hv_WidthImage, hv_HeightImage, hv_MapColorBarWidth,
                            hv_WeightsColors, hv_SegMaxWeight, hv_WindowImageRatio, hv_CurrentWindowHandle);
                        if ((int)(hv_ShowBottomDesc) != 0)
                        {
                            if (HDevWindowStack.IsOpen())
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Weight map" + hv_ImageIDString,
                                        "window", "bottom", "left", "white", "box", "false");
                                }
                            }
                        }
                    }
                    else if ((int)(new HTuple(((hv_KeysForDisplay.TupleSelect(hv_KeyIndex))).TupleEqual(
                        "segmentation_confidence_map"))) != 0)
                    {
                        //
                        //Segmentation confidence map on image.
                        ho_Image.Dispose();
                        get_image(out ho_Image, hv_SampleKeys, hv_DLSample);
                        ho_ImageConfidence.Dispose();
                        get_confidence_image(out ho_ImageConfidence, hv_ResultKeys, hv_DLResult);
                        //
                        //Get or open next window.
                        hv_WidthImage.Dispose(); hv_HeightImage.Dispose();
                        HOperatorSet.GetImageSize(ho_Image, out hv_WidthImage, out hv_HeightImage);
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CurrentWindowHandle.Dispose(); hv_WindowImageRatio.Dispose(); HTuple ExpTmpOutVar_0;
                            get_next_window(hv_Font, hv_FontSize, hv_ShowBottomDesc, hv_WidthImage,
                                hv_HeightImage, hv_MapColorBarWidth, hv_ScaleWindows, hv_ThresholdWidth,
                                hv_PrevWindowCoordinates, hv_WindowHandleDict, hv_KeysForDisplay.TupleSelect(
                                hv_KeyIndex), out hv_CurrentWindowHandle, out hv_WindowImageRatio,
                                out ExpTmpOutVar_0);
                            hv_PrevWindowCoordinates.Dispose();
                            hv_PrevWindowCoordinates = ExpTmpOutVar_0;
                        }
                        //
                        //Visualization.
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                        }
                        hv_ConfidenceColors.Dispose();
                        dev_display_confidence_regions(ho_ImageConfidence, hv_MapTransparency,
                            out hv_ConfidenceColors);
                        dev_display_map_color_bar(hv_WidthImage, hv_HeightImage, hv_MapColorBarWidth,
                            hv_ConfidenceColors, 1.0, hv_WindowImageRatio, hv_CurrentWindowHandle);
                        if ((int)(hv_ShowBottomDesc) != 0)
                        {
                            if (HDevWindowStack.IsOpen())
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Confidence map" + hv_ImageIDString,
                                        "window", "bottom", "left", "white", "box", "false");
                                }
                            }
                        }
                    }
                    else
                    {
                        //Reset flush buffer of existing windows before throwing an exception.
                        hv_WindowHandleKeys.Dispose();
                        HOperatorSet.GetDictParam(hv_WindowHandleDict, "keys", new HTuple(), out hv_WindowHandleKeys);
                        for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_WindowHandleKeys.TupleLength()
                            )) - 1); hv_Index = (int)hv_Index + 1)
                        {
                            //Only consider the WindowHandleKeys that are needed for the current visualization.
                            hv_KeyIndex.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_KeyIndex = hv_KeysForDisplay.TupleFind(
                                    hv_WindowHandleKeys.TupleSelect(hv_Index));
                            }
                            if ((int)((new HTuple(hv_KeyIndex.TupleNotEqual(-1))).TupleAnd(new HTuple(hv_KeyIndex.TupleNotEqual(
                                new HTuple())))) != 0)
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_WindowHandles.Dispose();
                                    HOperatorSet.GetDictTuple(hv_WindowHandleDict, hv_WindowHandleKeys.TupleSelect(
                                        hv_Index), out hv_WindowHandles);
                                }
                                for (hv_WindowIndex = 0; (int)hv_WindowIndex <= (int)((new HTuple(hv_WindowHandles.TupleLength()
                                    )) - 1); hv_WindowIndex = (int)hv_WindowIndex + 1)
                                {
                                    //Reset values of windows that have been changed temporarily.
                                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                    {
                                        HOperatorSet.SetWindowParam(hv_WindowHandles.TupleSelect(hv_WindowIndex),
                                            "flush", hv_FlushValues.TupleSelect(hv_Index));
                                    }
                                }
                            }
                        }
                        throw new HalconException("Key for display unknown: " + (hv_KeysForDisplay.TupleSelect(
                            hv_KeyIndex)));
                    }
                }
                //
                //Display results.
                hv_WindowHandleKeysNew.Dispose();
                HOperatorSet.GetDictParam(hv_WindowHandleDict, "keys", new HTuple(), out hv_WindowHandleKeysNew);
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_WindowHandleKeysNew.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    //Only consider the WindowHandleKeys that are needed for the current visualization.
                    hv_KeyIndex.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_KeyIndex = hv_KeysForDisplay.TupleFind(
                            hv_WindowHandleKeysNew.TupleSelect(hv_Index));
                    }
                    if ((int)((new HTuple(hv_KeyIndex.TupleNotEqual(-1))).TupleAnd(new HTuple(hv_KeyIndex.TupleNotEqual(
                        new HTuple())))) != 0)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_WindowHandles.Dispose();
                            HOperatorSet.GetDictTuple(hv_WindowHandleDict, hv_WindowHandleKeysNew.TupleSelect(
                                hv_Index), out hv_WindowHandles);
                        }
                        for (hv_WindowIndex = 0; (int)hv_WindowIndex <= (int)((new HTuple(hv_WindowHandles.TupleLength()
                            )) - 1); hv_WindowIndex = (int)hv_WindowIndex + 1)
                        {
                            //Display content of window handle.
                            if ((int)(new HTuple((new HTuple(hv_WindowHandleKeys.TupleLength())).TupleEqual(
                                new HTuple(hv_WindowHandleKeysNew.TupleLength())))) != 0)
                            {
                                //Reset values of windows that have been changed temporarily.
                                if ((int)(new HTuple(((hv_FlushValues.TupleSelect(hv_Index))).TupleEqual(
                                    "true"))) != 0)
                                {
                                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                    {
                                        HOperatorSet.FlushBuffer(hv_WindowHandles.TupleSelect(hv_WindowIndex));
                                    }
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.SetWindowParam(hv_WindowHandles.TupleSelect(hv_WindowIndex),
                                        "flush", hv_FlushValues.TupleSelect(hv_Index));
                                }
                            }
                            else
                            {
                                //Per default, 'flush' of new windows should be set to 'true'.
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.FlushBuffer(hv_WindowHandles.TupleSelect(hv_WindowIndex));
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.SetWindowParam(hv_WindowHandles.TupleSelect(hv_WindowIndex),
                                        "flush", "true");
                                }
                            }
                        }
                    }
                }
                ho_Image.Dispose();
                ho_ImageWeight.Dispose();
                ho_ImageConfidence.Dispose();
                ho_SegmentationImagGroundTruth.Dispose();
                ho_SegmentationImageResult.Dispose();
                ho_ImageAbsDiff.Dispose();
                ho_DiffRegion.Dispose();

                hv_ThresholdWidth.Dispose();
                hv_ScaleWindows.Dispose();
                hv_Font.Dispose();
                hv_FontSize.Dispose();
                hv_LineWidth.Dispose();
                hv_MapTransparency.Dispose();
                hv_MapColorBarWidth.Dispose();
                hv_SegMaxWeight.Dispose();
                hv_SegDraw.Dispose();
                hv_SegTransparency.Dispose();
                hv_SegExcludeClassIDs.Dispose();
                hv_BboxLabelColor.Dispose();
                hv_BboxDisplayConfidence.Dispose();
                hv_BboxTextColor.Dispose();
                hv_ShowBottomDesc.Dispose();
                hv_ShowLegend.Dispose();
                hv_ShowLabels.Dispose();
                hv_GenParamNames.Dispose();
                hv_ParamIndex.Dispose();
                hv_GenParamName.Dispose();
                hv_GenParamValue.Dispose();
                hv_SampleKeys.Dispose();
                hv_ResultKeys.Dispose();
                hv_ImageIDExists.Dispose();
                hv_ImageID.Dispose();
                hv_ImageIDString.Dispose();
                hv_NeededKeys.Dispose();
                hv_Index.Dispose();
                hv_DLDatasetInfoKeys.Dispose();
                hv_ClassNames.Dispose();
                hv_ClassIDs.Dispose();
                hv_Colors.Dispose();
                hv_ClassesLegend.Dispose();
                hv_PrevWindowCoordinates.Dispose();
                hv_Keys.Dispose();
                hv_Exception.Dispose();
                hv_MetaInfoIndex.Dispose();
                hv_MetaInfo.Dispose();
                hv_FlushValues.Dispose();
                hv_WindowHandleKeys.Dispose();
                hv_KeyIndex.Dispose();
                hv_WindowHandles.Dispose();
                hv_WindowIndex.Dispose();
                hv_FlushValue.Dispose();
                hv_WidthImage.Dispose();
                hv_HeightImage.Dispose();
                hv_CurrentWindowHandle.Dispose();
                hv_WindowImageRatio.Dispose();
                hv_BboxIDs.Dispose();
                hv_Text.Dispose();
                hv_BboxColors.Dispose();
                hv_BboxIDsUniq.Dispose();
                hv_BboxConfidences.Dispose();
                hv_TextConf.Dispose();
                hv_BboxClassIndex.Dispose();
                hv_BboxColorsResults.Dispose();
                hv_BboxClassIndexUniq.Dispose();
                hv_BboxLabelIndex.Dispose();
                hv_BboxColorsBoth.Dispose();
                hv_BboxClassLabelIndexUniq.Dispose();
                hv_ColorsSegmentation.Dispose();
                hv_DrawMode.Dispose();
                hv_Width.Dispose();
                hv_ImageClassIDs.Dispose();
                hv_ImageClassIDsUniq.Dispose();
                hv_ColorsResults.Dispose();
                hv_GroundTruthIDs.Dispose();
                hv_ResultIDs.Dispose();
                hv_StringSegExcludeClassIDs.Dispose();
                hv_StringIndex.Dispose();
                hv_Min.Dispose();
                hv_Max.Dispose();
                hv_Range.Dispose();
                hv_MinWeight.Dispose();
                hv_WeightsColors.Dispose();
                hv_ConfidenceColors.Dispose();
                hv_WindowHandleKeysNew.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Image.Dispose();
                ho_ImageWeight.Dispose();
                ho_ImageConfidence.Dispose();
                ho_SegmentationImagGroundTruth.Dispose();
                ho_SegmentationImageResult.Dispose();
                ho_ImageAbsDiff.Dispose();
                ho_DiffRegion.Dispose();

                hv_ThresholdWidth.Dispose();
                hv_ScaleWindows.Dispose();
                hv_Font.Dispose();
                hv_FontSize.Dispose();
                hv_LineWidth.Dispose();
                hv_MapTransparency.Dispose();
                hv_MapColorBarWidth.Dispose();
                hv_SegMaxWeight.Dispose();
                hv_SegDraw.Dispose();
                hv_SegTransparency.Dispose();
                hv_SegExcludeClassIDs.Dispose();
                hv_BboxLabelColor.Dispose();
                hv_BboxDisplayConfidence.Dispose();
                hv_BboxTextColor.Dispose();
                hv_ShowBottomDesc.Dispose();
                hv_ShowLegend.Dispose();
                hv_ShowLabels.Dispose();
                hv_GenParamNames.Dispose();
                hv_ParamIndex.Dispose();
                hv_GenParamName.Dispose();
                hv_GenParamValue.Dispose();
                hv_SampleKeys.Dispose();
                hv_ResultKeys.Dispose();
                hv_ImageIDExists.Dispose();
                hv_ImageID.Dispose();
                hv_ImageIDString.Dispose();
                hv_NeededKeys.Dispose();
                hv_Index.Dispose();
                hv_DLDatasetInfoKeys.Dispose();
                hv_ClassNames.Dispose();
                hv_ClassIDs.Dispose();
                hv_Colors.Dispose();
                hv_ClassesLegend.Dispose();
                hv_PrevWindowCoordinates.Dispose();
                hv_Keys.Dispose();
                hv_Exception.Dispose();
                hv_MetaInfoIndex.Dispose();
                hv_MetaInfo.Dispose();
                hv_FlushValues.Dispose();
                hv_WindowHandleKeys.Dispose();
                hv_KeyIndex.Dispose();
                hv_WindowHandles.Dispose();
                hv_WindowIndex.Dispose();
                hv_FlushValue.Dispose();
                hv_WidthImage.Dispose();
                hv_HeightImage.Dispose();
                hv_CurrentWindowHandle.Dispose();
                hv_WindowImageRatio.Dispose();
                hv_BboxIDs.Dispose();
                hv_Text.Dispose();
                hv_BboxColors.Dispose();
                hv_BboxIDsUniq.Dispose();
                hv_BboxConfidences.Dispose();
                hv_TextConf.Dispose();
                hv_BboxClassIndex.Dispose();
                hv_BboxColorsResults.Dispose();
                hv_BboxClassIndexUniq.Dispose();
                hv_BboxLabelIndex.Dispose();
                hv_BboxColorsBoth.Dispose();
                hv_BboxClassLabelIndexUniq.Dispose();
                hv_ColorsSegmentation.Dispose();
                hv_DrawMode.Dispose();
                hv_Width.Dispose();
                hv_ImageClassIDs.Dispose();
                hv_ImageClassIDsUniq.Dispose();
                hv_ColorsResults.Dispose();
                hv_GroundTruthIDs.Dispose();
                hv_ResultIDs.Dispose();
                hv_StringSegExcludeClassIDs.Dispose();
                hv_StringIndex.Dispose();
                hv_Min.Dispose();
                hv_Max.Dispose();
                hv_Range.Dispose();
                hv_MinWeight.Dispose();
                hv_WeightsColors.Dispose();
                hv_ConfidenceColors.Dispose();
                hv_WindowHandleKeysNew.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Deep Learning / Model
        // Short Description: Close all windows whose handle is contained in the dictionary WindowHandleDict. 
        public void dev_display_dl_data_close_windows(HTuple hv_WindowHandleDict)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_WindowHandleKeys = new HTuple();
            HTuple hv_Index = new HTuple(), hv_WindowHandles = new HTuple();
            HTuple hv_WindowIndex = new HTuple(), hv_Exception = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //
                //This procedure closes all window handles
                //that are contained in the dictionary WindowHandleDict.
                //
                hv_WindowHandleKeys.Dispose();
                HOperatorSet.GetDictParam(hv_WindowHandleDict, "keys", new HTuple(), out hv_WindowHandleKeys);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_WindowHandleKeys = hv_WindowHandleKeys.TupleRemove(
                            hv_WindowHandleKeys.TupleFind("meta_information"));
                        hv_WindowHandleKeys.Dispose();
                        hv_WindowHandleKeys = ExpTmpLocalVar_WindowHandleKeys;
                    }
                }
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_WindowHandleKeys.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_WindowHandles.Dispose();
                        HOperatorSet.GetDictTuple(hv_WindowHandleDict, hv_WindowHandleKeys.TupleSelect(
                            hv_Index), out hv_WindowHandles);
                    }
                    for (hv_WindowIndex = 0; (int)hv_WindowIndex <= (int)((new HTuple(hv_WindowHandles.TupleLength()
                        )) - 1); hv_WindowIndex = (int)hv_WindowIndex + 1)
                    {
                        //Since there are not only window handles saved in the dictionary, use try-catch.
                        try
                        {
                            HDevWindowStack.SetActive(hv_WindowHandles.TupleSelect(
                                hv_WindowIndex));
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.CloseWindow(HDevWindowStack.Pop());
                            }
                        }
                        // catch (Exception) 
                        catch (HalconException HDevExpDefaultException1)
                        {
                            HDevExpDefaultException1.ToHTuple(out hv_Exception);
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.RemoveDictKey(hv_WindowHandleDict, hv_WindowHandleKeys.TupleSelect(
                            hv_Index));
                    }
                }
                //

                hv_WindowHandleKeys.Dispose();
                hv_Index.Dispose();
                hv_WindowHandles.Dispose();
                hv_WindowIndex.Dispose();
                hv_Exception.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_WindowHandleKeys.Dispose();
                hv_Index.Dispose();
                hv_WindowHandles.Dispose();
                hv_WindowIndex.Dispose();
                hv_Exception.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Graphics / Output
        // Short Description: Display the ground truth bounding boxes of DLSample. 
        public void dev_display_ground_truth_detection(HTuple hv_DLSample, HTuple hv_SampleKeys,
            HTuple hv_LineWidthBbox, HTuple hv_ClassIDs, HTuple hv_BboxColors, HTuple hv_BboxLabelColor,
            HTuple hv_TextColor, HTuple hv_ShowLabels, HTuple hv_WindowHandle, out HTuple hv_BboxIDs)
        {



            // Local iconic variables 

            HObject ho_BboxRegion = null, ho_RectangleSelected = null;

            // Local control variables 

            HTuple hv_BboxRow1 = new HTuple(), hv_BboxCol1 = new HTuple();
            HTuple hv_BboxRow2 = new HTuple(), hv_BboxCol2 = new HTuple();
            HTuple hv_BboxLabels = new HTuple(), hv_DrawMode = new HTuple();
            HTuple hv_BboxClassIDs = new HTuple(), hv_IndexBbox = new HTuple();
            HTuple hv_ClassID = new HTuple(), hv_TxtColor = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_BboxRegion);
            HOperatorSet.GenEmptyObj(out ho_RectangleSelected);
            hv_BboxIDs = new HTuple();
            try
            {
                //
                //This procedure displays the ground truth bounding boxes of DLSample.
                //
                if ((int)(new HTuple(((hv_SampleKeys.TupleFind("bbox_row1"))).TupleNotEqual(
                    -1))) != 0)
                {
                    hv_BboxRow1.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLSample, "bbox_row1", out hv_BboxRow1);
                    hv_BboxCol1.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLSample, "bbox_col1", out hv_BboxCol1);
                    hv_BboxRow2.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLSample, "bbox_row2", out hv_BboxRow2);
                    hv_BboxCol2.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLSample, "bbox_col2", out hv_BboxCol2);
                    hv_BboxLabels.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLSample, "bbox_label_id", out hv_BboxLabels);
                }
                else
                {
                    throw new HalconException("Ground truth bounding box data could not be found in DLSample.");
                }
                if ((int)(new HTuple((new HTuple(hv_BboxLabels.TupleLength())).TupleGreater(
                    0))) != 0)
                {
                    //Generate bounding box regions. (Convert from pixel centered, subpixel-precise to pixel-precise format).
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_BboxRegion.Dispose();
                        HOperatorSet.GenRectangle1(out ho_BboxRegion, hv_BboxRow1 + .5, hv_BboxCol1 + .5,
                            hv_BboxRow2 - .5, hv_BboxCol2 - .5);
                    }
                    //
                    hv_DrawMode.Dispose();
                    HOperatorSet.GetDraw(hv_WindowHandle, out hv_DrawMode);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetDraw(HDevWindowStack.GetActive(), "fill");
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), hv_LineWidthBbox);
                    }
                    //
                    //Collect the ClassIDs of the bounding boxes.
                    hv_BboxIDs.Dispose();
                    hv_BboxIDs = new HTuple();
                    hv_BboxClassIDs.Dispose();
                    hv_BboxClassIDs = new HTuple();
                    //
                    //Draw the bounding boxes.
                    for (hv_IndexBbox = 0; (int)hv_IndexBbox <= (int)((new HTuple(hv_BboxRow1.TupleLength()
                        )) - 1); hv_IndexBbox = (int)hv_IndexBbox + 1)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_RectangleSelected.Dispose();
                            HOperatorSet.SelectObj(ho_BboxRegion, out ho_RectangleSelected, hv_IndexBbox + 1);
                        }
                        hv_ClassID.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ClassID = hv_ClassIDs.TupleFind(
                                hv_BboxLabels.TupleSelect(hv_IndexBbox));
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_BboxClassIDs = hv_BboxClassIDs.TupleConcat(
                                    hv_ClassID);
                                hv_BboxClassIDs.Dispose();
                                hv_BboxClassIDs = ExpTmpLocalVar_BboxClassIDs;
                            }
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_BboxIDs = hv_BboxIDs.TupleConcat(
                                    hv_ClassID);
                                hv_BboxIDs.Dispose();
                                hv_BboxIDs = ExpTmpLocalVar_BboxIDs;
                            }
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetColor(HDevWindowStack.GetActive(), (hv_BboxColors.TupleSelect(
                                    hv_ClassID)) + "60");
                            }
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_RectangleSelected, HDevWindowStack.GetActive()
                                );
                        }
                    }
                    //
                    //Draw text of bounding boxes.
                    if ((int)(hv_ShowLabels) != 0)
                    {
                        //For better visibility the text is displayed after all bounding boxes are drawn.
                        for (hv_IndexBbox = 0; (int)hv_IndexBbox <= (int)((new HTuple(hv_BboxRow1.TupleLength()
                            )) - 1); hv_IndexBbox = (int)hv_IndexBbox + 1)
                        {
                            hv_ClassID.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ClassID = hv_BboxClassIDs.TupleSelect(
                                    hv_IndexBbox);
                            }
                            if ((int)(new HTuple(hv_TextColor.TupleEqual(""))) != 0)
                            {
                                hv_TxtColor.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_TxtColor = hv_BboxColors.TupleSelect(
                                        hv_ClassID);
                                }
                            }
                            else
                            {
                                hv_TxtColor.Dispose();
                                hv_TxtColor = new HTuple(hv_TextColor);
                            }
                            if (HDevWindowStack.IsOpen())
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_BboxLabels.TupleSelect(
                                        hv_IndexBbox), "image", hv_BboxRow1.TupleSelect(hv_IndexBbox),
                                        hv_BboxCol1.TupleSelect(hv_IndexBbox), hv_TextColor, ((new HTuple("box_color")).TupleConcat(
                                        "shadow")).TupleConcat("border_radius"), hv_BboxLabelColor.TupleConcat(
                                        (new HTuple("false")).TupleConcat(0)));
                                }
                            }
                        }
                    }
                    //
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetDraw(HDevWindowStack.GetActive(), hv_DrawMode);
                    }
                }
                else
                {
                    //Do nothing if there are no ground truth bounding boxes.
                    hv_BboxIDs.Dispose();
                    hv_BboxIDs = new HTuple();
                }
                ho_BboxRegion.Dispose();
                ho_RectangleSelected.Dispose();

                hv_BboxRow1.Dispose();
                hv_BboxCol1.Dispose();
                hv_BboxRow2.Dispose();
                hv_BboxCol2.Dispose();
                hv_BboxLabels.Dispose();
                hv_DrawMode.Dispose();
                hv_BboxClassIDs.Dispose();
                hv_IndexBbox.Dispose();
                hv_ClassID.Dispose();
                hv_TxtColor.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_BboxRegion.Dispose();
                ho_RectangleSelected.Dispose();

                hv_BboxRow1.Dispose();
                hv_BboxCol1.Dispose();
                hv_BboxRow2.Dispose();
                hv_BboxCol2.Dispose();
                hv_BboxLabels.Dispose();
                hv_DrawMode.Dispose();
                hv_BboxClassIDs.Dispose();
                hv_IndexBbox.Dispose();
                hv_ClassID.Dispose();
                hv_TxtColor.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Graphics / Output
        // Short Description: Display a color bar next to an image. 
        public void dev_display_map_color_bar(HTuple hv_ImageWidth, HTuple hv_ImageHeight,
            HTuple hv_MapColorBarWidth, HTuple hv_Colors, HTuple hv_MaxValue, HTuple hv_WindowImageRatio,
            HTuple hv_WindowHandle)
        {



            // Local iconic variables 

            HObject ho_Rectangle = null;

            // Local control variables 

            HTuple hv_ClipRegion = new HTuple(), hv_ColorIndex = new HTuple();
            HTuple hv_RectHeight = new HTuple(), hv_DrawMode = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
            HTuple hv_Row2 = new HTuple(), hv_Column2 = new HTuple();
            HTuple hv__ = new HTuple(), hv_TextHeight = new HTuple();
            HTuple hv_Index = new HTuple(), hv_Text = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            try
            {
                //
                //This procedure displays a color bar next to the image
                //specified with ImageWidth and ImageHeight.
                //
                hv_ClipRegion.Dispose();
                HOperatorSet.GetSystem("clip_region", out hv_ClipRegion);
                HOperatorSet.SetSystem("clip_region", "false");
                //
                //Display the color bar.
                hv_ColorIndex.Dispose();
                hv_ColorIndex = 0;
                hv_RectHeight.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_RectHeight = (1.0 * hv_ImageHeight) / (new HTuple(hv_Colors.TupleLength()
                        ));
                }
                //Set draw mode to fill
                hv_DrawMode.Dispose();
                HOperatorSet.GetDraw(hv_WindowHandle, out hv_DrawMode);
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetDraw(HDevWindowStack.GetActive(), "fill");
                }
                HTuple end_val13 = 0;
                HTuple step_val13 = -hv_RectHeight;
                for (hv_Row = hv_ImageHeight - 1; hv_Row.Continue(end_val13, step_val13); hv_Row = hv_Row.TupleAdd(step_val13))
                {
                    //The color bar consists of multiple rectangle1.
                    hv_Row1.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Row1 = hv_Row - hv_RectHeight;
                    }
                    hv_Column1.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Column1 = hv_ImageWidth + (20 / hv_WindowImageRatio);
                    }
                    hv_Row2.Dispose();
                    hv_Row2 = new HTuple(hv_Row);
                    hv_Column2.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Column2 = (hv_ImageWidth + 20) + (hv_MapColorBarWidth / hv_WindowImageRatio);
                    }
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, hv_Row1, hv_Column1, hv_Row2,
                        hv_Column2);
                    if (HDevWindowStack.IsOpen())
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            HOperatorSet.SetColor(HDevWindowStack.GetActive(), hv_Colors.TupleSelect(
                                hv_ColorIndex));
                        }
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispObj(ho_Rectangle, HDevWindowStack.GetActive());
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_ColorIndex = hv_ColorIndex + 1;
                            hv_ColorIndex.Dispose();
                            hv_ColorIndex = ExpTmpLocalVar_ColorIndex;
                        }
                    }
                }
                //
                //Display labels for color bar.
                hv__.Dispose(); hv__.Dispose(); hv__.Dispose(); hv_TextHeight.Dispose();
                HOperatorSet.GetStringExtents(hv_WindowHandle, "0123456789", out hv__, out hv__,
                    out hv__, out hv_TextHeight);
                for (hv_Index = (double)(0); (double)hv_Index <= 1; hv_Index = (double)hv_Index + 0.2)
                {
                    hv_Text.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Text = ((hv_MaxValue - (hv_Index * hv_MaxValue))).TupleString(
                          ".1f");
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "image", hv_Index * (hv_ImageHeight - (2 * (hv_TextHeight / hv_WindowImageRatio))),
                                hv_ImageWidth + (40 / hv_WindowImageRatio), "black", "box", "false");
                        }
                    }
                }
                //
                HOperatorSet.SetSystem("clip_region", hv_ClipRegion);
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetDraw(HDevWindowStack.GetActive(), hv_DrawMode);
                }
                ho_Rectangle.Dispose();

                hv_ClipRegion.Dispose();
                hv_ColorIndex.Dispose();
                hv_RectHeight.Dispose();
                hv_DrawMode.Dispose();
                hv_Row.Dispose();
                hv_Row1.Dispose();
                hv_Column1.Dispose();
                hv_Row2.Dispose();
                hv_Column2.Dispose();
                hv__.Dispose();
                hv_TextHeight.Dispose();
                hv_Index.Dispose();
                hv_Text.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Rectangle.Dispose();

                hv_ClipRegion.Dispose();
                hv_ColorIndex.Dispose();
                hv_RectHeight.Dispose();
                hv_DrawMode.Dispose();
                hv_Row.Dispose();
                hv_Row1.Dispose();
                hv_Column1.Dispose();
                hv_Row2.Dispose();
                hv_Column2.Dispose();
                hv__.Dispose();
                hv_TextHeight.Dispose();
                hv_Index.Dispose();
                hv_Text.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Graphics / Output
        // Short Description: Display result bounding boxes. 
        public void dev_display_result_detection(HTuple hv_DLResult, HTuple hv_ResultKeys,
            HTuple hv_LineWidthBbox, HTuple hv_ClassIDs, HTuple hv_TextConf, HTuple hv_Colors,
            HTuple hv_BoxLabelColor, HTuple hv_WindowImageRatio, HTuple hv_TextPositionRow,
            HTuple hv_TextColor, HTuple hv_ShowLabels, HTuple hv_WindowHandle, out HTuple hv_BboxIDs)
        {



            // Local iconic variables 

            HObject ho_Rectangle = null, ho_RectangleSelected = null;

            // Local control variables 

            HTuple hv_BboxRow1 = new HTuple(), hv_BboxCol1 = new HTuple();
            HTuple hv_BboxRow2 = new HTuple(), hv_BboxCol2 = new HTuple();
            HTuple hv_BboxClasses = new HTuple(), hv_DrawMode = new HTuple();
            HTuple hv_Style = new HTuple(), hv_BboxClassIDs = new HTuple();
            HTuple hv_IndexBbox = new HTuple(), hv_ClassID = new HTuple();
            HTuple hv_LineWidth = new HTuple(), hv_Text = new HTuple();
            HTuple hv_Ascent = new HTuple(), hv_Descent = new HTuple();
            HTuple hv__ = new HTuple(), hv_TextRow = new HTuple();
            HTuple hv_TxtColor = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_RectangleSelected);
            hv_BboxIDs = new HTuple();
            try
            {
                //
                //This procedure displays the bounding boxes defined by DLResult.
                //The ClassIDs are necessary to display bounding boxes from the same class
                //always with the same color.
                //
                if ((int)(new HTuple(((hv_ResultKeys.TupleFind("bbox_class_id"))).TupleNotEqual(
                    -1))) != 0)
                {
                    hv_BboxRow1.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLResult, "bbox_row1", out hv_BboxRow1);
                    hv_BboxCol1.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLResult, "bbox_col1", out hv_BboxCol1);
                    hv_BboxRow2.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLResult, "bbox_row2", out hv_BboxRow2);
                    hv_BboxCol2.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLResult, "bbox_col2", out hv_BboxCol2);
                    hv_BboxClasses.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLResult, "bbox_class_id", out hv_BboxClasses);
                }
                else
                {
                    throw new HalconException("Result bounding box data could not be found in DLResult.");
                }
                if ((int)(new HTuple((new HTuple(hv_BboxClasses.TupleLength())).TupleGreater(
                    0))) != 0)
                {
                    //Generate bbox regions. (Convert from XLD to pixel-precise format).
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_Rectangle.Dispose();
                        HOperatorSet.GenRectangle1(out ho_Rectangle, hv_BboxRow1 + .5, hv_BboxCol1 + .5,
                            hv_BboxRow2 - .5, hv_BboxCol2 - .5);
                    }
                    //
                    hv_DrawMode.Dispose();
                    HOperatorSet.GetDraw(hv_WindowHandle, out hv_DrawMode);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetDraw(HDevWindowStack.GetActive(), "margin");
                    }
                    hv_Style.Dispose();
                    HOperatorSet.GetLineStyle(hv_WindowHandle, out hv_Style);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), hv_LineWidthBbox);
                    }
                    //
                    //Collect ClassIDs of the bounding boxes.
                    hv_BboxIDs.Dispose();
                    hv_BboxIDs = new HTuple();
                    hv_BboxClassIDs.Dispose();
                    hv_BboxClassIDs = new HTuple();
                    //
                    //Draw bounding boxes.
                    for (hv_IndexBbox = 0; (int)hv_IndexBbox <= (int)((new HTuple(hv_BboxRow1.TupleLength()
                        )) - 1); hv_IndexBbox = (int)hv_IndexBbox + 1)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_RectangleSelected.Dispose();
                            HOperatorSet.SelectObj(ho_Rectangle, out ho_RectangleSelected, hv_IndexBbox + 1);
                        }
                        hv_ClassID.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ClassID = hv_ClassIDs.TupleFind(
                                hv_BboxClasses.TupleSelect(hv_IndexBbox));
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_BboxClassIDs = hv_BboxClassIDs.TupleConcat(
                                    hv_ClassID);
                                hv_BboxClassIDs.Dispose();
                                hv_BboxClassIDs = ExpTmpLocalVar_BboxClassIDs;
                            }
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_BboxIDs = hv_BboxIDs.TupleConcat(
                                    hv_ClassID);
                                hv_BboxIDs.Dispose();
                                hv_BboxIDs = ExpTmpLocalVar_BboxIDs;
                            }
                        }
                        hv_LineWidth.Dispose();
                        HOperatorSet.GetLineWidth(hv_WindowHandle, out hv_LineWidth);
                        if (HDevWindowStack.IsOpen())
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), ((hv_LineWidth + 2)).TupleInt()
                                    );
                            }
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetColor(HDevWindowStack.GetActive(), "black");
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_RectangleSelected, HDevWindowStack.GetActive()
                                );
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), hv_LineWidth.TupleInt()
                                    );
                            }
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetColor(HDevWindowStack.GetActive(), hv_Colors.TupleSelect(
                                    hv_ClassID));
                            }
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_RectangleSelected, HDevWindowStack.GetActive()
                                );
                        }
                    }
                    //
                    //Draw text of bounding boxes.
                    if ((int)(hv_ShowLabels) != 0)
                    {
                        //For better visibility the text is displayed after all bboxes are drawn.
                        for (hv_IndexBbox = 0; (int)hv_IndexBbox <= (int)((new HTuple(hv_BboxRow1.TupleLength()
                            )) - 1); hv_IndexBbox = (int)hv_IndexBbox + 1)
                        {
                            hv_ClassID.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_ClassID = hv_BboxClassIDs.TupleSelect(
                                    hv_IndexBbox);
                            }
                            hv_Text.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_Text = (hv_BboxClasses.TupleSelect(
                                    hv_IndexBbox)) + (hv_TextConf.TupleSelect(hv_IndexBbox));
                            }
                            hv_Ascent.Dispose(); hv_Descent.Dispose(); hv__.Dispose(); hv__.Dispose();
                            HOperatorSet.GetStringExtents(hv_WindowHandle, hv_Text, out hv_Ascent,
                                out hv_Descent, out hv__, out hv__);
                            if ((int)(new HTuple(hv_TextPositionRow.TupleEqual("bottom"))) != 0)
                            {
                                hv_TextRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_TextRow = (hv_BboxRow2.TupleSelect(
                                        hv_IndexBbox)) - ((hv_Ascent + hv_Descent) / hv_WindowImageRatio);
                                }
                            }
                            else
                            {
                                hv_TextRow.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_TextRow = hv_BboxRow1.TupleSelect(
                                        hv_IndexBbox);
                                }
                            }
                            if ((int)(new HTuple(hv_TextColor.TupleEqual(""))) != 0)
                            {
                                hv_TxtColor.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_TxtColor = hv_Colors.TupleSelect(
                                        hv_ClassID);
                                }
                            }
                            else
                            {
                                hv_TxtColor.Dispose();
                                hv_TxtColor = new HTuple(hv_TextColor);
                            }
                            if (HDevWindowStack.IsOpen())
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "image",
                                        hv_TextRow, hv_BboxCol1.TupleSelect(hv_IndexBbox), hv_TxtColor,
                                        ((new HTuple("box_color")).TupleConcat("shadow")).TupleConcat("border_radius"),
                                        hv_BoxLabelColor.TupleConcat((new HTuple("false")).TupleConcat(
                                        0)));
                                }
                            }
                        }
                    }
                    //
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetDraw(HDevWindowStack.GetActive(), hv_DrawMode);
                    }
                    HOperatorSet.SetLineStyle(hv_WindowHandle, hv_Style);
                }
                else
                {
                    //Do nothing if no results are present.
                    hv_BboxIDs.Dispose();
                    hv_BboxIDs = new HTuple();
                }
                ho_Rectangle.Dispose();
                ho_RectangleSelected.Dispose();

                hv_BboxRow1.Dispose();
                hv_BboxCol1.Dispose();
                hv_BboxRow2.Dispose();
                hv_BboxCol2.Dispose();
                hv_BboxClasses.Dispose();
                hv_DrawMode.Dispose();
                hv_Style.Dispose();
                hv_BboxClassIDs.Dispose();
                hv_IndexBbox.Dispose();
                hv_ClassID.Dispose();
                hv_LineWidth.Dispose();
                hv_Text.Dispose();
                hv_Ascent.Dispose();
                hv_Descent.Dispose();
                hv__.Dispose();
                hv_TextRow.Dispose();
                hv_TxtColor.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Rectangle.Dispose();
                ho_RectangleSelected.Dispose();

                hv_BboxRow1.Dispose();
                hv_BboxCol1.Dispose();
                hv_BboxRow2.Dispose();
                hv_BboxCol2.Dispose();
                hv_BboxClasses.Dispose();
                hv_DrawMode.Dispose();
                hv_Style.Dispose();
                hv_BboxClassIDs.Dispose();
                hv_IndexBbox.Dispose();
                hv_ClassID.Dispose();
                hv_LineWidth.Dispose();
                hv_Text.Dispose();
                hv_Ascent.Dispose();
                hv_Descent.Dispose();
                hv__.Dispose();
                hv_TextRow.Dispose();
                hv_TxtColor.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Graphics / Output
        // Short Description: Display the ground truth/result segmentation as regions. 
        public void dev_display_segmentation_regions(HObject ho_SegmentationImage, HTuple hv_ClassIDs,
            HTuple hv_ColorsSegmentation, HTuple hv_ExcludeClassIDs, out HTuple hv_ImageClassIDs)
        {




            // Local iconic variables 

            HObject ho_Regions, ho_SelectedRegion = null;

            // Local control variables 

            HTuple hv_IncludedClassIDs = new HTuple();
            HTuple hv_Area = new HTuple(), hv_Index = new HTuple();
            HTuple hv_ClassID = new HTuple(), hv_IndexColor = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegion);
            hv_ImageClassIDs = new HTuple();
            try
            {
                //
                //This procedure displays the ground truth/result segmentation
                //given in SegmentationImage as regions. The ClassIDs are necessary to
                //display ground truth/result segmentations from the same class
                //always with the same color. It is possible to exclude certain ClassIDs
                //from being displayed. The displayed classes are returned in ImageClassIDs.
                //
                //
                //Remove excluded class IDs from the list.
                hv_IncludedClassIDs.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IncludedClassIDs = hv_ClassIDs.TupleDifference(
                        hv_ExcludeClassIDs);
                }
                //
                //Get a region for each class ID.
                ho_Regions.Dispose();
                HOperatorSet.Threshold(ho_SegmentationImage, out ho_Regions, hv_IncludedClassIDs,
                    hv_IncludedClassIDs);
                //
                //Get classes with non-empty regions.
                hv_Area.Dispose();
                HOperatorSet.RegionFeatures(ho_Regions, "area", out hv_Area);
                hv_ImageClassIDs.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_ImageClassIDs = hv_IncludedClassIDs.TupleSelectMask(
                        hv_Area.TupleGreaterElem(0));
                }
                //
                //Display all non-empty class regions in distinct colors.
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_IncludedClassIDs.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)(new HTuple(((hv_Area.TupleSelect(hv_Index))).TupleGreater(0))) != 0)
                    {
                        //Use class ID to determine region color.
                        hv_ClassID.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ClassID = hv_IncludedClassIDs.TupleSelect(
                                hv_Index);
                        }
                        hv_IndexColor.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_IndexColor = hv_ClassIDs.TupleFindFirst(
                                hv_ClassID);
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetColor(HDevWindowStack.GetActive(), hv_ColorsSegmentation.TupleSelect(
                                    hv_IndexColor));
                            }
                        }
                        //Display the segmentation region.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_SelectedRegion.Dispose();
                            HOperatorSet.SelectObj(ho_Regions, out ho_SelectedRegion, hv_Index + 1);
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispObj(ho_SelectedRegion, HDevWindowStack.GetActive());
                        }
                    }
                }
                ho_Regions.Dispose();
                ho_SelectedRegion.Dispose();

                hv_IncludedClassIDs.Dispose();
                hv_Area.Dispose();
                hv_Index.Dispose();
                hv_ClassID.Dispose();
                hv_IndexColor.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Regions.Dispose();
                ho_SelectedRegion.Dispose();

                hv_IncludedClassIDs.Dispose();
                hv_Area.Dispose();
                hv_Index.Dispose();
                hv_ClassID.Dispose();
                hv_IndexColor.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Graphics / Output
        // Short Description: Display a map of weights. 
        public void dev_display_weight_regions(HObject ho_ImageWeight, HTuple hv_DrawTransparency,
            HTuple hv_SegMaxWeight, out HTuple hv_Colors)
        {




            // Local iconic variables 

            HObject ho_Domain, ho_WeightsRegion = null;

            // Local control variables 

            HTuple hv_NumColors = new HTuple(), hv_WeightsColorsAlpha = new HTuple();
            HTuple hv_Rows = new HTuple(), hv_Columns = new HTuple();
            HTuple hv_GrayVal = new HTuple(), hv_GrayValWeight = new HTuple();
            HTuple hv_ColorIndex = new HTuple(), hv_ClassColor = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Domain);
            HOperatorSet.GenEmptyObj(out ho_WeightsRegion);
            hv_Colors = new HTuple();
            try
            {
                //
                //This procedure displays a map of the weights
                //given in ImageWeight as regions.
                //The transparency can be adjusted.
                //The used colors are returned.
                //
                //Define colors.
                hv_NumColors.Dispose();
                hv_NumColors = 20;
                hv_Colors.Dispose();
                get_distinct_colors(hv_NumColors, 0, 0, 160, out hv_Colors);
                {
                    HTuple ExpTmpOutVar_0;
                    HOperatorSet.TupleInverse(hv_Colors, out ExpTmpOutVar_0);
                    hv_Colors.Dispose();
                    hv_Colors = ExpTmpOutVar_0;
                }
                hv_WeightsColorsAlpha.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WeightsColorsAlpha = hv_Colors + hv_DrawTransparency;
                }
                //
                //Get gay values of ImageWeight.
                ho_Domain.Dispose();
                HOperatorSet.GetDomain(ho_ImageWeight, out ho_Domain);
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_Domain, out hv_Rows, out hv_Columns);
                hv_GrayVal.Dispose();
                HOperatorSet.GetGrayval(ho_ImageWeight, hv_Rows, hv_Columns, out hv_GrayVal);
                //
                //Check that the gray values of the image
                //are below the specified maximum.
                if ((int)(new HTuple(((hv_GrayVal.TupleMax())).TupleGreater(hv_SegMaxWeight))) != 0)
                {
                    throw new HalconException(((("The maximum weight (" + (hv_GrayVal.TupleMax()
                        )) + ") in the weight image is greater than the given SegMaxWeight (") + hv_SegMaxWeight) + ").");
                }
                //
                while ((int)(new HTuple(hv_GrayVal.TupleNotEqual(new HTuple()))) != 0)
                {
                    //Go through all gray value 'groups',
                    //starting from the maximum.
                    hv_GrayValWeight.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_GrayValWeight = hv_GrayVal.TupleMax()
                            ;
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_GrayVal = hv_GrayVal.TupleRemove(
                                hv_GrayVal.TupleFind(hv_GrayValWeight));
                            hv_GrayVal.Dispose();
                            hv_GrayVal = ExpTmpLocalVar_GrayVal;
                        }
                    }
                    ho_WeightsRegion.Dispose();
                    HOperatorSet.Threshold(ho_ImageWeight, out ho_WeightsRegion, hv_GrayValWeight,
                        hv_GrayValWeight);
                    //
                    //Visualize the respective group.
                    hv_ColorIndex.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ColorIndex = (((((hv_GrayValWeight / hv_SegMaxWeight) * (hv_NumColors - 1))).TupleCeil()
                            )).TupleInt();
                    }
                    hv_ClassColor.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ClassColor = hv_WeightsColorsAlpha.TupleSelect(
                            hv_ColorIndex);
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetColor(HDevWindowStack.GetActive(), hv_ClassColor);
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispObj(ho_WeightsRegion, HDevWindowStack.GetActive());
                    }
                }
                ho_Domain.Dispose();
                ho_WeightsRegion.Dispose();

                hv_NumColors.Dispose();
                hv_WeightsColorsAlpha.Dispose();
                hv_Rows.Dispose();
                hv_Columns.Dispose();
                hv_GrayVal.Dispose();
                hv_GrayValWeight.Dispose();
                hv_ColorIndex.Dispose();
                hv_ClassColor.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Domain.Dispose();
                ho_WeightsRegion.Dispose();

                hv_NumColors.Dispose();
                hv_WeightsColorsAlpha.Dispose();
                hv_Rows.Dispose();
                hv_Columns.Dispose();
                hv_GrayVal.Dispose();
                hv_GrayValWeight.Dispose();
                hv_ColorIndex.Dispose();
                hv_ClassColor.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Develop
        // Short Description: Open a new graphics window that preserves the aspect ratio of the given image size. 
        public void dev_open_window_fit_size(HTuple hv_Row, HTuple hv_Column, HTuple hv_Width,
            HTuple hv_Height, HTuple hv_WidthLimit, HTuple hv_HeightLimit, out HTuple hv_WindowHandle)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_MinWidth = new HTuple(), hv_MaxWidth = new HTuple();
            HTuple hv_MinHeight = new HTuple(), hv_MaxHeight = new HTuple();
            HTuple hv_ResizeFactor = new HTuple(), hv_TempWidth = new HTuple();
            HTuple hv_TempHeight = new HTuple(), hv_WindowWidth = new HTuple();
            HTuple hv_WindowHeight = new HTuple();
            // Initialize local and output iconic variables 
            hv_WindowHandle = new HTuple();
            try
            {
                //This procedure open a new graphic window
                //such that it fits into the limits specified by WidthLimit
                //and HeightLimit, but also maintains the correct aspect ratio
                //given by Width and Height.
                //
                //If it is impossible to match the minimum and maximum extent requirements
                //at the same time (f.e. if the image is very long but narrow),
                //the maximum value gets a higher priority.
                //
                //Parse input tuple WidthLimit
                if ((int)((new HTuple((new HTuple(hv_WidthLimit.TupleLength())).TupleEqual(
                    0))).TupleOr(new HTuple(hv_WidthLimit.TupleLess(0)))) != 0)
                {
                    hv_MinWidth.Dispose();
                    hv_MinWidth = 500;
                    hv_MaxWidth.Dispose();
                    hv_MaxWidth = 800;
                }
                else if ((int)(new HTuple((new HTuple(hv_WidthLimit.TupleLength())).TupleEqual(
                    1))) != 0)
                {
                    hv_MinWidth.Dispose();
                    hv_MinWidth = 0;
                    hv_MaxWidth.Dispose();
                    hv_MaxWidth = new HTuple(hv_WidthLimit);
                }
                else
                {
                    hv_MinWidth.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_MinWidth = hv_WidthLimit.TupleSelect(
                            0);
                    }
                    hv_MaxWidth.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_MaxWidth = hv_WidthLimit.TupleSelect(
                            1);
                    }
                }
                //Parse input tuple HeightLimit
                if ((int)((new HTuple((new HTuple(hv_HeightLimit.TupleLength())).TupleEqual(
                    0))).TupleOr(new HTuple(hv_HeightLimit.TupleLess(0)))) != 0)
                {
                    hv_MinHeight.Dispose();
                    hv_MinHeight = 400;
                    hv_MaxHeight.Dispose();
                    hv_MaxHeight = 600;
                }
                else if ((int)(new HTuple((new HTuple(hv_HeightLimit.TupleLength())).TupleEqual(
                    1))) != 0)
                {
                    hv_MinHeight.Dispose();
                    hv_MinHeight = 0;
                    hv_MaxHeight.Dispose();
                    hv_MaxHeight = new HTuple(hv_HeightLimit);
                }
                else
                {
                    hv_MinHeight.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_MinHeight = hv_HeightLimit.TupleSelect(
                            0);
                    }
                    hv_MaxHeight.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_MaxHeight = hv_HeightLimit.TupleSelect(
                            1);
                    }
                }
                //
                //Test, if window size has to be changed.
                hv_ResizeFactor.Dispose();
                hv_ResizeFactor = 1;
                //First, expand window to the minimum extents (if necessary).
                if ((int)((new HTuple(hv_MinWidth.TupleGreater(hv_Width))).TupleOr(new HTuple(hv_MinHeight.TupleGreater(
                    hv_Height)))) != 0)
                {
                    hv_ResizeFactor.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ResizeFactor = (((((hv_MinWidth.TupleReal()
                            ) / hv_Width)).TupleConcat((hv_MinHeight.TupleReal()) / hv_Height))).TupleMax()
                            ;
                    }
                }
                hv_TempWidth.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_TempWidth = hv_Width * hv_ResizeFactor;
                }
                hv_TempHeight.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_TempHeight = hv_Height * hv_ResizeFactor;
                }
                //Then, shrink window to maximum extents (if necessary).
                if ((int)((new HTuple(hv_MaxWidth.TupleLess(hv_TempWidth))).TupleOr(new HTuple(hv_MaxHeight.TupleLess(
                    hv_TempHeight)))) != 0)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_ResizeFactor = hv_ResizeFactor * ((((((hv_MaxWidth.TupleReal()
                                ) / hv_TempWidth)).TupleConcat((hv_MaxHeight.TupleReal()) / hv_TempHeight))).TupleMin()
                                );
                            hv_ResizeFactor.Dispose();
                            hv_ResizeFactor = ExpTmpLocalVar_ResizeFactor;
                        }
                    }
                }
                hv_WindowWidth.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WindowWidth = hv_Width * hv_ResizeFactor;
                }
                hv_WindowHeight.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WindowHeight = hv_Height * hv_ResizeFactor;
                }
                //Resize window
                HOperatorSet.SetWindowAttr("background_color", "black");
                HOperatorSet.OpenWindow(hv_Row, hv_Column, hv_WindowWidth, hv_WindowHeight, 0, "visible", "", out hv_WindowHandle);
                HDevWindowStack.Push(hv_WindowHandle);
                if (HDevWindowStack.IsOpen())
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.SetPart(HDevWindowStack.GetActive(), 0, 0, hv_Height - 1, hv_Width - 1);
                    }
                }

                hv_MinWidth.Dispose();
                hv_MaxWidth.Dispose();
                hv_MinHeight.Dispose();
                hv_MaxHeight.Dispose();
                hv_ResizeFactor.Dispose();
                hv_TempWidth.Dispose();
                hv_TempHeight.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_MinWidth.Dispose();
                hv_MaxWidth.Dispose();
                hv_MinHeight.Dispose();
                hv_MaxHeight.Dispose();
                hv_ResizeFactor.Dispose();
                hv_TempWidth.Dispose();
                hv_TempHeight.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Develop
        // Short Description: Switch dev_update_pc, dev_update_var and dev_update_window to 'off'. 
        public void dev_update_off()
        {

            // Initialize local and output iconic variables 
            //This procedure sets different update settings to 'off'.
            //This is useful to get the best performance and reduce overhead.
            //
            // dev_update_pc(...); only in hdevelop
            // dev_update_var(...); only in hdevelop
            // dev_update_window(...); only in hdevelop


            return;
        }

        // Chapter: Deep Learning / Model
        // Short Description: Store the given images in a tuple of dictionaries DLSamples. 
        public void gen_dl_samples_from_images(HObject ho_Images, out HTuple hv_DLSampleBatch)
        {



            // Local iconic variables 

            HObject ho_Image = null;

            // Local control variables 

            HTuple hv_NumImages = new HTuple(), hv_ImageIndex = new HTuple();
            HTuple hv_DLSample = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            hv_DLSampleBatch = new HTuple();
            try
            {
                //
                //This procedure creates DLSampleBatch, a tuple
                //containing a dictionary DLSample
                //for every image given in Images.
                //
                //Initialize output tuple.
                hv_NumImages.Dispose();
                HOperatorSet.CountObj(ho_Images, out hv_NumImages);
                hv_DLSampleBatch.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_DLSampleBatch = HTuple.TupleGenConst(
                        hv_NumImages, -1);
                }
                //
                //Loop through all given images.
                HTuple end_val10 = hv_NumImages - 1;
                HTuple step_val10 = 1;
                for (hv_ImageIndex = 0; hv_ImageIndex.Continue(end_val10, step_val10); hv_ImageIndex = hv_ImageIndex.TupleAdd(step_val10))
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_Image.Dispose();
                        HOperatorSet.SelectObj(ho_Images, out ho_Image, hv_ImageIndex + 1);
                    }
                    //Create DLSample from image.
                    hv_DLSample.Dispose();
                    HOperatorSet.CreateDict(out hv_DLSample);
                    HOperatorSet.SetDictObject(ho_Image, hv_DLSample, "image");
                    //
                    //Collect the DLSamples.
                    if (hv_DLSampleBatch == null)
                        hv_DLSampleBatch = new HTuple();
                    hv_DLSampleBatch[hv_ImageIndex] = hv_DLSample;
                }
                ho_Image.Dispose();

                hv_NumImages.Dispose();
                hv_ImageIndex.Dispose();
                hv_DLSample.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Image.Dispose();

                hv_NumImages.Dispose();
                hv_ImageIndex.Dispose();
                hv_DLSample.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Graphics / Window
        public void get_child_window(HTuple hv_HeightImage, HTuple hv_Font, HTuple hv_FontSize,
            HTuple hv_Text, HTuple hv_PrevWindowCoordinates, HTuple hv_WindowHandleDict,
            HTuple hv_WindowHandleKey, out HTuple hv_WindowImageRatio, out HTuple hv_PrevWindowCoordinatesOut)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_OpenNewWindow = new HTuple(), hv_WindowHandles = new HTuple();
            HTuple hv_ParentWindowHandle = new HTuple(), hv_ChildWindowHandle = new HTuple();
            HTuple hv_Exception = new HTuple(), hv_MetaInfo = new HTuple();
            HTuple hv_WindowRow = new HTuple(), hv_WindowColumn = new HTuple();
            HTuple hv_WindowWidth = new HTuple(), hv_WindowHeight = new HTuple();
            // Initialize local and output iconic variables 
            hv_WindowImageRatio = new HTuple();
            hv_PrevWindowCoordinatesOut = new HTuple();
            try
            {
                //
                //This procedure returns the next child window that
                //is used for visualization. If ReuseWindows is true
                //and WindowHandleList is suitable, the window handles
                //that are passed over are used. Else, this procedure
                //opens a new window, either next to the last ones, or
                //in a new row.
                //
                //First, check if the requested window is already available.
                hv_OpenNewWindow.Dispose();
                hv_OpenNewWindow = 0;
                try
                {
                    hv_WindowHandles.Dispose();
                    HOperatorSet.GetDictTuple(hv_WindowHandleDict, hv_WindowHandleKey, out hv_WindowHandles);
                    hv_ParentWindowHandle.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ParentWindowHandle = hv_WindowHandles.TupleSelect(
                            0);
                    }
                    hv_ChildWindowHandle.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ChildWindowHandle = hv_WindowHandles.TupleSelect(
                            1);
                    }
                    //Check if window handle is valid.
                    try
                    {
                        HOperatorSet.FlushBuffer(hv_ChildWindowHandle);
                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException2)
                    {
                        HDevExpDefaultException2.ToHTuple(out hv_Exception);
                        //Since there is something wrong with the current window, create a new one.
                        hv_OpenNewWindow.Dispose();
                        hv_OpenNewWindow = 1;
                    }
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    hv_OpenNewWindow.Dispose();
                    hv_OpenNewWindow = 1;
                }
                //
                //Get next child window.
                if ((int)(hv_OpenNewWindow.TupleNot()) != 0)
                {
                    //
                    //If possible, reuse existing window handles.
                    HDevWindowStack.SetActive(hv_ChildWindowHandle);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.ClearWindow(HDevWindowStack.GetActive());
                    }
                    set_display_font(hv_ChildWindowHandle, hv_FontSize, hv_Font, "true", "false");
                    //
                    hv_MetaInfo.Dispose();
                    HOperatorSet.GetDictTuple(hv_WindowHandleDict, "meta_information", out hv_MetaInfo);
                    //
                    //Get previous window coordinates.
                    hv_WindowRow.Dispose(); hv_WindowColumn.Dispose(); hv_WindowWidth.Dispose(); hv_WindowHeight.Dispose();
                    HOperatorSet.GetWindowExtents(hv_ParentWindowHandle, out hv_WindowRow, out hv_WindowColumn,
                        out hv_WindowWidth, out hv_WindowHeight);
                    hv_WindowImageRatio.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_WindowImageRatio = hv_WindowHeight / (hv_HeightImage * 1.0);
                    }
                    //
                    try
                    {
                        //
                        //Get WindowImageRatio from parent window.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_WindowImageRatio.Dispose();
                            HOperatorSet.GetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_window_image_ratio",
                                out hv_WindowImageRatio);
                        }
                        //
                        //Get previous window coordinates.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_PrevWindowCoordinatesOut.Dispose();
                            HOperatorSet.GetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_child_window_coordinates",
                                out hv_PrevWindowCoordinatesOut);
                        }
                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException1)
                    {
                        HDevExpDefaultException1.ToHTuple(out hv_Exception);
                        //
                        //Set WindowImageRatio from parent window.
                        hv_WindowRow.Dispose(); hv_WindowColumn.Dispose(); hv_WindowWidth.Dispose(); hv_WindowHeight.Dispose();
                        HOperatorSet.GetWindowExtents(hv_ParentWindowHandle, out hv_WindowRow,
                            out hv_WindowColumn, out hv_WindowWidth, out hv_WindowHeight);
                        hv_WindowImageRatio.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_WindowImageRatio = hv_WindowHeight / (hv_HeightImage * 1.0);
                        }
                        //
                        //Set previous window coordinates.
                        if (hv_PrevWindowCoordinatesOut == null)
                            hv_PrevWindowCoordinatesOut = new HTuple();
                        hv_PrevWindowCoordinatesOut[0] = hv_WindowRow;
                        if (hv_PrevWindowCoordinatesOut == null)
                            hv_PrevWindowCoordinatesOut = new HTuple();
                        hv_PrevWindowCoordinatesOut[1] = hv_WindowColumn;
                        if (hv_PrevWindowCoordinatesOut == null)
                            hv_PrevWindowCoordinatesOut = new HTuple();
                        hv_PrevWindowCoordinatesOut[2] = hv_WindowWidth;
                        if (hv_PrevWindowCoordinatesOut == null)
                            hv_PrevWindowCoordinatesOut = new HTuple();
                        hv_PrevWindowCoordinatesOut[3] = hv_WindowHeight;
                    }
                }
                else
                {
                    //Open a new child window.
                    hv_ChildWindowHandle.Dispose(); hv_PrevWindowCoordinatesOut.Dispose();
                    open_child_window(hv_ParentWindowHandle, hv_Font, hv_FontSize, hv_Text, hv_PrevWindowCoordinates,
                        hv_WindowHandleDict, hv_WindowHandleKey, out hv_ChildWindowHandle, out hv_PrevWindowCoordinatesOut);
                    HOperatorSet.SetWindowParam(hv_ChildWindowHandle, "flush", "false");
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.SetDictTuple(hv_WindowHandleDict, hv_WindowHandleKey, hv_ParentWindowHandle.TupleConcat(
                            hv_ChildWindowHandle));
                    }
                }

                hv_OpenNewWindow.Dispose();
                hv_WindowHandles.Dispose();
                hv_ParentWindowHandle.Dispose();
                hv_ChildWindowHandle.Dispose();
                hv_Exception.Dispose();
                hv_MetaInfo.Dispose();
                hv_WindowRow.Dispose();
                hv_WindowColumn.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_OpenNewWindow.Dispose();
                hv_WindowHandles.Dispose();
                hv_ParentWindowHandle.Dispose();
                hv_ChildWindowHandle.Dispose();
                hv_Exception.Dispose();
                hv_MetaInfo.Dispose();
                hv_WindowRow.Dispose();
                hv_WindowColumn.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Deep Learning / Semantic Segmentation
        public void get_confidence_image(out HObject ho_ImageConfidence, HTuple hv_ResultKeys,
            HTuple hv_DLResult)
        {


            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageConfidence);
            //
            if ((int)(new HTuple(((hv_ResultKeys.TupleFind("segmentation_confidence"))).TupleNotEqual(
                -1))) != 0)
            {
                ho_ImageConfidence.Dispose();
                HOperatorSet.GetDictObject(out ho_ImageConfidence, hv_DLResult, "segmentation_confidence");
            }
            else if ((int)(new HTuple(((hv_ResultKeys.TupleFind("segmentation_confidences"))).TupleNotEqual(
                -1))) != 0)
            {
                ho_ImageConfidence.Dispose();
                HOperatorSet.GetDictObject(out ho_ImageConfidence, hv_DLResult, "segmentation_confidences");
            }
            else
            {
                throw new HalconException("Confidence image could not be found in DLSample.");
            }


            return;
        }

        // Chapter: Deep Learning / Model
        // Short Description: Generates NumColors distinct colors 
        public void get_distinct_colors(HTuple hv_NumColors, HTuple hv_Random, HTuple hv_StartColor,
            HTuple hv_EndColor, out HTuple hv_Colors)
        {



            // Local iconic variables 

            HObject ho_HLSImageH, ho_HLSImageL, ho_HLSImageS;
            HObject ho_ImageR, ho_ImageG, ho_ImageB;

            // Local control variables 

            HTuple hv_IsString = new HTuple(), hv_Hue = new HTuple();
            HTuple hv_Lightness = new HTuple(), hv_Saturation = new HTuple();
            HTuple hv_Rows = new HTuple(), hv_Columns = new HTuple();
            HTuple hv_Red = new HTuple(), hv_Green = new HTuple();
            HTuple hv_Blue = new HTuple();
            HTuple hv_EndColor_COPY_INP_TMP = new HTuple(hv_EndColor);
            HTuple hv_Random_COPY_INP_TMP = new HTuple(hv_Random);

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_HLSImageH);
            HOperatorSet.GenEmptyObj(out ho_HLSImageL);
            HOperatorSet.GenEmptyObj(out ho_HLSImageS);
            HOperatorSet.GenEmptyObj(out ho_ImageR);
            HOperatorSet.GenEmptyObj(out ho_ImageG);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            hv_Colors = new HTuple();
            try
            {
                //
                //We get distinct color-values first in HLS color-space.
                //Assumes hue [0, EndColor), lightness [0, 1), saturation [0, 1).
                //
                //Parameter checks.
                //NumColors.
                if ((int)(new HTuple(hv_NumColors.TupleLess(1))) != 0)
                {
                    throw new HalconException("NumColors should be at least 1");
                }
                if ((int)(((hv_NumColors.TupleIsInt())).TupleNot()) != 0)
                {
                    throw new HalconException("NumColors should be of type int");
                }
                if ((int)(new HTuple((new HTuple(hv_NumColors.TupleLength())).TupleNotEqual(
                    1))) != 0)
                {
                    throw new HalconException("NumColors should have length 1");
                }
                //Random.
                if ((int)((new HTuple(hv_Random_COPY_INP_TMP.TupleNotEqual(0))).TupleAnd(new HTuple(hv_Random_COPY_INP_TMP.TupleNotEqual(
                    1)))) != 0)
                {
                    hv_IsString.Dispose();
                    HOperatorSet.TupleIsString(hv_Random_COPY_INP_TMP, out hv_IsString);
                    if ((int)(hv_IsString) != 0)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_Random = (new HTuple(hv_Random_COPY_INP_TMP.TupleEqual(
                                    "true"))).TupleOr("false");
                                hv_Random_COPY_INP_TMP.Dispose();
                                hv_Random_COPY_INP_TMP = ExpTmpLocalVar_Random;
                            }
                        }
                    }
                    else
                    {
                        throw new HalconException("Random should be either true or false");
                    }
                }
                //StartColor.
                if ((int)(new HTuple((new HTuple(hv_StartColor.TupleLength())).TupleNotEqual(
                    1))) != 0)
                {
                    throw new HalconException("StartColor should have length 1");
                }
                if ((int)((new HTuple(hv_StartColor.TupleLess(0))).TupleOr(new HTuple(hv_StartColor.TupleGreater(
                    255)))) != 0)
                {
                    throw new HalconException(new HTuple("StartColor should be in the range [0, 255]"));
                }
                if ((int)(((hv_StartColor.TupleIsInt())).TupleNot()) != 0)
                {
                    throw new HalconException("StartColor should be of type int");
                }
                //EndColor.
                if ((int)(new HTuple((new HTuple(hv_EndColor_COPY_INP_TMP.TupleLength())).TupleNotEqual(
                    1))) != 0)
                {
                    throw new HalconException("EndColor should have length 1");
                }
                if ((int)((new HTuple(hv_EndColor_COPY_INP_TMP.TupleLess(0))).TupleOr(new HTuple(hv_EndColor_COPY_INP_TMP.TupleGreater(
                    255)))) != 0)
                {
                    throw new HalconException(new HTuple("EndColor should be in the range [0, 255]"));
                }
                if ((int)(((hv_EndColor_COPY_INP_TMP.TupleIsInt())).TupleNot()) != 0)
                {
                    throw new HalconException("EndColor should be of type int");
                }
                //
                //Color generation.
                if ((int)(new HTuple(hv_StartColor.TupleGreater(hv_EndColor_COPY_INP_TMP))) != 0)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_EndColor = hv_EndColor_COPY_INP_TMP + 255;
                            hv_EndColor_COPY_INP_TMP.Dispose();
                            hv_EndColor_COPY_INP_TMP = ExpTmpLocalVar_EndColor;
                        }
                    }
                }
                if ((int)(new HTuple(hv_NumColors.TupleNotEqual(1))) != 0)
                {
                    hv_Hue.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Hue = (hv_StartColor + (((((hv_EndColor_COPY_INP_TMP - hv_StartColor) * ((HTuple.TupleGenSequence(
                            0, hv_NumColors - 1, 1)).TupleReal())) / (((hv_NumColors - 1)).TupleReal()))).TupleInt()
                            )) % 255;
                    }
                }
                else
                {
                    hv_Hue.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Hue = ((hv_StartColor.TupleConcat(
                            hv_EndColor_COPY_INP_TMP))).TupleMean();
                    }
                }
                if ((int)(hv_Random_COPY_INP_TMP) != 0)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Hue = hv_Hue.TupleSelect(
                                (HTuple.TupleRand(hv_NumColors)).TupleSortIndex());
                            hv_Hue.Dispose();
                            hv_Hue = ExpTmpLocalVar_Hue;
                        }
                    }
                    hv_Lightness.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Lightness = ((((5.0 + HTuple.TupleRand(
                            hv_NumColors)) * 255.0) / 10.0)).TupleInt();
                    }
                    hv_Saturation.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Saturation = ((((9.0 + HTuple.TupleRand(
                            hv_NumColors)) * 255.0) / 10.0)).TupleInt();
                    }
                }
                else
                {
                    hv_Lightness.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Lightness = ((HTuple.TupleGenConst(
                            hv_NumColors, 0.55) * 255.0)).TupleInt();
                    }
                    hv_Saturation.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Saturation = ((HTuple.TupleGenConst(
                            hv_NumColors, 0.95) * 255.0)).TupleInt();
                    }
                }
                //
                //Write colors to a 3-channel image in order to transform easier.
                ho_HLSImageH.Dispose();
                HOperatorSet.GenImageConst(out ho_HLSImageH, "byte", 1, hv_NumColors);
                ho_HLSImageL.Dispose();
                HOperatorSet.GenImageConst(out ho_HLSImageL, "byte", 1, hv_NumColors);
                ho_HLSImageS.Dispose();
                HOperatorSet.GenImageConst(out ho_HLSImageS, "byte", 1, hv_NumColors);
                hv_Rows.Dispose(); hv_Columns.Dispose();
                HOperatorSet.GetRegionPoints(ho_HLSImageH, out hv_Rows, out hv_Columns);
                HOperatorSet.SetGrayval(ho_HLSImageH, hv_Rows, hv_Columns, hv_Hue);
                HOperatorSet.SetGrayval(ho_HLSImageL, hv_Rows, hv_Columns, hv_Lightness);
                HOperatorSet.SetGrayval(ho_HLSImageS, hv_Rows, hv_Columns, hv_Saturation);
                //
                //Convert from HLS to RGB.
                ho_ImageR.Dispose(); ho_ImageG.Dispose(); ho_ImageB.Dispose();
                HOperatorSet.TransToRgb(ho_HLSImageH, ho_HLSImageL, ho_HLSImageS, out ho_ImageR,
                    out ho_ImageG, out ho_ImageB, "hls");
                //
                //Get RGB-values and transform to Hex.
                hv_Red.Dispose();
                HOperatorSet.GetGrayval(ho_ImageR, hv_Rows, hv_Columns, out hv_Red);
                hv_Green.Dispose();
                HOperatorSet.GetGrayval(ho_ImageG, hv_Rows, hv_Columns, out hv_Green);
                hv_Blue.Dispose();
                HOperatorSet.GetGrayval(ho_ImageB, hv_Rows, hv_Columns, out hv_Blue);
                hv_Colors.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Colors = (("#" + (hv_Red.TupleString(
                        "02x"))) + (hv_Green.TupleString("02x"))) + (hv_Blue.TupleString("02x"));
                }
                ho_HLSImageH.Dispose();
                ho_HLSImageL.Dispose();
                ho_HLSImageS.Dispose();
                ho_ImageR.Dispose();
                ho_ImageG.Dispose();
                ho_ImageB.Dispose();

                hv_EndColor_COPY_INP_TMP.Dispose();
                hv_Random_COPY_INP_TMP.Dispose();
                hv_IsString.Dispose();
                hv_Hue.Dispose();
                hv_Lightness.Dispose();
                hv_Saturation.Dispose();
                hv_Rows.Dispose();
                hv_Columns.Dispose();
                hv_Red.Dispose();
                hv_Green.Dispose();
                hv_Blue.Dispose();

                return;

            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_HLSImageH.Dispose();
                ho_HLSImageL.Dispose();
                ho_HLSImageS.Dispose();
                ho_ImageR.Dispose();
                ho_ImageG.Dispose();
                ho_ImageB.Dispose();

                hv_EndColor_COPY_INP_TMP.Dispose();
                hv_Random_COPY_INP_TMP.Dispose();
                hv_IsString.Dispose();
                hv_Hue.Dispose();
                hv_Lightness.Dispose();
                hv_Saturation.Dispose();
                hv_Rows.Dispose();
                hv_Columns.Dispose();
                hv_Red.Dispose();
                hv_Green.Dispose();
                hv_Blue.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Deep Learning / Model
        // Short Description: Generates certain colors for different ClassNames 
        public void get_dl_class_colors(HTuple hv_ClassNames, out HTuple hv_Colors)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_NumColors = new HTuple(), hv_ColorsRainbow = new HTuple();
            HTuple hv_ClassNamesGood = new HTuple(), hv_IndexFind = new HTuple();
            HTuple hv_GoodIdx = new HTuple(), hv_CurrentColor = new HTuple();
            HTuple hv_GreenIdx = new HTuple();
            // Initialize local and output iconic variables 
            hv_Colors = new HTuple();
            try
            {
                //
                //This procedure returns for each class a certain color.
                //
                //Define distinct colors for the classes.
                hv_NumColors.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_NumColors = new HTuple(hv_ClassNames.TupleLength()
                        );
                }
                //Get distinct colors without randomness makes neighboring colors look very similar.
                //We use a workaround to get deterministic colors where subsequent colors are distinguishable.
                hv_ColorsRainbow.Dispose();
                get_distinct_colors(hv_NumColors, 0, 0, 200, out hv_ColorsRainbow);
                {
                    HTuple ExpTmpOutVar_0;
                    HOperatorSet.TupleInverse(hv_ColorsRainbow, out ExpTmpOutVar_0);
                    hv_ColorsRainbow.Dispose();
                    hv_ColorsRainbow = ExpTmpOutVar_0;
                }
                hv_Colors.Dispose();
                make_neighboring_colors_distinguishable(hv_ColorsRainbow, out hv_Colors);
                //If a class 'OK','ok', 'good' or 'GOOD' is present set this class to green.
                //Only the first occurrence found is set to a green shade.
                hv_ClassNamesGood.Dispose();
                hv_ClassNamesGood = new HTuple();
                hv_ClassNamesGood[0] = "good";
                hv_ClassNamesGood[1] = "GOOD";
                hv_ClassNamesGood[2] = "ok";
                hv_ClassNamesGood[3] = "OK";
                for (hv_IndexFind = 0; (int)hv_IndexFind <= (int)((new HTuple(hv_ClassNamesGood.TupleLength()
                    )) - 1); hv_IndexFind = (int)hv_IndexFind + 1)
                {
                    hv_GoodIdx.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_GoodIdx = hv_ClassNames.TupleFindFirst(
                            hv_ClassNamesGood.TupleSelect(hv_IndexFind));
                    }
                    if ((int)((new HTuple(hv_GoodIdx.TupleNotEqual(-1))).TupleAnd(new HTuple((new HTuple(hv_ClassNames.TupleLength()
                        )).TupleLessEqual(8)))) != 0)
                    {
                        //If number of classes is <= 8, swap color with a green color.
                        hv_CurrentColor.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_CurrentColor = hv_Colors.TupleSelect(
                                hv_GoodIdx);
                        }
                        hv_GreenIdx.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_GreenIdx = (new HTuple((new HTuple(hv_ClassNames.TupleLength()
                                )) / 2.0)).TupleFloor();
                        }
                        //Set to pure green.
                        if (hv_Colors == null)
                            hv_Colors = new HTuple();
                        hv_Colors[hv_GoodIdx] = "#00ff00";
                        //Write original color to a green entry.
                        if (hv_Colors == null)
                            hv_Colors = new HTuple();
                        hv_Colors[hv_GreenIdx] = hv_CurrentColor;
                        break;
                    }
                    else if ((int)((new HTuple(hv_GoodIdx.TupleNotEqual(-1))).TupleAnd(
                        new HTuple((new HTuple(hv_ClassNames.TupleLength())).TupleGreater(8)))) != 0)
                    {
                        //If number of classes is larger than 8, set the respective color to green.
                        if (hv_Colors == null)
                            hv_Colors = new HTuple();
                        hv_Colors[hv_GoodIdx] = "#00ff00";
                        break;
                    }
                }

                hv_NumColors.Dispose();
                hv_ColorsRainbow.Dispose();
                hv_ClassNamesGood.Dispose();
                hv_IndexFind.Dispose();
                hv_GoodIdx.Dispose();
                hv_CurrentColor.Dispose();
                hv_GreenIdx.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_NumColors.Dispose();
                hv_ColorsRainbow.Dispose();
                hv_ClassNamesGood.Dispose();
                hv_IndexFind.Dispose();
                hv_GoodIdx.Dispose();
                hv_CurrentColor.Dispose();
                hv_GreenIdx.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Deep Learning / Model
        public void get_image(out HObject ho_Image, HTuple hv_SampleKeys, HTuple hv_DLSample)
        {


            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            //
            if ((int)(new HTuple(((hv_SampleKeys.TupleFind("image"))).TupleNotEqual(-1))) != 0)
            {
                ho_Image.Dispose();
                HOperatorSet.GetDictObject(out ho_Image, hv_DLSample, "image");
            }
            else
            {
                throw new HalconException("Image could not be found in DLSample.");
            }


            return;
        }

        // Chapter: Graphics / Window
        public void get_next_window(HTuple hv_Font, HTuple hv_FontSize, HTuple hv_ShowBottomDesc,
            HTuple hv_WidthImage, HTuple hv_HeightImage, HTuple hv_MapColorBarWidth, HTuple hv_ScaleWindows,
            HTuple hv_ThresholdWidth, HTuple hv_PrevWindowCoordinates, HTuple hv_WindowHandleDict,
            HTuple hv_WindowHandleKey, out HTuple hv_CurrentWindowHandle, out HTuple hv_WindowImageRatio,
            out HTuple hv_PrevWindowCoordinatesOut)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_OpenNewWindow = new HTuple(), hv_WindowHandles = new HTuple();
            HTuple hv_Value = new HTuple(), hv_Exception = new HTuple();
            HTuple hv_MetaInfo = new HTuple(), hv_SetPartRow2 = new HTuple();
            HTuple hv_SetpartColumn2 = new HTuple(), hv_MarginBottom = new HTuple();
            HTuple hv_SetPartColumn2 = new HTuple(), hv__ = new HTuple();
            HTuple hv_WindowWidth = new HTuple(), hv_WindowHeight = new HTuple();
            HTuple hv_WindowRow2 = new HTuple(), hv_WindowColumn2 = new HTuple();
            HTuple hv_WindowRatio = new HTuple(), hv_WindowImageRatioHeight = new HTuple();
            HTuple hv_WindowImageRatioWidth = new HTuple(), hv_ImageRow2 = new HTuple();
            HTuple hv_ImageColumn2 = new HTuple(), hv_ImageRatio = new HTuple();
            HTuple hv_ImageWindowRatio = new HTuple(), hv_ImageRow2InWindow = new HTuple();
            HTuple hv_ImageCol2InWindow = new HTuple();
            HTuple hv_MapColorBarWidth_COPY_INP_TMP = new HTuple(hv_MapColorBarWidth);

            // Initialize local and output iconic variables 
            hv_CurrentWindowHandle = new HTuple();
            hv_WindowImageRatio = new HTuple();
            hv_PrevWindowCoordinatesOut = new HTuple();
            try
            {
                //
                //This procedure returns the next window that
                //is used for visualization. If ReuseWindows is true
                //and WindowHandleList is suitable, the window handles
                //that are passed over are used. Else, this procedure
                //opens a new window, either next to the last ones, or
                //in a new row.
                //
                //First, check if the requested window is already available.
                hv_OpenNewWindow.Dispose();
                hv_OpenNewWindow = 0;
                try
                {
                    hv_WindowHandles.Dispose();
                    HOperatorSet.GetDictTuple(hv_WindowHandleDict, hv_WindowHandleKey, out hv_WindowHandles);
                    hv_CurrentWindowHandle.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_CurrentWindowHandle = hv_WindowHandles.TupleSelect(
                            0);
                    }
                    //Check if window handle is valid.
                    try
                    {
                        hv_Value.Dispose();
                        HOperatorSet.GetWindowParam(hv_CurrentWindowHandle, "flush", out hv_Value);
                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException2)
                    {
                        HDevExpDefaultException2.ToHTuple(out hv_Exception);
                        //If there is something wrong with the current window, create a new one.
                        hv_OpenNewWindow.Dispose();
                        hv_OpenNewWindow = 1;
                        HOperatorSet.RemoveDictKey(hv_WindowHandleDict, hv_WindowHandleKey);
                    }
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    hv_OpenNewWindow.Dispose();
                    hv_OpenNewWindow = 1;
                }
                //
                //Get next window.
                if ((int)(hv_OpenNewWindow.TupleNot()) != 0)
                {
                    //
                    //If possible, reuse existing window handles.
                    HDevWindowStack.SetActive(hv_CurrentWindowHandle);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.ClearWindow(HDevWindowStack.GetActive());
                    }
                    set_display_font(hv_CurrentWindowHandle, hv_FontSize, hv_Font, "true", "false");
                    //
                    hv_MetaInfo.Dispose();
                    HOperatorSet.GetDictTuple(hv_WindowHandleDict, "meta_information", out hv_MetaInfo);
                    //
                    try
                    {
                        //
                        //Clear window and set part, if possible.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_SetPartRow2.Dispose();
                            HOperatorSet.GetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_set_part_row2",
                                out hv_SetPartRow2);
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_SetpartColumn2.Dispose();
                            HOperatorSet.GetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_set_part_column2",
                                out hv_SetpartColumn2);
                        }
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetPart(HDevWindowStack.GetActive(), 0, 0, hv_SetPartRow2,
                                hv_SetpartColumn2);
                        }
                        //
                        //Get WindowImageRatio.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_WindowImageRatio.Dispose();
                            HOperatorSet.GetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_window_image_ratio",
                                out hv_WindowImageRatio);
                        }
                        //
                        //Get previous window coordinates.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_PrevWindowCoordinatesOut.Dispose();
                            HOperatorSet.GetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_window_coordinates",
                                out hv_PrevWindowCoordinatesOut);
                        }
                        //
                        //Get MarginBottom and MapColorBarWidth.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_MarginBottom.Dispose();
                            HOperatorSet.GetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_margin_bottom",
                                out hv_MarginBottom);
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_MapColorBarWidth_COPY_INP_TMP.Dispose();
                            HOperatorSet.GetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_map_color_bar_with",
                                out hv_MapColorBarWidth_COPY_INP_TMP);
                        }
                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException1)
                    {
                        HDevExpDefaultException1.ToHTuple(out hv_Exception);
                        //
                        //Some meta information is missing -> set appropriate values.
                        hv_WindowImageRatio.Dispose(); hv_SetPartRow2.Dispose(); hv_SetPartColumn2.Dispose(); hv_PrevWindowCoordinatesOut.Dispose(); hv_MarginBottom.Dispose();
                        get_window_meta_information(hv_CurrentWindowHandle, hv_WidthImage, hv_HeightImage,
                            hv_MapColorBarWidth_COPY_INP_TMP, 0, 0, hv_ShowBottomDesc, out hv_WindowImageRatio,
                            out hv_SetPartRow2, out hv_SetPartColumn2, out hv_PrevWindowCoordinatesOut,
                            out hv_MarginBottom);
                        //
                        //Set meta information about the current window handle.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            HOperatorSet.SetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_window_image_ratio",
                                hv_WindowImageRatio);
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            HOperatorSet.SetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_window_coordinates",
                                hv_PrevWindowCoordinatesOut);
                        }
                        HOperatorSet.SetDictTuple(hv_WindowHandleDict, "meta_information", hv_MetaInfo);
                    }
                    //
                    //Set window part such that image is displayed undistorted.
                    hv__.Dispose(); hv__.Dispose(); hv_WindowWidth.Dispose(); hv_WindowHeight.Dispose();
                    HOperatorSet.GetWindowExtents(hv_CurrentWindowHandle, out hv__, out hv__,
                        out hv_WindowWidth, out hv_WindowHeight);
                    hv_WindowRow2.Dispose();
                    hv_WindowRow2 = new HTuple(hv_WindowHeight);
                    hv_WindowColumn2.Dispose();
                    hv_WindowColumn2 = new HTuple(hv_WindowWidth);
                    hv_WindowRatio.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_WindowRatio = hv_WindowColumn2 / (hv_WindowRow2 * 1.0);
                    }
                    //
                    hv_WindowImageRatioHeight.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_WindowImageRatioHeight = hv_WindowHeight / (hv_HeightImage * 1.0);
                    }
                    hv_WindowImageRatioWidth.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_WindowImageRatioWidth = hv_WindowWidth / (hv_WidthImage * 1.0);
                    }
                    //
                    hv_ImageRow2.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ImageRow2 = hv_HeightImage + (hv_MarginBottom / hv_WindowImageRatioHeight);
                    }
                    hv_ImageColumn2.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ImageColumn2 = hv_WidthImage + (hv_MapColorBarWidth_COPY_INP_TMP / hv_WindowImageRatioWidth);
                    }
                    hv_ImageRatio.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ImageRatio = hv_ImageColumn2 / (hv_ImageRow2 * 1.0);
                    }
                    if ((int)(new HTuple(hv_ImageRatio.TupleGreater(hv_WindowRatio))) != 0)
                    {
                        //Extend image until right window border.
                        hv_SetPartColumn2.Dispose();
                        hv_SetPartColumn2 = new HTuple(hv_ImageColumn2);
                        //
                        hv_ImageWindowRatio.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ImageWindowRatio = hv_ImageColumn2 / (hv_WindowColumn2 * 1.0);
                        }
                        hv_ImageRow2InWindow.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ImageRow2InWindow = hv_ImageRow2 / hv_ImageWindowRatio;
                        }
                        hv_SetPartRow2.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_SetPartRow2 = hv_ImageRow2 + ((hv_WindowRow2 - hv_ImageRow2InWindow) / hv_WindowImageRatioWidth);
                        }
                    }
                    else
                    {
                        //Extend image until bottom of window.
                        hv_SetPartRow2.Dispose();
                        hv_SetPartRow2 = new HTuple(hv_ImageRow2);
                        //
                        hv_ImageWindowRatio.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ImageWindowRatio = hv_ImageRow2 / (hv_WindowRow2 * 1.0);
                        }
                        hv_ImageCol2InWindow.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ImageCol2InWindow = hv_ImageColumn2 / hv_ImageWindowRatio;
                        }
                        hv_SetPartColumn2.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_SetPartColumn2 = hv_ImageColumn2 + ((hv_WindowColumn2 - hv_ImageCol2InWindow) / hv_WindowImageRatioHeight);
                        }
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetPart(HDevWindowStack.GetActive(), 0, 0, hv_SetPartRow2,
                            hv_SetPartColumn2);
                    }
                    //
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.SetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_set_part_row2",
                            hv_SetPartRow2);
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.SetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_set_part_column2",
                            hv_SetPartColumn2);
                    }
                }
                else
                {
                    //Open a new window.
                    hv_CurrentWindowHandle.Dispose(); hv_WindowImageRatio.Dispose(); hv_PrevWindowCoordinatesOut.Dispose();
                    open_next_window(hv_Font, hv_FontSize, hv_ShowBottomDesc, hv_WidthImage,
                        hv_HeightImage, hv_MapColorBarWidth_COPY_INP_TMP, hv_ScaleWindows, hv_ThresholdWidth,
                        hv_PrevWindowCoordinates, hv_WindowHandleDict, hv_WindowHandleKey, out hv_CurrentWindowHandle,
                        out hv_WindowImageRatio, out hv_PrevWindowCoordinatesOut);
                    HOperatorSet.SetWindowParam(hv_CurrentWindowHandle, "flush", "false");
                }

                hv_MapColorBarWidth_COPY_INP_TMP.Dispose();
                hv_OpenNewWindow.Dispose();
                hv_WindowHandles.Dispose();
                hv_Value.Dispose();
                hv_Exception.Dispose();
                hv_MetaInfo.Dispose();
                hv_SetPartRow2.Dispose();
                hv_SetpartColumn2.Dispose();
                hv_MarginBottom.Dispose();
                hv_SetPartColumn2.Dispose();
                hv__.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();
                hv_WindowRow2.Dispose();
                hv_WindowColumn2.Dispose();
                hv_WindowRatio.Dispose();
                hv_WindowImageRatioHeight.Dispose();
                hv_WindowImageRatioWidth.Dispose();
                hv_ImageRow2.Dispose();
                hv_ImageColumn2.Dispose();
                hv_ImageRatio.Dispose();
                hv_ImageWindowRatio.Dispose();
                hv_ImageRow2InWindow.Dispose();
                hv_ImageCol2InWindow.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_MapColorBarWidth_COPY_INP_TMP.Dispose();
                hv_OpenNewWindow.Dispose();
                hv_WindowHandles.Dispose();
                hv_Value.Dispose();
                hv_Exception.Dispose();
                hv_MetaInfo.Dispose();
                hv_SetPartRow2.Dispose();
                hv_SetpartColumn2.Dispose();
                hv_MarginBottom.Dispose();
                hv_SetPartColumn2.Dispose();
                hv__.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();
                hv_WindowRow2.Dispose();
                hv_WindowColumn2.Dispose();
                hv_WindowRatio.Dispose();
                hv_WindowImageRatioHeight.Dispose();
                hv_WindowImageRatioWidth.Dispose();
                hv_ImageRow2.Dispose();
                hv_ImageColumn2.Dispose();
                hv_ImageRatio.Dispose();
                hv_ImageWindowRatio.Dispose();
                hv_ImageRow2InWindow.Dispose();
                hv_ImageCol2InWindow.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Deep Learning / Semantic Segmentation
        public void get_segmentation_image_ground_truth(out HObject ho_SegmentationImagGroundTruth,
            HTuple hv_SampleKeys, HTuple hv_DLSample)
        {


            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_SegmentationImagGroundTruth);
            //
            if ((int)(new HTuple(((hv_SampleKeys.TupleFind("segmentation_image"))).TupleNotEqual(
                -1))) != 0)
            {
                ho_SegmentationImagGroundTruth.Dispose();
                HOperatorSet.GetDictObject(out ho_SegmentationImagGroundTruth, hv_DLSample,
                    "segmentation_image");
            }
            else
            {
                throw new HalconException("Ground truth segmentation image could not be found in DLSample.");
            }


            return;
        }

        // Chapter: Deep Learning / Semantic Segmentation
        public void get_segmentation_image_result(out HObject ho_SegmentationImageResult,
            HTuple hv_ResultKeys, HTuple hv_DLResult)
        {


            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_SegmentationImageResult);
            //
            if ((int)(new HTuple(((hv_ResultKeys.TupleFind("segmentation_image"))).TupleNotEqual(
                -1))) != 0)
            {
                ho_SegmentationImageResult.Dispose();
                HOperatorSet.GetDictObject(out ho_SegmentationImageResult, hv_DLResult, "segmentation_image");
            }
            else
            {
                throw new HalconException("Result segmentation data could not be found in DLSample.");
            }


            return;
        }

        // Chapter: Deep Learning / Semantic Segmentation
        public void get_weight_image(out HObject ho_ImageWeight, HTuple hv_SampleKeys,
            HTuple hv_DLSample)
        {


            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageWeight);
            //
            if ((int)(new HTuple(((hv_SampleKeys.TupleFind("weight_image"))).TupleNotEqual(
                -1))) != 0)
            {
                ho_ImageWeight.Dispose();
                HOperatorSet.GetDictObject(out ho_ImageWeight, hv_DLSample, "weight_image");
            }
            else
            {
                throw new HalconException("Weight image could not be found in DLSample.");
            }


            return;
        }

        // Chapter: Graphics / Window
        public void get_window_meta_information(HTuple hv_WindowHandle, HTuple hv_WidthImage,
            HTuple hv_HeightImage, HTuple hv_MapColorBarWidth, HTuple hv_WindowRow1, HTuple hv_WindowColumn1,
            HTuple hv_ShowBottomDesc, out HTuple hv_WindowImageRatio, out HTuple hv_SetPartRow2,
            out HTuple hv_SetPartColumn2, out HTuple hv_PrevWindowCoordinatesOut, out HTuple hv_MarginBottom)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_Ascent = new HTuple(), hv_Descent = new HTuple();
            HTuple hv__ = new HTuple(), hv_WindowWidth = new HTuple();
            HTuple hv_WindowHeight = new HTuple(), hv_Row1 = new HTuple();
            HTuple hv_Column1 = new HTuple(), hv_Row2 = new HTuple();
            HTuple hv_Column2 = new HTuple(), hv_WindowImageRatioHeight = new HTuple();
            HTuple hv_WindowImageRatioWidth = new HTuple();
            // Initialize local and output iconic variables 
            hv_WindowImageRatio = new HTuple();
            hv_SetPartRow2 = new HTuple();
            hv_SetPartColumn2 = new HTuple();
            hv_PrevWindowCoordinatesOut = new HTuple();
            hv_MarginBottom = new HTuple();
            try
            {
                //
                //This procedure returns meta information to display images correctly
                //
                //Calculate MarginBottom.
                if ((int)(hv_ShowBottomDesc) != 0)
                {
                    hv_Ascent.Dispose(); hv_Descent.Dispose(); hv__.Dispose(); hv__.Dispose();
                    HOperatorSet.GetStringExtents(hv_WindowHandle, "test_string", out hv_Ascent,
                        out hv_Descent, out hv__, out hv__);
                    hv_MarginBottom.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_MarginBottom = ((2 * 12) + hv_Ascent) + hv_Descent;
                    }
                }
                else
                {
                    hv_MarginBottom.Dispose();
                    hv_MarginBottom = 0;
                }
                //Adapt window size (+ MarginBottom + MapColorBarWidth).
                hv__.Dispose(); hv__.Dispose(); hv_WindowWidth.Dispose(); hv_WindowHeight.Dispose();
                HOperatorSet.GetWindowExtents(hv_WindowHandle, out hv__, out hv__, out hv_WindowWidth,
                    out hv_WindowHeight);
                hv_WindowImageRatio.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WindowImageRatio = hv_WindowHeight / (hv_HeightImage * 1.0);
                }
                //
                //Set part for the image to be displayed later.
                hv_Row1.Dispose(); hv_Column1.Dispose(); hv_Row2.Dispose(); hv_Column2.Dispose();
                HOperatorSet.GetPart(hv_WindowHandle, out hv_Row1, out hv_Column1, out hv_Row2,
                    out hv_Column2);
                hv_WindowImageRatioHeight.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WindowImageRatioHeight = hv_WindowHeight / (hv_HeightImage * 1.0);
                }
                hv_WindowImageRatioWidth.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WindowImageRatioWidth = hv_WindowWidth / (hv_WidthImage * 1.0);
                }
                hv_SetPartRow2.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_SetPartRow2 = hv_Row2 + (hv_MarginBottom / hv_WindowImageRatioHeight);
                }
                hv_SetPartColumn2.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_SetPartColumn2 = hv_Column2 + (hv_MapColorBarWidth / hv_WindowImageRatioWidth);
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetPart(HDevWindowStack.GetActive(), 0, 0, hv_SetPartRow2, hv_SetPartColumn2);
                }
                //
                //Return the coordinates of the new window.
                hv_PrevWindowCoordinatesOut.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PrevWindowCoordinatesOut = new HTuple();
                    hv_PrevWindowCoordinatesOut = hv_PrevWindowCoordinatesOut.TupleConcat(hv_WindowRow1, hv_WindowColumn1, hv_WindowWidth, hv_WindowHeight);
                }
                //

                hv_Ascent.Dispose();
                hv_Descent.Dispose();
                hv__.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();
                hv_Row1.Dispose();
                hv_Column1.Dispose();
                hv_Row2.Dispose();
                hv_Column2.Dispose();
                hv_WindowImageRatioHeight.Dispose();
                hv_WindowImageRatioWidth.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_Ascent.Dispose();
                hv_Descent.Dispose();
                hv__.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();
                hv_Row1.Dispose();
                hv_Column1.Dispose();
                hv_Row2.Dispose();
                hv_Column2.Dispose();
                hv_WindowImageRatioHeight.Dispose();
                hv_WindowImageRatioWidth.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: File / Misc
        // Short Description: Get all image files under the given path 
        public void list_image_files(HTuple hv_ImageDirectory, HTuple hv_Extensions, HTuple hv_Options,
            out HTuple hv_ImageFiles)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_ImageDirectoryIndex = new HTuple();
            HTuple hv_ImageFilesTmp = new HTuple(), hv_CurrentImageDirectory = new HTuple();
            HTuple hv_HalconImages = new HTuple(), hv_OS = new HTuple();
            HTuple hv_Directories = new HTuple(), hv_Index = new HTuple();
            HTuple hv_Length = new HTuple(), hv_NetworkDrive = new HTuple();
            HTuple hv_Substring = new HTuple(), hv_FileExists = new HTuple();
            HTuple hv_AllFiles = new HTuple(), hv_i = new HTuple();
            HTuple hv_Selection = new HTuple();
            HTuple hv_Extensions_COPY_INP_TMP = new HTuple(hv_Extensions);

            // Initialize local and output iconic variables 
            hv_ImageFiles = new HTuple();
            try
            {
                //This procedure returns all files in a given directory
                //with one of the suffixes specified in Extensions.
                //
                //Input parameters:
                //ImageDirectory: Directory or a tuple of directories with images.
                //   If a directory is not found locally, the respective directory
                //   is searched under %HALCONIMAGES%/ImageDirectory.
                //   See the Installation Guide for further information
                //   in case %HALCONIMAGES% is not set.
                //Extensions: A string tuple containing the extensions to be found
                //   e.g. ['png','tif',jpg'] or others
                //If Extensions is set to 'default' or the empty string '',
                //   all image suffixes supported by HALCON are used.
                //Options: as in the operator list_files, except that the 'files'
                //   option is always used. Note that the 'directories' option
                //   has no effect but increases runtime, because only files are
                //   returned.
                //
                //Output parameter:
                //ImageFiles: A tuple of all found image file names
                //
                if ((int)((new HTuple((new HTuple(hv_Extensions_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                    new HTuple(hv_Extensions_COPY_INP_TMP.TupleEqual(""))))).TupleOr(new HTuple(hv_Extensions_COPY_INP_TMP.TupleEqual(
                    "default")))) != 0)
                {
                    hv_Extensions_COPY_INP_TMP.Dispose();
                    hv_Extensions_COPY_INP_TMP = new HTuple();
                    hv_Extensions_COPY_INP_TMP[0] = "ima";
                    hv_Extensions_COPY_INP_TMP[1] = "tif";
                    hv_Extensions_COPY_INP_TMP[2] = "tiff";
                    hv_Extensions_COPY_INP_TMP[3] = "gif";
                    hv_Extensions_COPY_INP_TMP[4] = "bmp";
                    hv_Extensions_COPY_INP_TMP[5] = "jpg";
                    hv_Extensions_COPY_INP_TMP[6] = "jpeg";
                    hv_Extensions_COPY_INP_TMP[7] = "jp2";
                    hv_Extensions_COPY_INP_TMP[8] = "jxr";
                    hv_Extensions_COPY_INP_TMP[9] = "png";
                    hv_Extensions_COPY_INP_TMP[10] = "pcx";
                    hv_Extensions_COPY_INP_TMP[11] = "ras";
                    hv_Extensions_COPY_INP_TMP[12] = "xwd";
                    hv_Extensions_COPY_INP_TMP[13] = "pbm";
                    hv_Extensions_COPY_INP_TMP[14] = "pnm";
                    hv_Extensions_COPY_INP_TMP[15] = "pgm";
                    hv_Extensions_COPY_INP_TMP[16] = "ppm";
                    //
                }
                hv_ImageFiles.Dispose();
                hv_ImageFiles = new HTuple();
                //Loop through all given image directories.
                for (hv_ImageDirectoryIndex = 0; (int)hv_ImageDirectoryIndex <= (int)((new HTuple(hv_ImageDirectory.TupleLength()
                    )) - 1); hv_ImageDirectoryIndex = (int)hv_ImageDirectoryIndex + 1)
                {
                    hv_ImageFilesTmp.Dispose();
                    hv_ImageFilesTmp = new HTuple();
                    hv_CurrentImageDirectory.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_CurrentImageDirectory = hv_ImageDirectory.TupleSelect(
                            hv_ImageDirectoryIndex);
                    }
                    if ((int)(new HTuple(hv_CurrentImageDirectory.TupleEqual(""))) != 0)
                    {
                        hv_CurrentImageDirectory.Dispose();
                        hv_CurrentImageDirectory = ".";
                    }
                    hv_HalconImages.Dispose();
                    HOperatorSet.GetSystem("image_dir", out hv_HalconImages);
                    hv_OS.Dispose();
                    HOperatorSet.GetSystem("operating_system", out hv_OS);
                    if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_HalconImages = hv_HalconImages.TupleSplit(
                                    ";");
                                hv_HalconImages.Dispose();
                                hv_HalconImages = ExpTmpLocalVar_HalconImages;
                            }
                        }
                    }
                    else
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_HalconImages = hv_HalconImages.TupleSplit(
                                    ":");
                                hv_HalconImages.Dispose();
                                hv_HalconImages = ExpTmpLocalVar_HalconImages;
                            }
                        }
                    }
                    hv_Directories.Dispose();
                    hv_Directories = new HTuple(hv_CurrentImageDirectory);
                    for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_HalconImages.TupleLength()
                        )) - 1); hv_Index = (int)hv_Index + 1)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_Directories = hv_Directories.TupleConcat(
                                    ((hv_HalconImages.TupleSelect(hv_Index)) + "/") + hv_CurrentImageDirectory);
                                hv_Directories.Dispose();
                                hv_Directories = ExpTmpLocalVar_Directories;
                            }
                        }
                    }
                    hv_Length.Dispose();
                    HOperatorSet.TupleStrlen(hv_Directories, out hv_Length);
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_NetworkDrive.Dispose();
                        HOperatorSet.TupleGenConst(new HTuple(hv_Length.TupleLength()), 0, out hv_NetworkDrive);
                    }
                    if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
                    {
                        for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_Length.TupleLength()
                            )) - 1); hv_Index = (int)hv_Index + 1)
                        {
                            if ((int)(new HTuple(((((hv_Directories.TupleSelect(hv_Index))).TupleStrlen()
                                )).TupleGreater(1))) != 0)
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_Substring.Dispose();
                                    HOperatorSet.TupleStrFirstN(hv_Directories.TupleSelect(hv_Index), 1,
                                        out hv_Substring);
                                }
                                if ((int)((new HTuple(hv_Substring.TupleEqual("//"))).TupleOr(new HTuple(hv_Substring.TupleEqual(
                                    "\\\\")))) != 0)
                                {
                                    if (hv_NetworkDrive == null)
                                        hv_NetworkDrive = new HTuple();
                                    hv_NetworkDrive[hv_Index] = 1;
                                }
                            }
                        }
                    }
                    hv_ImageFilesTmp.Dispose();
                    hv_ImageFilesTmp = new HTuple();
                    for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_Directories.TupleLength()
                        )) - 1); hv_Index = (int)hv_Index + 1)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_FileExists.Dispose();
                            HOperatorSet.FileExists(hv_Directories.TupleSelect(hv_Index), out hv_FileExists);
                        }
                        if ((int)(hv_FileExists) != 0)
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_AllFiles.Dispose();
                                HOperatorSet.ListFiles(hv_Directories.TupleSelect(hv_Index), (new HTuple("files")).TupleConcat(
                                    hv_Options), out hv_AllFiles);
                            }
                            hv_ImageFilesTmp.Dispose();
                            hv_ImageFilesTmp = new HTuple();
                            for (hv_i = 0; (int)hv_i <= (int)((new HTuple(hv_Extensions_COPY_INP_TMP.TupleLength()
                                )) - 1); hv_i = (int)hv_i + 1)
                            {
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    hv_Selection.Dispose();
                                    HOperatorSet.TupleRegexpSelect(hv_AllFiles, (((".*" + (hv_Extensions_COPY_INP_TMP.TupleSelect(
                                        hv_i))) + "$")).TupleConcat("ignore_case"), out hv_Selection);
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    {
                                        HTuple
                                          ExpTmpLocalVar_ImageFilesTmp = hv_ImageFilesTmp.TupleConcat(
                                            hv_Selection);
                                        hv_ImageFilesTmp.Dispose();
                                        hv_ImageFilesTmp = ExpTmpLocalVar_ImageFilesTmp;
                                    }
                                }
                            }
                            {
                                HTuple ExpTmpOutVar_0;
                                HOperatorSet.TupleRegexpReplace(hv_ImageFilesTmp, (new HTuple("\\\\")).TupleConcat(
                                    "replace_all"), "/", out ExpTmpOutVar_0);
                                hv_ImageFilesTmp.Dispose();
                                hv_ImageFilesTmp = ExpTmpOutVar_0;
                            }
                            if ((int)(hv_NetworkDrive.TupleSelect(hv_Index)) != 0)
                            {
                                {
                                    HTuple ExpTmpOutVar_0;
                                    HOperatorSet.TupleRegexpReplace(hv_ImageFilesTmp, (new HTuple("//")).TupleConcat(
                                        "replace_all"), "/", out ExpTmpOutVar_0);
                                    hv_ImageFilesTmp.Dispose();
                                    hv_ImageFilesTmp = ExpTmpOutVar_0;
                                }
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    {
                                        HTuple
                                          ExpTmpLocalVar_ImageFilesTmp = "/" + hv_ImageFilesTmp;
                                        hv_ImageFilesTmp.Dispose();
                                        hv_ImageFilesTmp = ExpTmpLocalVar_ImageFilesTmp;
                                    }
                                }
                            }
                            else
                            {
                                {
                                    HTuple ExpTmpOutVar_0;
                                    HOperatorSet.TupleRegexpReplace(hv_ImageFilesTmp, (new HTuple("//")).TupleConcat(
                                        "replace_all"), "/", out ExpTmpOutVar_0);
                                    hv_ImageFilesTmp.Dispose();
                                    hv_ImageFilesTmp = ExpTmpOutVar_0;
                                }
                            }
                            break;
                        }
                    }
                    //Concatenate the output image paths.
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_ImageFiles = hv_ImageFiles.TupleConcat(
                                hv_ImageFilesTmp);
                            hv_ImageFiles.Dispose();
                            hv_ImageFiles = ExpTmpLocalVar_ImageFiles;
                        }
                    }
                }

                hv_Extensions_COPY_INP_TMP.Dispose();
                hv_ImageDirectoryIndex.Dispose();
                hv_ImageFilesTmp.Dispose();
                hv_CurrentImageDirectory.Dispose();
                hv_HalconImages.Dispose();
                hv_OS.Dispose();
                hv_Directories.Dispose();
                hv_Index.Dispose();
                hv_Length.Dispose();
                hv_NetworkDrive.Dispose();
                hv_Substring.Dispose();
                hv_FileExists.Dispose();
                hv_AllFiles.Dispose();
                hv_i.Dispose();
                hv_Selection.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_Extensions_COPY_INP_TMP.Dispose();
                hv_ImageDirectoryIndex.Dispose();
                hv_ImageFilesTmp.Dispose();
                hv_CurrentImageDirectory.Dispose();
                hv_HalconImages.Dispose();
                hv_OS.Dispose();
                hv_Directories.Dispose();
                hv_Index.Dispose();
                hv_Length.Dispose();
                hv_NetworkDrive.Dispose();
                hv_Substring.Dispose();
                hv_FileExists.Dispose();
                hv_AllFiles.Dispose();
                hv_i.Dispose();
                hv_Selection.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Deep Learning / Model
        // Short Description: shuffles the input colors in a deterministic way 
        public void make_neighboring_colors_distinguishable(HTuple hv_ColorsRainbow, out HTuple hv_Colors)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_NumColors = new HTuple(), hv_NumChunks = new HTuple();
            HTuple hv_NumLeftOver = new HTuple(), hv_ColorsPerChunk = new HTuple();
            HTuple hv_StartIdx = new HTuple(), hv_S = new HTuple();
            HTuple hv_EndIdx = new HTuple(), hv_IdxsLeft = new HTuple();
            HTuple hv_IdxsRight = new HTuple();
            // Initialize local and output iconic variables 
            hv_Colors = new HTuple();
            try
            {
                //
                //Shuffle the input colors in a deterministic way
                //to make adjacent colors more distinguishable.
                //Neighboring colors from the input are distributed to every NumChunks
                //position in the output.
                //Depending on the number of colors, increase NumChunks.
                hv_NumColors.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_NumColors = new HTuple(hv_ColorsRainbow.TupleLength()
                        );
                }
                if ((int)(new HTuple(hv_NumColors.TupleGreaterEqual(8))) != 0)
                {
                    hv_NumChunks.Dispose();
                    hv_NumChunks = 3;
                    if ((int)(new HTuple(hv_NumColors.TupleGreaterEqual(40))) != 0)
                    {
                        hv_NumChunks.Dispose();
                        hv_NumChunks = 6;
                    }
                    else if ((int)(new HTuple(hv_NumColors.TupleGreaterEqual(20))) != 0)
                    {
                        hv_NumChunks.Dispose();
                        hv_NumChunks = 4;
                    }
                    hv_Colors.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Colors = HTuple.TupleGenConst(
                            hv_NumColors, -1);
                    }
                    //Check if the Number of Colors is dividable by NumChunks.
                    hv_NumLeftOver.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_NumLeftOver = hv_NumColors % hv_NumChunks;
                    }
                    hv_ColorsPerChunk.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ColorsPerChunk = ((hv_NumColors / hv_NumChunks)).TupleInt()
                            ;
                    }
                    hv_StartIdx.Dispose();
                    hv_StartIdx = 0;
                    HTuple end_val19 = hv_NumChunks - 1;
                    HTuple step_val19 = 1;
                    for (hv_S = 0; hv_S.Continue(end_val19, step_val19); hv_S = hv_S.TupleAdd(step_val19))
                    {
                        hv_EndIdx.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_EndIdx = (hv_StartIdx + hv_ColorsPerChunk) - 1;
                        }
                        if ((int)(new HTuple(hv_S.TupleLess(hv_NumLeftOver))) != 0)
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_EndIdx = hv_EndIdx + 1;
                                    hv_EndIdx.Dispose();
                                    hv_EndIdx = ExpTmpLocalVar_EndIdx;
                                }
                            }
                        }
                        hv_IdxsLeft.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_IdxsLeft = HTuple.TupleGenSequence(
                                hv_S, hv_NumColors - 1, hv_NumChunks);
                        }
                        hv_IdxsRight.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_IdxsRight = HTuple.TupleGenSequence(
                                hv_StartIdx, hv_EndIdx, 1);
                        }
                        if (hv_Colors == null)
                            hv_Colors = new HTuple();
                        hv_Colors[HTuple.TupleGenSequence(hv_S, hv_NumColors - 1, hv_NumChunks)] = hv_ColorsRainbow.TupleSelectRange(
                            hv_StartIdx, hv_EndIdx);
                        hv_StartIdx.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_StartIdx = hv_EndIdx + 1;
                        }
                    }
                }
                else
                {
                    hv_Colors.Dispose();
                    hv_Colors = new HTuple(hv_ColorsRainbow);
                }

                hv_NumColors.Dispose();
                hv_NumChunks.Dispose();
                hv_NumLeftOver.Dispose();
                hv_ColorsPerChunk.Dispose();
                hv_StartIdx.Dispose();
                hv_S.Dispose();
                hv_EndIdx.Dispose();
                hv_IdxsLeft.Dispose();
                hv_IdxsRight.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_NumColors.Dispose();
                hv_NumChunks.Dispose();
                hv_NumLeftOver.Dispose();
                hv_ColorsPerChunk.Dispose();
                hv_StartIdx.Dispose();
                hv_S.Dispose();
                hv_EndIdx.Dispose();
                hv_IdxsLeft.Dispose();
                hv_IdxsRight.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Graphics / Window
        // Short Description: Open a window next to the given WindowHandleFather.  
        public void open_child_window(HTuple hv_WindowHandleFather, HTuple hv_Font, HTuple hv_FontSize,
            HTuple hv_Text, HTuple hv_PrevWindowCoordinates, HTuple hv_WindowHandleDict,
            HTuple hv_WindowHandleKey, out HTuple hv_WindowHandleChild, out HTuple hv_PrevWindowCoordinatesOut)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_StringWidth = new HTuple(), hv_IndexText = new HTuple();
            HTuple hv__ = new HTuple(), hv_TextWidth = new HTuple();
            HTuple hv_WindowRow = new HTuple(), hv_WindowColumn = new HTuple();
            HTuple hv_WindowWidth = new HTuple(), hv_WindowHeight = new HTuple();
            HTuple hv_MetaInfo = new HTuple();
            // Initialize local and output iconic variables 
            hv_WindowHandleChild = new HTuple();
            hv_PrevWindowCoordinatesOut = new HTuple();
            try
            {
                //
                //This procedure opens a window next to the given WindowHandleFather.
                //
                //Get the maximum width of the text to be displayed.
                hv_StringWidth.Dispose();
                hv_StringWidth = 0;
                for (hv_IndexText = 0; (int)hv_IndexText <= (int)((new HTuple(hv_Text.TupleLength()
                    )) - 1); hv_IndexText = (int)hv_IndexText + 1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv__.Dispose(); hv__.Dispose(); hv_TextWidth.Dispose(); hv__.Dispose();
                        HOperatorSet.GetStringExtents(hv_WindowHandleFather, hv_Text.TupleSelect(
                            hv_IndexText), out hv__, out hv__, out hv_TextWidth, out hv__);
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_StringWidth = hv_StringWidth.TupleMax2(
                                hv_TextWidth);
                            hv_StringWidth.Dispose();
                            hv_StringWidth = ExpTmpLocalVar_StringWidth;
                        }
                    }
                }
                //
                //Define window coordinates.
                hv_WindowRow.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WindowRow = hv_PrevWindowCoordinates.TupleSelect(
                        0);
                }
                hv_WindowColumn.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WindowColumn = ((hv_PrevWindowCoordinates.TupleSelect(
                        1)) + (hv_PrevWindowCoordinates.TupleSelect(2))) + 5;
                }
                hv_WindowWidth.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WindowWidth = hv_StringWidth + (2 * 12.0);
                }
                hv_WindowHeight.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WindowHeight = hv_PrevWindowCoordinates.TupleSelect(
                        3);
                }
                //
                HOperatorSet.SetWindowAttr("background_color", "black");
                HOperatorSet.OpenWindow(hv_WindowRow, hv_WindowColumn, hv_WindowWidth, hv_WindowHeight, 0, "visible", "", out hv_WindowHandleChild);
                HDevWindowStack.Push(hv_WindowHandleChild);
                set_display_font(hv_WindowHandleChild, hv_FontSize, hv_Font, "true", "false");
                //
                //Return the coordinates of the new window.
                hv_PrevWindowCoordinatesOut.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PrevWindowCoordinatesOut = new HTuple();
                    hv_PrevWindowCoordinatesOut = hv_PrevWindowCoordinatesOut.TupleConcat(hv_WindowRow, hv_WindowColumn, hv_WindowWidth, hv_WindowHeight);
                }
                //
                //Set some meta information about the new child window handle.
                hv_MetaInfo.Dispose();
                HOperatorSet.GetDictTuple(hv_WindowHandleDict, "meta_information", out hv_MetaInfo);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_child_window_coordinates",
                        hv_PrevWindowCoordinatesOut);
                }
                HOperatorSet.SetDictTuple(hv_WindowHandleDict, "meta_information", hv_MetaInfo);
                //

                hv_StringWidth.Dispose();
                hv_IndexText.Dispose();
                hv__.Dispose();
                hv_TextWidth.Dispose();
                hv_WindowRow.Dispose();
                hv_WindowColumn.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();
                hv_MetaInfo.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_StringWidth.Dispose();
                hv_IndexText.Dispose();
                hv__.Dispose();
                hv_TextWidth.Dispose();
                hv_WindowRow.Dispose();
                hv_WindowColumn.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();
                hv_MetaInfo.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Graphics / Window
        // Short Description: Open a new window, either next to the last ones, or in a new row. 
        public void open_next_window(HTuple hv_Font, HTuple hv_FontSize, HTuple hv_ShowBottomDesc,
            HTuple hv_WidthImage, HTuple hv_HeightImage, HTuple hv_MapColorBarWidth, HTuple hv_ScaleWindows,
            HTuple hv_ThresholdWidth, HTuple hv_PrevWindowCoordinates, HTuple hv_WindowHandleDict,
            HTuple hv_WindowHandleKey, out HTuple hv_WindowHandleNew, out HTuple hv_WindowImageRatio,
            out HTuple hv_PrevWindowCoordinatesOut)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_PrevWindowRow = new HTuple(), hv_PrevWindowColumn = new HTuple();
            HTuple hv_PrevWindowWidth = new HTuple(), hv_PrevWindowHeight = new HTuple();
            HTuple hv_WindowRow = new HTuple(), hv_WindowColumn = new HTuple();
            HTuple hv_SetPartRow2 = new HTuple(), hv_SetPartColumn2 = new HTuple();
            HTuple hv_MarginBottom = new HTuple(), hv__ = new HTuple();
            HTuple hv_WindowWidth = new HTuple(), hv_WindowHeight = new HTuple();
            HTuple hv_MetaInfo = new HTuple();
            // Initialize local and output iconic variables 
            hv_WindowHandleNew = new HTuple();
            hv_WindowImageRatio = new HTuple();
            hv_PrevWindowCoordinatesOut = new HTuple();
            try
            {
                //
                //This procedure opens a new window, either next to
                //the last ones, or in a new row.
                //
                //Get coordinates of previous window.
                hv_PrevWindowRow.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PrevWindowRow = hv_PrevWindowCoordinates.TupleSelect(
                        0);
                }
                hv_PrevWindowColumn.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PrevWindowColumn = hv_PrevWindowCoordinates.TupleSelect(
                        1);
                }
                hv_PrevWindowWidth.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PrevWindowWidth = hv_PrevWindowCoordinates.TupleSelect(
                        2);
                }
                hv_PrevWindowHeight.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_PrevWindowHeight = hv_PrevWindowCoordinates.TupleSelect(
                        3);
                }
                //
                if ((int)(new HTuple(((hv_PrevWindowColumn + hv_PrevWindowWidth)).TupleGreater(
                    hv_ThresholdWidth))) != 0)
                {
                    //Open window in new row.
                    hv_WindowRow.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_WindowRow = (hv_PrevWindowRow + hv_PrevWindowHeight) + 55;
                    }
                    hv_WindowColumn.Dispose();
                    hv_WindowColumn = 0;
                }
                else
                {
                    //Open window in same row.
                    hv_WindowRow.Dispose();
                    hv_WindowRow = new HTuple(hv_PrevWindowRow);
                    hv_WindowColumn.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_WindowColumn = hv_PrevWindowColumn + hv_PrevWindowWidth;
                    }
                    if ((int)(new HTuple(hv_WindowColumn.TupleNotEqual(0))) != 0)
                    {
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_WindowColumn = hv_WindowColumn + 5;
                                hv_WindowColumn.Dispose();
                                hv_WindowColumn = ExpTmpLocalVar_WindowColumn;
                            }
                        }
                    }
                }
                //
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WindowHandleNew.Dispose();
                    dev_open_window_fit_size(hv_WindowRow, hv_WindowColumn, hv_WidthImage, hv_HeightImage,
                        (new HTuple(500)).TupleConcat(800) * hv_ScaleWindows, (new HTuple(400)).TupleConcat(
                        600) * hv_ScaleWindows, out hv_WindowHandleNew);
                }
                set_display_font(hv_WindowHandleNew, hv_FontSize, hv_Font, "true", "false");
                //
                //Get meta information of new window handle
                hv_WindowImageRatio.Dispose(); hv_SetPartRow2.Dispose(); hv_SetPartColumn2.Dispose(); hv_PrevWindowCoordinatesOut.Dispose(); hv_MarginBottom.Dispose();
                get_window_meta_information(hv_WindowHandleNew, hv_WidthImage, hv_HeightImage,
                    hv_MapColorBarWidth, hv_WindowRow, hv_WindowColumn, hv_ShowBottomDesc,
                    out hv_WindowImageRatio, out hv_SetPartRow2, out hv_SetPartColumn2, out hv_PrevWindowCoordinatesOut,
                    out hv_MarginBottom);
                //
                //Add space for displaying text at the bottom of the window
                hv__.Dispose(); hv__.Dispose(); hv_WindowWidth.Dispose(); hv_WindowHeight.Dispose();
                HOperatorSet.GetWindowExtents(hv_WindowHandleNew, out hv__, out hv__, out hv_WindowWidth,
                    out hv_WindowHeight);
                if (HDevWindowStack.IsOpen())
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.SetWindowExtents(HDevWindowStack.GetActive(), hv_WindowRow,
                            hv_WindowColumn, hv_WindowWidth + hv_MapColorBarWidth, hv_WindowHeight + hv_MarginBottom);
                    }
                }
                if (hv_PrevWindowCoordinatesOut == null)
                    hv_PrevWindowCoordinatesOut = new HTuple();
                hv_PrevWindowCoordinatesOut[2] = hv_WindowWidth + hv_MapColorBarWidth;
                if (hv_PrevWindowCoordinatesOut == null)
                    hv_PrevWindowCoordinatesOut = new HTuple();
                hv_PrevWindowCoordinatesOut[3] = hv_WindowHeight + hv_MarginBottom;
                //
                //Set window handle and some meta information about the new window handle.
                HOperatorSet.SetDictTuple(hv_WindowHandleDict, hv_WindowHandleKey, hv_WindowHandleNew);
                hv_MetaInfo.Dispose();
                HOperatorSet.GetDictTuple(hv_WindowHandleDict, "meta_information", out hv_MetaInfo);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_window_image_ratio",
                        hv_WindowImageRatio);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_set_part_row2",
                        hv_SetPartRow2);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_set_part_column2",
                        hv_SetPartColumn2);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_margin_bottom",
                        hv_MarginBottom);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_map_color_bar_with",
                        hv_MapColorBarWidth);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetDictTuple(hv_MetaInfo, hv_WindowHandleKey + "_window_coordinates",
                        hv_PrevWindowCoordinatesOut);
                }
                HOperatorSet.SetDictTuple(hv_WindowHandleDict, "meta_information", hv_MetaInfo);
                //

                hv_PrevWindowRow.Dispose();
                hv_PrevWindowColumn.Dispose();
                hv_PrevWindowWidth.Dispose();
                hv_PrevWindowHeight.Dispose();
                hv_WindowRow.Dispose();
                hv_WindowColumn.Dispose();
                hv_SetPartRow2.Dispose();
                hv_SetPartColumn2.Dispose();
                hv_MarginBottom.Dispose();
                hv__.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();
                hv_MetaInfo.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_PrevWindowRow.Dispose();
                hv_PrevWindowColumn.Dispose();
                hv_PrevWindowWidth.Dispose();
                hv_PrevWindowHeight.Dispose();
                hv_WindowRow.Dispose();
                hv_WindowColumn.Dispose();
                hv_SetPartRow2.Dispose();
                hv_SetPartColumn2.Dispose();
                hv_MarginBottom.Dispose();
                hv__.Dispose();
                hv_WindowWidth.Dispose();
                hv_WindowHeight.Dispose();
                hv_MetaInfo.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Deep Learning / Object Detection
        // Short Description: This procedure preprocesses the bounding boxes of a given sample. 
        public void preprocess_dl_model_bbox_rect1(HObject ho_ImageRaw, HTuple hv_DLSample,
            HTuple hv_DLPreprocessParam)
        {




            // Local iconic variables 

            HObject ho_DomainRaw = null;

            // Local control variables 

            HTuple hv_ImageWidth = new HTuple(), hv_ImageHeight = new HTuple();
            HTuple hv_DomainHandling = new HTuple(), hv_BBoxCol1 = new HTuple();
            HTuple hv_BBoxCol2 = new HTuple(), hv_BBoxRow1 = new HTuple();
            HTuple hv_BBoxRow2 = new HTuple(), hv_BBoxLabel = new HTuple();
            HTuple hv_Exception = new HTuple(), hv_ImageId = new HTuple();
            HTuple hv_ExceptionMessage = new HTuple(), hv_BoxesInvalid = new HTuple();
            HTuple hv_RowDomain1 = new HTuple(), hv_ColumnDomain1 = new HTuple();
            HTuple hv_RowDomain2 = new HTuple(), hv_ColumnDomain2 = new HTuple();
            HTuple hv_WidthRaw = new HTuple(), hv_HeightRaw = new HTuple();
            HTuple hv_Row1 = new HTuple(), hv_Col1 = new HTuple();
            HTuple hv_Row2 = new HTuple(), hv_Col2 = new HTuple();
            HTuple hv_MaskDelete = new HTuple(), hv_MaskNewBbox = new HTuple();
            HTuple hv_BBoxCol1New = new HTuple(), hv_BBoxCol2New = new HTuple();
            HTuple hv_BBoxRow1New = new HTuple(), hv_BBoxRow2New = new HTuple();
            HTuple hv_BBoxLabelNew = new HTuple(), hv_FactorResampleWidth = new HTuple();
            HTuple hv_FactorResampleHeight = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_DomainRaw);
            try
            {
                //
                //This procedure preprocesses the bounding box coordinates of a given sample.
                //
                //Check the validity of the preprocessing parameters.
                check_dl_preprocess_param(hv_DLPreprocessParam);
                //
                //Get the preprocessing parameters.
                hv_ImageWidth.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "image_width", out hv_ImageWidth);
                hv_ImageHeight.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "image_height", out hv_ImageHeight);
                hv_DomainHandling.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "domain_handling", out hv_DomainHandling);
                //
                //Get bounding box coordinates and labels.
                try
                {
                    hv_BBoxCol1.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLSample, "bbox_col1", out hv_BBoxCol1);
                    hv_BBoxCol2.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLSample, "bbox_col2", out hv_BBoxCol2);
                    hv_BBoxRow1.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLSample, "bbox_row1", out hv_BBoxRow1);
                    hv_BBoxRow2.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLSample, "bbox_row2", out hv_BBoxRow2);
                    hv_BBoxLabel.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLSample, "bbox_label_id", out hv_BBoxLabel);
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    hv_ImageId.Dispose();
                    HOperatorSet.GetDictTuple(hv_DLSample, "image_id", out hv_ImageId);
                    if ((int)(new HTuple(((hv_Exception.TupleSelect(0))).TupleEqual(1302))) != 0)
                    {
                        hv_ExceptionMessage.Dispose();
                        hv_ExceptionMessage = "A bounding box coordinate key is missing.";
                    }
                    else
                    {
                        hv_ExceptionMessage.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ExceptionMessage = hv_Exception.TupleSelect(
                                2);
                        }
                    }
                    throw new HalconException((("An error has occurred during preprocessing image_id " + hv_ImageId) + " when getting bounding box coordinates : ") + hv_ExceptionMessage);
                }
                //
                //Check that there are no invalid boxes.
                if ((int)(new HTuple((new HTuple(hv_BBoxRow1.TupleLength())).TupleGreater(0))) != 0)
                {
                    hv_BoxesInvalid.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_BoxesInvalid = ((hv_BBoxRow1.TupleGreaterEqualElem(
                            hv_BBoxRow2))).TupleOr(hv_BBoxCol1.TupleGreaterEqualElem(hv_BBoxCol2));
                    }
                    if ((int)(new HTuple(((hv_BoxesInvalid.TupleSum())).TupleGreater(0))) != 0)
                    {
                        hv_ImageId.Dispose();
                        HOperatorSet.GetDictTuple(hv_DLSample, "image_id", out hv_ImageId);
                        throw new HalconException(("An error has occurred during preprocessing image_id " + hv_ImageId) + new HTuple(": Sample contains at least one box with zero-area, i.e. bbox_col1 >= bbox_col2 or bbox_row1 >= bbox_row2."));
                    }
                }
                //
                //If the domain is cropped, crop bboxes.
                //
                if ((int)(new HTuple(hv_DomainHandling.TupleEqual("crop_domain"))) != 0)
                {
                    //
                    //Get domain.
                    ho_DomainRaw.Dispose();
                    HOperatorSet.GetDomain(ho_ImageRaw, out ho_DomainRaw);
                    //
                    //Set the size of the raw image to the domain extensions.
                    hv_RowDomain1.Dispose(); hv_ColumnDomain1.Dispose(); hv_RowDomain2.Dispose(); hv_ColumnDomain2.Dispose();
                    HOperatorSet.SmallestRectangle1(ho_DomainRaw, out hv_RowDomain1, out hv_ColumnDomain1,
                        out hv_RowDomain2, out hv_ColumnDomain2);
                    hv_WidthRaw.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_WidthRaw = (hv_ColumnDomain2 - hv_ColumnDomain1) + 1;
                    }
                    hv_HeightRaw.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_HeightRaw = (hv_RowDomain2 - hv_RowDomain1) + 1;
                    }
                    //
                    //Crop the bounding boxes.
                    hv_Row1.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Row1 = hv_BBoxRow1.TupleMax2(
                            hv_RowDomain1);
                    }
                    hv_Col1.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Col1 = hv_BBoxCol1.TupleMax2(
                            hv_ColumnDomain1);
                    }
                    hv_Row2.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Row2 = hv_BBoxRow2.TupleMin2(
                            hv_RowDomain2);
                    }
                    hv_Col2.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Col2 = hv_BBoxCol2.TupleMin2(
                            hv_ColumnDomain2);
                    }
                    hv_MaskDelete.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_MaskDelete = ((hv_Row1.TupleGreaterEqualElem(
                            hv_Row2))).TupleOr(hv_Col1.TupleGreaterEqualElem(hv_Col2));
                    }
                    hv_MaskNewBbox.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_MaskNewBbox = 1 - hv_MaskDelete;
                    }
                    //Store the preprocessed bbox entries.
                    hv_BBoxCol1New.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_BBoxCol1New = (hv_Col1.TupleSelectMask(
                            hv_MaskNewBbox)) - hv_ColumnDomain1;
                    }
                    hv_BBoxCol2New.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_BBoxCol2New = (hv_Col2.TupleSelectMask(
                            hv_MaskNewBbox)) - hv_ColumnDomain1;
                    }
                    hv_BBoxRow1New.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_BBoxRow1New = (hv_Row1.TupleSelectMask(
                            hv_MaskNewBbox)) - hv_RowDomain1;
                    }
                    hv_BBoxRow2New.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_BBoxRow2New = (hv_Row2.TupleSelectMask(
                            hv_MaskNewBbox)) - hv_RowDomain1;
                    }
                    hv_BBoxLabelNew.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_BBoxLabelNew = hv_BBoxLabel.TupleSelectMask(
                            hv_MaskNewBbox);
                    }
                    //
                }
                else if ((int)(new HTuple(hv_DomainHandling.TupleEqual("full_domain"))) != 0)
                {
                    //If the entire image is used, set the variables accordingly.
                    //Get the original size.
                    hv_WidthRaw.Dispose(); hv_HeightRaw.Dispose();
                    HOperatorSet.GetImageSize(ho_ImageRaw, out hv_WidthRaw, out hv_HeightRaw);
                    //Set new coordinates to input coordinates.
                    hv_BBoxCol1New.Dispose();
                    hv_BBoxCol1New = new HTuple(hv_BBoxCol1);
                    hv_BBoxCol2New.Dispose();
                    hv_BBoxCol2New = new HTuple(hv_BBoxCol2);
                    hv_BBoxRow1New.Dispose();
                    hv_BBoxRow1New = new HTuple(hv_BBoxRow1);
                    hv_BBoxRow2New.Dispose();
                    hv_BBoxRow2New = new HTuple(hv_BBoxRow2);
                    hv_BBoxLabelNew.Dispose();
                    hv_BBoxLabelNew = new HTuple(hv_BBoxLabel);
                }
                else
                {
                    throw new HalconException("Unsupported parameter value for 'domain_handling'");
                }
                //
                //Rescale the bounding boxes.
                //
                //Get required images width and height.
                //
                //Only rescale bounding boxes if the required image dimensions are not the raw dimensions.
                if ((int)((new HTuple(hv_ImageHeight.TupleNotEqual(hv_HeightRaw))).TupleOr(
                    new HTuple(hv_ImageWidth.TupleNotEqual(hv_WidthRaw)))) != 0)
                {
                    //Calculate rescaling factor.
                    hv_FactorResampleWidth.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_FactorResampleWidth = (hv_ImageWidth.TupleReal()
                            ) / hv_WidthRaw;
                    }
                    hv_FactorResampleHeight.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_FactorResampleHeight = (hv_ImageHeight.TupleReal()
                            ) / hv_HeightRaw;
                    }
                    //Rescale the bbox coordinates.
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_BBoxCol1New = ((hv_BBoxCol1New * hv_FactorResampleWidth)).TupleRound()
                                ;
                            hv_BBoxCol1New.Dispose();
                            hv_BBoxCol1New = ExpTmpLocalVar_BBoxCol1New;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_BBoxCol2New = ((hv_BBoxCol2New * hv_FactorResampleWidth)).TupleRound()
                                ;
                            hv_BBoxCol2New.Dispose();
                            hv_BBoxCol2New = ExpTmpLocalVar_BBoxCol2New;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_BBoxRow1New = ((hv_BBoxRow1New * hv_FactorResampleHeight)).TupleRound()
                                ;
                            hv_BBoxRow1New.Dispose();
                            hv_BBoxRow1New = ExpTmpLocalVar_BBoxRow1New;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_BBoxRow2New = ((hv_BBoxRow2New * hv_FactorResampleHeight)).TupleRound()
                                ;
                            hv_BBoxRow2New.Dispose();
                            hv_BBoxRow2New = ExpTmpLocalVar_BBoxRow2New;
                        }
                    }
                    //
                }
                HOperatorSet.SetDictTuple(hv_DLSample, "bbox_col1", hv_BBoxCol1New);
                HOperatorSet.SetDictTuple(hv_DLSample, "bbox_col2", hv_BBoxCol2New);
                HOperatorSet.SetDictTuple(hv_DLSample, "bbox_row1", hv_BBoxRow1New);
                HOperatorSet.SetDictTuple(hv_DLSample, "bbox_row2", hv_BBoxRow2New);
                HOperatorSet.SetDictTuple(hv_DLSample, "bbox_label_id", hv_BBoxLabelNew);
                ho_DomainRaw.Dispose();

                hv_ImageWidth.Dispose();
                hv_ImageHeight.Dispose();
                hv_DomainHandling.Dispose();
                hv_BBoxCol1.Dispose();
                hv_BBoxCol2.Dispose();
                hv_BBoxRow1.Dispose();
                hv_BBoxRow2.Dispose();
                hv_BBoxLabel.Dispose();
                hv_Exception.Dispose();
                hv_ImageId.Dispose();
                hv_ExceptionMessage.Dispose();
                hv_BoxesInvalid.Dispose();
                hv_RowDomain1.Dispose();
                hv_ColumnDomain1.Dispose();
                hv_RowDomain2.Dispose();
                hv_ColumnDomain2.Dispose();
                hv_WidthRaw.Dispose();
                hv_HeightRaw.Dispose();
                hv_Row1.Dispose();
                hv_Col1.Dispose();
                hv_Row2.Dispose();
                hv_Col2.Dispose();
                hv_MaskDelete.Dispose();
                hv_MaskNewBbox.Dispose();
                hv_BBoxCol1New.Dispose();
                hv_BBoxCol2New.Dispose();
                hv_BBoxRow1New.Dispose();
                hv_BBoxRow2New.Dispose();
                hv_BBoxLabelNew.Dispose();
                hv_FactorResampleWidth.Dispose();
                hv_FactorResampleHeight.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_DomainRaw.Dispose();

                hv_ImageWidth.Dispose();
                hv_ImageHeight.Dispose();
                hv_DomainHandling.Dispose();
                hv_BBoxCol1.Dispose();
                hv_BBoxCol2.Dispose();
                hv_BBoxRow1.Dispose();
                hv_BBoxRow2.Dispose();
                hv_BBoxLabel.Dispose();
                hv_Exception.Dispose();
                hv_ImageId.Dispose();
                hv_ExceptionMessage.Dispose();
                hv_BoxesInvalid.Dispose();
                hv_RowDomain1.Dispose();
                hv_ColumnDomain1.Dispose();
                hv_RowDomain2.Dispose();
                hv_ColumnDomain2.Dispose();
                hv_WidthRaw.Dispose();
                hv_HeightRaw.Dispose();
                hv_Row1.Dispose();
                hv_Col1.Dispose();
                hv_Row2.Dispose();
                hv_Col2.Dispose();
                hv_MaskDelete.Dispose();
                hv_MaskNewBbox.Dispose();
                hv_BBoxCol1New.Dispose();
                hv_BBoxCol2New.Dispose();
                hv_BBoxRow1New.Dispose();
                hv_BBoxRow2New.Dispose();
                hv_BBoxLabelNew.Dispose();
                hv_FactorResampleWidth.Dispose();
                hv_FactorResampleHeight.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Deep Learning / Model
        // Short Description: Preprocess images for deep-learning-based training and inference. 
        public void preprocess_dl_model_images(HObject ho_Images, out HObject ho_ImagesPreprocessed,
            HTuple hv_DLPreprocessParam)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_ObjectSelected = null, ho_ThreeChannelImage = null;
            HObject ho_SingleChannelImage = null;

            // Local copy input parameter variables 
            HObject ho_Images_COPY_INP_TMP;
            ho_Images_COPY_INP_TMP = new HObject(ho_Images);



            // Local control variables 

            HTuple hv_ImageWidth = new HTuple(), hv_ImageHeight = new HTuple();
            HTuple hv_ImageNumChannels = new HTuple(), hv_ImageRangeMin = new HTuple();
            HTuple hv_ImageRangeMax = new HTuple(), hv_DomainHandling = new HTuple();
            HTuple hv_ContrastNormalization = new HTuple(), hv_ImageWidthInput = new HTuple();
            HTuple hv_ImageHeightInput = new HTuple(), hv_EqualWidth = new HTuple();
            HTuple hv_EqualHeight = new HTuple(), hv_Type = new HTuple();
            HTuple hv_NumMatches = new HTuple(), hv_NumImages = new HTuple();
            HTuple hv_EqualByte = new HTuple(), hv_RescaleRange = new HTuple();
            HTuple hv_NumChannelsAllImages = new HTuple(), hv_ImageNumChannelsTuple = new HTuple();
            HTuple hv_IndicesWrongChannels = new HTuple(), hv_IndexWrongImages = new HTuple();
            HTuple hv_ImageIndex = new HTuple(), hv_NumChannels = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImagesPreprocessed);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelected);
            HOperatorSet.GenEmptyObj(out ho_ThreeChannelImage);
            HOperatorSet.GenEmptyObj(out ho_SingleChannelImage);
            try
            {
                //
                //This procedure preprocesses the provided Images
                //according to the parameters in the dictionary DLPreprocessParam.
                //Note that depending on the images,
                //additional preprocessing steps might be beneficial.
                //
                //Check the validity of the preprocessing parameters.
                check_dl_preprocess_param(hv_DLPreprocessParam);
                //
                //Get the preprocessing parameters.
                hv_ImageWidth.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "image_width", out hv_ImageWidth);
                hv_ImageHeight.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "image_height", out hv_ImageHeight);
                hv_ImageNumChannels.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "image_num_channels", out hv_ImageNumChannels);
                hv_ImageRangeMin.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "image_range_min", out hv_ImageRangeMin);
                hv_ImageRangeMax.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "image_range_max", out hv_ImageRangeMax);
                hv_DomainHandling.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "domain_handling", out hv_DomainHandling);
                hv_ContrastNormalization.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "contrast_normalization", out hv_ContrastNormalization);
                //
                //Preprocess the images.
                //
                if ((int)(new HTuple(hv_DomainHandling.TupleEqual("full_domain"))) != 0)
                {
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.FullDomain(ho_Images_COPY_INP_TMP, out ExpTmpOutVar_0);
                        ho_Images_COPY_INP_TMP.Dispose();
                        ho_Images_COPY_INP_TMP = ExpTmpOutVar_0;
                    }
                }
                else if ((int)(new HTuple(hv_DomainHandling.TupleEqual("crop_domain"))) != 0)
                {
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.CropDomain(ho_Images_COPY_INP_TMP, out ExpTmpOutVar_0);
                        ho_Images_COPY_INP_TMP.Dispose();
                        ho_Images_COPY_INP_TMP = ExpTmpOutVar_0;
                    }
                }
                else
                {
                    throw new HalconException("Unsupported parameter value for 'domain_handling'");
                }
                //
                //Zoom images only if they have a different size than the specified size.
                hv_ImageWidthInput.Dispose(); hv_ImageHeightInput.Dispose();
                HOperatorSet.GetImageSize(ho_Images_COPY_INP_TMP, out hv_ImageWidthInput, out hv_ImageHeightInput);
                hv_EqualWidth.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_EqualWidth = hv_ImageWidth.TupleEqualElem(
                        hv_ImageWidthInput);
                }
                hv_EqualHeight.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_EqualHeight = hv_ImageHeight.TupleEqualElem(
                        hv_ImageHeightInput);
                }
                if ((int)((new HTuple(((hv_EqualWidth.TupleMin())).TupleEqual(0))).TupleOr(
                    new HTuple(((hv_EqualHeight.TupleMin())).TupleEqual(0)))) != 0)
                {
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ZoomImageSize(ho_Images_COPY_INP_TMP, out ExpTmpOutVar_0, hv_ImageWidth,
                            hv_ImageHeight, "constant");
                        ho_Images_COPY_INP_TMP.Dispose();
                        ho_Images_COPY_INP_TMP = ExpTmpOutVar_0;
                    }
                }
                if ((int)(new HTuple(hv_ContrastNormalization.TupleEqual("true"))) != 0)
                {
                    //Scale the gray values to [0-255].
                    //Note that this converts the image to 'byte'.
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ScaleImageMax(ho_Images_COPY_INP_TMP, out ExpTmpOutVar_0);
                        ho_Images_COPY_INP_TMP.Dispose();
                        ho_Images_COPY_INP_TMP = ExpTmpOutVar_0;
                    }
                }
                else if ((int)(new HTuple(hv_ContrastNormalization.TupleNotEqual("false"))) != 0)
                {
                    throw new HalconException("Unsupported parameter value for 'contrast_normalization'");
                }
                //
                //Check the type of the input images.
                hv_Type.Dispose();
                HOperatorSet.GetImageType(ho_Images_COPY_INP_TMP, out hv_Type);
                hv_NumMatches.Dispose();
                HOperatorSet.TupleRegexpTest(hv_Type, "byte|real", out hv_NumMatches);
                hv_NumImages.Dispose();
                HOperatorSet.CountObj(ho_Images_COPY_INP_TMP, out hv_NumImages);
                if ((int)(new HTuple(hv_NumMatches.TupleNotEqual(hv_NumImages))) != 0)
                {
                    throw new HalconException("Please provide only images of type 'byte' or 'real'.");
                }
                //If the type is 'byte', convert it to 'real' and scale it.
                //The gray value scaling does not work on 'byte' images.
                //For 'real' images it is assumed that the range is already correct.
                hv_EqualByte.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_EqualByte = hv_Type.TupleEqualElem(
                        "byte");
                }
                if ((int)(new HTuple(((hv_EqualByte.TupleMax())).TupleEqual(1))) != 0)
                {
                    if ((int)(new HTuple(((hv_EqualByte.TupleMin())).TupleEqual(0))) != 0)
                    {
                        throw new HalconException("Passing mixed type images is not supported.");
                    }
                    //Convert the image type from 'byte' to 'real',
                    //because the model expects 'real' images.
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConvertImageType(ho_Images_COPY_INP_TMP, out ExpTmpOutVar_0,
                            "real");
                        ho_Images_COPY_INP_TMP.Dispose();
                        ho_Images_COPY_INP_TMP = ExpTmpOutVar_0;
                    }
                    //Scale/Shift the gray values from [0-255] to the expected range.
                    hv_RescaleRange.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_RescaleRange = (hv_ImageRangeMax - hv_ImageRangeMin) / 255.0;
                    }
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ScaleImage(ho_Images_COPY_INP_TMP, out ExpTmpOutVar_0, hv_RescaleRange,
                            hv_ImageRangeMin);
                        ho_Images_COPY_INP_TMP.Dispose();
                        ho_Images_COPY_INP_TMP = ExpTmpOutVar_0;
                    }
                }
                //
                //Check the number of channels.
                hv_NumImages.Dispose();
                HOperatorSet.CountObj(ho_Images_COPY_INP_TMP, out hv_NumImages);
                //Check all images for number of channels.
                hv_NumChannelsAllImages.Dispose();
                HOperatorSet.CountChannels(ho_Images_COPY_INP_TMP, out hv_NumChannelsAllImages);
                hv_ImageNumChannelsTuple.Dispose();
                HOperatorSet.TupleGenConst(hv_NumImages, hv_ImageNumChannels, out hv_ImageNumChannelsTuple);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_IndicesWrongChannels.Dispose();
                    HOperatorSet.TupleFind(hv_NumChannelsAllImages.TupleNotEqualElem(hv_ImageNumChannelsTuple),
                        1, out hv_IndicesWrongChannels);
                }
                //
                //Correct images with a wrong number of channels.
                //
                if ((int)(new HTuple(hv_IndicesWrongChannels.TupleNotEqual(-1))) != 0)
                {
                    //
                    for (hv_IndexWrongImages = 0; (int)hv_IndexWrongImages <= (int)((new HTuple(hv_IndicesWrongChannels.TupleLength()
                        )) - 1); hv_IndexWrongImages = (int)hv_IndexWrongImages + 1)
                    {
                        //Get the index, the number of channels and the image
                        //for each image with wrong number of channels.
                        hv_ImageIndex.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_ImageIndex = (hv_IndicesWrongChannels.TupleSelect(
                                hv_IndexWrongImages)) + 1;
                        }
                        hv_NumChannels.Dispose();
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            hv_NumChannels = hv_NumChannelsAllImages.TupleSelect(
                                hv_ImageIndex - 1);
                        }
                        ho_ObjectSelected.Dispose();
                        HOperatorSet.SelectObj(ho_Images_COPY_INP_TMP, out ho_ObjectSelected, hv_ImageIndex);
                        //
                        if ((int)((new HTuple(hv_NumChannels.TupleEqual(1))).TupleAnd(new HTuple(hv_ImageNumChannels.TupleEqual(
                            3)))) != 0)
                        {
                            //If the image is a grayscale image, but the model expects a color image:
                            //convert it to an image with three channels.
                            ho_ThreeChannelImage.Dispose();
                            HOperatorSet.Compose3(ho_ObjectSelected, ho_ObjectSelected, ho_ObjectSelected,
                                out ho_ThreeChannelImage);
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ReplaceObj(ho_Images_COPY_INP_TMP, ho_ThreeChannelImage,
                                    out ExpTmpOutVar_0, hv_ImageIndex);
                                ho_Images_COPY_INP_TMP.Dispose();
                                ho_Images_COPY_INP_TMP = ExpTmpOutVar_0;
                            }
                        }
                        else if ((int)((new HTuple(hv_NumChannels.TupleEqual(3))).TupleAnd(
                            new HTuple(hv_ImageNumChannels.TupleEqual(1)))) != 0)
                        {
                            //If the image is a color image, but the model expects a grayscale image:
                            //convert it to an image with only one channel.
                            ho_SingleChannelImage.Dispose();
                            HOperatorSet.Rgb1ToGray(ho_ObjectSelected, out ho_SingleChannelImage);
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ReplaceObj(ho_Images_COPY_INP_TMP, ho_SingleChannelImage,
                                    out ExpTmpOutVar_0, hv_ImageIndex);
                                ho_Images_COPY_INP_TMP.Dispose();
                                ho_Images_COPY_INP_TMP = ExpTmpOutVar_0;
                            }
                        }
                        else
                        {
                            throw new HalconException("Number of channels is not supported. Please check for images with a number of channels different to 1 and 3 and perform their preprocessing yourself.");
                        }
                        //
                    }
                }
                //
                //Write preprocessed image to output variable.
                ho_ImagesPreprocessed.Dispose();
                ho_ImagesPreprocessed = new HObject(ho_Images_COPY_INP_TMP);
                ho_Images_COPY_INP_TMP.Dispose();
                ho_ObjectSelected.Dispose();
                ho_ThreeChannelImage.Dispose();
                ho_SingleChannelImage.Dispose();

                hv_ImageWidth.Dispose();
                hv_ImageHeight.Dispose();
                hv_ImageNumChannels.Dispose();
                hv_ImageRangeMin.Dispose();
                hv_ImageRangeMax.Dispose();
                hv_DomainHandling.Dispose();
                hv_ContrastNormalization.Dispose();
                hv_ImageWidthInput.Dispose();
                hv_ImageHeightInput.Dispose();
                hv_EqualWidth.Dispose();
                hv_EqualHeight.Dispose();
                hv_Type.Dispose();
                hv_NumMatches.Dispose();
                hv_NumImages.Dispose();
                hv_EqualByte.Dispose();
                hv_RescaleRange.Dispose();
                hv_NumChannelsAllImages.Dispose();
                hv_ImageNumChannelsTuple.Dispose();
                hv_IndicesWrongChannels.Dispose();
                hv_IndexWrongImages.Dispose();
                hv_ImageIndex.Dispose();
                hv_NumChannels.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Images_COPY_INP_TMP.Dispose();
                ho_ObjectSelected.Dispose();
                ho_ThreeChannelImage.Dispose();
                ho_SingleChannelImage.Dispose();

                hv_ImageWidth.Dispose();
                hv_ImageHeight.Dispose();
                hv_ImageNumChannels.Dispose();
                hv_ImageRangeMin.Dispose();
                hv_ImageRangeMax.Dispose();
                hv_DomainHandling.Dispose();
                hv_ContrastNormalization.Dispose();
                hv_ImageWidthInput.Dispose();
                hv_ImageHeightInput.Dispose();
                hv_EqualWidth.Dispose();
                hv_EqualHeight.Dispose();
                hv_Type.Dispose();
                hv_NumMatches.Dispose();
                hv_NumImages.Dispose();
                hv_EqualByte.Dispose();
                hv_RescaleRange.Dispose();
                hv_NumChannelsAllImages.Dispose();
                hv_ImageNumChannelsTuple.Dispose();
                hv_IndicesWrongChannels.Dispose();
                hv_IndexWrongImages.Dispose();
                hv_ImageIndex.Dispose();
                hv_NumChannels.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Deep Learning / Semantic Segmentation
        // Short Description: Preprocess segmentation and weight images for deep-learning-based segmentation training and inference. 
        public void preprocess_dl_model_segmentations(HObject ho_ImagesRaw, HObject ho_Segmentations,
            out HObject ho_SegmentationsPreprocessed, HTuple hv_DLPreprocessParam)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_Domain = null, ho_SelectedSeg = null;
            HObject ho_SelectedDomain = null;

            // Local copy input parameter variables 
            HObject ho_Segmentations_COPY_INP_TMP;
            ho_Segmentations_COPY_INP_TMP = new HObject(ho_Segmentations);



            // Local control variables 

            HTuple hv_NumberImages = new HTuple(), hv_NumberSegmentations = new HTuple();
            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_WidthSeg = new HTuple(), hv_HeightSeg = new HTuple();
            HTuple hv_DLModelType = new HTuple(), hv_ImageWidth = new HTuple();
            HTuple hv_ImageHeight = new HTuple(), hv_ImageNumChannels = new HTuple();
            HTuple hv_ImageRangeMin = new HTuple(), hv_ImageRangeMax = new HTuple();
            HTuple hv_DomainHandling = new HTuple(), hv_SetBackgroundID = new HTuple();
            HTuple hv_ClassesToBackground = new HTuple(), hv_IgnoreClassIDs = new HTuple();
            HTuple hv_IsInt = new HTuple(), hv_IndexImage = new HTuple();
            HTuple hv_ImageWidthRaw = new HTuple(), hv_ImageHeightRaw = new HTuple();
            HTuple hv_EqualWidth = new HTuple(), hv_EqualHeight = new HTuple();
            HTuple hv_Type = new HTuple(), hv_EqualReal = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_SegmentationsPreprocessed);
            HOperatorSet.GenEmptyObj(out ho_Domain);
            HOperatorSet.GenEmptyObj(out ho_SelectedSeg);
            HOperatorSet.GenEmptyObj(out ho_SelectedDomain);
            try
            {
                //
                //This procedure preprocesses the segmentation or weight images
                //given by Segmentations so that they can be handled by
                //train_dl_model_batch and apply_dl_model.
                //
                //Check input data.
                //Examine umber of images.
                hv_NumberImages.Dispose();
                HOperatorSet.CountObj(ho_ImagesRaw, out hv_NumberImages);
                hv_NumberSegmentations.Dispose();
                HOperatorSet.CountObj(ho_Segmentations_COPY_INP_TMP, out hv_NumberSegmentations);
                if ((int)(new HTuple(hv_NumberImages.TupleNotEqual(hv_NumberSegmentations))) != 0)
                {
                    throw new HalconException("Equal number of images given in ImagesRaw and Segmentations required");
                }
                //Size of images.
                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_ImagesRaw, out hv_Width, out hv_Height);
                hv_WidthSeg.Dispose(); hv_HeightSeg.Dispose();
                HOperatorSet.GetImageSize(ho_Segmentations_COPY_INP_TMP, out hv_WidthSeg, out hv_HeightSeg);
                if ((int)((new HTuple(hv_Width.TupleNotEqual(hv_WidthSeg))).TupleOr(new HTuple(hv_Height.TupleNotEqual(
                    hv_HeightSeg)))) != 0)
                {
                    throw new HalconException("Equal size of the images given in ImagesRaw and Segmentations required.");
                }
                //Check the validity of the preprocessing parameters.
                check_dl_preprocess_param(hv_DLPreprocessParam);
                //
                //Get the relevant preprocessing parameters.
                hv_DLModelType.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "model_type", out hv_DLModelType);
                hv_ImageWidth.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "image_width", out hv_ImageWidth);
                hv_ImageHeight.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "image_height", out hv_ImageHeight);
                hv_ImageNumChannels.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "image_num_channels", out hv_ImageNumChannels);
                hv_ImageRangeMin.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "image_range_min", out hv_ImageRangeMin);
                hv_ImageRangeMax.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "image_range_max", out hv_ImageRangeMax);
                hv_DomainHandling.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "domain_handling", out hv_DomainHandling);
                //Segmentation specific parameters.
                hv_SetBackgroundID.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "set_background_id", out hv_SetBackgroundID);
                hv_ClassesToBackground.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "class_ids_background", out hv_ClassesToBackground);
                hv_IgnoreClassIDs.Dispose();
                HOperatorSet.GetDictTuple(hv_DLPreprocessParam, "ignore_class_ids", out hv_IgnoreClassIDs);
                //
                //Check the input parameter for setting the background ID.
                if ((int)(new HTuple(hv_SetBackgroundID.TupleNotEqual(new HTuple()))) != 0)
                {
                    //Check that the model is a segmentation model.
                    if ((int)(new HTuple(hv_DLModelType.TupleNotEqual("segmentation"))) != 0)
                    {
                        throw new HalconException("Setting class IDs to background is only implemented for segmentation.");
                    }
                    //Check the background ID.
                    hv_IsInt.Dispose();
                    HOperatorSet.TupleIsIntElem(hv_SetBackgroundID, out hv_IsInt);
                    if ((int)(new HTuple((new HTuple(hv_SetBackgroundID.TupleLength())).TupleNotEqual(
                        1))) != 0)
                    {
                        throw new HalconException("Only one class_id as 'set_background_id' allowed.");
                    }
                    else if ((int)(hv_IsInt.TupleNot()) != 0)
                    {
                        //Given class_id has to be of type int.
                        throw new HalconException("The class_id given as 'set_background_id' has to be of type int.");
                    }
                    //Check the values of ClassesToBackground.
                    if ((int)(new HTuple((new HTuple(hv_ClassesToBackground.TupleLength())).TupleEqual(
                        0))) != 0)
                    {
                        //Check that the given classes are of length > 0.
                        throw new HalconException(new HTuple("If 'set_background_id' is given, 'class_ids_background' must at least contain this class ID."));
                    }
                    else if ((int)(new HTuple(((hv_ClassesToBackground.TupleIntersection(
                        hv_IgnoreClassIDs))).TupleNotEqual(new HTuple()))) != 0)
                    {
                        //Check that class_ids_background is not included in the ignore_class_ids of the DLModel.
                        throw new HalconException("The given 'class_ids_background' must not be included in the 'ignore_class_ids' of the model.");
                    }
                }
                //
                //Domain handling of the image to be preprocessed.
                //
                if ((int)(new HTuple(hv_DomainHandling.TupleEqual("full_domain"))) != 0)
                {
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.FullDomain(ho_Segmentations_COPY_INP_TMP, out ExpTmpOutVar_0
                            );
                        ho_Segmentations_COPY_INP_TMP.Dispose();
                        ho_Segmentations_COPY_INP_TMP = ExpTmpOutVar_0;
                    }
                }
                else if ((int)(new HTuple(hv_DomainHandling.TupleEqual("crop_domain"))) != 0)
                {
                    //If the domain should be cropped the domain has to be transferred
                    //from the raw image to the segmentation image.
                    ho_Domain.Dispose();
                    HOperatorSet.GetDomain(ho_ImagesRaw, out ho_Domain);
                    HTuple end_val66 = hv_NumberImages;
                    HTuple step_val66 = 1;
                    for (hv_IndexImage = 1; hv_IndexImage.Continue(end_val66, step_val66); hv_IndexImage = hv_IndexImage.TupleAdd(step_val66))
                    {
                        ho_SelectedSeg.Dispose();
                        HOperatorSet.SelectObj(ho_Segmentations_COPY_INP_TMP, out ho_SelectedSeg,
                            hv_IndexImage);
                        ho_SelectedDomain.Dispose();
                        HOperatorSet.SelectObj(ho_Domain, out ho_SelectedDomain, hv_IndexImage);
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ChangeDomain(ho_SelectedSeg, ho_SelectedDomain, out ExpTmpOutVar_0
                                );
                            ho_SelectedSeg.Dispose();
                            ho_SelectedSeg = ExpTmpOutVar_0;
                        }
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ReplaceObj(ho_Segmentations_COPY_INP_TMP, ho_SelectedSeg,
                                out ExpTmpOutVar_0, hv_IndexImage);
                            ho_Segmentations_COPY_INP_TMP.Dispose();
                            ho_Segmentations_COPY_INP_TMP = ExpTmpOutVar_0;
                        }
                    }
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.CropDomain(ho_Segmentations_COPY_INP_TMP, out ExpTmpOutVar_0
                            );
                        ho_Segmentations_COPY_INP_TMP.Dispose();
                        ho_Segmentations_COPY_INP_TMP = ExpTmpOutVar_0;
                    }
                }
                else
                {
                    throw new HalconException("Unsupported parameter value for 'domain_handling'");
                }
                //
                //Preprocess the segmentation images.
                //
                //Set all background classes to the given background class ID.
                if ((int)(new HTuple(hv_SetBackgroundID.TupleNotEqual(new HTuple()))) != 0)
                {
                    {
                        HObject ExpTmpOutVar_0;
                        reassign_pixel_values(ho_Segmentations_COPY_INP_TMP, out ExpTmpOutVar_0,
                            hv_ClassesToBackground, hv_SetBackgroundID);
                        ho_Segmentations_COPY_INP_TMP.Dispose();
                        ho_Segmentations_COPY_INP_TMP = ExpTmpOutVar_0;
                    }
                }
                //
                //Zoom images only if they have a different size than the specified size.
                hv_ImageWidthRaw.Dispose(); hv_ImageHeightRaw.Dispose();
                HOperatorSet.GetImageSize(ho_Segmentations_COPY_INP_TMP, out hv_ImageWidthRaw,
                    out hv_ImageHeightRaw);
                hv_EqualWidth.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_EqualWidth = hv_ImageWidth.TupleEqualElem(
                        hv_ImageWidthRaw);
                }
                hv_EqualHeight.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_EqualHeight = hv_ImageHeight.TupleEqualElem(
                        hv_ImageHeightRaw);
                }
                if ((int)((new HTuple(((hv_EqualWidth.TupleMin())).TupleEqual(0))).TupleOr(
                    new HTuple(((hv_EqualHeight.TupleMin())).TupleEqual(0)))) != 0)
                {
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ZoomImageSize(ho_Segmentations_COPY_INP_TMP, out ExpTmpOutVar_0,
                            hv_ImageWidth, hv_ImageHeight, "nearest_neighbor");
                        ho_Segmentations_COPY_INP_TMP.Dispose();
                        ho_Segmentations_COPY_INP_TMP = ExpTmpOutVar_0;
                    }
                }
                //
                //Check the type of the input images
                //and convert if necessary.
                hv_Type.Dispose();
                HOperatorSet.GetImageType(ho_Segmentations_COPY_INP_TMP, out hv_Type);
                hv_EqualReal.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_EqualReal = hv_Type.TupleEqualElem(
                        "real");
                }
                //
                if ((int)(new HTuple(((hv_EqualReal.TupleMin())).TupleEqual(0))) != 0)
                {
                    //Convert the image type to 'real',
                    //because the model expects 'real' images.
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConvertImageType(ho_Segmentations_COPY_INP_TMP, out ExpTmpOutVar_0,
                            "real");
                        ho_Segmentations_COPY_INP_TMP.Dispose();
                        ho_Segmentations_COPY_INP_TMP = ExpTmpOutVar_0;
                    }
                }
                //
                //Write preprocessed Segmentations to output variable.
                ho_SegmentationsPreprocessed.Dispose();
                ho_SegmentationsPreprocessed = new HObject(ho_Segmentations_COPY_INP_TMP);
                ho_Segmentations_COPY_INP_TMP.Dispose();
                ho_Domain.Dispose();
                ho_SelectedSeg.Dispose();
                ho_SelectedDomain.Dispose();

                hv_NumberImages.Dispose();
                hv_NumberSegmentations.Dispose();
                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_WidthSeg.Dispose();
                hv_HeightSeg.Dispose();
                hv_DLModelType.Dispose();
                hv_ImageWidth.Dispose();
                hv_ImageHeight.Dispose();
                hv_ImageNumChannels.Dispose();
                hv_ImageRangeMin.Dispose();
                hv_ImageRangeMax.Dispose();
                hv_DomainHandling.Dispose();
                hv_SetBackgroundID.Dispose();
                hv_ClassesToBackground.Dispose();
                hv_IgnoreClassIDs.Dispose();
                hv_IsInt.Dispose();
                hv_IndexImage.Dispose();
                hv_ImageWidthRaw.Dispose();
                hv_ImageHeightRaw.Dispose();
                hv_EqualWidth.Dispose();
                hv_EqualHeight.Dispose();
                hv_Type.Dispose();
                hv_EqualReal.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Segmentations_COPY_INP_TMP.Dispose();
                ho_Domain.Dispose();
                ho_SelectedSeg.Dispose();
                ho_SelectedDomain.Dispose();

                hv_NumberImages.Dispose();
                hv_NumberSegmentations.Dispose();
                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_WidthSeg.Dispose();
                hv_HeightSeg.Dispose();
                hv_DLModelType.Dispose();
                hv_ImageWidth.Dispose();
                hv_ImageHeight.Dispose();
                hv_ImageNumChannels.Dispose();
                hv_ImageRangeMin.Dispose();
                hv_ImageRangeMax.Dispose();
                hv_DomainHandling.Dispose();
                hv_SetBackgroundID.Dispose();
                hv_ClassesToBackground.Dispose();
                hv_IgnoreClassIDs.Dispose();
                hv_IsInt.Dispose();
                hv_IndexImage.Dispose();
                hv_ImageWidthRaw.Dispose();
                hv_ImageHeightRaw.Dispose();
                hv_EqualWidth.Dispose();
                hv_EqualHeight.Dispose();
                hv_Type.Dispose();
                hv_EqualReal.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Deep Learning / Model
        // Short Description: Preprocess given DLSamples according to the preprocessing parameters given in DLPreprocessParam. 
        public void preprocess_dl_samples(HTuple hv_DLSampleBatch, HTuple hv_DLPreprocessParam)
        {



            // Local iconic variables 

            HObject ho_ImageRaw = null, ho_ImagePreprocessed = null;
            HObject ho_SegmentationRaw = null, ho_SegmentationPreprocessed = null;

            // Local control variables 

            HTuple hv_SampleIndex = new HTuple(), hv_KeysExists = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageRaw);
            HOperatorSet.GenEmptyObj(out ho_ImagePreprocessed);
            HOperatorSet.GenEmptyObj(out ho_SegmentationRaw);
            HOperatorSet.GenEmptyObj(out ho_SegmentationPreprocessed);
            try
            {
                //
                //This procedure preprocesses all images of the sample dictionaries in the tuple DLSampleBatch.
                //The images are preprocessed according to the parameters provided in DLPreprocessParam.
                //
                //
                //Check the validity of the preprocessing parameters.
                check_dl_preprocess_param(hv_DLPreprocessParam);
                //
                //Preprocess the sample entries.
                //
                for (hv_SampleIndex = 0; (int)hv_SampleIndex <= (int)((new HTuple(hv_DLSampleBatch.TupleLength()
                    )) - 1); hv_SampleIndex = (int)hv_SampleIndex + 1)
                {
                    //Check the existence of the sample keys.
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_KeysExists.Dispose();
                        HOperatorSet.GetDictParam(hv_DLSampleBatch.TupleSelect(hv_SampleIndex), "key_exists",
                            ((new HTuple("image")).TupleConcat("bbox_label_id")).TupleConcat("segmentation_image"),
                            out hv_KeysExists);
                    }
                    //
                    //Preprocess the images.
                    if ((int)(hv_KeysExists.TupleSelect(0)) != 0)
                    {
                        //
                        //Get the image.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_ImageRaw.Dispose();
                            HOperatorSet.GetDictObject(out ho_ImageRaw, hv_DLSampleBatch.TupleSelect(
                                hv_SampleIndex), "image");
                        }
                        //
                        //Preprocess the image.
                        ho_ImagePreprocessed.Dispose();
                        preprocess_dl_model_images(ho_ImageRaw, out ho_ImagePreprocessed, hv_DLPreprocessParam);
                        //
                        //Replace the image in the dictionary.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            HOperatorSet.SetDictObject(ho_ImagePreprocessed, hv_DLSampleBatch.TupleSelect(
                                hv_SampleIndex), "image");
                        }
                        //
                        //If bounding boxes are given rescale them as well.
                        if ((int)(hv_KeysExists.TupleSelect(1)) != 0)
                        {
                            //
                            //Preprocess the bounding boxes of the sample.
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                preprocess_dl_model_bbox_rect1(ho_ImageRaw, hv_DLSampleBatch.TupleSelect(
                                    hv_SampleIndex), hv_DLPreprocessParam);
                            }
                        }
                        //
                        //Preprocess the segmentation image if present.
                        if ((int)(hv_KeysExists.TupleSelect(2)) != 0)
                        {
                            //
                            //Get the segmentation image.
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                ho_SegmentationRaw.Dispose();
                                HOperatorSet.GetDictObject(out ho_SegmentationRaw, hv_DLSampleBatch.TupleSelect(
                                    hv_SampleIndex), "segmentation_image");
                            }
                            //
                            //Preprocess the segmentation image.
                            ho_SegmentationPreprocessed.Dispose();
                            preprocess_dl_model_segmentations(ho_ImageRaw, ho_SegmentationRaw, out ho_SegmentationPreprocessed,
                                hv_DLPreprocessParam);
                            //
                            //Set preprocessed segmentation image.
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                HOperatorSet.SetDictObject(ho_SegmentationPreprocessed, hv_DLSampleBatch.TupleSelect(
                                    hv_SampleIndex), "segmentation_image");
                            }
                        }
                        //
                    }
                    else
                    {
                        throw new HalconException((new HTuple("All samples processed need to include an image, but the sample with index ") + hv_SampleIndex) + " does not.");
                    }
                }
                ho_ImageRaw.Dispose();
                ho_ImagePreprocessed.Dispose();
                ho_SegmentationRaw.Dispose();
                ho_SegmentationPreprocessed.Dispose();

                hv_SampleIndex.Dispose();
                hv_KeysExists.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ImageRaw.Dispose();
                ho_ImagePreprocessed.Dispose();
                ho_SegmentationRaw.Dispose();
                ho_SegmentationPreprocessed.Dispose();

                hv_SampleIndex.Dispose();
                hv_KeysExists.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Image / Manipulation
        // Short Description: Changes a value of ValuesToChange in Image to NewValue. 
        public void reassign_pixel_values(HObject ho_Image, out HObject ho_ImageOut, HTuple hv_ValuesToChange,
            HTuple hv_NewValue)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_RegionToChange, ho_RegionClass = null;

            // Local control variables 

            HTuple hv_IndexReset = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageOut);
            HOperatorSet.GenEmptyObj(out ho_RegionToChange);
            HOperatorSet.GenEmptyObj(out ho_RegionClass);
            try
            {
                //
                //This procedure sets all pixels of Image
                //with the values given in ValuesToChange to the given value NewValue.
                //
                ho_RegionToChange.Dispose();
                HOperatorSet.GenEmptyRegion(out ho_RegionToChange);
                for (hv_IndexReset = 0; (int)hv_IndexReset <= (int)((new HTuple(hv_ValuesToChange.TupleLength()
                    )) - 1); hv_IndexReset = (int)hv_IndexReset + 1)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_RegionClass.Dispose();
                        HOperatorSet.Threshold(ho_Image, out ho_RegionClass, hv_ValuesToChange.TupleSelect(
                            hv_IndexReset), hv_ValuesToChange.TupleSelect(hv_IndexReset));
                    }
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.Union2(ho_RegionToChange, ho_RegionClass, out ExpTmpOutVar_0
                            );
                        ho_RegionToChange.Dispose();
                        ho_RegionToChange = ExpTmpOutVar_0;
                    }
                }
                HOperatorSet.OverpaintRegion(ho_Image, ho_RegionToChange, hv_NewValue, "fill");
                ho_ImageOut.Dispose();
                ho_ImageOut = new HObject(ho_Image);
                ho_RegionToChange.Dispose();
                ho_RegionClass.Dispose();

                hv_IndexReset.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_RegionToChange.Dispose();
                ho_RegionClass.Dispose();

                hv_IndexReset.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: Graphics / Text
        // Short Description: Set font independent of OS 
        public void set_display_font(HTuple hv_WindowHandle, HTuple hv_Size, HTuple hv_Font,
            HTuple hv_Bold, HTuple hv_Slant)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_OS = new HTuple(), hv_Fonts = new HTuple();
            HTuple hv_Style = new HTuple(), hv_Exception = new HTuple();
            HTuple hv_AvailableFonts = new HTuple(), hv_Fdx = new HTuple();
            HTuple hv_Indices = new HTuple();
            HTuple hv_Font_COPY_INP_TMP = new HTuple(hv_Font);
            HTuple hv_Size_COPY_INP_TMP = new HTuple(hv_Size);

            // Initialize local and output iconic variables 
            try
            {
                //This procedure sets the text font of the current window with
                //the specified attributes.
                //
                //Input parameters:
                //WindowHandle: The graphics window for which the font will be set
                //Size: The font size. If Size=-1, the default of 16 is used.
                //Bold: If set to 'true', a bold font is used
                //Slant: If set to 'true', a slanted font is used
                //
                hv_OS.Dispose();
                HOperatorSet.GetSystem("operating_system", out hv_OS);
                if ((int)((new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                    new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(-1)))) != 0)
                {
                    hv_Size_COPY_INP_TMP.Dispose();
                    hv_Size_COPY_INP_TMP = 16;
                }
                if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
                {
                    //Restore previous behaviour
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Size = ((1.13677 * hv_Size_COPY_INP_TMP)).TupleInt()
                                ;
                            hv_Size_COPY_INP_TMP.Dispose();
                            hv_Size_COPY_INP_TMP = ExpTmpLocalVar_Size;
                        }
                    }
                }
                else
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Size = hv_Size_COPY_INP_TMP.TupleInt()
                                ;
                            hv_Size_COPY_INP_TMP.Dispose();
                            hv_Size_COPY_INP_TMP = ExpTmpLocalVar_Size;
                        }
                    }
                }
                if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("Courier"))) != 0)
                {
                    hv_Fonts.Dispose();
                    hv_Fonts = new HTuple();
                    hv_Fonts[0] = "Courier";
                    hv_Fonts[1] = "Courier 10 Pitch";
                    hv_Fonts[2] = "Courier New";
                    hv_Fonts[3] = "CourierNew";
                    hv_Fonts[4] = "Liberation Mono";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("mono"))) != 0)
                {
                    hv_Fonts.Dispose();
                    hv_Fonts = new HTuple();
                    hv_Fonts[0] = "Consolas";
                    hv_Fonts[1] = "Menlo";
                    hv_Fonts[2] = "Courier";
                    hv_Fonts[3] = "Courier 10 Pitch";
                    hv_Fonts[4] = "FreeMono";
                    hv_Fonts[5] = "Liberation Mono";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("sans"))) != 0)
                {
                    hv_Fonts.Dispose();
                    hv_Fonts = new HTuple();
                    hv_Fonts[0] = "Luxi Sans";
                    hv_Fonts[1] = "DejaVu Sans";
                    hv_Fonts[2] = "FreeSans";
                    hv_Fonts[3] = "Arial";
                    hv_Fonts[4] = "Liberation Sans";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("serif"))) != 0)
                {
                    hv_Fonts.Dispose();
                    hv_Fonts = new HTuple();
                    hv_Fonts[0] = "Times New Roman";
                    hv_Fonts[1] = "Luxi Serif";
                    hv_Fonts[2] = "DejaVu Serif";
                    hv_Fonts[3] = "FreeSerif";
                    hv_Fonts[4] = "Utopia";
                    hv_Fonts[5] = "Liberation Serif";
                }
                else
                {
                    hv_Fonts.Dispose();
                    hv_Fonts = new HTuple(hv_Font_COPY_INP_TMP);
                }
                hv_Style.Dispose();
                hv_Style = "";
                if ((int)(new HTuple(hv_Bold.TupleEqual("true"))) != 0)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Style = hv_Style + "Bold";
                            hv_Style.Dispose();
                            hv_Style = ExpTmpLocalVar_Style;
                        }
                    }
                }
                else if ((int)(new HTuple(hv_Bold.TupleNotEqual("false"))) != 0)
                {
                    hv_Exception.Dispose();
                    hv_Exception = "Wrong value of control parameter Bold";
                    throw new HalconException(hv_Exception);
                }
                if ((int)(new HTuple(hv_Slant.TupleEqual("true"))) != 0)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Style = hv_Style + "Italic";
                            hv_Style.Dispose();
                            hv_Style = ExpTmpLocalVar_Style;
                        }
                    }
                }
                else if ((int)(new HTuple(hv_Slant.TupleNotEqual("false"))) != 0)
                {
                    hv_Exception.Dispose();
                    hv_Exception = "Wrong value of control parameter Slant";
                    throw new HalconException(hv_Exception);
                }
                if ((int)(new HTuple(hv_Style.TupleEqual(""))) != 0)
                {
                    hv_Style.Dispose();
                    hv_Style = "Normal";
                }
                hv_AvailableFonts.Dispose();
                HOperatorSet.QueryFont(hv_WindowHandle, out hv_AvailableFonts);
                hv_Font_COPY_INP_TMP.Dispose();
                hv_Font_COPY_INP_TMP = "";
                for (hv_Fdx = 0; (int)hv_Fdx <= (int)((new HTuple(hv_Fonts.TupleLength())) - 1); hv_Fdx = (int)hv_Fdx + 1)
                {
                    hv_Indices.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Indices = hv_AvailableFonts.TupleFind(
                            hv_Fonts.TupleSelect(hv_Fdx));
                    }
                    if ((int)(new HTuple((new HTuple(hv_Indices.TupleLength())).TupleGreater(
                        0))) != 0)
                    {
                        if ((int)(new HTuple(((hv_Indices.TupleSelect(0))).TupleGreaterEqual(0))) != 0)
                        {
                            hv_Font_COPY_INP_TMP.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_Font_COPY_INP_TMP = hv_Fonts.TupleSelect(
                                    hv_Fdx);
                            }
                            break;
                        }
                    }
                }
                if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual(""))) != 0)
                {
                    throw new HalconException("Wrong value of control parameter Font");
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_Font = (((hv_Font_COPY_INP_TMP + "-") + hv_Style) + "-") + hv_Size_COPY_INP_TMP;
                        hv_Font_COPY_INP_TMP.Dispose();
                        hv_Font_COPY_INP_TMP = ExpTmpLocalVar_Font;
                    }
                }
                HOperatorSet.SetFont(hv_WindowHandle, hv_Font_COPY_INP_TMP);

                hv_Font_COPY_INP_TMP.Dispose();
                hv_Size_COPY_INP_TMP.Dispose();
                hv_OS.Dispose();
                hv_Fonts.Dispose();
                hv_Style.Dispose();
                hv_Exception.Dispose();
                hv_AvailableFonts.Dispose();
                hv_Fdx.Dispose();
                hv_Indices.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_Font_COPY_INP_TMP.Dispose();
                hv_Size_COPY_INP_TMP.Dispose();
                hv_OS.Dispose();
                hv_Fonts.Dispose();
                hv_Style.Dispose();
                hv_Exception.Dispose();
                hv_AvailableFonts.Dispose();
                hv_Fdx.Dispose();
                hv_Indices.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Local procedures 
        public void check_model_availability(HTuple hv_ExampleDataDir, HTuple hv_PreprocessParamFileName,
            HTuple hv_TrainedModelFileName, HTuple hv_UsePretrainedModel)
        {



            // Local control variables 

            HTuple hv_FileExists = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                if ((int)(hv_UsePretrainedModel.TupleNot()) != 0)
                {
                    hv_FileExists.Dispose();
                    HOperatorSet.FileExists(hv_ExampleDataDir, out hv_FileExists);
                    if ((int)(hv_FileExists.TupleNot()) != 0)
                    {
                        throw new HalconException(hv_ExampleDataDir + " does not exist. Please run part 1 and 2 of example series.");
                    }

                    hv_FileExists.Dispose();
                    HOperatorSet.FileExists(hv_PreprocessParamFileName, out hv_FileExists);
                    if ((int)(hv_FileExists.TupleNot()) != 0)
                    {
                        throw new HalconException(hv_PreprocessParamFileName + " does not exist. Please run part 1 of example series.");
                    }

                    hv_FileExists.Dispose();
                    HOperatorSet.FileExists(hv_TrainedModelFileName, out hv_FileExists);
                    if ((int)(hv_FileExists.TupleNot()) != 0)
                    {
                        throw new HalconException(hv_TrainedModelFileName + " does not exist. Please run part 2 of example series.");
                    }
                }
                else
                {
                    hv_FileExists.Dispose();
                    HOperatorSet.FileExists(hv_PreprocessParamFileName, out hv_FileExists);
                    if ((int)(hv_FileExists.TupleNot()) != 0)
                    {
                        throw new HalconException(hv_PreprocessParamFileName + " does not exist. Please run the HALCON Deep Learning installer.");
                    }

                    hv_FileExists.Dispose();
                    HOperatorSet.FileExists(hv_TrainedModelFileName, out hv_FileExists);
                    if ((int)(hv_FileExists.TupleNot()) != 0)
                    {
                        throw new HalconException(hv_TrainedModelFileName + " does not exist. Please run the HALCON Deep Learning installer.");
                    }
                }


                hv_FileExists.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_FileExists.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_close_example_image_window(HTuple hv_ExampleInternals)
        {



            // Local control variables 

            HTuple hv_WindowHandleImages = new HTuple();
            HTuple hv_Exception = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure closes the image window.

                try
                {
                    hv_WindowHandleImages.Dispose();
                    HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images", out hv_WindowHandleImages);
                    HDevWindowStack.SetActive(hv_WindowHandleImages);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.CloseWindow(HDevWindowStack.Pop());
                    }
                    //Delete key.
                    HOperatorSet.RemoveDictKey(hv_ExampleInternals, "window_images");
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                }


                hv_WindowHandleImages.Dispose();
                hv_Exception.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_WindowHandleImages.Dispose();
                hv_Exception.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_close_example_legend_window(HTuple hv_ExampleInternals)
        {



            // Local control variables 

            HTuple hv_WindowHandleLegend = new HTuple();
            HTuple hv_Exception = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure closes the legend window.

                try
                {
                    hv_WindowHandleLegend.Dispose();
                    HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_legend", out hv_WindowHandleLegend);
                    HDevWindowStack.SetActive(hv_WindowHandleLegend);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.CloseWindow(HDevWindowStack.Pop());
                    }
                    //Delete key.
                    HOperatorSet.RemoveDictKey(hv_ExampleInternals, "window_legend");
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                }


                hv_WindowHandleLegend.Dispose();
                hv_Exception.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_WindowHandleLegend.Dispose();
                hv_Exception.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_close_example_text_window(HTuple hv_ExampleInternals)
        {



            // Local control variables 

            HTuple hv_WindowHandleImages = new HTuple();
            HTuple hv_Exception = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure closes the text window.

                try
                {
                    hv_WindowHandleImages.Dispose();
                    HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_text", out hv_WindowHandleImages);
                    HDevWindowStack.SetActive(hv_WindowHandleImages);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.CloseWindow(HDevWindowStack.Pop());
                    }
                    //Delete key.
                    HOperatorSet.RemoveDictKey(hv_ExampleInternals, "window_text");
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                }

                hv_WindowHandleImages.Dispose();
                hv_Exception.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_WindowHandleImages.Dispose();
                hv_Exception.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_close_example_windows(HTuple hv_ExampleInternals)
        {



            // Local control variables 

            HTuple hv_ShowExampleScreens = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure closes all example windows.

                hv_ShowExampleScreens.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "show_example_screens", out hv_ShowExampleScreens);
                if ((int)(hv_ShowExampleScreens.TupleNot()) != 0)
                {

                    hv_ShowExampleScreens.Dispose();

                    return;
                }

                dev_close_example_text_window(hv_ExampleInternals);
                dev_close_example_image_window(hv_ShowExampleScreens);
                dev_close_example_legend_window(hv_ShowExampleScreens);


                hv_ShowExampleScreens.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_ShowExampleScreens.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_example_reset_windows(HTuple hv_ExampleInternals)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_WindowHandlesToClose = new HTuple();
            HTuple hv_Exception = new HTuple(), hv_I = new HTuple();
            HTuple hv_WindowHandleKeys = new HTuple(), hv_Index = new HTuple();
            HTuple hv_WindowImagesNeeded = new HTuple(), hv_WindowHandleImages = new HTuple();
            HTuple hv_WindowLegendNeeded = new HTuple(), hv_WindowHandleLegend = new HTuple();
            HTuple hv_WindowHandleText = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure resets the graphics windows.

                //Close any windows that are listed in key 'window_handles_to_close'.
                try
                {
                    hv_WindowHandlesToClose.Dispose();
                    HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_handles_to_close",
                        out hv_WindowHandlesToClose);
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    hv_WindowHandlesToClose.Dispose();
                    hv_WindowHandlesToClose = new HTuple();
                }
                for (hv_I = 0; (int)hv_I <= (int)((new HTuple(hv_WindowHandlesToClose.TupleLength()
                    )) - 1); hv_I = (int)hv_I + 1)
                {
                    HDevWindowStack.SetActive(hv_WindowHandlesToClose.TupleSelect(
                        hv_I));
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.CloseWindow(HDevWindowStack.Pop());
                    }
                }
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_handles_to_close", new HTuple());

                //Open image window if needed.
                hv_WindowHandleKeys.Dispose();
                HOperatorSet.GetDictParam(hv_ExampleInternals, "keys", new HTuple(), out hv_WindowHandleKeys);
                hv_Index.Dispose();
                HOperatorSet.TupleFind(hv_WindowHandleKeys, "window_images", out hv_Index);
                hv_WindowImagesNeeded.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images_needed", out hv_WindowImagesNeeded);
                if ((int)(hv_WindowImagesNeeded.TupleAnd(new HTuple(hv_Index.TupleEqual(-1)))) != 0)
                {
                    //Open new window for images.
                    dev_open_example_image_window(hv_ExampleInternals);
                }
                else if ((int)((new HTuple(hv_WindowImagesNeeded.TupleNot())).TupleAnd(
                    new HTuple(hv_Index.TupleNotEqual(-1)))) != 0)
                {
                    //Window for images exists but is not needed -> close it.
                    hv_WindowHandleImages.Dispose();
                    HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images", out hv_WindowHandleImages);
                    HDevWindowStack.SetActive(hv_WindowHandleImages);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.CloseWindow(HDevWindowStack.Pop());
                    }
                    //Delete key.
                    HOperatorSet.RemoveDictKey(hv_ExampleInternals, "window_images");
                }

                //Open legend window if needed
                hv_WindowHandleKeys.Dispose();
                HOperatorSet.GetDictParam(hv_ExampleInternals, "keys", new HTuple(), out hv_WindowHandleKeys);
                hv_Index.Dispose();
                HOperatorSet.TupleFind(hv_WindowHandleKeys, "window_legend", out hv_Index);
                hv_WindowLegendNeeded.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_legend_needed", out hv_WindowLegendNeeded);
                if ((int)(hv_WindowLegendNeeded.TupleAnd(new HTuple(hv_Index.TupleEqual(-1)))) != 0)
                {
                    //Open new window for legend
                    dev_open_example_legend_window(hv_ExampleInternals, 280);
                }
                else if ((int)((new HTuple(hv_WindowLegendNeeded.TupleNot())).TupleAnd(
                    new HTuple(hv_Index.TupleNotEqual(-1)))) != 0)
                {
                    //Window for legend exists but is not needed -> close it
                    dev_close_example_legend_window(hv_ExampleInternals);
                }


                //Set the correct area (part) of the image window.
                try
                {
                    hv_WindowHandleImages.Dispose();
                    HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images", out hv_WindowHandleImages);
                    HDevWindowStack.SetActive(hv_WindowHandleImages);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.ClearWindow(HDevWindowStack.GetActive());
                    }
                    //Set default window extents
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetWindowExtents(HDevWindowStack.GetActive(), 360, 0, 500,
                            500);
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetPart(HDevWindowStack.GetActive(), 1, 1, -1, -1);
                    }
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                }
                //Set the correct area (part) of the legend window.
                try
                {
                    hv_WindowHandleLegend.Dispose();
                    HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_legend", out hv_WindowHandleLegend);
                    HDevWindowStack.SetActive(hv_WindowHandleLegend);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.ClearWindow(HDevWindowStack.GetActive());
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetPart(HDevWindowStack.GetActive(), 1, 1, -1, -1);
                    }
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                }

                hv_WindowHandleText.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_text", out hv_WindowHandleText);
                HDevWindowStack.SetActive(hv_WindowHandleText);
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.ClearWindow(HDevWindowStack.GetActive());
                }

                hv_WindowHandlesToClose.Dispose();
                hv_Exception.Dispose();
                hv_I.Dispose();
                hv_WindowHandleKeys.Dispose();
                hv_Index.Dispose();
                hv_WindowImagesNeeded.Dispose();
                hv_WindowHandleImages.Dispose();
                hv_WindowLegendNeeded.Dispose();
                hv_WindowHandleLegend.Dispose();
                hv_WindowHandleText.Dispose();

                return;

            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_WindowHandlesToClose.Dispose();
                hv_Exception.Dispose();
                hv_I.Dispose();
                hv_WindowHandleKeys.Dispose();
                hv_Index.Dispose();
                hv_WindowImagesNeeded.Dispose();
                hv_WindowHandleImages.Dispose();
                hv_WindowLegendNeeded.Dispose();
                hv_WindowHandleLegend.Dispose();
                hv_WindowHandleText.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_model_output_image(HTuple hv_ExampleInternals)
        {



            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_Image, ho_SegImage;

            // Local control variables 

            HTuple hv_PreprocessParamFileName = new HTuple();
            HTuple hv_DLPreprocessParam = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_SegImage);
            try
            {
                //This procedure visualizes the output of a segmentation model,
                //by displaying a segmentation image.

                //Read the preprocessed image.
                ho_Image.Dispose();
                HOperatorSet.ReadImage(out ho_Image, "pill/ginseng/contamination/pill_ginseng_contamination_007.png");
                ho_SegImage.Dispose();
                HOperatorSet.ReadImage(out ho_SegImage, "labels/pill/ginseng/contamination/pill_ginseng_contamination_007_gt.png");
                //Preprocess the segmentation image.
                hv_PreprocessParamFileName.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "preprocess_param_file_name",
                    out hv_PreprocessParamFileName);
                hv_DLPreprocessParam.Dispose();
                HOperatorSet.ReadDict(hv_PreprocessParamFileName, new HTuple(), new HTuple(),
                    out hv_DLPreprocessParam);
                {
                    HObject ExpTmpOutVar_0;
                    preprocess_dl_model_segmentations(ho_Image, ho_SegImage, out ExpTmpOutVar_0,
                        hv_DLPreprocessParam);
                    ho_SegImage.Dispose();
                    ho_SegImage = ExpTmpOutVar_0;
                }

                //Display segmentation image.
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispObj(ho_SegImage, HDevWindowStack.GetActive());
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Segmentation image \n(output of 'apply_dl_model')",
                        "window", "top", "left", "black", new HTuple(), new HTuple());
                }

                //Write segmentation image to file.
                HOperatorSet.SetDictObject(ho_SegImage, hv_ExampleInternals, "segmentation_image");

                ho_Image.Dispose();
                ho_SegImage.Dispose();

                hv_PreprocessParamFileName.Dispose();
                hv_DLPreprocessParam.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Image.Dispose();
                ho_SegImage.Dispose();

                hv_PreprocessParamFileName.Dispose();
                hv_DLPreprocessParam.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_ok_nok(HTuple hv_Areas, HTuple hv_WindowHandleImage)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_Text = new HTuple(), hv_BoxColor = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedures displays OK if no defects are segmented and NOK otherwise.

                //The first entry of Area corresponds to class 'good'.
                if ((int)(new HTuple((((hv_Areas.TupleSum()) - (hv_Areas.TupleSelect(0)))).TupleGreater(
                    0))) != 0)
                {
                    hv_Text.Dispose();
                    hv_Text = "NOK";
                    hv_BoxColor.Dispose();
                    hv_BoxColor = "red";
                }
                else
                {
                    hv_Text.Dispose();
                    hv_Text = "OK";
                    hv_BoxColor.Dispose();
                    hv_BoxColor = "green";
                }
                set_display_font(hv_WindowHandleImage, 24, "mono", "true", "false");
                if (HDevWindowStack.IsOpen())
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window", "top",
                            "left", "black", (new HTuple("box_color")).TupleConcat("shadow"), hv_BoxColor.TupleConcat(
                            "false"));
                    }
                }
                set_display_font(hv_WindowHandleImage, 16, "mono", "true", "false");
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Press Run (F5) to continue",
                        "window", "bottom", "left", "black", "box", "true");
                }
                HOperatorSet.FlushBuffer(hv_WindowHandleImage);

                hv_Text.Dispose();
                hv_BoxColor.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_Text.Dispose();
                hv_BoxColor.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_pills_example_dataset_preview()
        {


            // Local iconic variables 

            HObject ho_Image = null, ho_TiledImage = null;
            HObject ho_ImageR = null, ho_ImageG = null, ho_ImageB = null;
            HObject ho_ImageRG = null, ho_ImageRGB = null;

            // Local control variables 

            HTuple hv_GinsengPath = new HTuple(), hv_MagnesiumPath = new HTuple();
            HTuple hv_MintPath = new HTuple(), hv_Width = new HTuple();
            HTuple hv_Height = new HTuple(), hv_Width1 = new HTuple();
            HTuple hv_Height1 = new HTuple(), hv_Exception = new HTuple();
            HTuple hv_ErrorAndAdviceText = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_TiledImage);
            HOperatorSet.GenEmptyObj(out ho_ImageR);
            HOperatorSet.GenEmptyObj(out ho_ImageG);
            HOperatorSet.GenEmptyObj(out ho_ImageB);
            HOperatorSet.GenEmptyObj(out ho_ImageRG);
            HOperatorSet.GenEmptyObj(out ho_ImageRGB);
            try
            {
                //This procedure displays a selection of pill images.

                try
                {
                    //Read some example images.
                    hv_GinsengPath.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_GinsengPath = new HTuple("pill/ginseng/") + (
                            (new HTuple("good/pill_ginseng_good_001")).TupleConcat("contamination/pill_ginseng_contamination_004")).TupleConcat(
                            "crack/pill_ginseng_crack_001");
                    }
                    hv_MagnesiumPath.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_MagnesiumPath = new HTuple("pill/magnesium/") + (
                            (new HTuple("good/pill_magnesium_good_001")).TupleConcat("contamination/pill_magnesium_contamination_001")).TupleConcat(
                            "crack/pill_magnesium_crack_001");
                    }
                    hv_MintPath.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_MintPath = new HTuple("pill/mint/") + (
                            (new HTuple("good/pill_mint_good_001")).TupleConcat("contamination/pill_mint_contamination_001")).TupleConcat(
                            "crack/pill_mint_crack_009");
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        ho_Image.Dispose();
                        HOperatorSet.ReadImage(out ho_Image, ((hv_GinsengPath.TupleConcat(hv_MagnesiumPath))).TupleConcat(
                            hv_MintPath));
                    }
                    ho_TiledImage.Dispose();
                    HOperatorSet.TileImages(ho_Image, out ho_TiledImage, 3, "horizontal");
                    //Generate background image.
                    hv_Width.Dispose(); hv_Height.Dispose();
                    HOperatorSet.GetImageSize(ho_TiledImage, out hv_Width, out hv_Height);
                    ho_ImageR.Dispose();
                    HOperatorSet.GenImageProto(ho_TiledImage, out ho_ImageR, 18);
                    ho_ImageG.Dispose();
                    HOperatorSet.GenImageProto(ho_TiledImage, out ho_ImageG, 22);
                    ho_ImageB.Dispose();
                    HOperatorSet.GenImageProto(ho_TiledImage, out ho_ImageB, 28);
                    ho_ImageRG.Dispose();
                    HOperatorSet.AppendChannel(ho_ImageR, ho_ImageG, out ho_ImageRG);
                    ho_ImageRGB.Dispose();
                    HOperatorSet.AppendChannel(ho_ImageRG, ho_ImageB, out ho_ImageRGB);
                    //Display the background and the images.
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetWindowExtents(HDevWindowStack.GetActive(), 360, 0, 800,
                            400);
                    }
                    hv_Width1.Dispose(); hv_Height1.Dispose();
                    HOperatorSet.GetImageSize(ho_ImageRGB, out hv_Width1, out hv_Height1);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetPart(HDevWindowStack.GetActive(), 0, 0, hv_Height1, hv_Width1);
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispObj(ho_ImageRGB, HDevWindowStack.GetActive());
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispObj(ho_TiledImage, HDevWindowStack.GetActive());
                    }
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    //If the example image files are not found, an error message is displayed.
                    hv_ErrorAndAdviceText.Dispose();
                    hv_ErrorAndAdviceText = "The images required for this example could not be found.";
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_ErrorAndAdviceText = hv_ErrorAndAdviceText.TupleConcat(
                                "");
                            hv_ErrorAndAdviceText.Dispose();
                            hv_ErrorAndAdviceText = ExpTmpLocalVar_ErrorAndAdviceText;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_ErrorAndAdviceText = hv_ErrorAndAdviceText.TupleConcat(
                                "These images are part of a separate installer. Please");
                            hv_ErrorAndAdviceText.Dispose();
                            hv_ErrorAndAdviceText = ExpTmpLocalVar_ErrorAndAdviceText;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_ErrorAndAdviceText = hv_ErrorAndAdviceText.TupleConcat(
                                "refer to the Installation Guide for more information on");
                            hv_ErrorAndAdviceText.Dispose();
                            hv_ErrorAndAdviceText = ExpTmpLocalVar_ErrorAndAdviceText;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_ErrorAndAdviceText = hv_ErrorAndAdviceText.TupleConcat(
                                "this topic!");
                            hv_ErrorAndAdviceText.Dispose();
                            hv_ErrorAndAdviceText = ExpTmpLocalVar_ErrorAndAdviceText;
                        }
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.ClearWindow(HDevWindowStack.GetActive());
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_ErrorAndAdviceText,
                            "window", "center", "left", "red", new HTuple(), new HTuple());
                    }
                }
                ho_Image.Dispose();
                ho_TiledImage.Dispose();
                ho_ImageR.Dispose();
                ho_ImageG.Dispose();
                ho_ImageB.Dispose();
                ho_ImageRG.Dispose();
                ho_ImageRGB.Dispose();

                hv_GinsengPath.Dispose();
                hv_MagnesiumPath.Dispose();
                hv_MintPath.Dispose();
                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_Width1.Dispose();
                hv_Height1.Dispose();
                hv_Exception.Dispose();
                hv_ErrorAndAdviceText.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Image.Dispose();
                ho_TiledImage.Dispose();
                ho_ImageR.Dispose();
                ho_ImageG.Dispose();
                ho_ImageB.Dispose();
                ho_ImageRG.Dispose();
                ho_ImageRGB.Dispose();

                hv_GinsengPath.Dispose();
                hv_MagnesiumPath.Dispose();
                hv_MintPath.Dispose();
                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_Width1.Dispose();
                hv_Height1.Dispose();
                hv_Exception.Dispose();
                hv_ErrorAndAdviceText.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_preprocessed_image(HTuple hv_ExampleInternals)
        {



            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_Image;

            // Local control variables 

            HTuple hv_PreprocessParamFileName = new HTuple();
            HTuple hv_DLPreprocessParam = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            try
            {
                //This procedure displays an example of a preprocessed image.

                //Read image.
                ho_Image.Dispose();
                HOperatorSet.ReadImage(out ho_Image, "pill/ginseng/contamination/pill_ginseng_contamination_007.png");

                //Preprocess image.
                hv_PreprocessParamFileName.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "preprocess_param_file_name",
                    out hv_PreprocessParamFileName);
                hv_DLPreprocessParam.Dispose();
                HOperatorSet.ReadDict(hv_PreprocessParamFileName, new HTuple(), new HTuple(),
                    out hv_DLPreprocessParam);

                {
                    HObject ExpTmpOutVar_0;
                    preprocess_dl_model_images(ho_Image, out ExpTmpOutVar_0, hv_DLPreprocessParam);
                    ho_Image.Dispose();
                    ho_Image = ExpTmpOutVar_0;
                }
                HOperatorSet.SetDictObject(ho_Image, hv_ExampleInternals, "preprocessed_image");

                //Display preprocessed image.
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetPart(HDevWindowStack.GetActive(), 0, 0, 400, 400);
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Preprocessed image",
                        "window", "top", "left", "black", new HTuple(), new HTuple());
                }
                ho_Image.Dispose();

                hv_PreprocessParamFileName.Dispose();
                hv_DLPreprocessParam.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Image.Dispose();

                hv_PreprocessParamFileName.Dispose();
                hv_DLPreprocessParam.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_raw_image(HTuple hv_WindowHandleImages)
        {



            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_Image;

            // Local control variables 

            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_Width1 = new HTuple(), hv_Height1 = new HTuple();
            HTuple hv_ZoomFactor = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            try
            {
                //This procedure displays a raw image as inserted into the sample.

                HDevWindowStack.SetActive(hv_WindowHandleImages);
                //Read image and fit the window handle.
                ho_Image.Dispose();
                HOperatorSet.ReadImage(out ho_Image, "pill/ginseng/contamination/pill_ginseng_contamination_007.png");

                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                hv_Row.Dispose(); hv_Column.Dispose(); hv_Width1.Dispose(); hv_Height1.Dispose();
                HOperatorSet.GetWindowExtents(hv_WindowHandleImages, out hv_Row, out hv_Column,
                    out hv_Width1, out hv_Height1);

                if ((int)(new HTuple(hv_Height.TupleLess(hv_Width))) != 0)
                {
                    hv_ZoomFactor.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ZoomFactor = hv_Width1 / (hv_Width.TupleReal()
                            );
                    }
                }
                else
                {
                    hv_ZoomFactor.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ZoomFactor = hv_Height1 / (hv_Height.TupleReal()
                            );
                    }
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ZoomImageFactor(ho_Image, out ExpTmpOutVar_0, hv_ZoomFactor, hv_ZoomFactor,
                        "bilinear");
                    ho_Image.Dispose();
                    ho_Image = ExpTmpOutVar_0;
                }

                if (HDevWindowStack.IsOpen())
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.SetWindowExtents(HDevWindowStack.GetActive(), 360, 0, hv_ZoomFactor * hv_Width,
                            hv_ZoomFactor * hv_Height);
                    }
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetPart(HDevWindowStack.GetActive(), 1, 1, -1, -1);
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
                }

                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Raw image", "window",
                        "top", "left", "black", new HTuple(), new HTuple());
                }
                ho_Image.Dispose();

                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_Width1.Dispose();
                hv_Height1.Dispose();
                hv_ZoomFactor.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Image.Dispose();

                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_Width1.Dispose();
                hv_Height1.Dispose();
                hv_ZoomFactor.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_screen_example_images(HTuple hv_ExampleInternals, HTuple hv_UsePretrainedModel)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_ShowExampleScreens = new HTuple();
            HTuple hv_WindowHandleText = new HTuple(), hv_ExampleDataDir = new HTuple();
            HTuple hv_ExampleDataDirExists = new HTuple(), hv_PreprocessParamFileName = new HTuple();
            HTuple hv_PreprocessParamsExist = new HTuple(), hv_RetrainedModelFileName = new HTuple();
            HTuple hv_ModelExists = new HTuple(), hv_ExceptionText = new HTuple();
            HTuple hv_NumMissing = new HTuple(), hv_Text = new HTuple();
            HTuple hv_WindowHandleImages = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure introduces the task and displays some example images.

                hv_ShowExampleScreens.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "show_example_screens", out hv_ShowExampleScreens);
                if ((int)(hv_ShowExampleScreens.TupleNot()) != 0)
                {

                    hv_ShowExampleScreens.Dispose();
                    hv_WindowHandleText.Dispose();
                    hv_ExampleDataDir.Dispose();
                    hv_ExampleDataDirExists.Dispose();
                    hv_PreprocessParamFileName.Dispose();
                    hv_PreprocessParamsExist.Dispose();
                    hv_RetrainedModelFileName.Dispose();
                    hv_ModelExists.Dispose();
                    hv_ExceptionText.Dispose();
                    hv_NumMissing.Dispose();
                    hv_Text.Dispose();
                    hv_WindowHandleImages.Dispose();

                    return;
                }

                //Reset the open windows for a clean display.
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images_needed", 0);
                dev_display_example_reset_windows(hv_ExampleInternals);

                //Set text window handle.
                hv_WindowHandleText.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_text", out hv_WindowHandleText);
                HDevWindowStack.SetActive(hv_WindowHandleText);

                //Check if the trained model and preprocessing parameters exist.

                //Example data directory where the outputs of previous examples are saved.
                hv_ExampleDataDir.Dispose();
                hv_ExampleDataDir = "segment_pill_defects_data";
                hv_ExampleDataDirExists.Dispose();
                HOperatorSet.FileExists(hv_ExampleDataDir, out hv_ExampleDataDirExists);

                //Preprocessing parameters.
                if ((int)(hv_UsePretrainedModel) != 0)
                {
                    hv_PreprocessParamFileName.Dispose();
                    hv_PreprocessParamFileName = "segment_pill_defects_preprocess_param.hdict";
                }
                else
                {
                    hv_PreprocessParamFileName.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_PreprocessParamFileName = hv_ExampleDataDir + "/dl_preprocess_param_400x400.hdict";
                    }
                }
                hv_PreprocessParamsExist.Dispose();
                HOperatorSet.FileExists(hv_PreprocessParamFileName, out hv_PreprocessParamsExist);

                //Retrained model file name.
                if ((int)(hv_UsePretrainedModel) != 0)
                {
                    hv_RetrainedModelFileName.Dispose();
                    hv_RetrainedModelFileName = "segment_pill_defects.hdl";
                }
                else
                {
                    hv_RetrainedModelFileName.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_RetrainedModelFileName = hv_ExampleDataDir + "/best_dl_model_segmentation.hdl";
                    }
                }
                hv_ModelExists.Dispose();
                HOperatorSet.FileExists(hv_RetrainedModelFileName, out hv_ModelExists);
                if ((int)(hv_UsePretrainedModel) != 0)
                {
                    if ((int)((new HTuple(hv_ModelExists.TupleNot())).TupleOr(hv_PreprocessParamsExist.TupleNot()
                        )) != 0)
                    {
                        hv_WindowHandleText.Dispose();
                        HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_text", out hv_WindowHandleText);
                        HDevWindowStack.SetActive(hv_WindowHandleText);

                        hv_ExceptionText.Dispose();
                        hv_ExceptionText = "To run this example you need the output of:";
                        if (hv_ExceptionText == null)
                            hv_ExceptionText = new HTuple();
                        hv_ExceptionText[new HTuple(hv_ExceptionText.TupleLength())] = new HTuple(" - Deep learning installer, see Installation Guide.");
                        if (hv_ExceptionText == null)
                            hv_ExceptionText = new HTuple();
                        hv_ExceptionText[new HTuple(hv_ExceptionText.TupleLength())] = "";
                        //Display warning.
                        set_display_font(hv_WindowHandleText, 20, "mono", "true", "false");
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_ExceptionText,
                                "window", "top", "left", "red", "box", "true");
                        }

                        hv_ShowExampleScreens.Dispose();
                        hv_WindowHandleText.Dispose();
                        hv_ExampleDataDir.Dispose();
                        hv_ExampleDataDirExists.Dispose();
                        hv_PreprocessParamFileName.Dispose();
                        hv_PreprocessParamsExist.Dispose();
                        hv_RetrainedModelFileName.Dispose();
                        hv_ModelExists.Dispose();
                        hv_ExceptionText.Dispose();
                        hv_NumMissing.Dispose();
                        hv_Text.Dispose();
                        hv_WindowHandleImages.Dispose();

                        return;
                    }
                }
                else
                {
                    if ((int)((new HTuple((new HTuple(hv_ExampleDataDirExists.TupleNot())).TupleOr(
                        hv_ModelExists.TupleNot()))).TupleOr(hv_PreprocessParamsExist.TupleNot()
                        )) != 0)
                    {
                        hv_ExceptionText.Dispose();
                        hv_ExceptionText = "To run this example you need the output of:";
                        hv_NumMissing.Dispose();
                        hv_NumMissing = 0;
                        if ((int)((new HTuple(hv_PreprocessParamsExist.TupleNot())).TupleOr(hv_ExampleDataDirExists.TupleNot()
                            )) != 0)
                        {
                            if (hv_ExceptionText == null)
                                hv_ExceptionText = new HTuple();
                            hv_ExceptionText[new HTuple(hv_ExceptionText.TupleLength())] = " - 'segmentation_pill_defects_deep_learning_1_preprocess.hdev'";
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_NumMissing = hv_NumMissing + 1;
                                    hv_NumMissing.Dispose();
                                    hv_NumMissing = ExpTmpLocalVar_NumMissing;
                                }
                            }
                        }
                        if ((int)(hv_ModelExists.TupleNot()) != 0)
                        {
                            if (hv_ExceptionText == null)
                                hv_ExceptionText = new HTuple();
                            hv_ExceptionText[new HTuple(hv_ExceptionText.TupleLength())] = " - 'segmentation_pill_defects_deep_learning_2_train.hdev'";
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                {
                                    HTuple
                                      ExpTmpLocalVar_NumMissing = hv_NumMissing + 1;
                                    hv_NumMissing.Dispose();
                                    hv_NumMissing = ExpTmpLocalVar_NumMissing;
                                }
                            }
                        }
                        if (hv_ExceptionText == null)
                            hv_ExceptionText = new HTuple();
                        hv_ExceptionText[new HTuple(hv_ExceptionText.TupleLength())] = "";
                        if ((int)(new HTuple(hv_NumMissing.TupleEqual(1))) != 0)
                        {
                            if (hv_ExceptionText == null)
                                hv_ExceptionText = new HTuple();
                            hv_ExceptionText[new HTuple(hv_ExceptionText.TupleLength())] = "Please run this example first.";
                        }
                        else if ((int)(new HTuple(hv_NumMissing.TupleGreater(1))) != 0)
                        {
                            if (hv_ExceptionText == null)
                                hv_ExceptionText = new HTuple();
                            hv_ExceptionText[new HTuple(hv_ExceptionText.TupleLength())] = "Please run these examples first.";
                        }
                        if (hv_ExceptionText == null)
                            hv_ExceptionText = new HTuple();
                        hv_ExceptionText[HTuple.TupleGenSequence(new HTuple(hv_ExceptionText.TupleLength()
                            ), (new HTuple(hv_ExceptionText.TupleLength())) + 2, 1)] = ((new HTuple("Alternatively, you can set 'UsePretrainedModel := true' ")).TupleConcat(
                            "at the top of the example script to use an already trained")).TupleConcat(
                            "model shipped with the HALCON installation.");

                        //Display the warning.
                        set_display_font(hv_WindowHandleText, 20, "mono", "true", "false");
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_ExceptionText,
                                "window", "top", "left", "red", "box", "true");
                        }
                        set_display_font(hv_WindowHandleText, 16, "mono", "true", "false");

                        hv_ShowExampleScreens.Dispose();
                        hv_WindowHandleText.Dispose();
                        hv_ExampleDataDir.Dispose();
                        hv_ExampleDataDirExists.Dispose();
                        hv_PreprocessParamFileName.Dispose();
                        hv_PreprocessParamsExist.Dispose();
                        hv_RetrainedModelFileName.Dispose();
                        hv_ModelExists.Dispose();
                        hv_ExceptionText.Dispose();
                        hv_NumMissing.Dispose();
                        hv_Text.Dispose();
                        hv_WindowHandleImages.Dispose();

                        return;
                    }
                }

                //Set preprocess param file to dict to be read later.
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "preprocess_param_file_name",
                    hv_PreprocessParamFileName);

                //Reset the open windows for a clean display.
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images_needed", 1);
                dev_display_example_reset_windows(hv_ExampleInternals);

                //Display instruction text.
                hv_WindowHandleText.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_text", out hv_WindowHandleText);
                HDevWindowStack.SetActive(hv_WindowHandleText);

                hv_Text.Dispose();
                hv_Text = new HTuple("We now have a trained segmentation model,");
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "which is ready to be applied to new images.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = new HTuple("Below, you see a few example images.");
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = new HTuple("These images represent newly incoming images,");
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "i.e. they are not part of the preprocessed dataset.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = new HTuple("The images have to be preprocessed in the same way as the DLDataset,");
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "which was used for training.";

                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window", "top",
                        "left", "black", "box", "true");
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Press Run (F5) to continue",
                        "window", "bottom", "right", "black", "box", "true");
                }

                //Display example images.
                hv_WindowHandleImages.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images", out hv_WindowHandleImages);
                HDevWindowStack.SetActive(hv_WindowHandleImages);
                dev_display_pills_example_dataset_preview();


                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_ExampleDataDir.Dispose();
                hv_ExampleDataDirExists.Dispose();
                hv_PreprocessParamFileName.Dispose();
                hv_PreprocessParamsExist.Dispose();
                hv_RetrainedModelFileName.Dispose();
                hv_ModelExists.Dispose();
                hv_ExceptionText.Dispose();
                hv_NumMissing.Dispose();
                hv_Text.Dispose();
                hv_WindowHandleImages.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_ExampleDataDir.Dispose();
                hv_ExampleDataDirExists.Dispose();
                hv_PreprocessParamFileName.Dispose();
                hv_PreprocessParamsExist.Dispose();
                hv_RetrainedModelFileName.Dispose();
                hv_ModelExists.Dispose();
                hv_ExceptionText.Dispose();
                hv_NumMissing.Dispose();
                hv_Text.Dispose();
                hv_WindowHandleImages.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_screen_final(HTuple hv_ExampleInternals)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_ShowExampleScreens = new HTuple();
            HTuple hv_WindowHandleText = new HTuple(), hv_Text = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure shows the final screen to conclude this example.

                hv_ShowExampleScreens.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "show_example_screens", out hv_ShowExampleScreens);
                if ((int)(hv_ShowExampleScreens.TupleNot()) != 0)
                {

                    hv_ShowExampleScreens.Dispose();
                    hv_WindowHandleText.Dispose();
                    hv_Text.Dispose();

                    return;
                }

                dev_open_example_text_window(hv_ExampleInternals);

                hv_WindowHandleText.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_text", out hv_WindowHandleText);
                HDevWindowStack.SetActive(hv_WindowHandleText);

                //Display instruction text.
                hv_Text.Dispose();
                hv_Text = "Congratulations!";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "You have finished the series of examples for DL segmentation.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "You can now train a segmentation model on your own data.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = new HTuple("If you want to learn more about further DL functionality in HALCON,");
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "please have a look at the examples on DL Classification";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "and DL Object Detection.";

                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window", "top",
                        "left", "black", "box", "true");
                }

                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "End of program.", "window",
                        "bottom", "right", "black", "box", "true");
                }


                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_screen_inference_step_1(HTuple hv_ExampleInternals)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_ShowExampleScreens = new HTuple();
            HTuple hv_WindowHandleText = new HTuple(), hv_Text = new HTuple();
            HTuple hv_WindowHandleImages = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure explains the step of sample generation.

                hv_ShowExampleScreens.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "show_example_screens", out hv_ShowExampleScreens);
                if ((int)(hv_ShowExampleScreens.TupleNot()) != 0)
                {

                    hv_ShowExampleScreens.Dispose();
                    hv_WindowHandleText.Dispose();
                    hv_Text.Dispose();
                    hv_WindowHandleImages.Dispose();

                    return;
                }

                //Reset the open windows for a clean display.
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images_needed", 1);
                dev_display_example_reset_windows(hv_ExampleInternals);

                hv_WindowHandleText.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_text", out hv_WindowHandleText);
                HDevWindowStack.SetActive(hv_WindowHandleText);

                //Display first part of inference instruction.
                hv_Text.Dispose();
                hv_Text = "Inference steps for one image:";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "1. Generate a DLSample for the image";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "   using the procedure 'gen_dl_samples_from_images'.";

                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window", "top",
                        "left", "black", "box", "true");
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Press Run (F5) to continue",
                        "window", "bottom", "right", "black", "box", "true");
                }

                //Display raw image.
                hv_WindowHandleImages.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images", out hv_WindowHandleImages);
                dev_display_raw_image(hv_WindowHandleImages);


                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();
                hv_WindowHandleImages.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();
                hv_WindowHandleImages.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_screen_inference_step_2(HTuple hv_ExampleInternals)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_ShowExampleScreens = new HTuple();
            HTuple hv_WindowHandleText = new HTuple(), hv_Text = new HTuple();
            HTuple hv_WindowHandleImages = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure explains the preprocessing step during inference.

                hv_ShowExampleScreens.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "show_example_screens", out hv_ShowExampleScreens);
                if ((int)(hv_ShowExampleScreens.TupleNot()) != 0)
                {

                    hv_ShowExampleScreens.Dispose();
                    hv_WindowHandleText.Dispose();
                    hv_Text.Dispose();
                    hv_WindowHandleImages.Dispose();

                    return;
                }

                //Reset the open windows for a clean display.
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images_needed", 1);
                dev_display_example_reset_windows(hv_ExampleInternals);

                hv_WindowHandleText.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_text", out hv_WindowHandleText);
                HDevWindowStack.SetActive(hv_WindowHandleText);

                //Display second part of inference instruction.
                hv_Text.Dispose();
                hv_Text = "Inference steps for one image:";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "1. Generate a DLSample for the image";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "   using the procedure 'gen_dl_samples_from_images'.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "2. Preprocess the image to fulfill the requirements of the trained model";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "   using the procedure 'preprocess_dl_samples'.";

                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window", "top",
                        "left", "black", "box", "true");
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Press Run (F5) to continue",
                        "window", "bottom", "right", "black", "box", "true");
                }

                //Display preprocessed image.
                hv_WindowHandleImages.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images", out hv_WindowHandleImages);
                HDevWindowStack.SetActive(hv_WindowHandleImages);
                dev_display_preprocessed_image(hv_ExampleInternals);


                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();
                hv_WindowHandleImages.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();
                hv_WindowHandleImages.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_screen_inference_step_3(HTuple hv_ExampleInternals)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_ShowExampleScreens = new HTuple();
            HTuple hv_WindowHandleText = new HTuple(), hv_Text = new HTuple();
            HTuple hv_WindowHandleImages = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure visualizes the apply step.

                hv_ShowExampleScreens.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "show_example_screens", out hv_ShowExampleScreens);
                if ((int)(hv_ShowExampleScreens.TupleNot()) != 0)
                {

                    hv_ShowExampleScreens.Dispose();
                    hv_WindowHandleText.Dispose();
                    hv_Text.Dispose();
                    hv_WindowHandleImages.Dispose();

                    return;
                }

                //Reset the open windows for a clean display.
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images_needed", 1);
                dev_display_example_reset_windows(hv_ExampleInternals);

                hv_WindowHandleText.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_text", out hv_WindowHandleText);
                HDevWindowStack.SetActive(hv_WindowHandleText);

                //Display third part of inference instruction.
                hv_Text.Dispose();
                hv_Text = "Inference steps for one image:";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "1. Generate a DLSample for the image";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "   using the procedure 'gen_dl_samples_from_images'.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "2. Preprocess the image to fulfill the requirements of the trained model";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "   using the procedure 'preprocess_dl_samples'.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "3. Apply the model using the operator 'apply_dl_model'.";

                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window", "top",
                        "left", "black", "box", "true");
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Press Run (F5) to continue",
                        "window", "bottom", "right", "black", "box", "true");
                }

                //Display output image of the model.
                hv_WindowHandleImages.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images", out hv_WindowHandleImages);
                HDevWindowStack.SetActive(hv_WindowHandleImages);
                dev_display_model_output_image(hv_ExampleInternals);


                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();
                hv_WindowHandleImages.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();
                hv_WindowHandleImages.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_screen_inference_step_4(HTuple hv_ExampleInternals)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_ShowExampleScreens = new HTuple();
            HTuple hv_WindowHandleText = new HTuple(), hv_Text = new HTuple();
            HTuple hv_WindowHandleImages = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure lists the postprocessing as step and displays an example.

                hv_ShowExampleScreens.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "show_example_screens", out hv_ShowExampleScreens);
                if ((int)(hv_ShowExampleScreens.TupleNot()) != 0)
                {

                    hv_ShowExampleScreens.Dispose();
                    hv_WindowHandleText.Dispose();
                    hv_Text.Dispose();
                    hv_WindowHandleImages.Dispose();

                    return;
                }

                //Reset the open windows for a clean display.
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images_needed", 1);
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_legend_needed", 1);
                dev_display_example_reset_windows(hv_ExampleInternals);

                hv_WindowHandleText.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_text", out hv_WindowHandleText);
                HDevWindowStack.SetActive(hv_WindowHandleText);

                //Display fourth part of inference instruction.
                hv_Text.Dispose();
                hv_Text = "Inference steps for one image:";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "1. Generate a DLSample for the image";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "   using the procedure 'gen_dl_samples_from_images'.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "2. Preprocess the image to fulfill the requirements of the trained model";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "   using the procedure 'preprocess_dl_samples'.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "3. Apply the model using the operator 'apply_dl_model'.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "4. Postprocess the output segmentation image";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "   to get segmented regions for each class.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window", "top",
                        "left", "black", "box", "true");
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Press Run (F5) to continue",
                        "window", "bottom", "right", "black", "box", "true");
                }

                //Display segmented regions.
                hv_WindowHandleImages.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images", out hv_WindowHandleImages);
                HDevWindowStack.SetActive(hv_WindowHandleImages);
                dev_display_segmented_regions(hv_ExampleInternals);


                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();
                hv_WindowHandleImages.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();
                hv_WindowHandleImages.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_screen_introduction(HTuple hv_ExampleInternals)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_ShowExampleScreens = new HTuple();
            HTuple hv_WindowHandleText = new HTuple(), hv_Text = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure shows an overview on all example parts.

                hv_ShowExampleScreens.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "show_example_screens", out hv_ShowExampleScreens);
                if ((int)(hv_ShowExampleScreens.TupleNot()) != 0)
                {

                    hv_ShowExampleScreens.Dispose();
                    hv_WindowHandleText.Dispose();
                    hv_Text.Dispose();

                    return;
                }

                //Reset the open windows for a clean display.
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images_needed", 0);
                dev_display_example_reset_windows(hv_ExampleInternals);

                hv_WindowHandleText.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_text", out hv_WindowHandleText);
                HDevWindowStack.SetActive(hv_WindowHandleText);

                //Display introductional text.
                hv_Text.Dispose();
                hv_Text = new HTuple("This example is part of a series of examples, which summarize ");
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "the workflow for DL segmentation.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "The four parts are: ";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "1. Dataset preprocessing.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "2. Training of the model.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "3. Evaluation of the trained model.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "4. Inference on new images.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "This example covers part 4: 'Inference on new images'.";

                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window", "top",
                        "left", "black", "box", "true");
                }

                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Press Run (F5) to continue",
                        "window", "bottom", "right", "black", "box", "true");
                }


                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_screen_ready_to_infer(HTuple hv_ExampleInternals)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_ShowExampleScreens = new HTuple();
            HTuple hv_WindowHandleText = new HTuple(), hv_Text = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure informs, that the execution will start in the next step
                //and what will be done.
                hv_ShowExampleScreens.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "show_example_screens", out hv_ShowExampleScreens);
                if ((int)(hv_ShowExampleScreens.TupleNot()) != 0)
                {

                    hv_ShowExampleScreens.Dispose();
                    hv_WindowHandleText.Dispose();
                    hv_Text.Dispose();

                    return;
                }

                //Reset the open windows for a clean display.
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images_needed", 0);
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_legend_needed", 0);
                dev_display_example_reset_windows(hv_ExampleInternals);

                hv_WindowHandleText.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_text", out hv_WindowHandleText);
                HDevWindowStack.SetActive(hv_WindowHandleText);

                //Display instruction text.
                hv_Text.Dispose();
                hv_Text = "We will now apply the trained model from example part 2 ";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "(segmentation_pill_defects_deep_learning_2_train.hdev)";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "to some new images using 'apply_dl_model'.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = new HTuple("Additionally, we apply postprocessing steps to the");
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "output segmentation image:";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = " 1. Get a segmented region for each class.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = " 2. Split the segmented regions into connected components.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = " 3. Compute the area of each defect instance.";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "";
                if (hv_Text == null)
                    hv_Text = new HTuple();
                hv_Text[new HTuple(hv_Text.TupleLength())] = "The final result will be displayed for each image.";

                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_Text, "window", "top",
                        "left", "black", "box", "true");
                }

                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Press Run (F5) to continue",
                        "window", "bottom", "right", "black", "box", "true");
                }

                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_handles_to_close", hv_WindowHandleText);


                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_ShowExampleScreens.Dispose();
                hv_WindowHandleText.Dispose();
                hv_Text.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_display_segmented_regions(HTuple hv_ExampleInternals)
        {



            // Local iconic variables 

            HObject ho_Image, ho_SegImage, ho_Defect;

            // Local control variables 

            HTuple hv_Area = new HTuple(), hv_Row = new HTuple();
            HTuple hv_Column = new HTuple(), hv_Info = new HTuple();
            HTuple hv_ClassIDs = new HTuple(), hv_ClassNames = new HTuple();
            HTuple hv_Sample = new HTuple(), hv_Result = new HTuple();
            HTuple hv_GenParam = new HTuple(), hv_WindowHandleDict = new HTuple();
            HTuple hv_WindowHandleImages = new HTuple(), hv_WindowHandleLegend = new HTuple();
            HTuple hv_WindowHandles = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_SegImage);
            HOperatorSet.GenEmptyObj(out ho_Defect);
            try
            {
                //This procedure displays an example of extracted regions.

                //Get preprocessed image and segmentation image.
                ho_Image.Dispose();
                HOperatorSet.GetDictObject(out ho_Image, hv_ExampleInternals, "preprocessed_image");
                ho_SegImage.Dispose();
                HOperatorSet.GetDictObject(out ho_SegImage, hv_ExampleInternals, "segmentation_image");

                //Get defect region and compute its area.
                ho_Defect.Dispose();
                HOperatorSet.Threshold(ho_SegImage, out ho_Defect, 1, 1);
                hv_Area.Dispose(); hv_Row.Dispose(); hv_Column.Dispose();
                HOperatorSet.AreaCenter(ho_Defect, out hv_Area, out hv_Row, out hv_Column);

                //Display the image with overlayed regions.
                //Dataset info.
                hv_Info.Dispose();
                HOperatorSet.CreateDict(out hv_Info);
                hv_ClassIDs.Dispose();
                hv_ClassIDs = new HTuple();
                hv_ClassIDs[0] = 0;
                hv_ClassIDs[1] = 1;
                hv_ClassIDs[2] = 2;
                hv_ClassNames.Dispose();
                hv_ClassNames = new HTuple();
                hv_ClassNames[0] = "good";
                hv_ClassNames[1] = "contamination";
                hv_ClassNames[2] = "crack";
                HOperatorSet.SetDictTuple(hv_Info, "class_ids", hv_ClassIDs);
                HOperatorSet.SetDictTuple(hv_Info, "class_names", hv_ClassNames);
                //Sample.
                hv_Sample.Dispose();
                HOperatorSet.CreateDict(out hv_Sample);
                HOperatorSet.SetDictObject(ho_Image, hv_Sample, "image");
                //Result.
                hv_Result.Dispose();
                HOperatorSet.CreateDict(out hv_Result);
                HOperatorSet.SetDictObject(ho_SegImage, hv_Result, "segmentation_image");
                //GenParam.
                hv_GenParam.Dispose();
                HOperatorSet.CreateDict(out hv_GenParam);
                HOperatorSet.SetDictTuple(hv_GenParam, "segmentation_exclude_class_ids", 0);
                HOperatorSet.SetDictTuple(hv_GenParam, "segmentation_transparency", "80");
                HOperatorSet.SetDictTuple(hv_GenParam, "font_size", 16);
                //WindowHandleDict.
                hv_WindowHandleDict.Dispose();
                HOperatorSet.CreateDict(out hv_WindowHandleDict);
                hv_WindowHandleImages.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images", out hv_WindowHandleImages);
                hv_WindowHandleLegend.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_legend", out hv_WindowHandleLegend);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.SetDictTuple(hv_WindowHandleDict, "segmentation_image_result",
                        hv_WindowHandleImages.TupleConcat(hv_WindowHandleLegend));
                }
                //Display results.
                dev_display_dl_data(hv_Sample, hv_Result, hv_Info, "segmentation_image_result",
                    hv_GenParam, hv_WindowHandleDict);

                //Display area.
                hv_WindowHandles.Dispose();
                HOperatorSet.GetDictTuple(hv_WindowHandleDict, "segmentation_image_result",
                    out hv_WindowHandles);
                HDevWindowStack.SetActive(hv_WindowHandles.TupleSelect(
                    0));
                if (HDevWindowStack.IsOpen())
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        HOperatorSet.DispText(HDevWindowStack.GetActive(), (((hv_ClassNames.TupleSelect(
                            1)) + "\narea: ") + hv_Area) + "px", "image", hv_Row - 10, hv_Column + 10, "black",
                            new HTuple(), new HTuple());
                    }
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispText(HDevWindowStack.GetActive(), "Segmented defect regions",
                        "window", "top", "left", "black", new HTuple(), new HTuple());
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.FlushBuffer(hv_WindowHandles.TupleSelect(0));
                }

                ho_Image.Dispose();
                ho_SegImage.Dispose();
                ho_Defect.Dispose();

                hv_Area.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_Info.Dispose();
                hv_ClassIDs.Dispose();
                hv_ClassNames.Dispose();
                hv_Sample.Dispose();
                hv_Result.Dispose();
                hv_GenParam.Dispose();
                hv_WindowHandleDict.Dispose();
                hv_WindowHandleImages.Dispose();
                hv_WindowHandleLegend.Dispose();
                hv_WindowHandles.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Image.Dispose();
                ho_SegImage.Dispose();
                ho_Defect.Dispose();

                hv_Area.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_Info.Dispose();
                hv_ClassIDs.Dispose();
                hv_ClassNames.Dispose();
                hv_Sample.Dispose();
                hv_Result.Dispose();
                hv_GenParam.Dispose();
                hv_WindowHandleDict.Dispose();
                hv_WindowHandleImages.Dispose();
                hv_WindowHandleLegend.Dispose();
                hv_WindowHandles.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_example_init(HTuple hv_ShowExampleScreens, out HTuple hv_ExampleInternals)
        {


            // Initialize local and output iconic variables 
            hv_ExampleInternals = new HTuple();
            //This procedure initializes the graphic windows that are used for explanations during the example.

            //A dict that will be used/adapted by other example procedures.
            hv_ExampleInternals.Dispose();
            HOperatorSet.CreateDict(out hv_ExampleInternals);
            HOperatorSet.SetDictTuple(hv_ExampleInternals, "show_example_screens", hv_ShowExampleScreens);
            if ((int)(hv_ShowExampleScreens.TupleNot()) != 0)
            {


                return;
            }

            if (HDevWindowStack.IsOpen())
            {
                HOperatorSet.CloseWindow(HDevWindowStack.Pop());
            }
            dev_open_example_text_window(hv_ExampleInternals);

            HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images_needed", 0);
            HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_legend_needed", 0);



            return;
        }

        public void dev_open_example_image_window(HTuple hv_ExampleInternals)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_WindowHeightText = new HTuple();
            HTuple hv_WindowWidthImage = new HTuple(), hv_WindowHeightImages = new HTuple();
            HTuple hv_WindowBGColor = new HTuple(), hv_WindowYImages = new HTuple();
            HTuple hv_WindowXImages = new HTuple(), hv_WindowHandleImages = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure initializes the graphic windows that are used to display example images.

                hv_WindowHeightText.Dispose();
                hv_WindowHeightText = 300;
                hv_WindowWidthImage.Dispose();
                hv_WindowWidthImage = 500;
                hv_WindowHeightImages.Dispose();
                hv_WindowHeightImages = 500;
                hv_WindowBGColor.Dispose();
                hv_WindowBGColor = "black";

                hv_WindowYImages.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_WindowYImages = hv_WindowHeightText + 60;
                }
                hv_WindowXImages.Dispose();
                hv_WindowXImages = 0;
                HOperatorSet.SetWindowAttr("background_color", hv_WindowBGColor);
                HOperatorSet.OpenWindow(hv_WindowYImages, hv_WindowXImages, hv_WindowWidthImage, hv_WindowHeightImages, 0, "visible", "", out hv_WindowHandleImages);
                HDevWindowStack.Push(hv_WindowHandleImages);
                set_display_font(hv_WindowHandleImages, 16, "mono", "true", "false");
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images", hv_WindowHandleImages);
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images_width", hv_WindowWidthImage);
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images_height", hv_WindowHeightImages);
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images_x", hv_WindowXImages);
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_images_y", hv_WindowYImages);

                hv_WindowHeightText.Dispose();
                hv_WindowWidthImage.Dispose();
                hv_WindowHeightImages.Dispose();
                hv_WindowBGColor.Dispose();
                hv_WindowYImages.Dispose();
                hv_WindowXImages.Dispose();
                hv_WindowHandleImages.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_WindowHeightText.Dispose();
                hv_WindowWidthImage.Dispose();
                hv_WindowHeightImages.Dispose();
                hv_WindowBGColor.Dispose();
                hv_WindowYImages.Dispose();
                hv_WindowXImages.Dispose();
                hv_WindowHandleImages.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_open_example_legend_window(HTuple hv_ExampleInternals, HTuple hv_WindowWidth)
        {



            // Local control variables 

            HTuple hv_WindowImagesHeight = new HTuple();
            HTuple hv_WindowImagesWidth = new HTuple(), hv_WindowImagesX = new HTuple();
            HTuple hv_WindowImagesY = new HTuple(), hv_WindowHandleLegend = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedure initializes the graphic windows that are used to display a legend.

                hv_WindowImagesHeight.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images_height", out hv_WindowImagesHeight);
                hv_WindowImagesWidth.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images_width", out hv_WindowImagesWidth);
                hv_WindowImagesX.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images_x", out hv_WindowImagesX);
                hv_WindowImagesY.Dispose();
                HOperatorSet.GetDictTuple(hv_ExampleInternals, "window_images_y", out hv_WindowImagesY);
                HOperatorSet.SetWindowAttr("background_color", "black");
                HOperatorSet.OpenWindow(hv_WindowImagesY, (hv_WindowImagesX + hv_WindowImagesWidth) + 5, hv_WindowWidth, hv_WindowImagesHeight, 0, "visible", "", out hv_WindowHandleLegend);
                HDevWindowStack.Push(hv_WindowHandleLegend);
                set_display_font(hv_WindowHandleLegend, 14, "mono", "true", "false");
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_legend", hv_WindowHandleLegend);

                hv_WindowImagesHeight.Dispose();
                hv_WindowImagesWidth.Dispose();
                hv_WindowImagesX.Dispose();
                hv_WindowImagesY.Dispose();
                hv_WindowHandleLegend.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_WindowImagesHeight.Dispose();
                hv_WindowImagesWidth.Dispose();
                hv_WindowImagesX.Dispose();
                hv_WindowImagesY.Dispose();
                hv_WindowHandleLegend.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void dev_open_example_text_window(HTuple hv_ExampleInternals)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_WindowWidthText = new HTuple(), hv_WindowHeightText = new HTuple();
            HTuple hv_WindowBGColor = new HTuple(), hv_WindowHandleText = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                hv_WindowWidthText.Dispose();
                hv_WindowWidthText = 800;
                hv_WindowHeightText.Dispose();
                hv_WindowHeightText = 300;
                hv_WindowBGColor.Dispose();
                hv_WindowBGColor = "gray";
                HOperatorSet.SetWindowAttr("background_color", hv_WindowBGColor);
                HOperatorSet.OpenWindow(0, 0, hv_WindowWidthText, hv_WindowHeightText, 0, "visible", "", out hv_WindowHandleText);
                HDevWindowStack.Push(hv_WindowHandleText);
                set_display_font(hv_WindowHandleText, 16, "mono", "true", "false");
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_text", hv_WindowHandleText);
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_text_width", hv_WindowWidthText);
                HOperatorSet.SetDictTuple(hv_ExampleInternals, "window_text_height", hv_WindowHeightText);

                hv_WindowWidthText.Dispose();
                hv_WindowHeightText.Dispose();
                hv_WindowBGColor.Dispose();
                hv_WindowHandleText.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_WindowWidthText.Dispose();
                hv_WindowHeightText.Dispose();
                hv_WindowBGColor.Dispose();
                hv_WindowHandleText.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void get_inference_images(HTuple hv_ImageDir, out HTuple hv_ImageFiles)
        {



            // Local iconic variables 
            // Initialize local and output iconic variables 
            hv_ImageFiles = new HTuple();
            //This procedure selects some images for the demonstration purposes of this example.

            hv_ImageFiles.Dispose();
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_ImageFiles = hv_ImageDir + "/ginseng/contamination/pill_ginseng_contamination_154";
            }
            if (hv_ImageFiles == null)
                hv_ImageFiles = new HTuple();
            hv_ImageFiles[new HTuple(hv_ImageFiles.TupleLength())] = hv_ImageDir + "/magnesium/crack/pill_magnesium_crack_036";
            if (hv_ImageFiles == null)
                hv_ImageFiles = new HTuple();
            hv_ImageFiles[new HTuple(hv_ImageFiles.TupleLength())] = hv_ImageDir + "/mint/good/pill_mint_good_165";
            if (hv_ImageFiles == null)
                hv_ImageFiles = new HTuple();
            hv_ImageFiles[new HTuple(hv_ImageFiles.TupleLength())] = hv_ImageDir + "/ginseng/crack/pill_ginseng_crack_004";
            if (hv_ImageFiles == null)
                hv_ImageFiles = new HTuple();
            hv_ImageFiles[new HTuple(hv_ImageFiles.TupleLength())] = hv_ImageDir + "/magnesium/good/pill_magnesium_good_066";
            if (hv_ImageFiles == null)
                hv_ImageFiles = new HTuple();
            hv_ImageFiles[new HTuple(hv_ImageFiles.TupleLength())] = hv_ImageDir + "/ginseng/contamination/pill_ginseng_contamination_244";
            if (hv_ImageFiles == null)
                hv_ImageFiles = new HTuple();
            hv_ImageFiles[new HTuple(hv_ImageFiles.TupleLength())] = hv_ImageDir + "/mint/contamination/pill_mint_contamination_076";
            if (hv_ImageFiles == null)
                hv_ImageFiles = new HTuple();
            hv_ImageFiles[new HTuple(hv_ImageFiles.TupleLength())] = hv_ImageDir + "/magnesium/contamination/pill_magnesium_contamination_011";
            if (hv_ImageFiles == null)
                hv_ImageFiles = new HTuple();
            hv_ImageFiles[new HTuple(hv_ImageFiles.TupleLength())] = hv_ImageDir + "/ginseng/good/pill_ginseng_good_042";
            if (hv_ImageFiles == null)
                hv_ImageFiles = new HTuple();
            hv_ImageFiles[new HTuple(hv_ImageFiles.TupleLength())] = hv_ImageDir + "/mint/crack/pill_mint_crack_121";


            return;
        }












        public void ImageProcess()
        {
            MySqlConnection conn = new MySqlConnection(Global.conString);
            MySqlCommand comm = new MySqlCommand();
            comm.Connection = conn;
            MySqlDataReader defectionTableReader;
            string Query3 = "";





            int tempx = 0;
            // Local iconic variables 
            HObject ho_Image;
            HObject ho_ImageBatch = null;
            HObject ho_SegmentationImage = null, ho_ClassRegions = null;
            HObject ho_ClassRegion = null, ho_ConnectedRegions = null, ho_CurrentRegion = null;

            // Local control variables 

            HTuple hv_UsePretrainedModel = new HTuple();
            HTuple hv_ImageDir = new HTuple(), hv_ExampleDataDir = new HTuple();
            //文件夹、文件名、扩展名
            HTuple hv_BaseName = new HTuple(), hv_Extension = new HTuple(), hv_Directory = new HTuple();
            HTuple hv_PreprocessParamFileName = new HTuple(), hv_RetrainedModelFileName = new HTuple();
            HTuple hv_ClassNames = new HTuple(), hv_ClassIDs = new HTuple();
            HTuple hv_BatchSizeInference = new HTuple(), hv_UseGPU = new HTuple();
            HTuple hv_CudaLoaded = new HTuple(), hv_CuDNNLoaded = new HTuple();
            HTuple hv_CuBlasLoaded = new HTuple(), hv_DLModelHandle = new HTuple();
            HTuple hv_DLPreprocessParam = new HTuple(), hv_WindowHandleDict = new HTuple();
            HTuple hv_DatasetInfo = new HTuple(), hv_GenParamDisplay = new HTuple();
            HTuple hv_ImageFiles = new HTuple(), hv_BatchIndex = new HTuple();
            HTuple hv_Batch = new HTuple(), hv_DLSampleBatch = new HTuple();
            HTuple hv_Start = new HTuple(), hv_DLResultBatch = new HTuple();
            HTuple hv_SampleIndex = new HTuple(), hv_WindowHandles = new HTuple();
            HTuple hv_Areas = new HTuple(), hv_ClassIndex = new HTuple();
            HTuple hv_Area = new HTuple(), hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_ConnectIndex = new HTuple(), hv_End = new HTuple();
            HTuple hv_TimeSeg = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageBatch);
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_SegmentationImage);
            HOperatorSet.GenEmptyObj(out ho_ClassRegions);
            HOperatorSet.GenEmptyObj(out ho_ClassRegion);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_CurrentRegion);

            // hv_DefectTypeCoordinate = "";


            try
            {

                dev_update_off();
                //
                hv_UsePretrainedModel.Dispose();
                hv_UsePretrainedModel = 0;
                //*************************************************
                //**   Set paths and parameters for inference   ***
                //*************************************************
                //
                //In a real application newly incoming images (not used for training or evaluation)
                //would be used here.
                //
                //Directory name with the images to be segmented.
                hv_ImageDir.Dispose();
                // hv_ImageDir = "xiamen";
                //
                //Example data folder containing the outputs of the previous example series.
                hv_ExampleDataDir.Dispose();
                hv_ExampleDataDir = "D:/model/work1/work1/segment_filter_defects_data";
                //hv_ExampleDataDir = "E:/大学/大四下/毕设/最新模型，今天4.2号要用/work1/work1/segment_filter_defects_data";
                //
                //Load the trained or pretrained model for defects detection
                //
                //File name of dict containing parameters used for preprocessing.
                hv_PreprocessParamFileName.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    // hv_PreprocessParamFileName = "F:/codetest/DLtest/work/segment_filter_defects_data/dl_preprocess_param_512x512.hdict";
                    hv_PreprocessParamFileName = hv_ExampleDataDir + "/dl_preprocess_param_512x512.hdict";
                }
                //Path of the retrained segmentation model.
                hv_RetrainedModelFileName.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_RetrainedModelFileName = hv_ExampleDataDir + "/best_dl_model_segmentation.hdl";
                }
                //
                //Provide the class names and IDs.
                //Class names.
                hv_ClassNames.Dispose();
                hv_ClassNames = new HTuple();
                hv_ClassNames[0] = "";
                hv_ClassNames[1] = "Digs";
                hv_ClassNames[2] = "Scratches";
                hv_ClassNames[3] = "Prints";
                hv_ClassNames[4] = "Corrosions";
                hv_ClassNames[5] = "Swabs";
                //Respective class IDs.
                hv_ClassIDs.Dispose();
                hv_ClassIDs = new HTuple();
                hv_ClassIDs[0] = 0;
                hv_ClassIDs[1] = 1;
                hv_ClassIDs[2] = 2;
                hv_ClassIDs[3] = 3;
                hv_ClassIDs[4] = 4;
                hv_ClassIDs[5] = 5;
                //
                //Batch Size used during inference.
                hv_BatchSizeInference.Dispose();
                hv_BatchSizeInference = 1;
                //
                //The inference can be done on GPU or CPU.
                //See the respective system requirements in the Installation Guide.
                hv_UseGPU.Dispose();
                hv_UseGPU = 1;
                //
                //********************
                //**   Inference   ***
                //********************
                //
                //Check availability of GPU mode.













                if ((int)(hv_UseGPU) != 0)
                {
                    hv_CudaLoaded.Dispose();
                    HOperatorSet.GetSystem("cuda_loaded", out hv_CudaLoaded);
                    hv_CuDNNLoaded.Dispose();
                    HOperatorSet.GetSystem("cudnn_loaded", out hv_CuDNNLoaded);
                    hv_CuBlasLoaded.Dispose();
                    HOperatorSet.GetSystem("cublas_loaded", out hv_CuBlasLoaded);
                    if ((int)((new HTuple((new HTuple((new HTuple(hv_CudaLoaded.TupleEqual("true"))).TupleAnd(
                        new HTuple(hv_CuDNNLoaded.TupleEqual("true"))))).TupleAnd(new HTuple(hv_CuBlasLoaded.TupleEqual(
                        "true"))))).TupleNot()) != 0)
                    {
                        hv_UseGPU.Dispose();
                        hv_UseGPU = 0;
                    }
                }







                check_model_availability(hv_ExampleDataDir, hv_PreprocessParamFileName, hv_RetrainedModelFileName,
                    hv_UsePretrainedModel);
                //
                //Read in the retrained model.



                hv_DLModelHandle.Dispose();
                HOperatorSet.ReadDlModel(hv_RetrainedModelFileName, out hv_DLModelHandle);
                //Set the batch size.
                HOperatorSet.SetDlModelParam(hv_DLModelHandle, "batch_size", hv_BatchSizeInference);
                //Initialize the model for inference.
                if ((int)(hv_UseGPU.TupleNot()) != 0)
                {
                    HOperatorSet.SetDlModelParam(hv_DLModelHandle, "runtime", "cpu");
                }
                HOperatorSet.SetDlModelParam(hv_DLModelHandle, "runtime_init", "immediately");
                //
                //Get the parameters used for preprocessing.
                hv_DLPreprocessParam.Dispose();
                HOperatorSet.ReadDict(hv_PreprocessParamFileName, new HTuple(), new HTuple(),
                    out hv_DLPreprocessParam);













                //
                //Set parameters for visualization of results.
                hv_WindowHandleDict.Dispose();
                HOperatorSet.CreateDict(out hv_WindowHandleDict);
                hv_DatasetInfo.Dispose();
                HOperatorSet.CreateDict(out hv_DatasetInfo);
                HOperatorSet.SetDictTuple(hv_DatasetInfo, "class_ids", hv_ClassIDs);
                HOperatorSet.SetDictTuple(hv_DatasetInfo, "class_names", hv_ClassNames);
                hv_GenParamDisplay.Dispose();
                HOperatorSet.CreateDict(out hv_GenParamDisplay);
                HOperatorSet.SetDictTuple(hv_GenParamDisplay, "segmentation_exclude_class_ids",
                    0);
                HOperatorSet.SetDictTuple(hv_GenParamDisplay, "segmentation_transparency",
                    "80");
                HOperatorSet.SetDictTuple(hv_GenParamDisplay, "font_size", 16);
                //
                //List the files, the model should be applied to (e.g. using list_image_files).
                //For this example, we select some images manually.
                //get_inference_images (ImageDir, ImageFiles)
                //list_image_files ('F:/儒勒/opt&lasers/inference', 'default', [], ImageFiles)


                hv_ImageFiles.Dispose();
                list_image_files("C:/Users/admin/Desktop/testpicture3/camera1/ring",
                    "default", new HTuple(), out hv_ImageFiles);
                //list_image_files("E:/大学/大四下/毕设/滤光片图像/pic",
                //    "default", new HTuple(), out hv_ImageFiles);
                //
                //Loop over all images in batches of size BatchSizeInference for inference.









                HTuple end_val84 = ((((new HTuple(hv_ImageFiles.TupleLength()
                   )) / (hv_BatchSizeInference.TupleReal()))).TupleFloor()) - 1;
                HTuple step_val84 = 1;

                for (hv_BatchIndex = 0; hv_BatchIndex.Continue(end_val84, step_val84); hv_BatchIndex = hv_BatchIndex.TupleAdd(step_val84))
                {

                    //Get the paths to the images of the batch.
                    hv_Batch.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Batch = hv_ImageFiles.TupleSelectRange(
                            hv_BatchIndex * hv_BatchSizeInference, ((hv_BatchIndex + 1) * hv_BatchSizeInference) - 1);
                    }
                    //Read the images of the batch.

                    //读取文件名
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_BaseName.Dispose(); hv_Extension.Dispose(); hv_Directory.Dispose();
                        parse_filename(hv_ImageFiles.TupleSelect(hv_BatchIndex), out hv_BaseName, out hv_Extension,
                             out hv_Directory);
                    }






                    ho_ImageBatch.Dispose();
                    HOperatorSet.ReadImage(out ho_ImageBatch, hv_Batch);
                    //
                    //Generate the DLSampleBatch.
                    hv_DLSampleBatch.Dispose();
                    gen_dl_samples_from_images(ho_ImageBatch, out hv_DLSampleBatch);
                    //
                    //Preprocess the DLSampleBatch.
                    preprocess_dl_samples(hv_DLSampleBatch, hv_DLPreprocessParam);
                    //
                    hv_Start.Dispose();
                    HOperatorSet.CountSeconds(out hv_Start);
                    //Apply the DL model on the DLSampleBatch.
                    hv_DLResultBatch.Dispose();
                    HOperatorSet.ApplyDlModel(hv_DLModelHandle, hv_DLSampleBatch, (new HTuple("segmentation_image")).TupleConcat(
                        "segmentation_confidence"), out hv_DLResultBatch);



                    HTuple end_val103 = hv_BatchSizeInference - 1;
                    HTuple step_val103 = 1;


                    for (hv_SampleIndex = 0; hv_SampleIndex.Continue(end_val103, step_val103); hv_SampleIndex = hv_SampleIndex.TupleAdd(step_val103))
                    {
                        //
                        //Get image.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            //   ho_Image.Dispose();
                            HOperatorSet.GetDictObject(out ho_Image, hv_DLSampleBatch.TupleSelect(hv_SampleIndex),
                                "image");
                        }
                        //Get result image.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            ho_SegmentationImage.Dispose();
                            HOperatorSet.GetDictObject(out ho_SegmentationImage, hv_DLResultBatch.TupleSelect(
                                hv_SampleIndex), "segmentation_image");
                        }


                        //Postprocessing: Get segmented regions for each class.
                        ho_ClassRegions.Dispose();
                        HOperatorSet.Threshold(ho_SegmentationImage, out ho_ClassRegions, hv_ClassIDs,
                            hv_ClassIDs);
                        //
                        //Display results.




                        hv_Areas.Dispose();
                        HOperatorSet.RegionFeatures(ho_ClassRegions, "area", out hv_Areas);
                        //
                        //Here, we do not display the first class, since it is the class 'good'
                        //and we only want to display the defect regions.

                    }









                    Query3 = "";
                    for (hv_ClassIndex = 0; (int)hv_ClassIndex <= (int)((new HTuple(hv_Areas.TupleLength()
                            )) - 1); hv_ClassIndex = (int)hv_ClassIndex + 1)
                    {
                        if ((int)(new HTuple(((hv_Areas.TupleSelect(hv_ClassIndex))).TupleGreater(
                            0))) != 0)
                        {
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                ho_ClassRegion.Dispose();
                                HOperatorSet.SelectObj(ho_ClassRegions, out ho_ClassRegion, hv_ClassIndex + 1);
                            }
                            //Get connected components of the segmented class region.
                            ho_ConnectedRegions.Dispose();
                            HOperatorSet.Connection(ho_ClassRegion, out ho_ConnectedRegions);
                            hv_Area.Dispose(); hv_Row.Dispose(); hv_Column.Dispose();
                            HOperatorSet.AreaCenter(ho_ConnectedRegions, out hv_Area, out hv_Row,
                                out hv_Column);


                            #region 这里是把新检测的数据给存入进去
                            double picx, picy, trayx, trayy;
                            int trayxnum, trayynum;
                            zx = hv_Row.TupleSelect(0).ToString();
                            zy = hv_Column.TupleSelect(0).ToString();
                            picx = Math.Round(Convert.ToDouble(zx), 2);
                            picy = Math.Round(Convert.ToDouble(zy), 2);
                            int cameraId, row, line;
                            string[] split = hv_BaseName.TupleSplit("-");
                            cameraId = Convert.ToInt32(split[0]);
                            row = Convert.ToInt32(split[1]);
                            line = Convert.ToInt32(split[2]);
                            double[] xy = getDefLoc(cameraId, row, line, Convert.ToDouble(zx), Convert.ToDouble(zy));
                            trayx = Math.Round(xy[0], 2);
                            trayy = Math.Round(xy[1], 2);
                            trayxnum = Global.optRow - (int)xy[3] - 1;
                            trayynum = Global.optLine - (int)xy[2] - 1;
                            //MySqlCommand cmd2;
                            

                            if (hv_Area.TupleSelect(0) <= 150000 && trayx > 0 && trayy > 0 && trayy < 348 && trayx < 351 && hv_ClassIndex != 1)
                            {
                                MessageBox.Show(trayxnum + "," + trayynum);
                                //conn.Open();
                                Query3 += "insert into defection(qrcode,picName,picX,picY,trayX,trayY,posX,posY,defectionType,area) value('" + Global.qrCode + "','" + hv_BaseName + "','" + picx + "','" + picy + "','" + trayx + "','" + trayy + "','" + trayxnum + "','"+ trayynum + "','" + hv_ClassNames[hv_ClassIndex] + "','" + hv_Area.TupleSelect(0) + "');";
                                //cmd2 = new MySqlCommand(Query3, conn);
                                //cmd2.ExecuteNonQuery();
                                //conn.Close();
                                ////查询照片检测情况
                            }
                            #endregion  新增数据结束






                            for (hv_ConnectIndex = 0; (int)hv_ConnectIndex <= (int)((new HTuple(hv_Area.TupleLength()
                                    )) - 1); hv_ConnectIndex = (int)hv_ConnectIndex + 1)
                            {

                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    ho_CurrentRegion.Dispose();
                                    HOperatorSet.SelectObj(ho_ConnectedRegions, out ho_CurrentRegion,
                                        hv_ConnectIndex + 1);
                                }
                                hv_End.Dispose();
                                HOperatorSet.CountSeconds(out hv_End);
                                hv_TimeSeg.Dispose();
                                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                {
                                    //  hv_TimeSeg = 1000 * (hv_End - hv_Start);
                                }
                                if (HDevWindowStack.IsOpen())
                                {
                                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                    {
                                        // HOperatorSet.DispText(HDevWindowStack.GetActive(), ("Detection time: " + (hv_TimeSeg.TupleString(
                                        //       "0.0f"))) + "ms", "window", "top", "right", "black", new HTuple(),
                                        //   new HTuple());
                                    }
                                }



                                if (HDevWindowStack.IsOpen())
                                {
                                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                                    {
                                        //      HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_ClassNames.TupleSelect(
                                        //        hv_ClassIndex), "image", (hv_Row.TupleSelect(hv_ConnectIndex)) - 10,
                                        //     (hv_Column.TupleSelect(hv_ConnectIndex)) + 10, "red", new HTuple(),
                                        //      new HTuple());

                                        //x = hv_Row.TupleSelect(hv_ConnectIndex);
                                        //y = hv_Column.TupleSelect(hv_ConnectIndex);
                                    }
                                }
                            }

                        }
                    }

                    #region 照片的检测情况管理，存在则加一，不存在则新增一条记录
                    string Query2 = "";
                    conn.Open();
                    comm.Connection = conn;
                    comm.CommandText = "select count(picStatus) from picture WHERE qrcode='" + Global.qrCode + "' and picName='" + hv_BaseName + "'";     //拼接sql语句
                    MySqlDataReader trayTableReader;
                    trayTableReader = comm.ExecuteReader();
                    trayTableReader.Read();
                    int status = Convert.ToInt32(trayTableReader.GetValue(0));
                    conn.Close();
                    //查询照片检测情况
                    if (Query3 != "")
                    {
                        conn.Open();
                        comm.CommandText = Query3;
                        comm.ExecuteNonQuery();
                        conn.Close();
                    }
                    //查询照片检测情况
                    conn.Open();
                    if (status == 0)
                    {
                        Query2 = "insert into picture(qrcode,picName,path,time,picStatus) value('" + Global.qrCode + "','" + hv_BaseName + "','C:/Users/86157/Desktop/123/ring',now(),1)";
                    }
                    else
                    {
                        Query2 = "update picture set picStatus=picStatus+1 , time=now() WHERE qrcode='" + Global.qrCode + "' and picName='" + hv_BaseName + "'";
                    }
                    comm.CommandText = Query2;
                    comm.ExecuteNonQuery();
                    conn.Close();

                    #endregion   照片检测结束

                    //
                    //Display whether the filter is OK, or not.
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        // dev_display_ok_nok(hv_Areas, hv_WindowHandles.TupleSelect(0));
                        //以上一行代码会抛出异常
                    }
                    //
                    // stop(...); only in hdevelop
                }
            }
            //
            //Close windows.
            //   dev_display_dl_data_close_windows(hv_WindowHandleDict);
            //



            catch (HalconException HDevExpDefaultException)
            {
                ho_ImageBatch.Dispose();
                //  ho_Image.Dispose();
                ho_SegmentationImage.Dispose();
                ho_ClassRegions.Dispose();
                ho_ClassRegion.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_CurrentRegion.Dispose();

                hv_UsePretrainedModel.Dispose();
                hv_ImageDir.Dispose();
                hv_ExampleDataDir.Dispose();
                hv_PreprocessParamFileName.Dispose();
                hv_RetrainedModelFileName.Dispose();
                hv_ClassNames.Dispose();
                hv_ClassIDs.Dispose();
                hv_BatchSizeInference.Dispose();
                hv_UseGPU.Dispose();
                hv_CudaLoaded.Dispose();
                hv_CuDNNLoaded.Dispose();
                hv_CuBlasLoaded.Dispose();
                hv_DLModelHandle.Dispose();
                hv_DLPreprocessParam.Dispose();
                hv_WindowHandleDict.Dispose();
                hv_DatasetInfo.Dispose();
                hv_GenParamDisplay.Dispose();
                hv_ImageFiles.Dispose();
                hv_BatchIndex.Dispose();
                hv_Batch.Dispose();
                hv_DLSampleBatch.Dispose();
                hv_Start.Dispose();
                hv_DLResultBatch.Dispose();
                hv_SampleIndex.Dispose();
                hv_WindowHandles.Dispose();
                hv_Areas.Dispose();
                hv_ClassIndex.Dispose();
                hv_Area.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_ConnectIndex.Dispose();
                hv_End.Dispose();
                hv_TimeSeg.Dispose();
                throw HDevExpDefaultException;
            }
            ho_ImageBatch.Dispose();
            // ho_Image.Dispose();
            ho_SegmentationImage.Dispose();
            ho_ClassRegions.Dispose();
            ho_ClassRegion.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_CurrentRegion.Dispose();

            hv_UsePretrainedModel.Dispose();
            hv_ImageDir.Dispose();
            hv_ExampleDataDir.Dispose();
            hv_PreprocessParamFileName.Dispose();
            hv_RetrainedModelFileName.Dispose();
            hv_ClassNames.Dispose();
            hv_ClassIDs.Dispose();
            hv_BatchSizeInference.Dispose();
            hv_UseGPU.Dispose();
            hv_CudaLoaded.Dispose();
            hv_CuDNNLoaded.Dispose();
            hv_CuBlasLoaded.Dispose();
            hv_DLModelHandle.Dispose();
            hv_DLPreprocessParam.Dispose();
            hv_WindowHandleDict.Dispose();
            hv_DatasetInfo.Dispose();
            hv_GenParamDisplay.Dispose();
            hv_ImageFiles.Dispose();
            hv_BatchIndex.Dispose();
            hv_Batch.Dispose();
            hv_DLSampleBatch.Dispose();
            hv_Start.Dispose();
            hv_DLResultBatch.Dispose();
            hv_SampleIndex.Dispose();
            hv_WindowHandles.Dispose();
            hv_Areas.Dispose();
            hv_ClassIndex.Dispose();
            hv_Area.Dispose();
            hv_Row.Dispose();
            hv_Column.Dispose();
            hv_ConnectIndex.Dispose();
            hv_End.Dispose();
            hv_TimeSeg.Dispose();

            comm.Dispose();
            conn.Close();

        }
        #region 识别瑕疵滤光片位置
        double[] getDefLoc(int cameraId, int row, int col, double defX, double defY)
        {
            double YLoc = 0;    //照片Y轴位置
            double XLoc = 0;    //照片X轴位置
            //int defRow = 0;
            //int defCol = 0;

            //计算瑕疵的实际位置
            if (row % 2 == 0)
            {
                switch (cameraId)
                {
                    case 3:
                        YLoc = ((col - 6) * 12.012 - 9) + defY * (12.30 / 512);
                        XLoc = 0 + row * 59 + defX * (15.80 / 512) - 4.1;
                        break;
                    case 1:
                        YLoc = ((col - 11) * 12.012 - 4.2) + defY * (12.30 / 512);
                        XLoc = 18.02 + row * 59 + defX * (15.80 / 512) - 4.1;
                        break;
                    case 4:
                        YLoc = ((col - 2) * 12.012 - 0.85) + defY * (12.30 / 512);
                        XLoc = 32.66 + row * 59 + defX * (15.80 / 512) - 4.1;
                        break;
                    case 2:
                        YLoc = ((col - 7) * 12.012 - 9.35) + defY * (12.30 / 512);
                        XLoc = 47.67 + row * 59 + defX * (15.80 / 512) - 4.1;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (cameraId)
                {
                    case 3:
                        YLoc = ((37 - col) * 12.012 - 8.85) + defY * (12.30 / 512);
                        XLoc = 0 + row * 59 + defX * (15.80 / 512) - 4.1;
                        break;
                    case 1:
                        YLoc = ((31 - col) * 12.012 - 4.45) + defY * (12.30 / 512);
                        XLoc = 18.02 + row * 59 + defX * (15.80 / 512) - 4.1;
                        break;
                    case 4:
                        YLoc = ((40 - col) * 12.012 - 0.6) + defY * (12.30 / 512);
                        XLoc = 32.66 + row * 59 + defX * (15.80 / 512) - 4.1;
                        break;
                    case 2:
                        YLoc = ((35 - col) * 12.012 - 9.8) + defY * (12.30 / 512);
                        XLoc = 47.67 + row * 59 + defX * (15.80 / 512) - 4.1;
                        break;
                    default:
                        break;
                }
            }




            double dx = (((350 - 30 - 0.2) / (11 - 1)) - 30 - 0.2);
            double dy = (((340 - 30 - 0.2) / (10 - 1)) - 30 - 0.2);
            double defCol, defRow;
            defCol = (int)Math.Floor(XLoc / (30 + 0.2 + dx));
            defRow = (int)Math.Floor(YLoc / (30 + 0.2 + dy));
            double[] res = { XLoc, YLoc,defCol,defRow };
            return res;
            ////将有瑕疵的滤光片标记为1
            //optArray[10 - defRow - 1, 11 - defCol - 1] = 1;
            //MessageBox.Show(defRow.ToString());
            //MessageBox.Show(defCol.ToString());
        }
        #endregion

    }

}
