# 测量数据处理及程序设计课程作业（NJUPT）

本仓库用于整理南京邮电大学测绘工程专业课程《测量数据处理及程序设计》的实验项目与代码实现，当前包含两个相对独立的 WPF 子项目：

- 三角形闭合差统计（Triangle Closure Error Statistics）
- 水准测量观测数据精度评价（Accuracy Evaluation of Leveling Survey Observation Data）

项目定位是**学习交流与实验复现参考**，每个子目录内均提供了更细致的说明文档与示例数据。

## 仓库结构

```text
.
├─Triangle_Closure_Error_Statistics/
│  ├─MDP_1/
│  ├─MDP_1.sln
│  └─README.md
├─Accuracy_Evaluation_of_Leveling_Survey_Observation_Data/
│  ├─MDP_2/
│  ├─MDP_2.sln
│  ├─sample_example.csv
│  ├─项目代码介绍.md
│  └─README.md
└─README.md
```

## 子项目概览

### 1) Triangle_Closure_Error_Statistics（MDP_1）

- 任务目标：按区间统计三角形闭合差。
- 核心功能：
  - 读取闭合差数据（原始值模式/分组计数模式）
  - 计算区间个数、频率、频率密度
  - 表格展示与统计结果导出（CSV/TXT）
  - WPF 折线图显示误差分布
- 详细文档：`Triangle_Closure_Error_Statistics/README.md`

### 2) Accuracy_Evaluation_of_Leveling_Survey_Observation_Data（MDP_2）

- 任务目标：水准观测数据读取、精度指标计算与平差结果展示。
- 核心功能：
  - 解析 `[Points]` + `[Edges]` 分段 CSV
  - 计算闭合差与限差判定
  - 计算往返差精度指标并进行改正数配赋
  - 输出平差高程与图形化结果
- 详细文档：`Accuracy_Evaluation_of_Leveling_Survey_Observation_Data/README.md`

## 环境要求

- 操作系统：Windows（WPF）
- SDK：.NET 10（项目目标框架为 `net10.0-windows`）
- IDE（可选）：Visual Studio / Rider

## 快速运行

### 运行 MDP_1

```powershell
cd .\Triangle_Closure_Error_Statistics
dotnet build .\MDP_1.sln
```

### 运行 MDP_2

```powershell
cd .\Accuracy_Evaluation_of_Leveling_Survey_Observation_Data
dotnet build .\MDP_2.sln
```

随后可在 IDE 中启动对应项目，按子项目 README 的流程导入数据并完成计算。

## 数据与说明文档

- MDP_1 示例数据位于 `Triangle_Closure_Error_Statistics/MDP_1/Data/`
- MDP_2 示例数据位于 `Accuracy_Evaluation_of_Leveling_Survey_Observation_Data/sample_example.csv`
- 补充说明见各子项目目录下的 `README.md` 或项目介绍文档

## 学术诚信与使用声明

你可以将本仓库用于课程学习、实验参考和二次开发，但请遵守学术规范：

- 建议先理解算法与流程，再结合自己的实验数据独立完成报告
- 不建议直接复制粘贴后原样提交
- 如用于课程作业提交，请务必进行充分修改并标注参考来源

一句话建议：**借鉴思路可以，原样提交风险很高。**

## 免责声明

本仓库内容仅用于学习与交流，不对任何直接使用本仓库内容导致的学术后果负责。

## 致谢

感谢课程教学与实验训练提供的实践场景，也欢迎同学在理解原理的基础上继续改进实现。
