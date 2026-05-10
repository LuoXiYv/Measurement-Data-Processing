# 矩阵计算示例

这是一个基于 WPF 的矩阵计算小工具，包含矩阵类与常见运算（加、减、乘、求逆），并提供简单的输入输出界面。

## 输入格式

- 行与行之间使用换行分隔。
- 列内使用空格、逗号或制表符分隔。

示例：
```
1 2 3
4 5 6
```

## 功能说明

- 随机生成矩阵：根据输入的行列数，生成 1~100 的随机矩阵。
- 矩阵运算：支持 A+B、A-B、A×B。
- 矩阵求逆：支持 A 或 B 的逆矩阵（要求方阵且可逆）。
- 矩阵转置：支持 A 或 B 的转置。

## 运行与测试

- 直接打开 `Matrix_mod.sln` 并运行 WPF 项目。
- 运行测试：
```
dotnet test .\Matrix_mod.Tests\Matrix_mod.Tests.csproj
```

## 代码结构

- `Matrix_mod\Matrix.cs`：矩阵类实现（使用一维数组存储）。
- `Matrix_mod\MatrixParser.cs`：矩阵文本解析。
- `Matrix_mod\MainWindow.xaml`：界面布局。
- `Matrix_mod\MainWindow.xaml.cs`：界面逻辑。
- `Matrix_mod.Tests\MatrixTests.cs`：单元测试示例。
