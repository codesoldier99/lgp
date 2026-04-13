using System;
using HalconDotNet;
using System.Collections.Generic;
using IndustryDemo;

namespace IndustryDemo.Controllerui
{
    public partial class DetectionUnderRingLight
    {
        public void calculate_lines_gauss_parameters(HTuple hv_MaxLineWidth, HTuple hv_Contrast,
      out HTuple hv_Sigma, out HTuple hv_Low, out HTuple hv_High)
        {

            // Local control variables 

            HTuple hv_ContrastHigh = new HTuple(), hv_ContrastLow = new HTuple();
            HTuple hv_HalfWidth = new HTuple(), hv_Help = new HTuple();
            HTuple hv_MaxLineWidth_COPY_INP_TMP = new HTuple(hv_MaxLineWidth);

            // Initialize local and output iconic variables 
            hv_Sigma = new HTuple();
            hv_Low = new HTuple();
            hv_High = new HTuple();
            try
            {
                //Check control parameters
                if ((int)(new HTuple((new HTuple(hv_MaxLineWidth_COPY_INP_TMP.TupleLength()
                    )).TupleNotEqual(1))) != 0)
                {
                    throw new HalconException("Wrong number of values of control parameter: 1");
                }
                if ((int)(((hv_MaxLineWidth_COPY_INP_TMP.TupleIsNumber())).TupleNot()) != 0)
                {
                    throw new HalconException("Wrong type of control parameter: 1");
                }
                if ((int)(new HTuple(hv_MaxLineWidth_COPY_INP_TMP.TupleLessEqual(0))) != 0)
                {
                    throw new HalconException("Wrong value of control parameter: 1");
                }
                if ((int)((new HTuple((new HTuple(hv_Contrast.TupleLength())).TupleNotEqual(
                    1))).TupleAnd(new HTuple((new HTuple(hv_Contrast.TupleLength())).TupleNotEqual(
                    2)))) != 0)
                {
                    throw new HalconException("Wrong number of values of control parameter: 2");
                }
                if ((int)(new HTuple(((((hv_Contrast.TupleIsNumber())).TupleMin())).TupleEqual(
                    0))) != 0)
                {
                    throw new HalconException("Wrong type of control parameter: 2");
                }
                //Set and check ContrastHigh
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_ContrastHigh = hv_Contrast.TupleSelect(
                        0);
                }
                if ((int)(new HTuple(hv_ContrastHigh.TupleLess(0))) != 0)
                {
                    throw new HalconException("Wrong value of control parameter: 2");
                }
                //Set or derive ContrastLow
                if ((int)(new HTuple((new HTuple(hv_Contrast.TupleLength())).TupleEqual(2))) != 0)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ContrastLow = hv_Contrast.TupleSelect(
                            1);
                    }
                }
                else
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ContrastLow = hv_ContrastHigh / 3.0;
                    }
                }
                //Check ContrastLow
                if ((int)(new HTuple(hv_ContrastLow.TupleLess(0))) != 0)
                {
                    throw new HalconException("Wrong value of control parameter: 2");
                }
                if ((int)(new HTuple(hv_ContrastLow.TupleGreater(hv_ContrastHigh))) != 0)
                {
                    throw new HalconException("Wrong value of control parameter: 2");
                }
                //
                //Calculate the parameters Sigma, Low, and High for lines_gauss
                if ((int)(new HTuple(hv_MaxLineWidth_COPY_INP_TMP.TupleLess((new HTuple(3.0)).TupleSqrt()
                    ))) != 0)
                {
                    //Note that LineWidthMax < sqrt(3.0) would result in a Sigma < 0.5,
                    //which does not make any sense, because the corresponding smoothing
                    //filter mask would be of size 1x1.
                    //To avoid this, LineWidthMax is restricted to values greater or equal
                    //to sqrt(3.0) and the contrast values are adapted to reflect the fact
                    //that lines that are thinner than sqrt(3.0) pixels have a lower contrast
                    //in the smoothed image (compared to lines that are sqrt(3.0) pixels wide).
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_ContrastLow = (hv_ContrastLow * hv_MaxLineWidth_COPY_INP_TMP) / ((new HTuple(3.0)).TupleSqrt()
                                );
                            hv_ContrastLow = ExpTmpLocalVar_ContrastLow;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_ContrastHigh = (hv_ContrastHigh * hv_MaxLineWidth_COPY_INP_TMP) / ((new HTuple(3.0)).TupleSqrt()
                                );
                            hv_ContrastHigh = ExpTmpLocalVar_ContrastHigh;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_MaxLineWidth_COPY_INP_TMP = (new HTuple(3.0)).TupleSqrt()
                            ;
                    }
                }
                //Convert LineWidthMax and the given contrast values into the input parameters
                //Sigma, Low, and High required by lines_gauss
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_HalfWidth = hv_MaxLineWidth_COPY_INP_TMP / 2.0;
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Sigma = hv_HalfWidth / ((new HTuple(3.0)).TupleSqrt()
                        );
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Help = ((-2.0 * hv_HalfWidth) / (((new HTuple(6.283185307178)).TupleSqrt()
                        ) * (hv_Sigma.TuplePow(3.0)))) * (((-0.5 * (((hv_HalfWidth / hv_Sigma)).TuplePow(
                        2.0)))).TupleExp());
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_High = ((hv_ContrastHigh * hv_Help)).TupleFabs()
                        ;
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Low = ((hv_ContrastLow * hv_Help)).TupleFabs()
                        ;
                }

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                 throw HDevExpDefaultException;
            }
        }
        public HObject ImageProcess(HObject ho_Image, ref List<string[]> hv_DefectTypeCoordinate)
        {


            // Local iconic variables 

            HObject ho_Regions, ho_ConnectedRegions;
            HObject ho_SelectedRegions, ho_SelectedRegions1, ho_RegionFillUp;
            HObject ho_RegionDilation, ho_RegionErosion, ho_RegionUnion;
            HObject ho_ImageReduced, ho_ImageMean, ho_DarkPixels, ho_RegionDilation1;
            HObject ho_RegionUnion1, ho_RegionDifference1, ho_ImageReduced1;
            HObject ho_Lines, ho_UnionContours, ho_SelectedXLD1, ho_UnionContours3;
            HObject ho_UnionContours4, ho_SelectedContours, ho_SelectedXLD;
            HObject ho_Region, ho_ImageReduced3, ho_ImageMean1, ho_RegionDynThresh;
            HObject ho_ConnectedRegions1, ho_RegionDilation2, ho_RegionDifference;
            HObject ho_ImageReduced2, ho_ObjectSelected1 = null;
            HObject ho_SelectedRegions2;
            HObject ho_RegionClosing, ho_Regions2, ho_ConnectedRegions3;
            HObject ho_bumaoDefects, ho_RegionDilation3, ho_RegionDifference2;
            HObject ho_SelectedRegions3, ho_SelectedRegions4, ho_spotDefects;
            // Local control variables 

            HTuple hv_WindowHandle = new HTuple(), hv_MaxLineWidth = new HTuple();
            HTuple hv_Contrast = new HTuple(), hv_Sigma = new HTuple();
            HTuple hv_Low = new HTuple(), hv_High = new HTuple(), hv_Number2 = new HTuple();
            HTuple hv_Number1 = new HTuple(), hv_messages = new HTuple();
            HTuple hv_Area_pinhole = new HTuple(), hv_Row_pinhole = new HTuple();
            HTuple hv_Column_pinhole = new HTuple(), hv_Row_pinhole_String = new HTuple();
            HTuple hv_Column_pinhole_String = new HTuple(), hv_pinholeCoordinate = new HTuple();
            HTuple hv_pinholeType = new HTuple(), hv_Row_bruise_All = new HTuple();
            HTuple hv_Column_bruise_All = new HTuple(), hv_Index = new HTuple();
            HTuple hv_Row_bruise = new HTuple(), hv_Column_bruise = new HTuple();
            HTuple hv_Row_bruise_String = new HTuple(), hv_Column_bruise_String = new HTuple();
            HTuple hv_bruiseCoordinate = new HTuple(), hv_bruiseType = new HTuple();
            HTuple hv_bumaoNumber = new HTuple(), hv_spotNumber = new HTuple();
            HTuple hv_Area_bumao = new HTuple(), hv_Row_bumao = new HTuple();
            HTuple hv_Column_bumao = new HTuple(), hv_Area_spot = new HTuple();
            HTuple hv_Row_spot = new HTuple(), hv_Column_spot = new HTuple();
            HTuple hv_Type = new HTuple();
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
            HOperatorSet.GenEmptyObj(out ho_Lines);
            HOperatorSet.GenEmptyObj(out ho_UnionContours);
            HOperatorSet.GenEmptyObj(out ho_SelectedXLD1);
            HOperatorSet.GenEmptyObj(out ho_UnionContours3);
            HOperatorSet.GenEmptyObj(out ho_UnionContours4);
            HOperatorSet.GenEmptyObj(out ho_SelectedContours);
            HOperatorSet.GenEmptyObj(out ho_SelectedXLD);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced3);
            HOperatorSet.GenEmptyObj(out ho_ImageMean1);
            HOperatorSet.GenEmptyObj(out ho_RegionDynThresh);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation2);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced2);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelected1);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_Regions2);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions3);
            HOperatorSet.GenEmptyObj(out ho_bumaoDefects);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation3);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference2);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions3);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions4);
            HOperatorSet.GenEmptyObj(out ho_spotDefects);

            //ho_Image.Dispose();
            //HOperatorSet.ReadImage(out ho_Image, "F:/图片/20200908点光+环光/针孔+腐蚀印（待确认）/腐蚀印3-1（待确认）.bmp");
            //**************** 滤光片和料盘分割 ***********************
            //灰度直方图，阈值根据滤光片实际情况进行设置
            ho_Regions.Dispose();
            HOperatorSet.Threshold(ho_Image, out ho_Regions, 0, 30);
            //打散连通域
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_Regions, out ho_ConnectedRegions);
            //使用面积特征选择感兴趣区域：阈值可考虑两帧图像滤光片区域重叠大小设置
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area",
                "and", 500000, 1e+007);
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
            HOperatorSet.DynThreshold(ho_ImageReduced, ho_ImageMean, out ho_DarkPixels, 10,
                "light");
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
            HOperatorSet.Difference(ho_ImageReduced, ho_RegionUnion1, out ho_RegionDifference1
                );
            //获取去除气泡的区域，用于划伤等缺陷的检测
            ho_ImageReduced1.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageReduced, ho_RegionDifference1, out ho_ImageReduced1
                );

            //******************* 划伤、划痕、擦伤、道子检测 (擦伤瑕疵需要优化) ****************************
            hv_MaxLineWidth = 4;
            hv_Contrast = 13;
            ho_Lines.Dispose();
            calculate_lines_gauss_parameters(hv_MaxLineWidth, hv_Contrast, out hv_Sigma,
          out hv_Low, out hv_High);
            HOperatorSet.LinesGauss(ho_ImageReduced1, out ho_Lines, hv_Sigma, hv_Low, hv_High,
                "light", "true", "gaussian", "true");

            //合并位于同一条直线上的轮廓
            ho_UnionContours.Dispose();
            HOperatorSet.UnionCollinearContoursXld(ho_Lines, out ho_UnionContours, 120, 1.5,
                4, 0.7, "attr_keep");
            ho_SelectedXLD1.Dispose();
            HOperatorSet.SelectShapeXld(ho_UnionContours, out ho_SelectedXLD1, "max_diameter",
                "and", 17, 99999);
            ho_UnionContours3.Dispose();
            HOperatorSet.UnionCollinearContoursXld(ho_SelectedXLD1, out ho_UnionContours3,
                200, 10, 24, 0.3, "attr_keep");
            ho_UnionContours4.Dispose();
            HOperatorSet.UnionAdjacentContoursXld(ho_UnionContours3, out ho_UnionContours4,
                5, 1, "attr_keep");
            //外接圆最大直径
            ho_SelectedContours.Dispose();
            HOperatorSet.SelectShapeXld(ho_UnionContours4, out ho_SelectedContours, "max_diameter",
                "and", 22, 99999);
            ho_SelectedXLD.Dispose();
            HOperatorSet.SelectShapeXld(ho_SelectedContours, out ho_SelectedXLD, "area_points",
                "and", 19, 99999);

            //********************* 针孔检测（灰尘仍无法识别）  ****************************
            //xld 转化为 region
            ho_Region.Dispose();
            HOperatorSet.GenRegionContourXld(ho_SelectedXLD, out ho_Region, "filled");
            //膨胀，将整块擦伤/道子/划痕区域提取出来
            //注意阈值不能太小，否则擦伤/道子/划痕区域提取不完全，会对后续其他缺陷检测造成影响
            ho_RegionDilation.Dispose();
            HOperatorSet.DilationRectangle1(ho_Region, out ho_RegionDilation, 150, 150);
            //合并，形成同一个连通域
            ho_RegionUnion.Dispose();
            HOperatorSet.Union1(ho_RegionDilation, out ho_RegionUnion);/**/

            ho_ImageReduced3.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageReduced1, ho_RegionUnion, out ho_ImageReduced3
                );
            ho_ImageMean1.Dispose();
            HOperatorSet.MeanImage(ho_ImageReduced3, out ho_ImageMean1, 15, 15);
            ho_RegionDynThresh.Dispose();
            HOperatorSet.DynThreshold(ho_ImageReduced3, ho_ImageMean1, out ho_RegionDynThresh,
                10, "light");

            ho_ConnectedRegions1.Dispose();
            HOperatorSet.Connection(ho_RegionDynThresh, out ho_ConnectedRegions1);

            ho_RegionDilation2.Dispose();
            HOperatorSet.DilationRectangle1(ho_ConnectedRegions1, out ho_RegionDilation2,
                5, 5);



            //区域相减：减去擦伤/道子/划痕区域，获得的区域后续进行针孔检测
            ho_RegionDifference.Dispose();
            HOperatorSet.Difference(ho_ImageReduced1, ho_RegionUnion, out ho_RegionDifference
                );
            ho_ImageReduced2.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageReduced1, ho_RegionDifference, out ho_ImageReduced2
                );

            ho_ImageMean.Dispose();
            HOperatorSet.MeanImage(ho_ImageReduced2, out ho_ImageMean, 100, 100);
            ho_DarkPixels.Dispose();
            HOperatorSet.DynThreshold(ho_ImageReduced2, ho_ImageMean, out ho_DarkPixels,
                10, "light");
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_DarkPixels, out ho_ConnectedRegions);
            //面积特征
            ho_SelectedRegions1.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions1, "area",
                "and", 10, 650);
            //最大直径特征去除道子
            ho_SelectedRegions2.Dispose();
            HOperatorSet.SelectShape(ho_SelectedRegions1, out ho_SelectedRegions2, "max_diameter",
                "and", 0, 50);

            //********************* 引入点光有效链路：布毛 + 麻点/手印 ****************************
            ho_ImageMean.Dispose();
            HOperatorSet.MeanImage(ho_ImageReduced2, out ho_ImageMean, 50, 50);
            ho_DarkPixels.Dispose();
            HOperatorSet.DynThreshold(ho_ImageReduced2, ho_ImageMean, out ho_DarkPixels, 10,
                "light");
            ho_RegionClosing.Dispose();
            HOperatorSet.Closing(ho_DarkPixels, ho_DarkPixels, out ho_RegionClosing);
            ho_ImageReduced3.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageReduced2, ho_RegionClosing, out ho_ImageReduced3
                );
            ho_Regions2.Dispose();
            HOperatorSet.Threshold(ho_ImageReduced3, out ho_Regions2, 155, 255);
            ho_ConnectedRegions3.Dispose();
            HOperatorSet.Connection(ho_Regions2, out ho_ConnectedRegions3);
            ho_bumaoDefects.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions3, out ho_bumaoDefects, (new HTuple("area")).TupleConcat(
                "roundness"), "and", (new HTuple(50)).TupleConcat(-1), (new HTuple(200000)).TupleConcat(0.67));

            ho_RegionDilation3.Dispose();
            HOperatorSet.DilationCircle(ho_bumaoDefects, out ho_RegionDilation3, 100);
            ho_RegionDifference2.Dispose();
            HOperatorSet.Difference(ho_ImageReduced2, ho_RegionDilation3, out ho_RegionDifference2
                );
            ho_ImageReduced3.Dispose();
            HOperatorSet.ReduceDomain(ho_ImageReduced2, ho_RegionDifference2, out ho_ImageReduced3
                );

            ho_ImageMean.Dispose();
            HOperatorSet.MeanImage(ho_ImageReduced3, out ho_ImageMean, 10, 10);
            ho_DarkPixels.Dispose();
            HOperatorSet.DynThreshold(ho_ImageReduced3, ho_ImageMean, out ho_DarkPixels, 10,
                "dark");
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_DarkPixels, out ho_ConnectedRegions);
            ho_SelectedRegions3.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions3, (new HTuple("area")).TupleConcat(
                "area_holes"), "and", (new HTuple(10)).TupleConcat(0), (new HTuple(130)).TupleConcat(0.0001));
            ho_SelectedRegions4.Dispose();
            HOperatorSet.SelectShape(ho_SelectedRegions3, out ho_SelectedRegions4, "max_diameter",
                "and", 0, 20);
            ho_spotDefects.Dispose();
            HOperatorSet.SelectShape(ho_SelectedRegions4, out ho_spotDefects, "roundness", "and",
                0.5, 1);

            HOperatorSet.CountObj(ho_bumaoDefects, out hv_bumaoNumber);
            HOperatorSet.CountObj(ho_spotDefects, out hv_spotNumber);

            if ((int)(new HTuple(hv_bumaoNumber.TupleGreater(0))) != 0)
            {
                HOperatorSet.AreaCenter(ho_bumaoDefects, out hv_Area_bumao, out hv_Row_bumao,
                    out hv_Column_bumao);
                HTuple hv_Row_String = new HTuple(), hv_Column_String = new HTuple();
                HOperatorSet.TupleString(hv_Row_bumao, "10.2f", out hv_Row_String);
                HOperatorSet.TupleString(hv_Column_bumao, "10.2f", out hv_Column_String);
                for (int i = 0; i < hv_Row_String.Length; i++)
                {
                    hv_DefectTypeCoordinate.Add(new string[] { "布毛", hv_Row_String[i], hv_Column_String[i] });
                }
            }

            if ((int)(new HTuple(hv_spotNumber.TupleGreater(0))) != 0)
            {
                // 复用点光链路中的聚类策略：密集点簇判为手印，稀疏点保留为麻点
                HOperatorSet.AreaCenter(ho_spotDefects, out hv_Area_spot, out hv_Row_spot, out hv_Column_spot);

                int totalSpots = hv_Row_spot.Length;
                double distanceThresholdPixels = Global.FingerprintDistanceThresholdMm * Global.PixelsPerMmAvg;

                List<double> spotRows = new List<double>();
                List<double> spotCols = new List<double>();
                List<int> clusterIds = new List<int>();

                for (int i = 0; i < totalSpots; i++)
                {
                    spotRows.Add(hv_Row_spot[i].D);
                    spotCols.Add(hv_Column_spot[i].D);
                    clusterIds.Add(-1);
                }

                int currentClusterId = 0;
                for (int i = 0; i < totalSpots; i++)
                {
                    if (clusterIds[i] != -1)
                    {
                        continue;
                    }

                    List<int> clusterMembers = new List<int>();
                    clusterMembers.Add(i);
                    clusterIds[i] = currentClusterId;

                    bool foundNew = true;
                    while (foundNew)
                    {
                        foundNew = false;
                        for (int j = 0; j < totalSpots; j++)
                        {
                            if (clusterIds[j] != -1)
                            {
                                continue;
                            }

                            bool isNearCluster = false;
                            foreach (int memberIdx in clusterMembers)
                            {
                                double deltaRow = spotRows[j] - spotRows[memberIdx];
                                double deltaCol = spotCols[j] - spotCols[memberIdx];
                                double distance = Math.Sqrt(deltaRow * deltaRow + deltaCol * deltaCol);

                                if (distance <= distanceThresholdPixels)
                                {
                                    isNearCluster = true;
                                    break;
                                }
                            }

                            if (isNearCluster)
                            {
                                clusterMembers.Add(j);
                                clusterIds[j] = currentClusterId;
                                foundNew = true;
                            }
                        }
                    }

                    currentClusterId++;
                }

                Dictionary<int, List<int>> clusters = new Dictionary<int, List<int>>();
                for (int i = 0; i < totalSpots; i++)
                {
                    int cid = clusterIds[i];
                    if (!clusters.ContainsKey(cid))
                    {
                        clusters[cid] = new List<int>();
                    }
                    clusters[cid].Add(i);
                }

                foreach (var cluster in clusters)
                {
                    int spotCount = cluster.Value.Count;
                    if (spotCount >= Global.FingerprintMinSpotCount)
                    {
                        double avgRow = 0, avgCol = 0;
                        foreach (int idx in cluster.Value)
                        {
                            avgRow += spotRows[idx];
                            avgCol += spotCols[idx];
                        }
                        avgRow /= spotCount;
                        avgCol /= spotCount;

                        hv_DefectTypeCoordinate.Add(new string[] {
                            "手印",
                            avgRow.ToString("F2"),
                            avgCol.ToString("F2")
                        });
                    }
                    else
                    {
                        foreach (int idx in cluster.Value)
                        {
                            hv_DefectTypeCoordinate.Add(new string[] {
                                "麻点",
                                spotRows[idx].ToString("F2"),
                                spotCols[idx].ToString("F2")
                            });
                        }
                    }
                }
            }

            

            HOperatorSet.CountObj(ho_SelectedXLD, out hv_Number2);
            HOperatorSet.CountObj(ho_SelectedRegions2, out hv_Number1);
            if ((int)(new HTuple(hv_Number1.TupleGreater(0))) != 0)
            {
                hv_messages = "There are pinhole in the filter!";

                //获取针孔坐标
                HOperatorSet.AreaCenter(ho_SelectedRegions2, out hv_Area_pinhole, out hv_Row_pinhole,
                    out hv_Column_pinhole);
                HOperatorSet.TupleString(hv_Row_pinhole, "10.2f", out hv_Row_pinhole_String);
                HOperatorSet.TupleString(hv_Column_pinhole, "10.2f", out hv_Column_pinhole_String);
                for (int i = 0; i < hv_Row_pinhole_String.Length; i++)
                {
                    hv_DefectTypeCoordinate.Add(new string[] {"针孔", hv_Row_pinhole_String[i], hv_Column_pinhole_String[i] });
                }
                //hv_DefectTypeCoordinate += hv_pinholeCoordinate.ToString();
                hv_pinholeType = "瑕疵类型：针孔";

            }
            if ((int)(new HTuple(hv_Number2.TupleGreater(0))) != 0)
            {
                hv_messages = "There are bruise in the filter!";

                //获取划痕坐标
                hv_Row_bruise_All = new HTuple();
                hv_Column_bruise_All = new HTuple();
                HTuple end_val139 = hv_Number2;
                HTuple step_val139 = 1;
                for (hv_Index = 1; hv_Index.Continue(end_val139, step_val139); hv_Index = hv_Index.TupleAdd(step_val139))
                {
                    ho_ObjectSelected1.Dispose();
                    HOperatorSet.SelectObj(ho_SelectedXLD, out ho_ObjectSelected1, hv_Index);
                    //获取每个瑕疵所有坐标
                    HOperatorSet.GetContourXld(ho_ObjectSelected1, out hv_Row_bruise, out hv_Column_bruise);
                    //获取每个瑕疵的首尾坐标：[首，尾，首，尾，...，首，尾]
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Row_bruise_All = ((hv_Row_bruise_All.TupleConcat(
                                hv_Row_bruise.TupleSelect(0)))).TupleConcat(hv_Row_bruise.TupleSelect(
                                (new HTuple(hv_Row_bruise.TupleLength())) - 1));
                            hv_Row_bruise_All = ExpTmpLocalVar_Row_bruise_All;
                        }
                    }
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Column_bruise_All = ((hv_Column_bruise_All.TupleConcat(
                                hv_Column_bruise.TupleSelect(0)))).TupleConcat(hv_Column_bruise.TupleSelect(
                                (new HTuple(hv_Column_bruise.TupleLength())) - 1));
                            hv_Column_bruise_All = ExpTmpLocalVar_Column_bruise_All;
                        }
                    }
                }
                HOperatorSet.TupleString(hv_Row_bruise_All, "10.2f", out hv_Row_bruise_String);
                HOperatorSet.TupleString(hv_Column_bruise_All, "10.2f", out hv_Column_bruise_String);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_bruiseCoordinate = ((("(" + hv_Row_bruise_String) + new HTuple(",")) + hv_Column_bruise_String) + ")";
                }
                for (int i = 0; i < hv_Row_bruise_String.Length; i++)
                {
                    hv_DefectTypeCoordinate.Add(new string[] { "划伤", hv_Row_bruise_String[i], hv_Column_bruise_String[i] });
                }
                //hv_DefectTypeCoordinate += "瑕疵类型：划伤" + hv_bruiseCoordinate.ToString();
                //hv_bruiseType = "瑕疵类型：划伤";
            }
            if ((int)((new HTuple((new HTuple((new HTuple(hv_Number1.TupleEqual(0))).TupleAnd(
                new HTuple(hv_Number2.TupleEqual(0))))).TupleAnd(new HTuple(hv_bumaoNumber.TupleEqual(
                0))))).TupleAnd(new HTuple(hv_spotNumber.TupleEqual(0)))) != 0)
            {
                hv_messages = "There is no flaw in the filter!";
                hv_Type = "合格";
            }

            return ho_SelectedXLD;

        }
    }
}
