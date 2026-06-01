using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using HalconDotNet;
using MySql.Data.MySqlClient;

namespace IndustryDemo.Controllerui
{
    public partial class HDevelopExport
    {
#if !(NO_EXPORT_MAIN || NO_EXPORT_APP_MAIN)
        public HDevelopExport()
        {
            // Default settings used in HDevelop
            HOperatorSet.SetSystem("width", 512);
            HOperatorSet.SetSystem("height", 512);
            if (HalconAPI.isWindows)
                HOperatorSet.SetSystem("use_window_thread", "true");
            //action();
        }
#endif

        // Procedures 
        // External procedures 
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

        // Chapter: File / Misc
        // Short Description: Parse a filename into directory, base filename, and extension 
        public void parse_filename(HTuple hv_FileName, out HTuple hv_BaseName, out HTuple hv_Extension,
            out HTuple hv_Directory)
        {



            // Local control variables 

            HTuple hv_DirectoryTmp = new HTuple(), hv_Substring = new HTuple();
            // Initialize local and output iconic variables 
            hv_BaseName = new HTuple();
            hv_Extension = new HTuple();
            hv_Directory = new HTuple();
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

#if !NO_EXPORT_MAIN
        // Main procedure 
        public (double, double, double, double, double, double, double, double, double, double, double, double) ProcessImage(string picDir)
        {
            string x, y;
            double pix, piy;
            double x0 = 0, y0 = 0, x1 = 0, y1 = 0, x2 = 0, y2 = 0, x3 = 0, y3 = 0, x4 = 0, y4 = 0, x5 = 0, y5 = 0;
            double c0 = 0;
            double d0 = 0;
            double e0 = 0;
            double f0 = 0;
            double g0 = 0;
            double h0 = 0;
            double c1 = 0;
            double d1 = 0;
            double e1 = 0;
            double f1 = 0;
            double g1 = 0;
            double h1 = 0;
            double c2 = 0;
            double d2 = 0;
            double e2 = 0;
            double f2 = 0;
            double g2 = 0;
            double h2 = 0;
            double c3 = 0;
            double d3 = 0;
            double e3 = 0;
            double f3 = 0;
            double g3 = 0;
            double h3 = 0;
            double c4 = 0;
            double d4 = 0;
            double e4 = 0;
            double f4 = 0;
            double g4 = 0;
            double h4 = 0;
            double c5 = 0;
            double d5 = 0;
            double e5 = 0;
            double f5 = 0;
            double g5 = 0;
            double h5 = 0;
            // Local iconic variables 

            HObject ho_ImageBatch = null, ho_ImageMedian = null;
            HObject ho_Regions = null, ho_Contours = null, ho_SmoothedContours = null;
            HObject ho_ContoursSplit = null, ho_SelectedXLD = null, ho_UnionContours = null;
            HObject ho_ContCircle = null, ho_Circle = null, ho_SelectedRegions = null;

            // Local control variables 

            HTuple hv_Files = new HTuple(), hv_BatchSizeInference = new HTuple();
            HTuple hv_BatchIndex = new HTuple(), hv_Batch = new HTuple();
            HTuple hv_BaseName = new HTuple(), hv_Extension = new HTuple();
            HTuple hv_Directory = new HTuple(), hv_Row = new HTuple();
            HTuple hv_Column = new HTuple(), hv_Radius = new HTuple();
            HTuple hv_StartPhi = new HTuple(), hv_EndPhi = new HTuple();
            HTuple hv_PointOrder = new HTuple(), hv_RegionCount = new HTuple();
            HTuple hv_Area = new HTuple(), hv_Row1 = new HTuple();
            HTuple hv_Column1 = new HTuple(), hv_ImageFiles = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageBatch);
            HOperatorSet.GenEmptyObj(out ho_ImageMedian);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_Contours);
            HOperatorSet.GenEmptyObj(out ho_SmoothedContours);
            HOperatorSet.GenEmptyObj(out ho_ContoursSplit);
            HOperatorSet.GenEmptyObj(out ho_SelectedXLD);
            HOperatorSet.GenEmptyObj(out ho_UnionContours);
            HOperatorSet.GenEmptyObj(out ho_ContCircle);
            HOperatorSet.GenEmptyObj(out ho_Circle);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            dev_update_off();
            //hv_Files.Dispose();
            //HOperatorSet.ListFiles(picDir, "files", out hv_Files);
            hv_ImageFiles.Dispose();
            list_image_files(picDir, "default", new HTuple(), out hv_ImageFiles);
            hv_BatchSizeInference.Dispose();
            hv_BatchSizeInference = 1;
            HTuple end_val3 = ((((new HTuple(hv_ImageFiles.TupleLength()
                )) / (hv_BatchSizeInference.TupleReal()))).TupleFloor()) - 1;
            HTuple step_val3 = 1;
            for (hv_BatchIndex = 0; hv_BatchIndex.Continue(end_val3, step_val3); hv_BatchIndex = hv_BatchIndex.TupleAdd(step_val3))
            {
                hv_Batch.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Batch = hv_ImageFiles.TupleSelectRange(
                        hv_BatchIndex * hv_BatchSizeInference, ((hv_BatchIndex + 1) * hv_BatchSizeInference) - 1);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_BaseName.Dispose(); hv_Extension.Dispose(); hv_Directory.Dispose();
                    parse_filename(hv_ImageFiles.TupleSelect(hv_BatchIndex), out hv_BaseName, out hv_Extension,
                        out hv_Directory);
                }

