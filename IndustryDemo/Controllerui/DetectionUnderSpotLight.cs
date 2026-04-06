using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using IndustryDemo;

namespace IndustryDemo.Controllerui
{
    class DetectionUnderSpotLight
    {
        public HObject ImageProcess(HObject ho_Image, ref List<string[]> hv_DefectTypeCoordinate)
        {
            try
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
                HObject ho_bumaoDefects, ho_spotDefects;
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
                    HTuple hv_Row_String, hv_Column_String;
                    HOperatorSet.TupleString(hv_Row_bubble, "10.2f", out hv_Row_String);
                    HOperatorSet.TupleString(hv_Column_bubble, "10.2f", out hv_Column_String);
                    for (int i = 0; i < hv_Row_String.Length; i++)
                    {
                        hv_DefectTypeCoordinate.Add(new string[] { "气泡", hv_Row_String[i],
                        hv_Column_String[i] });
                    }

                }
                // 获取布毛坐标
                if ((int)(new HTuple(hv_bumaoNumber.TupleGreater(0))) != 0)
                {
                    HOperatorSet.AreaCenter(ho_bumaoDefects, out hv_Area_bumao, out hv_Row_bumao, out hv_Column_bumao);
                    HTuple hv_Row_String, hv_Column_String;
                    HOperatorSet.TupleString(hv_Row_bumao, "10.2f", out hv_Row_String);
                    HOperatorSet.TupleString(hv_Column_bumao, "10.2f", out hv_Column_String);
                    for (int i = 0; i < hv_Row_String.Length; i++)
                    {
                        hv_DefectTypeCoordinate.Add(new string[] { "布毛", hv_Row_String[i],
                        hv_Column_String[i] });
                    }

                }
                // 获取麻点坐标并通过坐标聚类识别手印
                if ((int)(new HTuple(hv_spotNumber.TupleGreater(0))) != 0)
                {
                    // 获取所有点子的坐标
                    HOperatorSet.AreaCenter(ho_spotDefects, out hv_Area_spot, out hv_Row_spot, out hv_Column_spot);
                    
                    int totalSpots = hv_Row_spot.Length;
                    double distanceThresholdPixels = Global.FingerprintDistanceThresholdMm * Global.PixelsPerMmAvg;
                    
                    // 存储每个点子的坐标和所属簇ID（-1表示未分配）
                    List<double> spotRows = new List<double>();
                    List<double> spotCols = new List<double>();
                    List<int> clusterIds = new List<int>();
                    
                    for (int i = 0; i < totalSpots; i++)
                    {
                        spotRows.Add(hv_Row_spot[i].D);
                        spotCols.Add(hv_Column_spot[i].D);
                        clusterIds.Add(-1); // 初始未分配
                    }
                    
                    // 简单的聚类算法：遍历每个点子，将距离小于阈值的点子归为同一簇
                    int currentClusterId = 0;
                    
                    for (int i = 0; i < totalSpots; i++)
                    {
                        if (clusterIds[i] != -1)
                            continue; // 已分配过
                        
                        // 创建新簇
                        List<int> clusterMembers = new List<int>();
                        clusterMembers.Add(i);
                        clusterIds[i] = currentClusterId;
                        
                        // 扩展簇：找出所有与当前簇中任意点距离小于阈值的点子
                        bool foundNew = true;
                        while (foundNew)
                        {
                            foundNew = false;
                            for (int j = 0; j < totalSpots; j++)
                            {
                                if (clusterIds[j] != -1)
                                    continue; // 已分配
                                
                                // 检查j是否与簇中任意点距离小于阈值
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
                    
                    // 统计每个簇的点子数量并输出
                    Dictionary<int, List<int>> clusters = new Dictionary<int, List<int>>();
                    for (int i = 0; i < totalSpots; i++)
                    {
                        int cid = clusterIds[i];
                        if (!clusters.ContainsKey(cid))
                            clusters[cid] = new List<int>();
                        clusters[cid].Add(i);
                    }
                    
                    // 处理每个簇
                    foreach (var cluster in clusters)
                    {
                        int spotCount = cluster.Value.Count;
                        
                        if (spotCount >= Global.FingerprintMinSpotCount)
                        {
                            // 识别为手印
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
                            // 保留为独立点子
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
                if ((int)((new HTuple((new HTuple(hv_bubbleNumber.TupleEqual(0))).TupleAnd(new HTuple(hv_bumaoNumber.TupleEqual(
                    0))))).TupleAnd(new HTuple(hv_spotNumber.TupleEqual(0)))) != 0)
                {
                    //hv_DefectTypeCoordinate = "合格";
                }
                return ho_bubbleDefects;
            }
            catch
            {
                return null;
            }
            
        }
    }
}
