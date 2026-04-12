# 三角形闭合差统计程序（WPF）

本项目用于完成“按区间统计三角形闭合差”的实验任务，支持：

- 读取闭合差数据（原始值模式、分组计数模式）
- 按分组宽度 `d=0.2` 统计 `-Δ` 与 `+Δ` 的个数、频率、频率密度
- 导出统计结果到 `CSV` 或 `TXT`
- 在界面中用折线图展示统计分布（红色 `-Δ`，蓝色 `+Δ`）

---

## 1. 程序功能

1. 数据读取：从 `.csv/.txt` 文件读取闭合差。
2. 统计计算：按区间计算个数 `v_i`、频率 `v_i/n`、频率密度 `(v_i/n)/d`。
3. 结果展示：在表格中展示每个区间的 `-Δ` 与 `+Δ` 统计值。
4. 结果导出：可导出为 `CSV` 或 `TXT`，便于实验报告使用。
5. 图形描述：使用 WPF `Canvas` 绘制频率折线图，观察分布形态。

---

## 2. 类的定义及使用方法

### 2.1 `ClosureIntervalRow`（统计结果行模型）

文件：`MDP_1/Models/ClosureIntervalRow.cs`

作用：表示一个误差区间对应的一行统计结果。

主要属性：

- `IntervalLabel`：区间标签（如 `0.20~0.40`、`>2.60`）
- `NegativeCount` / `PositiveCount`：`-Δ`、`+Δ` 个数
- `NegativeFrequency` / `PositiveFrequency`：频率
- `NegativeDensity` / `PositiveDensity`：频率密度

### 2.2 `TriangleClosureStatistics`（三角形闭合差统计类）

文件：`MDP_1/Models/TriangleClosureStatistics.cs`

作用：核心统计类，负责把闭合差序列分箱并计算统计量。

主要属性：

- `BinWidth`：分组宽度 `d`
- `MaxAbsoluteValue`：统计上限（本程序为 `2.6`）
- `Rows`：每个区间的统计结果集合
- `NegativeTotalCount` / `PositiveTotalCount`：负、正闭合差总数

主要方法：

- `CreateFromValues(values, binWidth, maxAbsoluteValue)`：从闭合差数组生成统计结果
- `SumNegativeCounts()` / `SumPositiveCounts()`：个数求和
- `SumNegativeFrequencies()` / `SumPositiveFrequencies()`：频率求和

使用方式（逻辑示意）：

```csharp
var stats = TriangleClosureStatistics.CreateFromValues(values, 0.2, 2.6);
var rows = stats.Rows;
var sumNeg = stats.SumNegativeFrequencies();
```

### 2.3 `TriangleClosureFileHandler`（文件处理类）

文件：`MDP_1/Services/TriangleClosureFileHandler.cs`

作用：负责输入数据读取与统计结果输出。

主要属性：

- `TriangleClosureDifferences`：读取后的闭合差数据集合

主要方法：

- `ReadTriangleClosureDifferences(filePath)`：读取输入文件
- `WriteStatisticsResult(filePath, statistics)`：输出统计文件（自动按扩展名选择 CSV/TXT）

### 2.4 `MainWindow`（操作逻辑层）

文件：`MDP_1/MainWindow.xaml.cs`

作用：只做流程编排，不承担底层统计与文件解析。

按钮流程：

1. `LoadDataButton_Click`：读取文件
2. `CalculateButton_Click`：调用统计类计算并绑定表格
3. `ExportButton_Click`：导出结果
4. `DrawFrequencyChart`：绘制频率折线图

---

## 3. 文件操作的基本原理与方法

### 3.1 输入文件支持两种模式

#### 模式 A：原始闭合差

每个数值都视为一个闭合差样本。

```txt
-0.12
0.34
-0.48, 0.51 0.62
```

#### 模式 B：分组计数（与实验表格对应）

格式：`lower,upper,negativeCount,positiveCount`

```csv
# lower,upper,negativeCount,positiveCount
0.00,0.20,40,37
0.20,0.40,34,36
```

程序会把分组计数“展开”为样本数据后再统一统计。

### 3.2 读取原理

- 逐行读取文本
- 跳过空行、注释行（以 `#` 开头）
- 支持分隔符：逗号、分号、空格、Tab
- 尝试按数字解析（兼容系统文化与 InvariantCulture）

### 3.3 导出原理

- 若输出路径扩展名为 `.csv`，导出逗号分隔表格
- 否则导出文本对齐格式（`.txt`）
- 结果包含每个区间统计值、总数与频率和

---

## 4. 高斯分布的基本特征（结合本程序）

高斯分布（正态分布）常用于描述随机误差，其典型特征：

1. **钟形曲线**：中间高、两侧低。
2. **以均值为中心**：若误差无系统偏差，分布应围绕 0 对称。
3. **标准差决定离散程度**：标准差越大，曲线越“扁平”。
4. **面积为 1**：概率密度函数积分为 1，对应离散统计中的“频率和约为 1”。

在本程序中：

- 通过 `v_i/n` 观察离散频率是否在零点附近更集中
- 通过 `(v_i/n)/d` 近似观察密度曲线形态
- 若 `-Δ` 与 `+Δ` 分布接近对称，通常说明观测误差更接近随机误差模型

---

## 5. 程序测试

### 5.1 已提供样例数据

样例文件：`MDP_1/Data/closure_grouped_sample.csv`（对应题图中的分组个数）

### 5.2 基本功能测试流程

1. 启动程序
2. 点击“读取数据”，选择样例文件
3. 点击“统计计算”，检查：
   - 表格区间是否完整（`0.00~0.20` 到 `>2.60`）
   - 频率和是否接近 1（负、正分别统计）
4. 点击“导出结果”，检查输出文件内容

### 5.3 构建验证

```powershell
cd "E:\NJUPT\Measurement Data Processing\MDP_1"
dotnet build .\MDP_1.sln
```

---

## 6. 主要代码（阅读索引）

- 统计核心类：`MDP_1/Models/TriangleClosureStatistics.cs`
  - `CreateFromValues(...)`
  - `SumNegativeCounts()` / `SumPositiveCounts()`
  - `SumNegativeFrequencies()` / `SumPositiveFrequencies()`

- 文件处理类：`MDP_1/Services/TriangleClosureFileHandler.cs`
  - `ReadTriangleClosureDifferences(...)`
  - `WriteStatisticsResult(...)`

- 界面流程与绘图：`MDP_1/MainWindow.xaml.cs`
  - `LoadDataButton_Click(...)`
  - `CalculateButton_Click(...)`
  - `ExportButton_Click(...)`
  - `DrawFrequencyChart(...)`

---

## 7. 运行方法

```powershell
cd "E:\NJUPT\Measurement Data Processing\MDP_1"
dotnet build .\MDP_1.sln
```

在 IDE 中启动后，按顺序点击：

1. `读取数据`
2. `统计计算`
3. `导出结果`