                ho_ImageBatch.Dispose();
                HOperatorSet.ReadImage(out ho_ImageBatch, hv_Batch);
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetDraw(HDevWindowStack.GetActive(), "margin");
                }
                int cameraId, row, col;
                string[] split = hv_BaseName.TupleSplit("-");
                cameraId = Convert.ToInt32(split[0]);
                row = Convert.ToInt32(split[1]);
                col = Convert.ToInt32(split[2]);
                if (cameraId == 2 && row == 0 && col == 8)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        c0 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        d0 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((c0 - 50.20) >= -10 && (c0 - 50.20) <= 10 && (d0 - 0) >= -10 && (d0 - 0) <= 10)
                        {
                            c0 = c0 - 50.20;
                            d0 = d0 - 0;
                        }
                        else
                        {
                            c0 = 0;
                            d0 = 0;
                        }
                        //c0 = c0 - 50.20;
                        //d0 = d0 - 0;
                    }
                    else
                    {
                        c0 = 0;
                        d0 = 0;

                    }
                }
                else if (cameraId == 2 && row == 0 && col == 22)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        e0 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        f0 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((e0 - 50.20) >= -10 && (e0 - 50.20) <= 10 && (f0 - 166.12) >= -10 && (f0 - 166.12) <= 10)
                        {
                            e0 = e0 - 50.20;
                            f0 = f0 - 166.12;
                        }
                        else
                        {
                            e0 = 0;
                            f0 = 0;
                        }
                        //e0 = e0 - 50.20;
                        //f0 = f0 - 166.12;
                    }
                    else
                    {
                        e0 = 0;
                        f0 = 0;

                    }
                }
                else if (cameraId == 2 && row == 0 && col == 37)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        g0 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        h0 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((g0 - 50.20) >= -10 && (g0 - 50.20) <= 10 && (h0 - 340) >= -10 && (h0 - 340) <= 10)
                        {
                            g0 = g0 - 50.20;
                            h0 = h0 - 340;
                        }
                        else
                        {
                            g0 = 0;
                            h0 = 0;
                        }
                        //g0 = g0 - 50.20;
                        //h0 = h0 - 340;
                    }
                    else
                    {
                        g0 = 0;
                        h0 = 0;

                    }
                }
                else if (cameraId == 2 && row == 1 && col == 6)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        c1 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        d1 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((c1 - 110.16) >= -10 && (c1 - 110.16) <= 10 && (d1 - 340.0) >= -10 && (d1 - 340.0) <= 10)
                        {
                            c1 = c1 - 110.16;
                            d1 = d1 - 340.0;
                        }
                        else
                        {
                            c1 = 0;
                            d1 = 0;
                        }
                    }
                    else
                    {
                        c1 = 0;
                        d1 = 0;

                    }
                }
                else if (cameraId == 2 && row == 1 && col == 11)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        e1 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        f1 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((e1 - 110.16) >= -10 && (e1 - 110.16) <= 10 && (f1 - 282.04) >= -10 && (f1 - 282.04) <= 10)
                        {
                            e1 = e1 - 110.16;
                            f1 = f1 - 282.04;
                        }
                        else
                        {
                            e1 = 0;
                            f1 = 0;
                        }
                    }
                    else
                    {
                        e1 = 0;
                        f1 = 0;

                    }
                }
                else if (cameraId == 2 && row == 1 && col == 34)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        g1 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        h1 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((g1 - 110.16) >= -10 && (g1 - 110.16) <= 10 && (h1 - 0) >= -10 && (h1 - 0) <= 10)
                        {
                            g1 = g1 - 110.16;
                            h1 = h1 - 0;
                        }
                        else
                        {
                            g1 = 0; // 如果g1不满足条件，则设为0
                            h1 = 0;
                        }
                    }
                    else
                    {
                        g1 = 0;
                        h1 = 0;

                    }
                }
                else if (cameraId == 2 && row == 2 && col == 8)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        c2 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        d2 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((c2 - 170.12) >= -10 && (c2 - 170.12) <= 10 && (d2 - 0) >= -10 && (d2 - 0) <= 10)
                        {
                            c2 = c2 - 170.12;
                            d2 = d2 - 0;
                        }
                        else
                        {
                            c2 = 0; // 如果c2不满足条件，则设为0
                            d2 = 0;
                        }
                    }
                    else
                    {
                        c2 = 0;
                        d2 = 0;

                    }
                }
                else if (cameraId == 2 && row == 2 && col == 22)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        e2 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        f2 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((e2 - 170.12) >= -10 && (e2 - 170.12) <= 10 && (f2 - 166.12) >= -10 && (f2 - 166.12) <= 10)
                        {
                            e2 = e2 - 170.12;
                            f2 = f2 - 166.12;
                        }
                        else
                        {
                            e2 = 0;
                            f2 = 0;
                        }
                    }
                    else
                    {
                        e2 = 0;
                        f2 = 0;

                    }
                }
                else if (cameraId == 2 && row == 2 && col == 37)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        g2 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        h2 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((g2 - 170.12) >= -10 && (g2 - 170.12) <= 10 && (h2 - 340) >= -10 && (h2 - 340) <= 10)
                        {
                            g2 = g2 - 170.12;
                            h2 = h2 - 340;
                        }
                        else
                        {
                            g2 = 0;
                            h2 = 0;

                        }
                    }
                    else
                    {
                        g2 = 0;
                        h2 = 0;

                    }
                }
                else if (cameraId == 2 && row == 3 && col == 6)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        c3 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        d3 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((c3 - 230.08) >= -10 && (c3 - 230.08) <= 10 && (d3 - 340) >= -10 && (d3 - 340) <= 10)
                        {
                            c3 = c3 - 230.08;
                            d3 = d3 - 340;
                        }
                        else
                        {
                            c3 = 0;
                            d3 = 0;

                        }
                    }
                    else
                    {
                        c3 = 0;
                        d3 = 0;

                    }
                }
                else if (cameraId == 2 && row == 3 && col == 11)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        e3 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        f3 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((e3 - 230.08) >= -10 && (e3 - 230.08) <= 10 && (f3 - 282.04) >= -10 && (f3 - 282.04) <= 10)
                        {
                            e3 = e3 - 230.08;
                            f3 = f3 - 282.04;
                        }
                        else
                        {
                            e3 = 0;
                            f3 = 0;

                        }
                    }
                    else
                    {
                        e3 = 0;
                        f3 = 0;

                    }
                }
                else if (cameraId == 2 && row == 3 && col == 34)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        g3 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        h3 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((g3 - 230.08) >= -10 && (g3 - 230.08) <= 10 && (h3 - 0) >= -10 && (h3 - 0) <= 10)
                        {
                            g3 = g3 - 230.08;
                            h3 = h3 - 0;
                        }
                        else
                        {
                            g3 = 0;
                            h3 = 0;

                        }
                    }
                    else
                    {
                        g3 = 0;
                        h3 = 0;

                    }
                }
                else if (cameraId == 2 && row == 4 && col == 8)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        c4 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        d4 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((c4 - 290.04) >= -10 && (c4 - 290.04) <= 10 && (d4 - 0) >= -10 && (d4 - 0) <= 10)
                        {
                            c4 = c4 - 290.04;
                            d4 = d4 - 0;
                        }
                        else
                        {
                            c4 = 0;
                            d4 = 0;

                        }
                    }
                    else
                    {
                        c4 = 0;
                        d4 = 0;

                    }
                }
                else if (cameraId == 2 && row == 4 && col == 23)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        e4 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        f4 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((e4 - 290.04) >= -10 && (e4 - 290.04) <= 10 && (f4 - 173.88) >= -10 && (f4 - 173.88) <= 10)
                        {
                            e4 = e4 - 290.04;
                            f4 = f4 - 173.88;
                        }
                        else
                        {
                            e4 = 0;
                            f4 = 0;

                        }
                    }
                    else
                    {
                        e4 = 0;
                        f4 = 0;

                    }
                }
                else if (cameraId == 2 && row == 4 && col == 37)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        g4 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        h4 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((g4 - 290.04) >= -10 && (g4 - 290.04) <= 10 && (h4 - 340.0) >= -10 && (h4 - 340.0) <= 10)
                        {
                            g4 = g4 - 290.04;
                            h4 = h4 - 340.0;
                        }
                        else
                        {
                            g4 = 0;
                            h4 = 0;

                        }
                    }
                    else
                    {
                        g4 = 0;
                        h4 = 0;

                    }
                }
                else if (cameraId == 2 && row == 5 && col == 6)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        c5 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        d5 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((c5 - 350) >= -10 && (c5 - 350) <= 10 && (d5 - 340.0) >= -10 && (d5 - 340.0) <= 10)
                        {
                            c5 = c5 - 350;
                            d5 = d5 - 340;
                        }
                        else
                        {
                            c5 = 0;
                            d5 = 0;

                        }
                    }
                    else
                    {
                        c5 = 0;
                        d5 = 0;

                    }
                }
                else if (cameraId == 2 && row == 5 && col == 15)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        e5 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        f5 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((e5 - 350) >= -10 && (e5 - 350) <= 10 && (f5 - 231.84) >= -10 && (f5 - 231.84) <= 10)
                        {
                            e5 = e5 - 350;
                            f5 = f5 - 231.84;
                        }
                        else
                        {
                            e5 = 0;
                            f5 = 0;

                        }
                    }
                    else
                    {
                        e5 = 0;
                        f5 = 0;

                    }
                }
                else if (cameraId == 2 && row == 5 && col == 34)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        g5 = Convert.ToDouble(dt3.Rows[0][1]) + (piy * 16 / 2592);
                        h5 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((g5 - 350) >= -10 && (g5 - 350) <= 10 && (h5 - 0) >= -10 && (h5 - 0) <= 10)
                        {
                            g5 = g5 - 350;
                            h5 = h5 - 0;
                        }
                        else
                        {
                            g5 = 0;
                            h5 = 0;

                        }
                    }
                    else
                    {
                        g5 = 0;
                        h5 = 0;

                    }
                }


            }
            x0 = (c0 + e0 + g0) / 3;
            y0 = (d0 + f0 + h0) / 3;
            x1 = (c1 + e1 + g1) / 3;
            y1 = (d1 + f1 + h1) / 3;
            x2 = (c2 + e2 + g2) / 3;
            y2 = (d2 + f2 + h2) / 3;
            x3 = (c3 + e3 + g3) / 3;
            y3 = (d3 + f3 + h3) / 3;
            x4 = (c4 + e4 + g4) / 3;
            y4 = (d4 + f4 + h4) / 3;
            x5 = (c5 + e5 + g5) / 3;
            y5 = (d5 + f5 + h5) / 3;

            return (x0, y0, x1, y1, x2, y2, x3, y3, x4, y4, x5, y5);





            ho_ImageBatch.Dispose();
            ho_ImageMedian.Dispose();
            ho_Regions.Dispose();
            ho_Contours.Dispose();
            ho_SmoothedContours.Dispose();
            ho_ContoursSplit.Dispose();
            ho_SelectedXLD.Dispose();
            ho_UnionContours.Dispose();
            ho_ContCircle.Dispose();
            ho_Circle.Dispose();
            ho_SelectedRegions.Dispose();

            hv_Files.Dispose();
            hv_ImageFiles.Dispose();
            hv_BatchSizeInference.Dispose();
            hv_BatchIndex.Dispose();
            hv_Batch.Dispose();
            hv_BaseName.Dispose();
            hv_Extension.Dispose();
            hv_Directory.Dispose();
            hv_Row.Dispose();
            hv_Column.Dispose();
            hv_Radius.Dispose();
            hv_StartPhi.Dispose();
            hv_EndPhi.Dispose();
            hv_PointOrder.Dispose();
            hv_RegionCount.Dispose();
            hv_Area.Dispose();
            hv_Row1.Dispose();
            hv_Column1.Dispose();

        }

