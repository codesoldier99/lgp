using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndustryDemo.Controllerui
{
    /// <summary>
    /// 点子聚类识别手印辅助类
    /// 使用DBSCAN算法识别聚集的点子并标记为手印
    /// </summary>
    public class SpotClusteringHelper
    {
        private double distanceThresholdMm;  // 距离阈值（毫米）
        private int minClusterSize;          // 最小聚类点数
        private double pixelsPerMm;          // 像素到毫米的转换比例

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="distThresholdMm">点子间距阈值（毫米），默认0.3mm</param>
        /// <param name="minSize">形成手印的最小点子数量，默认20个</param>
        public SpotClusteringHelper(double distThresholdMm = 0.3, int minSize = 20)
        {
            this.distanceThresholdMm = distThresholdMm;
            this.minClusterSize = minSize;
            // 使用平均像素比例
            this.pixelsPerMm = Global.PixelsPerMmAvg;
        }

        /// <summary>
        /// 点子数据结构
        /// </summary>
        private class SpotPoint
        {
            public double Row { get; set; }      // 行坐标（像素）
            public double Column { get; set; }   // 列坐标（像素）
            public int ClusterId { get; set; }   // 聚类ID，-1表示未分类
            public bool IsVisited { get; set; }  // 是否已访问
            public string OriginalData { get; set; } // 原始数据

            public SpotPoint(double row, double column, string originalData)
            {
                Row = row;
                Column = column;
                ClusterId = -1;
                IsVisited = false;
                OriginalData = originalData;
            }
        }

        /// <summary>
        /// 处理点子聚类，识别手印
        /// </summary>
        /// <param name="spotList">输入的点子列表，格式：[类型, 行坐标, 列坐标]</param>
        /// <param name="fingerprintDefects">输出的手印列表</param>
        /// <param name="remainingSpots">输出的独立点子列表</param>
        public void ProcessSpotClustering(
            List<string[]> spotList,
            out List<string[]> fingerprintDefects,
            out List<string[]> remainingSpots)
        {
            fingerprintDefects = new List<string[]>();
            remainingSpots = new List<string[]>();

            if (spotList == null || spotList.Count == 0)
            {
                return;
            }

            // 转换为SpotPoint对象列表
            List<SpotPoint> points = new List<SpotPoint>();
            foreach (var spot in spotList)
            {
                if (spot.Length >= 3)
                {
                    double row, column;
                    if (double.TryParse(spot[1], out row) && double.TryParse(spot[2], out column))
                    {
                        string originalData = string.Join(",", spot);
                        points.Add(new SpotPoint(row, column, originalData));
                    }
                }
            }

            if (points.Count == 0)
            {
                return;
            }

            // 执行DBSCAN聚类算法
            int clusterId = 0;
            double distanceThresholdPixels = distanceThresholdMm * pixelsPerMm;

            // 调试信息（可在运行时查看）
            System.Diagnostics.Debug.WriteLine($"聚类参数: 距离阈值={distanceThresholdMm}mm ({distanceThresholdPixels}像素), 最小点数={minClusterSize}, 总点数={points.Count}");

            foreach (var point in points)
            {
                if (point.IsVisited)
                    continue;

                point.IsVisited = true;

                // 找到邻域内的所有点（不包含自己）
                List<SpotPoint> neighbors = GetNeighbors(point, points, distanceThresholdPixels);

                // DBSCAN: 如果邻域点数+自己 >= 最小点数，则形成核心点
                if (neighbors.Count + 1 < minClusterSize)
                {
                    // 不是核心点，暂不标记聚类ID（可能后续被其他核心点吸收）
                    continue;
                }

                // 创建新聚类
                point.ClusterId = clusterId;
                ExpandCluster(point, neighbors, clusterId, points, distanceThresholdPixels);
                clusterId++;
            }

            System.Diagnostics.Debug.WriteLine($"聚类结果: 共形成 {clusterId} 个聚类");

            // 分组统计聚类
            Dictionary<int, List<SpotPoint>> clusters = new Dictionary<int, List<SpotPoint>>();
            foreach (var point in points)
            {
                if (point.ClusterId >= 0)
                {
                    if (!clusters.ContainsKey(point.ClusterId))
                    {
                        clusters[point.ClusterId] = new List<SpotPoint>();
                    }
                    clusters[point.ClusterId].Add(point);
                }
            }

            // 处理聚类结果
            foreach (var cluster in clusters)
            {
                int clusterSize = cluster.Value.Count;
                System.Diagnostics.Debug.WriteLine($"聚类 {cluster.Key}: 包含 {clusterSize} 个点子");

                if (clusterSize >= minClusterSize)
                {
                    // 满足条件，识别为手印
                    // 计算聚类中心坐标
                    double avgRow = cluster.Value.Average(p => p.Row);
                    double avgColumn = cluster.Value.Average(p => p.Column);

                    fingerprintDefects.Add(new string[] 
                    { 
                        "手印", 
                        avgRow.ToString("F2"), 
                        avgColumn.ToString("F2") 
                    });
                    System.Diagnostics.Debug.WriteLine($"  -> 识别为手印，中心坐标: ({avgRow:F2}, {avgColumn:F2})");
                }
                else
                {
                    // 不满足条件，保留为独立点子
                    foreach (var point in cluster.Value)
                    {
                        remainingSpots.Add(new string[] 
                        { 
                            "麻点", 
                            point.Row.ToString("F2"), 
                            point.Column.ToString("F2") 
                        });
                    }
                    System.Diagnostics.Debug.WriteLine($"  -> 不满足条件，保留为独立点子");
                }
            }

            System.Diagnostics.Debug.WriteLine($"最终结果: {fingerprintDefects.Count} 个手印, {remainingSpots.Count} 个独立点子");

            // 未分类的点（噪声点）作为独立点子
            foreach (var point in points)
            {
                if (point.ClusterId == -1)
                {
                    remainingSpots.Add(new string[] 
                    { 
                        "麻点", 
                        point.Row.ToString("F2"), 
                        point.Column.ToString("F2") 
                    });
                }
            }
        }

        /// <summary>
        /// 获取邻域内的所有点
        /// </summary>
        private List<SpotPoint> GetNeighbors(SpotPoint centerPoint, List<SpotPoint> allPoints, double distanceThreshold)
        {
            List<SpotPoint> neighbors = new List<SpotPoint>();

            foreach (var point in allPoints)
            {
                if (point == centerPoint)
                    continue;

                double distance = CalculateDistance(centerPoint, point);
                if (distance <= distanceThreshold)
                {
                    neighbors.Add(point);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// 扩展聚类
        /// </summary>
        private void ExpandCluster(SpotPoint point, List<SpotPoint> neighbors, int clusterId, 
            List<SpotPoint> allPoints, double distanceThreshold)
        {
            Queue<SpotPoint> queue = new Queue<SpotPoint>();
            foreach (var neighbor in neighbors)
            {
                queue.Enqueue(neighbor);
            }

            while (queue.Count > 0)
            {
                SpotPoint currentPoint = queue.Dequeue();

                if (!currentPoint.IsVisited)
                {
                    currentPoint.IsVisited = true;

                    // 找到当前点的邻域
                    List<SpotPoint> currentNeighbors = GetNeighbors(currentPoint, allPoints, distanceThreshold);

                    // 如果邻域足够大（包含自己），继续扩展
                    if (currentNeighbors.Count + 1 >= minClusterSize)
                    {
                        foreach (var neighbor in currentNeighbors)
                        {
                            if (!neighbor.IsVisited)
                            {
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }

                // 标记为当前聚类
                if (currentPoint.ClusterId == -1)
                {
                    currentPoint.ClusterId = clusterId;
                }
            }
        }

        /// <summary>
        /// 计算两点之间的欧氏距离（像素）
        /// </summary>
        private double CalculateDistance(SpotPoint p1, SpotPoint p2)
        {
            double deltaRow = p1.Row - p2.Row;
            double deltaColumn = p1.Column - p2.Column;
            return Math.Sqrt(deltaRow * deltaRow + deltaColumn * deltaColumn);
        }
    }
}
