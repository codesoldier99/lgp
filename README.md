本项目用途为用途为滤光片瑕疵检测。
整套装备软件部分，分为上位机和下位机。本项目代码部分内容为上位机部分。
上位机的软件用 C Sharp 写成。检测部分用深度学习来完成，深度学习模型来自 HALCON。

## 项目代码分析说明（面向后继开发者快速入门）

### 1. 项目定位与技术栈
- 本仓库核心是上位机程序，解决滤光片瑕疵检测与分拣流程中的“采图-检测-记录-展示-控制”闭环。
- 主要技术：
  - **C# WinForms + DevExpress**（UI 与业务流程）
  - **HALCON / HalconDotNet**（传统视觉与深度学习推理）
  - **MySQL**（检测记录存储）
  - **串口 + Modbus**（与 STM32/电机控制交互）
  - **海康工业相机 SDK（MvCameraControl.Net）**（多相机采集）

### 2. 目录与核心文件
- 根目录说明文档：`/tmp/workspace/codesoldier99/lgp/README.md`
- 上位机主工程目录：`/tmp/workspace/codesoldier99/lgp/我的上位机程序(3.3) - 定位划痕/IndustryDemo`
- 解决方案：`IndustryDemo.sln`
- 工程文件：`IndustryDemo.csproj`
- 关键代码：
  - `mainForm.cs`：主窗口与页面切换、主流程按钮入口
  - `Controllerui/Detectionui.cs`：检测业务总控（线程、相机、串口、检测调度、进度/耗时）
  - `Controllerui/detection.cs`：`DetectionWithDL2`，深度学习检测与结果处理核心
  - `Program.cs` / `Program1.cs`：包含检测相关 HALCON 过程封装（环光/点光/DL）
  - `Controllerui/STM32Control.cs`：电机与执行机构控制（Modbus 寄存器写入）
  - `Global.cs`：全局参数与运行时共享状态
  - `MySqlHelper.cs`、`Controllerui/DataBaseOperate.cs`：数据库读写封装

### 3. 运行流程（理解系统的最短路径）
1. 程序入口启动主窗体（`mainForm`）。
2. 进入检测页面（`Detectionui`），初始化相机、串口、控制线程等资源。
3. 采图后进入检测核心（`DetectionWithDL2` + HALCON 相关流程）：
   - 图像预处理与区域分割；
   - 传统规则/深度学习推理；
   - 缺陷分类、定位、结果汇总。
4. 检测结果在 UI 展示并记录数据库，同时与下位机联动完成动作控制。
5. 检测结束后输出统计信息（含耗时记录）并进行资源释放。

### 4. 关键配置与依赖关注点
- 目标框架：`.NET Framework 4.8`（`IndustryDemo.csproj` 与 `App.config`）。
- 第三方依赖包括 DevExpress、HalconDotNet、MySQL Connector、NModbus4、ReportViewer 等。
- 路径/环境耦合点较多（如 HALCON DLL、模型目录、图像目录、数据库连接串等），新环境部署需优先核对：
  - `Global.cs` 中 `conString`、`deepLearningModelDir`、`imgLocation`；
  - HALCON 安装路径与 `halcondotnet.dll` 引用是否可用；
  - 相机 SDK 与串口号、波特率等现场参数。

### 5. 新开发者建议的阅读顺序
1. 先看 `mainForm.cs`：理解“启动检测 / 参数设置 / 历史查询”入口。
2. 再看 `Detectionui.cs`：理解检测调度、线程、设备交互、日志与进度机制。
3. 深入 `detection.cs` 与 `Program*.cs`：理解视觉算法与深度学习推理细节。
4. 最后看 `STM32Control.cs` 与数据库类：打通“检测结果 -> 设备动作/数据落库”链路。

### 6. 当前维护建议（稳定性视角）
- 尽量减少在 `Global` 中新增强耦合全局状态，优先明确参数来源与生命周期。
- 检测线程、串口与相机资源释放要在异常与退出路径上保持一致。
- 涉及现场路径、串口、模型文件的改动，建议统一做配置化，避免硬编码扩散。

### 7. 本地构建说明（当前仓库环境）
- 项目为典型 Windows .NET Framework 工程。
- 在当前 Linux 容器中执行 `dotnet build` 会因缺少 **.NET Framework 4.8 Reference Assemblies** 失败。
- 建议在 Windows + Visual Studio 环境中进行完整构建、联机调试与联机设备验证。
