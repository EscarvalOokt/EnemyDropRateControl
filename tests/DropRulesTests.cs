using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnemyDropRateControl.Tests;

[TestClass]
public class DropRulesTests
{
    [DataTestMethod]
    [DataRow(6, 0.0, 0)]
    [DataRow(6, 0.5, 3)]
    [DataRow(6, 1.0, 6)]
    [DataRow(6, 2.0, 12)]
    [DataRow(6, 1.37, 8)]
    [DataRow(6, 0.25, 2)]
    [DataRow(6, 20.0, 100)]
    [DataRow(6, double.MaxValue, 100)]
    [DataRow(0, 20.0, 0)]
    [DataRow(100, 0.0, 100)]
    [DataRow(100, 0.5, 100)]
    public void ScalesOnlyOrdinaryDropRates(int baseRate, double multiplier, int expected)
    {
        Assert.AreEqual(expected, DropRules.ScaleDropRate(baseRate, multiplier));
    }

    [TestMethod]
    public void MixedPoolKeepsRelativeWeightsAndDuplicateItemSlots()
    {
        int[] itemIds = { 10, 0, 10, -1, 50, 60, 70, 80 };
        int[] weights = { 43, 31, 19, 7, 0, -2, 0, 5 };
        var draws = 0;

        int slot = DropRules.ChooseGuaranteedSlot(
            itemIds,
            weights,
            eligibleWeights =>
            {
                draws++;
                CollectionAssert.AreEqual(new[] { 43, 0, 19, 0, 0, 0, 0, 5 }, eligibleWeights);
                return 7;
            });

        Assert.AreEqual(1, draws);
        Assert.AreEqual(7, slot);
    }

    [TestMethod]
    public void EmptyPoolAndItemsWithNoPositiveWeightYieldNoDrop()
    {
        int[] itemIds = { 0, 0, -1, 0, 21, 22, 0, 0 };
        int[] weights = { 43, 31, 19, 7, 0, -2, 0, 0 };

        int slot = DropRules.ChooseGuaranteedSlot(
            itemIds,
            weights,
            _ => throw new AssertFailedException("An empty pool must skip the roulette."));

        Assert.AreEqual(-1, slot);
    }

    [DataTestMethod]
    [DataRow(-1)]
    [DataRow(8)]
    [DataRow(7)]
    public void InvalidRouletteResultYieldsNoDrop(int rouletteResult)
    {
        int[] itemIds = { 75, 75, 75, 75, 0, 0, 0, 0 };
        int[] weights = { 43, 31, 19, 7, 0, 0, 0, 0 };

        int slot = DropRules.ChooseGuaranteedSlot(itemIds, weights, _ => rouletteResult);

        Assert.AreEqual(-1, slot);
    }
}