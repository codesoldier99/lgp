# IndustryDemo - 滤光片瑕疵检测系统

这是一个基于 C# WinForms + HALCON 的工业视觉检测上位机程序，用于检测滤光片的各类缺陷（划痕、针孔、擦伤、气泡、斑点等）。

## 架构概览

### 核心组件层次
- **mainForm.cs**: 主窗口，使用 DevExpress Ribbon 控制界面，管理三个主要页面切换
  - `detectionui1`: 检测界面（默认显示）
  - `configInfoui1`: 配置界面（相机参数设置）
  - `historyui1`: 历史记录查询界面

- **Controllerui/Detectionui.cs**: 检测控制核心（3148行），统筹所有检测流程
  - 管理 8 个工业相机（Camera1-8）的实时采集
  - 协调多线程：`detectionThread`（检测）、`ToDiskThread`（存盘）、`DefectThread`（缺陷显示）
  - 处理 STM32 通信（Modbus RTU 协议控制电机）
  - 处理二维码扫描（串口通信）
  - 管理虚拟盘显示（distributeGraph1）

### 图像处理流程
1. **Camera.cs**: 封装 Hikvision 工业相机（MvCamCtrl.NET SDK）
   - 每个相机绑定到 HALCON HWindow 用于实时显示
   - 图像保存路径格式：`G://{二维码}/{检测时间}/camera{id}/{ring|spot}/`

2. **CamArray.cs**: 相机数据队列管理
   - 维护图像缓冲列表 `List<byte[]> list`
   - 存储图像位置信息 `List<ImageInfo> Location`（包含相机ID、光源类型、行列、距离）
   - 关联检测算法实例（`DetectionUnderSpotLight`、`DetectionUnderRingLight`）

3. **检测算法模块**（多态设计）:
   - **DetectionUnderRingLight.cs**: 环形光源下检测（针孔、划痕、擦伤、道子）
   - **DetectionUnderSpotLight.cs**: 点光源下检测（不同光照条件）
   - **DetectionWithDL.cs**: 基于深度学习的分割检测（20166行，使用 HALCON DL 模型）
     - 模型路径：`segment_filter_defects_data/best_dl_model_segmentation_info.hdict`
     - 预处理参数检查：`check_dl_preprocess_param()`

4. **Program.cs**: 历史遗留代码，包含原始检测算法实现（10565行），主要供参考

### 硬件控制层
- **STM32Control.cs**: Modbus RTU 串口通信控制电机（M1-M7）
  - 使用 `Modbus.Device` 库
  - 默认连接参数需在代码中配置（串口号、波特率）
  
- **串口管理**: 
  - `_serialPort1`, `_serialPort2`: 用于二维码扫描器通信
  - 通过 `System.IO.Ports.SerialPort` 实现

### 数据持久层
- **Global.cs**: 全局配置与状态
  - 数据库连接字符串: `conString = "Server=127.0.0.1;Port=3306;Database=filterdetectiondatabase;Uid=root;Pwd=123456;"`
  - 全局变量：滤光片参数（形状、直径、厚度）、检测状态、缺陷计数
  - 结构体 `defInfo`: 存储缺陷位置信息（相机ID、行列、X/Y坐标）

- **MySqlOperate.cs / MySqlHelper.cs**: MySQL 数据库操作
  - 存储检测记录、缺陷信息
  - DefectionClass.cs: 缺陷数据模型，生成 INSERT SQL

- **DataBaseOperate.cs**: 图像与数据库关联操作

### 缺陷类型可视化
- **Controllerui/bubble.cs, pinhole.cs, scratch.cs, spot.cs, nap.cs**: 
  - 自定义 UserControl 用于虚拟盘上显示不同缺陷类型
  - 通过 Paint 事件绘制不同颜色的标识矩形

