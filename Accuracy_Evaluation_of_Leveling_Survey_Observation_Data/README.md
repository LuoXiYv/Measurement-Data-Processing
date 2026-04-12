# MDP_2 - 水准测量观测数据精度评价工具

本项目是一个 WPF 小工具，用于完成水准测量实验中的数据读取、计算与结果展示。

## 功能

- 点类(`LPointClass`)与观测边类(`LineClass`)数据结构。
- 自动读取 CSV（`[Points]` + `[Edges]` 双分段格式）。
- 支持示例数据一键加载。
- 计算并输出 4 类结果：
  1. 未知点(1-4等)初始高程。
  2. 高差闭合差与限差判定。
  3. 往返差精度指标(`di`, `di^2`, `p*di^2`)。
  4. 按距离配赋改正后的高差与平差高程。
- 图形区显示简化高程剖面图。

## 项目结构

```text
MDP_2/
  MDP_2.sln
  README.md
  sample_example315.csv
  MDP_2/
    MainWindow.xaml
    MainWindow.xaml.cs
    Models/
      LPointClass.cs
      LineClass.cs
    Services/
      CsvDataLoader.cs
      LevelingAdjustmentService.cs
```

## 核心类说明

- `LPointClass`：点数据模型。
  - 关键字段：`PID`、`H`、`InitialH`、`AdjustedH`。
  - 控制属性：`IsControlP`、`IsH0`、`IsCommonP`。
  - 实现 `INotifyPropertyChanged`，用于界面数据联动刷新。

- `LineClass`：观测边数据模型。
  - 关键字段：`LID`、`SPID`、`EPID`、`ForwardDH`、`BackwardDH`、`Distance`。
  - 计算属性：`dH`（均值高差）、`Di/DiMm`、`Di2Mm2`、`PDi2Mm2PerKm`。
  - 结果字段：`Correction`、`CorrectedDH`。

- `CsvDataLoader`：CSV 解析与示例数据提供。
  - `Parse`：读取 `[Points]`/`[Edges]` 两段并生成点、边集合。
  - `GetEmbeddedExampleCsv`：返回内置示例文本。

- `LevelingAdjustmentService`：计算服务。
  - `Compute`：完成初始高程估计、精度指标计算、闭合差检查、改正数配赋与平差高程求解。

- `MainWindow`：界面与交互控制。
  - 负责导入数据、触发计算、刷新表格、输出结果文本、绘制简图。

## CSV 格式

```csv
[Points]
PID,H,IsControlP,IsCommonP
PA,12.248,true,false
1,,false,false
...
PB,10.505,true,false

[Edges]
LID,SPID,EPID,ForwardDH,BackwardDH,Distance
1,PA,1,3.248,3.240,4.0
...
```

- `H` 为空表示未知点，界面默认初始值 `10000m`。
- `Distance` 单位为 `km`。
- `ForwardDH`/`BackwardDH` 单位为 `m`，程序取均值作为测段高差。

## 快速运行

1. 打开 `MDP_2.sln`。
2. 运行后点击“加载示例数据”或“导入CSV”。
3. 点击“计算”查看结果文本与简图。

示例文件：`sample_example.csv`。