#endif
        public (double, double, double, double, double, double, double, double, double, double, double, double) ProcessImage1(string picDir)
        {
            double X0 = 0, Y0 = 0, X1 = 0, Y1 = 0, X2 = 0, Y2 = 0, X3 = 0, Y3 = 0, X4 = 0, Y4 = 0, X5 = 0, Y5 = 0;
            string x, y;
            double pix, piy;
            double C0 = 0;
            double D0 = 0;
            double E0 = 0;
            double F0 = 0;
            double G0 = 0;
            double H0 = 0;
            double C1 = 0;
            double D1 = 0;
            double E1 = 0;
            double F1 = 0;
            double G1 = 0;
            double H1 = 0;
            double C2 = 0;
            double D2 = 0;
            double E2 = 0;
            double F2 = 0;
            double G2 = 0;
            double H2 = 0;
            double C3 = 0;
            double D3 = 0;
            double E3 = 0;
            double F3 = 0;
            double G3 = 0;
            double H3 = 0;
            double C4 = 0;
            double D4 = 0;
            double E4 = 0;
            double F4 = 0;
            double G4 = 0;
            double H4 = 0;
            double C5 = 0;
            double D5 = 0;
            double E5 = 0;
            double F5 = 0;
            double G5 = 0;
            double H5 = 0;
            // Local iconic variables 

            HObject ho_ImageBatch = null, ho_ImageMedian = null;
            HObject ho_Regions = null, ho_Contours = null, ho_SmoothedContours = null;
            HObject ho_ContoursSplit = null, ho_SelectedXLD = null, ho_UnionContours = null;
            HObject ho_ContCircle = null, ho_Circle = null, ho_SelectedRegions = null;

            // Local control variables 

            HTuple hv_Files = new HTuple(), hv_BatchSizeInference = new HTuple();
            HTuple hv_BatchIndex = new HTuple(), hv_Batch = new HTuple();
            HTuple hv_BaseName = new HTuple(), hv_Extension = new HTuple();
            HTuple hv_Directory = new HTuple(), hv_Row = new HTuple();
            HTuple hv_Column = new HTuple(), hv_Radius = new HTuple();
            HTuple hv_StartPhi = new HTuple(), hv_EndPhi = new HTuple();
            HTuple hv_PointOrder = new HTuple(), hv_RegionCount = new HTuple();
            HTuple hv_Area = new HTuple(), hv_Row1 = new HTuple();
            HTuple hv_Column1 = new HTuple(), hv_ImageFiles = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageBatch);
            HOperatorSet.GenEmptyObj(out ho_ImageMedian);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_Contours);
            HOperatorSet.GenEmptyObj(out ho_SmoothedContours);
            HOperatorSet.GenEmptyObj(out ho_ContoursSplit);
            HOperatorSet.GenEmptyObj(out ho_SelectedXLD);
            HOperatorSet.GenEmptyObj(out ho_UnionContours);
            HOperatorSet.GenEmptyObj(out ho_ContCircle);
            HOperatorSet.GenEmptyObj(out ho_Circle);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            dev_update_off();
            //hv_Files.Dispose();
            //HOperatorSet.ListFiles(picDir, "files", out hv_Files);
            hv_ImageFiles.Dispose();
            list_image_files(picDir, "default", new HTuple(), out hv_ImageFiles);
            hv_BatchSizeInference.Dispose();
            hv_BatchSizeInference = 1;
            HTuple end_val3 = ((((new HTuple(hv_ImageFiles.TupleLength()
                )) / (hv_BatchSizeInference.TupleReal()))).TupleFloor()) - 1;
            HTuple step_val3 = 1;
            for (hv_BatchIndex = 0; hv_BatchIndex.Continue(end_val3, step_val3); hv_BatchIndex = hv_BatchIndex.TupleAdd(step_val3))
            {
                hv_Batch.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Batch = hv_ImageFiles.TupleSelectRange(
                        hv_BatchIndex * hv_BatchSizeInference, ((hv_BatchIndex + 1) * hv_BatchSizeInference) - 1);
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_BaseName.Dispose(); hv_Extension.Dispose(); hv_Directory.Dispose();
                    parse_filename(hv_ImageFiles.TupleSelect(hv_BatchIndex), out hv_BaseName, out hv_Extension,
                        out hv_Directory);
                }

                ho_ImageBatch.Dispose();
                HOperatorSet.ReadImage(out ho_ImageBatch, hv_Batch);
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetDraw(HDevWindowStack.GetActive(), "margin");
                }
                int cameraId, row, col;
                string[] split = hv_BaseName.TupleSplit("-");
                cameraId = Convert.ToInt32(split[0]);
                row = Convert.ToInt32(split[1]);
                col = Convert.ToInt32(split[2]);
                if (cameraId == 5 && row == 0 && col == 8)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        C0 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        D0 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((C0 - 0) >= -10 && (C0 - 0) <= 10 && (D0 - 0) >= -10 && (D0 - 0) <= 10)
                        {
                            C0 = C0 - 0;
                            D0 = D0 - 0;
                        }
                        else
                        {
                            C0 = 0;
                            D0 = 0;
                        }
                        //C0 = C0 - 50.20;
                        //D0 = D0 - 0;
                    }
                    else
                    {
                        C0 = 0;
                        D0 = 0;

                    }
                }
                else if (cameraId == 5 && row == 0 && col == 18)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        E0 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        F0 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((E0 - 0) >= -10 && (E0 - 0) <= 10 && (F0 - 115.92) >= -10 && (F0 - 115.92) <= 10)
                        {
                            E0 = E0 - 0;
                            F0 = F0 - 115.92;
                        }
                        else
                        {
                            E0 = 0;
                            F0 = 0;
                        }
                        //E0 = E0 - 50.20;
                        //F0 = F0 - 166.12;
                    }
                    else
                    {
                        E0 = 0;
                        F0 = 0;

                    }
                }
                else if (cameraId == 5 && row == 0 && col == 36)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        G0 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        H0 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((G0 - 0) >= -10 && (G0 - 0) <= 10 && (H0 - 340) >= -10 && (H0 - 340) <= 10)
                        {
                            G0 = G0 - 0;
                            H0 = H0 - 340;
                        }
                        else
                        {
                            G0 = 0;
                            H0 = 0;
                        }
                        //G0 = G0 - 50.20;
                        //H0 = H0 - 340;
                    }
                    else
                    {
                        G0 = 0;
                        H0 = 0;

                    }
                }
                else if (cameraId == 5 && row == 1 && col == 11)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        C1 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        D1 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((C1 - 59.96) >= -10 && (C1 - 59.96) <= 10 && (D1 - 282.04) >= -10 && (D1 - 282.04) <= 10)
                        {
                            C1 = C1 - 59.96;
                            D1 = D1 - 282.04;
                        }
                        else
                        {
                            C1 = 0;
                            D1 = 0;
                        }
                    }
                    else
                    {
                        C1 = 0;
                        D1 = 0;

                    }
                }
                else if (cameraId == 5 && row == 1 && col == 16)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        E1 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        F1 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((E1 - 59.96) >= -10 && (E1 - 59.96) <= 10 && (F1 - 224.08) >= -10 && (F1 - 224.08) <= 10)
                        {
                            E1 = E1 - 59.96;
                            F1 = F1 - 224.08;
                        }
                        else
                        {
                            E1 = 0;
                            F1 = 0;
                        }
                    }
                    else
                    {
                        E1 = 0;
                        F1 = 0;

                    }
                }
                else if (cameraId == 5 && row == 1 && col == 21)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        G1 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        H1 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((G1 - 59.96) >= -10 && (G1 - 59.96) <= 10 && (H1 - 166.12) >= -10 && (H1 - 166.12) <= 10)
                        {
                            G1 = G1 - 59.96;
                            H1 = H1 - 166.12;
                        }
                        else
                        {
                            G1 = 0; // 如果g1不满足条件，则设为0
                            H1 = 0;
                        }
                    }
                    else
                    {
                        G1 = 0;
                        H1 = 0;

                    }
                }
                else if (cameraId == 5 && row == 2 && col == 8)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        C2 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        D2 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((C2 - 119.92) >= -10 && (C2 - 119.92) <= 10 && (D2 - 0) >= -10 && (D2 - 0) <= 10)
                        {
                            C2 = C2 - 119.92;
                            D2 = D2 - 0;
                        }
                        else
                        {
                            C2 = 0; // 如果c2不满足条件，则设为0
                            D2 = 0;
                        }
                    }
                    else
                    {
                        C2 = 0;
                        D2 = 0;

                    }
                }
                else if (cameraId == 2 && row == 2 && col == 18)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        E2 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        F2 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((E2 - 119.92) >= -10 && (E2 - 119.92) <= 10 && (F2 - 115.92) >= -10 && (F2 - 115.92) <= 10)
                        {
                            E2 = E2 - 119.92;
                            F2 = F2 - 115.92;
                        }
                        else
                        {
                            E2 = 0;
                            F2 = 0;
                        }
                    }
                    else
                    {
                        E2 = 0;
                        F2 = 0;

                    }
                }
                else if (cameraId == 5 && row == 2 && col == 36)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        G2 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        H2 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((G2 - 119.92) >= -10 && (G2 - 119.92) <= 10 && (H2 - 340) >= -10 && (H2 - 340) <= 10)
                        {
                            G2 = G2 - 119.92;
                            H2 = H2 - 340;
                        }
                        else
                        {
                            G2 = 0;
                            H2 = 0;

                        }
                    }
                    else
                    {
                        G2 = 0;
                        H2 = 0;

                    }
                }
                else if (cameraId == 5 && row == 3 && col == 6)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        C3 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        D3 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((C3 - 179.88) >= -10 && (C3 - 179.88) <= 10 && (D3 - 340) >= -10 && (D3 - 340) <= 10)
                        {
                            C3 = C3 - 179.88;
                            D3 = D3 - 340;
                        }
                        else
                        {
                            C3 = 0;
                            D3 = 0;

                        }
                    }
                    else
                    {
                        C3 = 0;
                        D3 = 0;

                    }
                }
                else if (cameraId == 5 && row == 3 && col == 15)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        E3 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        F3 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((E3 - 179.88) >= -10 && (E3 - 179.88) <= 10 && (F3 - 231.84) >= -10 && (F3 - 231.84) <= 10)
                        {
                            E3 = E3 - 179.88;
                            F3 = F3 - 231.84;
                        }
                        else
                        {
                            E3 = 0;
                            F3 = 0;

                        }
                    }
                    else
                    {
                        E3 = 0;
                        F3 = 0;

                    }
                }
                else if (cameraId == 5 && row == 3 && col == 21)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        G3 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        H3 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((G3 - 179.88) >= -10 && (G3 - 179.88) <= 10 && (H3 - 166.12) >= -10 && (H3 - 166.12) <= 10)
                        {
                            G3 = G3 - 179.88;
                            H3 = H3 - 166.12;
                        }
                        else
                        {
                            G3 = 0;
                            H3 = 0;

                        }
                    }
                    else
                    {
                        G3 = 0;
                        H3 = 0;

                    }
                }
                else if (cameraId == 5 && row == 4 && col == 8)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        C4 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        D4 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((C4 - 239.84) >= -10 && (C4 - 239.84) <= 10 && (D4 - 0) >= -10 && (D4 - 0) <= 10)
                        {
                            C4 = C4 - 239.84;
                            D4 = D4 - 0;
                        }
                        else
                        {
                            C4 = 0;
                            D4 = 0;

                        }
                    }
                    else
                    {
                        C4 = 0;
                        D4 = 0;

                    }
                }
                else if (cameraId == 2 && row == 4 && col == 18)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        E4 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        F4 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((E4 - 239.84) >= -10 && (E4 - 239.84) <= 10 && (F4 - 115.92) >= -10 && (F4 - 115.92) <= 10)
                        {
                            E4 = E4 - 239.84;
                            F4 = F4 - 115.92;
                        }
                        else
                        {
                            E4 = 0;
                            F4 = 0;

                        }
                    }
                    else
                    {
                        E4 = 0;
                        F4 = 0;

                    }
                }
                else if (cameraId == 5 && row == 4 && col == 36)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        G4 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        H4 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((G4 - 239.84) >= -10 && (G4 - 239.84) <= 10 && (H4 - 340.0) >= -10 && (H4 - 340.0) <= 10)
                        {
                            G4 = G4 - 239.84;
                            H4 = H4 - 340.0;
                        }
                        else
                        {
                            G4 = 0;
                            H4 = 0;

                        }
                    }
                    else
                    {
                        G4 = 0;
                        H4 = 0;

                    }
                }
                else if (cameraId == 5 && row == 5 && col == 6)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        C5 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        D5 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((C5 - 299.8) >= -10 && (C5 - 299.8) <= 10 && (D5 - 340.0) >= -10 && (D5 - 340.0) <= 10)
                        {
                            C5 = C5 - 299.8;
                            D5 = D5 - 340;
                        }
                        else
                        {
                            C5 = 0;
                            D5 = 0;

                        }
                    }
                    else
                    {
                        C5 = 0;
                        D5 = 0;

                    }
                }
                else if (cameraId == 5 && row == 5 && col == 15)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        E5 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        F5 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((E5 - 299.8) >= -10 && (E5 - 299.8) <= 10 && (F5 - 231.84) >= -10 && (F5 - 231.84) <= 10)
                        {
                            E5 = E5 - 299.8;
                            F5 = F5 - 231.84;
                        }
                        else
                        {
                            E5 = 0;
                            F5 = 0;

                        }
                    }
                    else
                    {
                        E5 = 0;
                        F5 = 0;

                    }
                }
                else if (cameraId == 5 && row == 5 && col == 21)
                {
                    //对图像进行中值滤波，使用半径为20的圆形结构元素，以减少噪声并保留边缘信息。
                    ho_ImageMedian.Dispose();
                    HOperatorSet.MedianImage(ho_ImageBatch, out ho_ImageMedian, "circle", 20, "mirrored");
                    //对滤波后的图像进行阈值化，生成二值图像Regions，其中像素值大于0且小于99的区域被设置为白色（前景），其余为黑色（背景）。
                    ho_Regions.Dispose();
                    HOperatorSet.Threshold(ho_ImageMedian, out ho_Regions, 0, 99);
                    //从二值图像中提取轮廓，并以扩展线段列表（XLD）格式存储在Contours中。
                    ho_Contours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Regions, out ho_Contours, "border");
                    //对提取的轮廓进行平滑处理，以减少小的不规则性。
                    ho_SmoothedContours.Dispose();
                    HOperatorSet.SmoothContoursXld(ho_Contours, out ho_SmoothedContours, 99);
                    //根据轮廓的形状特征（如圆形和线条）进行分割，参数60, 200, 100与圆形检测的阈值有关。
                    ho_ContoursSplit.Dispose();
                    HOperatorSet.SegmentContoursXld(ho_SmoothedContours, out ho_ContoursSplit,
                        "lines_circles", 60, 200, 100);
                    //选择具有特定圆形度（circularity）范围内的轮廓，circularity是一个衡量形状接近圆形的指标，计算公式为4π*面积/周长的平方。
                    ho_SelectedXLD.Dispose();
                    HOperatorSet.SelectShapeXld(ho_ContoursSplit, out ho_SelectedXLD, "circularity",
                        "and", 0.08189, 1);
                    //合并相邻的轮廓，参数10可能定义了轮廓之间的最大距离。
                    ho_UnionContours.Dispose();
                    HOperatorSet.UnionAdjacentContoursXld(ho_SelectedXLD, out ho_UnionContours,
                        10, 1, "attr_keep");
                    //使用代数方法拟合圆形轮廓，输出圆心的坐标（Row, Column）和半径Radius。
                    hv_Row.Dispose(); hv_Column.Dispose(); hv_Radius.Dispose(); hv_StartPhi.Dispose(); hv_EndPhi.Dispose(); hv_PointOrder.Dispose();
                    HOperatorSet.FitCircleContourXld(ho_UnionContours, "algebraic", -1, 0, 0, 3,
                        2, out hv_Row, out hv_Column, out hv_Radius, out hv_StartPhi, out hv_EndPhi,
                        out hv_PointOrder);
                    //根据拟合的圆参数生成圆形轮廓。
                    ho_ContCircle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_Row, hv_Column, hv_Radius,
                        hv_StartPhi, hv_StartPhi, "positive", 1);
                    //直接根据圆心坐标和半径生成圆形。
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircle(out ho_Circle, hv_Row, hv_Column, hv_Radius);
                    //选择面积在指定范围内的圆形。
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShape(ho_Circle, out ho_SelectedRegions, "area", "and",
                        224122, 410000);
                    hv_RegionCount.Dispose();
                    HOperatorSet.CountObj(ho_SelectedRegions, out hv_RegionCount);
                    if ((int)(new HTuple(hv_RegionCount.TupleEqual(1))) != 0)
                    {
                        hv_Area.Dispose(); hv_Row1.Dispose(); hv_Column1.Dispose();
                        HOperatorSet.AreaCenter(ho_SelectedRegions, out hv_Area, out hv_Row1, out hv_Column1);
                        x = hv_Row1.ToString();
                        y = hv_Column1.ToString();
                        pix = Math.Round(Convert.ToDouble(x), 2);
                        piy = Math.Round(Convert.ToDouble(y), 2);
                        string err;
                        DataTable dt3 = MySqlHelper.GetDataTable(out err, "select * from pictureLocation where picName='" + cameraId + "-" + row + "-" + col + "'");
                        G5 = Convert.ToDouble(dt3.Rows[0][1]) - (piy * 16 / 2592);
                        H5 = Convert.ToDouble(dt3.Rows[0][2]) + (pix * 12 / 2048);
                        if ((G5 - 299.8) >= -10 && (G5 - 299.8) <= 10 && (H5 - 166.12) >= -10 && (H5 - 166.12) <= 10)
                        {
                            G5 = G5 - 299.8;
                            H5 = H5 - 166.12;
                        }
                        else
                        {
                            G5 = 0;
                            H5 = 0;

                        }
                    }
                    else
                    {
                        G5 = 0;
                        H5 = 0;

                    }
                }



            }
            X0 = (C0 + E0 + G0) / 3;
            Y0 = (D0 + F0 + H0) / 3;
            X1 = (C1 + E1 + G1) / 3;
            Y1 = (D1 + F1 + H1) / 3;
            X2 = (C2 + E2 + G2) / 3;
            Y2 = (D2 + F2 + H2) / 3;
            X3 = (C3 + E3 + G3) / 3;
            Y3 = (D3 + F3 + H3) / 3;
            X4 = (C4 + E4 + G4) / 3;
            Y4 = (D4 + F4 + H4) / 3;
            X5 = (C5 + E5 + G5) / 3;
            Y5 = (D5 + F5 + H5) / 3;

            return (X0, Y0, X1, Y1, X2, Y2, X3, Y3, X4, Y4, X5, Y5);





            ho_ImageBatch.Dispose();
            ho_ImageMedian.Dispose();
            ho_Regions.Dispose();
            ho_Contours.Dispose();
            ho_SmoothedContours.Dispose();
            ho_ContoursSplit.Dispose();
            ho_SelectedXLD.Dispose();
            ho_UnionContours.Dispose();
            ho_ContCircle.Dispose();
            ho_Circle.Dispose();
            ho_SelectedRegions.Dispose();

            hv_Files.Dispose();
            hv_ImageFiles.Dispose();
            hv_BatchSizeInference.Dispose();
            hv_BatchIndex.Dispose();
            hv_Batch.Dispose();
            hv_BaseName.Dispose();
            hv_Extension.Dispose();
            hv_Directory.Dispose();
            hv_Row.Dispose();
            hv_Column.Dispose();
            hv_Radius.Dispose();
            hv_StartPhi.Dispose();
            hv_EndPhi.Dispose();
            hv_PointOrder.Dispose();
            hv_RegionCount.Dispose();
            hv_Area.Dispose();
            hv_Row1.Dispose();
            hv_Column1.Dispose();
        }

    }
#if !(NO_EXPORT_MAIN || NO_EXPORT_APP_MAIN)

#endif

}


