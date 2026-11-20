using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnemyDropRateControl.Tests;

[TestClass]
public class MultiplierTextTests
{
    [DataTestMethod]
    [DataRow("0", 0.0)]
    [DataRow("0.5", 0.5)]
    [DataRow("1.37", 1.37)]
    [DataRow("2e1", 20.0)]
    public void ReadsValidConfigValues(string text, double expected)
    {
        Assert.IsTrue(MultiplierText.TryParse(text, out double multiplier));
        Assert.AreEqual(expected, multiplier);
    }

    [DataTestMethod]
    [DataRow("0,5")]
    [DataRow("1,37")]
    [DataRow("1,000")]
    [DataRow("-1")]
    [DataRow("NaN")]
    [DataRow("Infinity")]
    [DataRow("1e999")]
    [DataRow("")]
    public void RejectsInvalidOrAmbiguousValues(string text)
    {
        Assert.IsFalse(MultiplierText.TryParse(text, out _));
    }

    [TestMethod]
    public void CustomValuesRoundTripWithoutLosingPrecision()
    {
        double[] values = { 0.5, 1.37, Math.BitIncrement(1.0), double.Epsilon, double.MaxValue };
        foreach (double value in values)
        {
            string text = MultiplierText.Format(value);
            Assert.IsTrue(MultiplierText.TryParse(text, out double parsed), text);
            Assert.AreEqual(value, parsed, text);
        }
    }
}