### 报表系统
- **RDLC/**: ReportViewer 报表定义
  - `selectByDefection.rdlc`: 按缺陷类型查询报表
  - `selectByTime.rdlc`: 按时间查询报表
- **reportClass/**: 报表数据模型
- 依赖: Microsoft.ReportViewer.WinForms 150.1449.0

## 开发规范

### 命名约定
- **中文变量名**: 代码中广泛使用拼音或中文命名（如 `xiacileixing` 瑕疵类型、`detectiontime` 检测时间）
- **界面控件**: DevExpress 控件前缀（`simpleButton`, `textEdit`, `gridControl`）
- **相机实例**: `camera1` - `camera8`（固定 8 个相机）

### 线程模式
- **检测主线程**: `detectionThread = new Thread(DetectionThreadExecute)`
  - 循环采集图像 → 调用 HALCON 算法 → 存储结果
- **存盘线程**: `ToDiskThread`，异步保存图像到磁盘
- **UI更新**: 通过 `Invoke()` 跨线程更新 WinForms 控件
- **停止信号**: `Global.stop` 全局标志控制线程退出

### HALCON 集成
- **图像格式转换**: `byte[] → HObject` 通过 `HOperatorSet.GenImage1()` 或相机 SDK
- **窗口管理**: 每个 PictureEdit 控件关联一个 `HWindow`
  - 初始化: `m_Window.OpenWindow(row, col, width, height, hWindowID, "visible", "")`
- **释放资源**: HObject 需显式 Dispose（使用 `HDevDisposeHelper`）

### 关键路径依赖
- **HALCON 20.11/22.05**: `halcondotnet.dll` 路径硬编码在 csproj（需手动调整）
- **Hikvision SDK**: `MvCameraControl.Net.dll` 在 `bin\Debug\` 目录
- **DevExpress 20.2**: UI 控件库
- **NuGet 包**: ReportViewer, SqlServer.Types, MySql.Data（通过 packages.config）

### 图像存储规范
- **根目录**: 默认 `G://` 盘（见 Camera.cs 构造函数）
- **目录结构**: `{二维码}/{检测时间}/camera{id}/{ring|spot}/图片.bmp`
- **文件名**: 包含行列位置信息（由 CamArray 管理）

### 配置热点
1. **数据库连接**: 修改 [Global.cs](IndustryDemo/Global.cs) `conString`
2. **深度学习模型**: 路径在 `Global.deepLearningModelDir`（默认 `c:/work`）
3. **串口配置**: 在 Detectionui.cs 构造函数或 STM32Control 初始化时设置
4. **相机数量**: 当前固定 8 个，修改需同步调整 `Camera camera1-8` 声明和初始化逻辑

## 调试与运行

### 前置条件
- 安装 HALCON 20.11+ （并配置 License）
- 安装 Hikvision 相机驱动
- MySQL 5.7+ 运行中，导入数据库架构（`filterdetectiondatabase`）
- 硬件连接：8 个工业相机、STM32 控制板、二维码扫描器

### 构建配置
- **平台**: x86（因硬件驱动限制）
- **目标框架**: .NET Framework 4.8
- **允许不安全代码**: 已启用（处理图像指针）

### 常见问题
- **相机无法打开**: 检查相机 IP 或 USB 连接，确认 `m_stDeviceList` 是否枚举到设备
- **HALCON 错误**: 确认 License 文件位置，检查 DLL 路径是否匹配安装版本
- **数据库连接失败**: 验证 MySQL 服务状态和 Global.cs 中的连接字符串
- **图像保存失败**: 确认 G 盘存在且有写权限（或修改 Camera.cs 中的路径）

### 启动流程
1. mainForm 构造时自动创建并显示 Detectionui
2. 点击 Ribbon "开始检测" → `barButtonItem1_ItemClick()` → `detectionui1.Start_Detection()`
3. Start_Detection() 初始化相机、打开串口、启动检测线程
4. 检测过程中实时更新日志（gridControl1）和虚拟盘（distributeGraph1）

## 扩展指南

### 添加新缺陷类型
1. 在 `Controllerui/` 创建新 UserControl（参考 scratch.cs）
2. 在检测算法中添加识别逻辑（DetectionUnderRingLight/SpotLight）
3. 更新 `defectType[]` 数组和数据库表结构
4. 在虚拟盘显示逻辑中添加新缺陷的可视化

### 修改相机数量
1. 调整 `Camera camera1-8` 变量声明
2. 修改 CamArray.cs 中的 `relatedX[]` 数组（相机位置偏移量）
3. 更新 Detectionui.cs 中的相机初始化和图像采集循环

### 切换检测算法
- 算法实例在 CamArray 中创建（`SpotLight`/`RingLight`）
- 调用位置：CamArray.ProcessImage() 根据光源类型选择算法
- 添加新算法：实现类似接口，在 CamArray 中实例化并调用
