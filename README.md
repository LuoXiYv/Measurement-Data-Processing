# Measurement Data Processing（测量数据处理与程序设计）

这是南京邮电大学测绘工程专业《测量数据处理及程序设计》课程作业仓库。

仓库里包含多个小项目，每个项目都以**独立文件夹**存在，并在各自目录下提供 `README.md` 与简要说明文档，便于直接下载运行、用于实验报告参考或二次开发。

> 说明：本仓库开源仅用于学习交流。你当然可以将其用于自己的实验作业提交或者借鉴，但如果你不想你的代码和老学长一样“高度相似”被当场逮捕，建议你在思路、界面、输入输出、注释与参数设置等方面做出自己的修改与完善。

## 项目清单

- `Triangle_Closure_Error_Statistics/`（MDP_1）
  - 三角形闭合差统计与相关指标计算。
  - 具体说明见该目录下的 README。

- `Accuracy_Evaluation_of_Leveling_Survey_Observation_Data/`（MDP_2）
  - 水准测量观测数据的平差与精度评定（WPF 桌面程序）。
  - 含示例数据：`sample_example.csv`。

- `Matrix_mod/`
  - 一个矩阵运算/矩阵解析相关的小项目，并包含对应的单元测试工程。

## 环境与运行

这些项目主要是 C# / .NET（部分为 WPF 桌面）。推荐环境：

- Windows 10/11
- Visual Studio 2022 或 JetBrains Rider
- 对应项目 `*.csproj` 所需的 .NET SDK（以项目文件设置为准）

一般可以通过打开对应的 `*.sln` 直接运行；或在命令行进入项目目录后执行 `dotnet build` / `dotnet test`（若包含测试项目）。

## 目录结构约定

为避免把本机构建路径（例如 `E:\NJUPT\...`）带进仓库，本仓库已忽略构建产物与 IDE 缓存：

- `**/bin/`
- `**/obj/`
- `.vs/`, `.idea/`


## License

未特别声明时，默认仅用于学习交流；如需用于课程之外的用途，请自行评估并注明来源。

