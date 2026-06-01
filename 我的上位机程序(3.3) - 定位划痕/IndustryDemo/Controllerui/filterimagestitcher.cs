using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using HalconDotNet;
using MySqlHelper = IndustryDemo.Controllerui.MySqlHelper;

namespace IndustryDemo.Controllerui
{
    /// <summary>
    /// 滤光片图片拼接类
    /// 功能：将属于同一张滤光片的照片按照相机顺序和picY大小拼接成完整图片
    /// 支持正面（FRONT）和背面（BACK）两种逻辑类型
    /// </summary>
    public class FilterImageStitcher
    {
        // 正面相机顺序（从左往右）：2, 4, 1, 3
        private static readonly int[] FrontCameraOrder = { 2, 4, 1, 3 };

        // 背面相机顺序（从左往右）：8, 6, 7, 5
        private static readonly int[] BackCameraOrder = { 8, 6, 7, 5 };

        /// <summary>
        /// 图片信息结构
        /// </summary>
        public class ImageInfo
        {
            public string PicName { get; set; }      // 图片名称，格式：cameraId-row-col
            public int CameraId { get; set; }        // 相机ID
            public int Row { get; set; }             // 行号
            public int Col { get; set; }             // 列号
            public double PicY { get; set; }         // 从pictureLocation表获取的picY值
            public string ImagePath { get; set; }    // 图片完整路径
        }

        /// <summary>
        /// 垂直偏移配置（相机ID -> 偏移像素数）
        /// 正偏移：上移（在下方添加黑色区域）
        /// 负偏移：裁剪（从顶部裁剪）
        /// </summary>
        public class VerticalOffsetConfig
        {
            public Dictionary<int, int> Offsets { get; set; } = new Dictionary<int, int>();
        }

