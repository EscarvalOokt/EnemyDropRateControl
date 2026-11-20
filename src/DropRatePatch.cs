using System;
using HarmonyLib;
using Last.Battle;
using Last.Data;
using Last.Data.Master;
using Last.Defaine;
using Last.Systems;

namespace EnemyDropRateControl;

[HarmonyPatch(typeof(BattleItemDropPlug), nameof(BattleItemDropPlug.Lot), new[] { typeof(Monster) })]
internal static class DropRatePatch
{
    internal static void Apply(Harmony harmony)
    {
        harmony.CreateClassProcessor(typeof(DropRatePatch)).Patch();
    }

    [HarmonyPrefix]
    // ReSharper disable InconsistentNaming
    private static bool Prefix(Monster? __0, ref DropItemData __result, out RollState __state)
        // ReSharper restore InconsistentNaming
    {
        __state = default;
        if (__0 is null)
        {
            return true;
        }

        if (ModSettings.GuaranteedEnemyDrops)
        {
            __result = RollGuaranteed(__0);
            return false;
        }

        double multiplier = ModSettings.DropRateMultiplier;
        if (!multiplier.Equals(ModSettings.DefaultMultiplier))
        {
            int originalRate = __0.DropRate;
            int effectiveRate = DropRules.ScaleDropRate(originalRate, multiplier);
            if (effectiveRate != originalRate)
            {
                __state = new RollState(__0, originalRate);
                __0.DropRate = effectiveRate;
            }
        }

        return true;
    }

    private static DropItemData RollGuaranteed(Monster monster)
    {
        // Lot consumes a chance roll even at 100%. Keep the game's RNG sequence.
        CommonUtility.GetBoolRand(100);

        ReadOnlySpan<int> contentIds = stackalloc int[]
        {
            monster.DropContentId1, monster.DropContentId2, monster.DropContentId3, monster.DropContentId4,
            monster.DropContentId5, monster.DropContentId6, monster.DropContentId7, monster.DropContentId8
        };
        int[] weights =
        {
            GameConstantIntUtility.GetValue1ByKey(GameConstants.MONSTER_DROP_RATE_1),
            GameConstantIntUtility.GetValue1ByKey(GameConstants.MONSTER_DROP_RATE_2),
            GameConstantIntUtility.GetValue1ByKey(GameConstants.MONSTER_DROP_RATE_3),
            GameConstantIntUtility.GetValue1ByKey(GameConstants.MONSTER_DROP_RATE_4),
            GameConstantIntUtility.GetValue1ByKey(GameConstants.MONSTER_DROP_RATE_5),
            GameConstantIntUtility.GetValue1ByKey(GameConstants.MONSTER_DROP_RATE_6),
            GameConstantIntUtility.GetValue1ByKey(GameConstants.MONSTER_DROP_RATE_7),
            GameConstantIntUtility.GetValue1ByKey(GameConstants.MONSTER_DROP_RATE_8)
        };
        int slot = DropRules.ChooseGuaranteedSlot(
            contentIds,
            weights,
            static eligibleWeights => CommonUtility.GetRandWeight(eligibleWeights));
        if (slot < 0)
        {
            return new DropItemData(0, 0);
        }

        int quantity = slot switch
        {
            0 => monster.DropContentId1Value,
            1 => monster.DropContentId2Value,
            2 => monster.DropContentId3Value,
            3 => monster.DropContentId4Value,
            4 => monster.DropContentId5Value,
            5 => monster.DropContentId6Value,
            6 => monster.DropContentId7Value,
            _ => monster.DropContentId8Value
        };
        return new DropItemData(contentIds[slot], quantity);
    }

    [HarmonyFinalizer]
    // ReSharper disable once InconsistentNaming
    private static void Finalizer(RollState __state)
    {
        if (__state.Monster is { } monster)
        {
            monster.DropRate = __state.OriginalRate;
        }
    }

    private readonly record struct RollState(Monster? Monster, int OriginalRate);
}