using System;

namespace EnemyDropRateControl;

internal static class DropRules
{
    internal static int ScaleDropRate(int originalRate, double multiplier)
    {
        if (originalRate is <= 0 or >= 100)
        {
            return originalRate;
        }

        double scaledRate = Math.Min(originalRate * multiplier, 100.0);
        return (int)Math.Round(scaledRate, MidpointRounding.AwayFromZero);
    }

    internal static int ChooseGuaranteedSlot(ReadOnlySpan<int> contentIds, int[] weights, Func<int[], int> selectSlot)
    {
        if (weights.Length != contentIds.Length)
        {
            throw new ArgumentException("Each drop slot must have one weight.", nameof(weights));
        }

        var hasEligibleSlot = false;
        for (var index = 0; index < weights.Length; index++)
        {
            if (contentIds[index] > 0 && weights[index] > 0)
            {
                hasEligibleSlot = true;
            }
            else
            {
                weights[index] = 0;
            }
        }

        if (!hasEligibleSlot)
        {
            return -1;
        }

        int slot = selectSlot(weights);
        return (uint)slot < (uint)weights.Length && weights[slot] > 0 ? slot : -1;
    }
}