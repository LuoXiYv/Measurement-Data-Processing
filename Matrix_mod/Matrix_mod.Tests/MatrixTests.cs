using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Matrix_mod.Tests;

[TestClass]
public class MatrixTests
{
    [TestMethod]
    public void Add_Matrix_AddsValues()
    {
        var a = Matrix.CreateZero(2, 2);
        a.SetValue(0, 0, 1);
        a.SetValue(0, 1, 2);
        a.SetValue(1, 0, 3);
        a.SetValue(1, 1, 4);

        var b = Matrix.CreateZero(2, 2);
        b.SetValue(0, 0, 5);
        b.SetValue(0, 1, 6);
        b.SetValue(1, 0, 7);
        b.SetValue(1, 1, 8);

        var result = a + b;

        Assert.AreEqual(6, result[0, 0]);
        Assert.AreEqual(8, result[0, 1]);
        Assert.AreEqual(10, result[1, 0]);
        Assert.AreEqual(12, result[1, 1]);
    }

    [TestMethod]
    public void Multiply_Matrix_MultipliesValues()
    {
        var a = Matrix.CreateZero(2, 3);
        a.SetValue(0, 0, 1);
        a.SetValue(0, 1, 2);
        a.SetValue(0, 2, 3);
        a.SetValue(1, 0, 4);
        a.SetValue(1, 1, 5);
        a.SetValue(1, 2, 6);

        var b = Matrix.CreateZero(3, 2);
        b.SetValue(0, 0, 7);
        b.SetValue(0, 1, 8);
        b.SetValue(1, 0, 9);
        b.SetValue(1, 1, 10);
        b.SetValue(2, 0, 11);
        b.SetValue(2, 1, 12);

        var result = a * b;

        Assert.AreEqual(58, result[0, 0]);
        Assert.AreEqual(64, result[0, 1]);
        Assert.AreEqual(139, result[1, 0]);
        Assert.AreEqual(154, result[1, 1]);
    }

    [TestMethod]
    public void Inverse_Matrix_ReturnsInverse()
    {
        var a = Matrix.CreateZero(2, 2);
        a.SetValue(0, 0, 4);
        a.SetValue(0, 1, 7);
        a.SetValue(1, 0, 2);
        a.SetValue(1, 1, 6);

        var inv = a.Inverse();

        Assert.IsTrue(AreClose(0.6, inv[0, 0]));
        Assert.IsTrue(AreClose(-0.7, inv[0, 1]));
        Assert.IsTrue(AreClose(-0.2, inv[1, 0]));
        Assert.IsTrue(AreClose(0.4, inv[1, 1]));
    }

    [TestMethod]
    public void Parse_Matrix_ParsesText()
    {
        var text = "1 2\n3 4";
        var ok = MatrixParser.TryParse(text, out var matrix, out var error);

        Assert.IsTrue(ok, error);
        Assert.IsNotNull(matrix);
        Assert.AreEqual(2, matrix!.Rows);
        Assert.AreEqual(2, matrix.Cols);
        Assert.AreEqual(3, matrix[1, 0]);
    }

    private static bool AreClose(double expected, double actual, double tolerance = 1e-9)
    {
        return Math.Abs(expected - actual) < tolerance;
    }
}