        /// <summary>
        /// 拼接指定滤光片的所有照片
        /// </summary>
        /// <param name="posX">虚拟盘行位置</param>
        /// <param name="posY">虚拟盘列位置</param>
        /// <param name="qrCode">二维码</param>
        /// <param name="detectiontime">检测时间</param>
        /// <param name="lightSource">光源类型（ring或spot）</param>
        /// <param name="offsetConfig">垂直偏移配置，如果为null则使用默认值0</param>
        /// <param name="outputPath">输出拼接后图片的保存路径</param>
        /// <param name="maxDisplayWidth">最大显示宽度（像素），如果图片超过此宽度会自动缩放，默认1920</param>
        /// <param name="maxDisplayHeight">最大显示高度（像素），如果图片超过此高度会自动缩放，默认1080</param>
        /// <returns>拼接后的图片对象，失败返回null</returns>
        public static HObject StitchFilterImages(int posX, int posY, string qrCode, string detectiontime,
            string lightSource = "ring", VerticalOffsetConfig offsetConfig = null, string outputPath = null,
            int maxDisplayWidth = 1920, int maxDisplayHeight = 1080)
        {
            try
            {
                // 步骤1：查询属于同一滤光片的所有照片
                List<ImageInfo> imageList = GetFilterImages(posX, posY, qrCode, detectiontime, lightSource);

                if (imageList == null || imageList.Count == 0)
                {
                    return null;
                }

                // 步骤2：从pictureLocation表获取每个图片的picY值
                GetPicYFromDatabase(imageList);

                // 步骤3：判断是正面还是背面（根据相机ID判断）
                bool isBackSide = DetermineSide(imageList);

                // 步骤4：按相机分组并拼接
                HObject stitchedImage = StitchByCameraGroups(imageList, isBackSide, offsetConfig);

                // 步骤5：如果图片太大，进行缩放以适应显示
                if (stitchedImage != null && stitchedImage.IsInitialized())
                {
                    HTuple width, height;
                    HOperatorSet.GetImageSize(stitchedImage, out width, out height);

                    // 如果图片尺寸超过最大显示尺寸，进行缩放
                    if (width.I > maxDisplayWidth || height.I > maxDisplayHeight)
                    {
                        double scaleX = (double)maxDisplayWidth / width.I;
                        double scaleY = (double)maxDisplayHeight / height.I;
                        double scale = Math.Min(scaleX, scaleY); // 使用较小的缩放比例，保持宽高比

                        int newWidth = (int)(width.I * scale);
                        int newHeight = (int)(height.I * scale);

                        HObject scaledImage = new HObject();
                        HOperatorSet.ZoomImageSize(stitchedImage, out scaledImage, newWidth, newHeight, "constant");
                        stitchedImage.Dispose();
                        stitchedImage = scaledImage;
                    }
                }

                // 步骤6：如果指定了输出路径，保存拼接后的图片
                if (!string.IsNullOrEmpty(outputPath) && stitchedImage != null)
                {
                    SaveImage(stitchedImage, outputPath);
                }

                return stitchedImage;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"拼接图片时发生错误：{ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// 判断是正面还是背面（根据相机ID判断）
        /// 如果所有相机都是1-4，则为正面；如果所有相机都是5-8，则为背面
        /// </summary>
        private static bool DetermineSide(List<ImageInfo> imageList)
        {
            bool hasFrontCamera = imageList.Any(img => img.CameraId >= 1 && img.CameraId <= 4);
            bool hasBackCamera = imageList.Any(img => img.CameraId >= 5 && img.CameraId <= 8);

            // 如果同时有正面和背面相机，默认使用正面逻辑
            if (hasFrontCamera && hasBackCamera)
            {
                return false; // 正面
            }

            return hasBackCamera; // 如果有背面相机且没有正面相机，则为背面
        }

        /// <summary>
        /// 从文件系统扫描并查询属于同一滤光片的所有照片
        /// 不再依赖数据库中的defection表，而是扫描所有图片文件，然后通过getDefLoc逻辑判断归属
        /// </summary>
        private static List<ImageInfo> GetFilterImages(int posX, int posY, string qrCode, string detectiontime, string lightSource)
        {
            List<ImageInfo> imageList = new List<ImageInfo>();
            string err;

            // 确定要扫描的相机ID（根据posX和posY，可能是正面或背面）
            // 先尝试正面相机（1-4），如果找不到图片，再尝试背面相机（5-8）
            int[] cameraIdsToScan = { 1, 2, 3, 4, 5, 6, 7, 8 };

            // 扫描所有相机目录，找出所有图片文件
            // 优先使用 D:/{qrCode}/{Global.detectiontime_test}/camera{k}/ring 路径
            foreach (int cameraId in cameraIdsToScan)
            {
                // 优先使用指定的路径格式：D:/{qrCode}/{Global.detectiontime_test}/camera{k}/ring
                string[] possibleBasePaths = new string[]
                {
                    $"D:/{qrCode}/{Global.detectiontime_test}/camera{cameraId}/{lightSource}",
                    $"D:/{qrCode}/{detectiontime}/camera{cameraId}/{lightSource}",
                    $"G://{qrCode}/{Global.detectiontime_test}/camera{cameraId}/{lightSource}",
                    $"G://{qrCode}/{detectiontime}/camera{cameraId}/{lightSource}"
                };

                string basePath = null;
                foreach (string path in possibleBasePaths)
                {
                    if (Directory.Exists(path))
                    {
                        basePath = path;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(basePath))
                {
                    continue; // 跳过不存在的目录
                }

                // 扫描该目录下的所有.bmp文件
                string[] imageFiles = Directory.GetFiles(basePath, "*.bmp", SearchOption.TopDirectoryOnly);

                foreach (string imagePath in imageFiles)
                {
                    // 解析文件名（格式：cameraId-row-col.bmp）
                    string fileName = Path.GetFileNameWithoutExtension(imagePath);
                    string[] parts = fileName.Split('-');

                    if (parts.Length >= 3)
                    {
                        int fileCameraId = Convert.ToInt32(parts[0]);
                        int rowNum = Convert.ToInt32(parts[1]);
                        int colNum = Convert.ToInt32(parts[2]);

                        // 只处理当前相机的图片
                        if (fileCameraId != cameraId)
                        {
                            continue;
                        }

                        // 使用getDefLoc逻辑判断该图片是否属于目标滤光片
                        // defX=0, defY=0 因为参考点在照片右下角
                        int[] filterPos = CalculateFilterPosition(fileCameraId, rowNum, colNum, 0, 0);

                        if (filterPos != null && filterPos[0] == posX && filterPos[1] == posY)
                        {
                            // 该图片属于目标滤光片
                            ImageInfo info = new ImageInfo
                            {
                                PicName = fileName,
                                CameraId = fileCameraId,
                                Row = rowNum,
                                Col = colNum,
                                ImagePath = imagePath
                            };
                            imageList.Add(info);
                        }
                    }
                }
            }

            return imageList;
        }

        /// <summary>
        /// 根据图片信息计算其所属的滤光片位置（posX, posY）
        /// 使用与detection.cs中getDefLoc相同的逻辑
        /// </summary>
        private static int[] CalculateFilterPosition(int cameraId, int row, int col, double defX, double defY)
        {
            try
            {
                string err;
                // 从pictureLocation表获取标准位置
                DataTable dt2 = MySqlHelper.GetDataTable(out err, $"SELECT * FROM pictureLocation WHERE picName='{cameraId}-{row}-{col}'");

                if (!string.IsNullOrEmpty(err) || dt2 == null || dt2.Rows.Count == 0)
                {
                    return null; // 无法获取标准位置，跳过该图片
                }

                double standardX = Convert.ToDouble(dt2.Rows[0][1]); // picX
                double standardY = Convert.ToDouble(dt2.Rows[0][2]); // picY

                double XLoc = 0;
                double YLoc = 0;

                // 根据cameraId和row计算XLoc和YLoc（使用与detection.cs相同的逻辑）
                if (cameraId <= 4)
                {
                    // 正面相机
                    double a_offset = 0, b_offset = 0;
                    // 根据row选择偏移量（使用DetectionWithDL2类中的静态变量）
                    switch (row)
                    {
                        case 0: a_offset = DetectionWithDL2.a0; b_offset = DetectionWithDL2.b0; break;
                        case 1: a_offset = DetectionWithDL2.a1; b_offset = DetectionWithDL2.b1; break;
                        case 2: a_offset = DetectionWithDL2.a2; b_offset = DetectionWithDL2.b2; break;
                        case 3: a_offset = DetectionWithDL2.a3; b_offset = DetectionWithDL2.b3; break;
                        case 4: a_offset = DetectionWithDL2.a4; b_offset = DetectionWithDL2.b4; break;
                        case 5: a_offset = DetectionWithDL2.a5; b_offset = DetectionWithDL2.b5; break;
                    }

                    XLoc = standardX + defY * 0.03125 + a_offset;
                    YLoc = standardY + defX * 0.02344 + b_offset;
                }
                else
                {
                    // 背面相机
                    double A_offset = 0, B_offset = 0;
                    // 根据row选择偏移量（使用DetectionWithDL2类中的静态变量）
                    switch (row)
                    {
                        case 0: A_offset = DetectionWithDL2.A0; B_offset = DetectionWithDL2.B0; break;
                        case 1: A_offset = DetectionWithDL2.A1; B_offset = DetectionWithDL2.B1; break;
                        case 2: A_offset = DetectionWithDL2.A2; B_offset = DetectionWithDL2.B2; break;
                        case 3: A_offset = DetectionWithDL2.A3; B_offset = DetectionWithDL2.B3; break;
                        case 4: A_offset = DetectionWithDL2.A4; B_offset = DetectionWithDL2.B4; break;
                        case 5: A_offset = DetectionWithDL2.A5; B_offset = DetectionWithDL2.B5; break;
                    }

                    XLoc = standardX - defY * 0.03125 + A_offset;
                    YLoc = standardY + defX * 0.02344 + B_offset;
                }

                // 计算虚拟盘位置（只支持方形，去除圆形判断）
                double dx, dy, defCol, defRow;

                // 只处理方形滤光片
                dx = (((350 - Global.length - 0.2) / (Global.optLine - 1)) - Global.length - 0.2);
                dy = (((340 - Global.width - 0.2) / (Global.optRow - 1)) - Global.width - 0.2);
                defCol = Math.Floor(XLoc / (Global.length + 0.2 + dx));
                defRow = Math.Floor(YLoc / (Global.width + 0.2 + dy));

                // 处理参考点位于滤光片之间托盘上的情况
                double unit_size_y = Global.width + 0.2 + dy;
                double relative_y = YLoc - defRow * unit_size_y;

                // 如果relative_y > width 且 < unit_size_y，说明在间隙中，归属到下一行
                if (relative_y > Global.width && relative_y < unit_size_y)
                {
                    defRow = defRow + 1;
                }

                // 转换为数组索引（与detection.cs中的逻辑一致）
                if (defRow >= 0 && defRow < Global.optRow && defCol >= 0 && defCol < Global.optLine)
                {
                    int arrayRow = Global.optRow - (int)defRow - 1;
                    int arrayCol = Global.optLine - (int)defCol - 1;

                    return new int[] { arrayRow, arrayCol };
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 查找图片文件路径
        /// 优先使用 D:/{qrCode}/{Global.detectiontime_test}/camera{k}/ring 路径
        /// </summary>
        private static string FindImagePath(string qrCode, string detectiontime, int cameraId, string lightSource, string picName)
        {
            // 优先使用指定的路径格式：D:/{qrCode}/{Global.detectiontime_test}/camera{k}/ring
            string[] possiblePaths = new string[]
            {
                $"D:/{qrCode}/{Global.detectiontime_test}/camera{cameraId}/{lightSource}/{picName}.bmp",
                $"D:/{qrCode}/{detectiontime}/camera{cameraId}/{lightSource}/{picName}.bmp",
                $"G://{qrCode}/{Global.detectiontime_test}/camera{cameraId}/{lightSource}/{picName}.bmp",
                $"G://{qrCode}/{detectiontime}/camera{cameraId}/{lightSource}/{picName}.bmp"
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// 从pictureLocation表获取每个图片的picY值
        /// </summary>
        private static void GetPicYFromDatabase(List<ImageInfo> imageList)
        {
            string err;

            foreach (ImageInfo info in imageList)
            {
                // 查询pictureLocation表获取picY值（picY在表的第3列，索引为2）
                string sql = $"SELECT * FROM pictureLocation WHERE picName='{info.PicName}'";
                DataTable dt = MySqlHelper.GetDataTable(out err, sql);

                if (!string.IsNullOrEmpty(err))
                {
                    continue;
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    // picY在pictureLocation表的第3列（索引为2），即dt2.Rows[0][2]
                    if (dt.Columns.Count > 2 && dt.Rows[0][2] != DBNull.Value)
                    {
                        info.PicY = Convert.ToDouble(dt.Rows[0][2]);
                    }
                }
            }
        }

        /// <summary>
        /// 按相机分组并拼接图片
        /// 逻辑：1. 按相机分组；2. 每个相机的图片按picY排序并垂直拼接；3. 对每列应用偏移；4. 横向拼接
        /// </summary>
        private static HObject StitchByCameraGroups(List<ImageInfo> imageList, bool isBackSide, VerticalOffsetConfig offsetConfig)
        {
            // 确定相机顺序
            int[] cameraOrder = isBackSide ? BackCameraOrder : FrontCameraOrder;

            // 按相机分组
            Dictionary<int, List<ImageInfo>> cameraGroups = new Dictionary<int, List<ImageInfo>>();
            foreach (ImageInfo info in imageList)
            {
                if (!cameraGroups.ContainsKey(info.CameraId))
                {
                    cameraGroups[info.CameraId] = new List<ImageInfo>();
                }
                cameraGroups[info.CameraId].Add(info);
            }

            // 对每个相机的图片按文件名末尾数字倒序排列（与Python代码一致）
            // Python代码：sorted(file_list, key=lambda x: extract_last_number(x), reverse=True)
            foreach (var group in cameraGroups.Values)
            {
                // 按col值从大到小排序（对应Python代码中的reverse=True）
                // col值就是文件名末尾的数字
                group.Sort((img1, img2) => img2.Col.CompareTo(img1.Col));
            }

            // 为每个相机创建垂直拼接的列
            Dictionary<int, HObject> columns = new Dictionary<int, HObject>();

            foreach (int cameraId in cameraOrder)
            {
                if (!cameraGroups.ContainsKey(cameraId))
                {
                    continue; // 跳过没有图片的相机
                }

                List<ImageInfo> cameraImages = cameraGroups[cameraId];

                // 加载并处理每张图片
                List<HObject> processedImages = new List<HObject>();

                foreach (ImageInfo info in cameraImages)
                {
                    HObject image = new HObject();
                    HOperatorSet.ReadImage(out image, info.ImagePath);

                    // 根据逻辑类型处理图片
                    HObject processedImage = new HObject();
                    if (isBackSide)
                    {
                        // 背面逻辑：上下翻转
                        HOperatorSet.MirrorImage(image, out processedImage, "row");
                    }
                    else
                    {
                        // 正面逻辑：旋转180度
                        HOperatorSet.RotateImage(image, out processedImage, 180, "constant");
                    }

                    processedImages.Add(processedImage);
                    image.Dispose();
                }

                // 垂直拼接该相机的所有图片
                if (processedImages.Count > 0)
                {
                    HObject columnImage = VerticalStitch(processedImages);
                    columns[cameraId] = columnImage;

                    // 释放中间图片
                    foreach (HObject img in processedImages)
                    {
                        if (img != null && img.IsInitialized())
                        {
                            img.Dispose();
                        }
                    }
                }
            }

            // 应用垂直偏移
            Dictionary<int, HObject> shiftedColumns = new Dictionary<int, HObject>();
            foreach (var kvp in columns)
            {
                int cameraId = kvp.Key;
                HObject column = kvp.Value;

                int offset = 0;
                if (offsetConfig != null && offsetConfig.Offsets.ContainsKey(cameraId))
                {
                    offset = offsetConfig.Offsets[cameraId];
                }

                HObject shiftedColumn = ApplyVerticalOffset(column, offset, isBackSide);
                shiftedColumns[cameraId] = shiftedColumn;
                column.Dispose();
            }

            // 横向拼接所有列
            HObject finalImage = HorizontalStitch(shiftedColumns, cameraOrder);

            // 释放列图片
            foreach (var img in shiftedColumns.Values)
            {
                if (img != null && img.IsInitialized())
                {
                    img.Dispose();
                }
            }

            return finalImage;
        }

        /// <summary>
        /// 垂直拼接多张图片
        /// </summary>
        private static HObject VerticalStitch(List<HObject> images)
        {
            if (images == null || images.Count == 0)
            {
                return null;
            }

            if (images.Count == 1)
            {
                HObject result = new HObject();
                HOperatorSet.CopyImage(images[0], out result);
                return result;
            }

            // 创建图像元组
            HObject imageTuple = new HObject();
            HOperatorSet.GenEmptyObj(out imageTuple);

            foreach (HObject img in images)
            {
                HOperatorSet.ConcatObj(imageTuple, img, out imageTuple);
            }

            // 垂直拼接
            HObject stitched = new HObject();
            HOperatorSet.TileImages(imageTuple, out stitched, images.Count, "vertical");

            imageTuple.Dispose();

            return stitched;
        }

        /// <summary>
        /// 应用垂直偏移
        /// 注意：由于Halcon API限制，偏移功能暂时简化处理
        /// 正偏移：在底部添加黑色区域
        /// 负偏移：从顶部裁剪
        /// </summary>
        private static HObject ApplyVerticalOffset(HObject image, int offset, bool isBackSide)
        {
            if (offset == 0)
            {
                HObject result = new HObject();
                HOperatorSet.CopyImage(image, out result);
                return result;
            }

            HTuple width, height;
            HOperatorSet.GetImageSize(image, out width, out height);

            if (isBackSide)
            {
                // 背面逻辑：支持正负偏移
                if (offset >= 0)
                {
                    // 正偏移：在下方添加黑色区域（视觉上移）
                    // 创建更大的黑色背景图像，高度为height + offset
                    HObject newImage = new HObject();
                    HOperatorSet.GenImageConst(out newImage, "byte", width, height + offset);

                    // 使用TileImages将原图和新图拼接（原图在上，黑色区域在下）
                    // 但由于需要将原图放在顶部，我们使用更简单的方法：
                    // 创建一个包含原图和黑色区域的元组，然后垂直拼接
                    HObject imageTuple = new HObject();
                    HOperatorSet.GenEmptyObj(out imageTuple);
                    HOperatorSet.ConcatObj(imageTuple, image, out imageTuple);

                    // 创建黑色填充图像
                    HObject blackImage = new HObject();
                    HOperatorSet.GenImageConst(out blackImage, "byte", width, offset);
                    HOperatorSet.ConcatObj(imageTuple, blackImage, out imageTuple);

                    // 垂直拼接
                    HObject finalImage = new HObject();
                    HOperatorSet.TileImages(imageTuple, out finalImage, 2, "vertical");

                    imageTuple.Dispose();
                    blackImage.Dispose();
                    newImage.Dispose();

                    return finalImage;
                }
                else
                {
                    // 负偏移：从顶部裁剪
                    int absOffset = Math.Abs(offset);
                    if (absOffset >= height.I)
                    {
                        // 裁剪量过大，返回原图
                        HObject result = new HObject();
                        HOperatorSet.CopyImage(image, out result);
                        return result;
                    }
                    HObject cropped = new HObject();
                    HOperatorSet.CropRectangle1(image, out cropped, absOffset, 0, height - 1, width - 1);
                    return cropped;
                }
            }
            else
            {
                // 正面逻辑：只支持正偏移
                if (offset >= 0)
                {
                    // 正偏移：在下方添加黑色区域（视觉上移）
                    // 创建黑色填充图像
                    HObject blackImage = new HObject();
                    HOperatorSet.GenImageConst(out blackImage, "byte", width, offset);

                    // 将原图和黑色区域垂直拼接
                    HObject imageTuple = new HObject();
                    HOperatorSet.GenEmptyObj(out imageTuple);
                    HOperatorSet.ConcatObj(imageTuple, image, out imageTuple);
                    HOperatorSet.ConcatObj(imageTuple, blackImage, out imageTuple);

                    HObject finalImage = new HObject();
                    HOperatorSet.TileImages(imageTuple, out finalImage, 2, "vertical");

                    imageTuple.Dispose();
                    blackImage.Dispose();

                    return finalImage;
                }
                else
                {
                    // 正面逻辑不支持负偏移，返回原图
                    HObject result = new HObject();
                    HOperatorSet.CopyImage(image, out result);
                    return result;
                }
            }
        }

        /// <summary>
        /// 横向拼接多列图片
        /// </summary>
        private static HObject HorizontalStitch(Dictionary<int, HObject> columns, int[] cameraOrder)
        {
            List<HObject> orderedColumns = new List<HObject>();

            foreach (int cameraId in cameraOrder)
            {
                if (columns.ContainsKey(cameraId))
                {
                    orderedColumns.Add(columns[cameraId]);
                }
            }

            if (orderedColumns.Count == 0)
            {
                return null;
            }

            if (orderedColumns.Count == 1)
            {
                HObject result = new HObject();
                HOperatorSet.CopyImage(orderedColumns[0], out result);
                return result;
            }

            // 创建图像元组
            HObject imageTuple = new HObject();
            HOperatorSet.GenEmptyObj(out imageTuple);

            foreach (HObject img in orderedColumns)
            {
                HOperatorSet.ConcatObj(imageTuple, img, out imageTuple);
            }

            // 横向拼接
            HObject stitched = new HObject();
            HOperatorSet.TileImages(imageTuple, out stitched, orderedColumns.Count, "horizontal");

            imageTuple.Dispose();

            return stitched;
        }

        /// <summary>
        /// 保存图片到指定路径
        /// </summary>
        private static void SaveImage(HObject image, string outputPath)
        {
            try
            {
                // 确保输出目录存在
                string directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 保存图片
                HOperatorSet.WriteImage(image, "bmp", 0, outputPath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"保存图片时发生错误：{ex.Message}");
            }
        }
    }
}
