using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Last.UI;
using Last.UI.KeyInput;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace EnemyDropRateControl;

internal static class BoostMenuPatch
{
    private const ConfigCommandType MultiplierType = (ConfigCommandType)7251;
    private const ConfigCommandType GuaranteedType = (ConfigCommandType)7252;
    private const string MultiplierObjectName = "EnemyDropRateControl.Multiplier";
    private const string GuaranteedObjectName = "EnemyDropRateControl.Guaranteed";

    private static readonly double[] _presets = { 0, 0.5, 1, 2, 5, 10, 20 };
    private static ManualLogSource _log = null!;

    internal static void Apply(Harmony harmony, ManualLogSource log)
    {
        _log = log;
        try
        {
            Patch(harmony, nameof(ConfigActualDetailsControllerBase.SetCommandUseType), prefix: nameof(ConfigureRow));
            Patch(harmony, nameof(ConfigActualDetailsControllerBase.SetCommandNameList), prefix: nameof(ConfigureRow));
            Patch(
                harmony,
                nameof(ConfigActualDetailsControllerBase.SwitchArrowSelectTypeProcess),
                postfix: nameof(SaveChoice));
            Patch(
                harmony,
                nameof(ConfigActualDetailsControllerBase.SwitchDescriptionText),
                prefix: nameof(DescribeRow));
            Patch(harmony, nameof(ConfigActualDetailsControllerBase.InitializeCommandList), prefix: nameof(InsertRows));
        }
        catch (Exception error)
        {
            log.LogWarning($"Enemy drop options could not be added to Boost. Use the BepInEx config instead. {error}");
        }
    }

    private static void Patch(Harmony harmony, string target, string? prefix = null, string? postfix = null)
    {
        MethodInfo original = AccessTools.DeclaredMethod(typeof(ConfigActualDetailsControllerBase), target) ??
                              throw new MissingMethodException(
                                  typeof(ConfigActualDetailsControllerBase).FullName,
                                  target);
        harmony.Patch(
            original,
            prefix is null ? null : new HarmonyMethod(typeof(BoostMenuPatch), prefix),
            postfix is null ? null : new HarmonyMethod(typeof(BoostMenuPatch), postfix));
    }

    // ReSharper disable once InconsistentNaming
    private static void InsertRows(ConfigActualDetailsControllerBase __instance)
    {
        List<ConfigCommandController>? commands = __instance.commandList;
        if (commands is null)
        {
            return;
        }

        int anchorIndex = -1;
        int multiplierIndex = -1;
        int guaranteedIndex = -1;
        for (var i = 0; i < commands.Count; i++)
        {
            ConfigCommandController? row = commands[i];
            switch (IdentifyCommand(row))
            {
                case MultiplierType:
                    multiplierIndex = i;
                    break;
                case GuaranteedType:
                    guaranteedIndex = i;
                    break;
                case ConfigCommandType.ExpRate:
                case ConfigCommandType.GilRate:
                    anchorIndex = i;
                    break;
            }
        }

        if (anchorIndex < 0 || (multiplierIndex >= 0 && guaranteedIndex >= 0))
        {
            return;
        }

        ConfigCommandController? addedMultiplier = null;
        ConfigCommandController? addedGuaranteed = null;
        try
        {
            ConfigCommandController anchor = commands[anchorIndex];
            int previousIndex = multiplierIndex >= 0 ? multiplierIndex : anchorIndex;
            int neighborIndex = guaranteedIndex >= 0 ? guaranteedIndex : previousIndex;
            int offset = guaranteedIndex >= 0 ? 0 : 1;
            Transform neighbor = commands[neighborIndex].transform;
            Transform parent = neighbor.parent;
            int insertAt = neighborIndex + offset;
            int siblingIndex = neighbor.GetSiblingIndex() + offset;

            if (multiplierIndex < 0)
            {
                addedMultiplier = CloneRow(anchor, parent, MultiplierType, MultiplierObjectName);
                addedMultiplier.transform.SetSiblingIndex(siblingIndex++);
                commands.Insert(insertAt++, addedMultiplier);
            }

            if (guaranteedIndex < 0)
            {
                addedGuaranteed = CloneRow(anchor, parent, GuaranteedType, GuaranteedObjectName);
                addedGuaranteed.transform.SetSiblingIndex(siblingIndex);
                commands.Insert(insertAt, addedGuaranteed);
            }
        }
        catch (Exception error)
        {
            RemoveRow(commands, addedGuaranteed);
            RemoveRow(commands, addedMultiplier);
            _log.LogWarning($"Could not initialize the enemy drop rows in Boost. {error}");
        }
    }

    private static ConfigCommandController CloneRow(
        ConfigCommandController anchor,
        Transform parent,
        ConfigCommandType type,
        string name)
    {
        GameObject? clone = UnityObject.Instantiate(anchor.gameObject, parent);
        try
        {
            ConfigCommandController? row = clone.GetComponent<ConfigCommandController>();
            if (row == null || row.commandsData == null || !row.commandsData.transform.IsChildOf(clone.transform))
            {
                throw new InvalidOperationException("The cloned Boost row does not own its command data.");
            }

            clone.name = name;
            row.commandsData.configCommandType = type;
            row.nameList = new List<string>();
            return row;
        }
        catch
        {
            UnityObject.Destroy(clone);
            throw;
        }
    }

    private static void RemoveRow(List<ConfigCommandController> commands, ConfigCommandController? row)
    {
        if (row != null)
        {
            commands.Remove(row);
            UnityObject.Destroy(row.gameObject);
        }
    }

    private static bool ConfigureRow(ConfigCommandController __0)
    {
        ConfigCommandType type = GetModCommandType(__0);
        if (type == ConfigCommandType.None)
        {
            return true;
        }

        var choices = new List<string>();
        string title;
        int index;
        if (type == MultiplierType)
        {
            title = "Enemy Drop Rate";
            string current = FormatMultiplier(ModSettings.DropRateMultiplier);
            foreach (double preset in _presets)
            {
                choices.Add(FormatMultiplier(preset));
            }

            index = choices.IndexOf(current);
            if (index < 0)
            {
                index = choices.Count;
                choices.Add(current);
            }
        }
        else
        {
            title = "Guaranteed Enemy Drops";
            choices.Add("Off");
            choices.Add("On");
            index = ModSettings.GuaranteedEnemyDrops ? 1 : 0;
        }

        __0.nameList = choices;
        __0.targetIndex = index;
        __0.SetNameText(title);
        __0.SetArrowChangeText(choices[index]);
        __0.SetEnableArrowSelectType(true);
        return false;
    }

    private static void SaveChoice(ConfigCommandController __0)
    {
        ConfigCommandType type = GetModCommandType(__0);
        if (type == ConfigCommandType.None)
        {
            return;
        }

        List<string>? choices = __0.nameList;
        int index = __0.targetIndex;
        if (choices is null || index < 0 || index >= choices.Count)
        {
            return;
        }

        try
        {
            if (type == GuaranteedType)
            {
                ModSettings.GuaranteedEnemyDrops = index == 1;
            }
            else if (TryParseMultiplier(choices[index], out double multiplier))
            {
                ModSettings.DropRateMultiplier = multiplier;
            }
        }
        catch (Exception error)
        {
            _log.LogWarning($"Could not save the enemy drop setting. {error}");
        }
    }

    // ReSharper disable InconsistentNaming
    private static bool DescribeRow(
            ConfigActualDetailsControllerBase __instance,
            ConfigCommandType __0,
            ref string __result)
        // ReSharper restore InconsistentNaming
    {
        if ((__0 != MultiplierType && __0 != GuaranteedType) || GetModCommandType(__instance.SelectedCommand) != __0)
        {
            return true;
        }

        __result = __0 == MultiplierType
            ? "Scale the chance of regular enemy drops. Applies when Guaranteed Enemy Drops is off."
            : "Get one item from each enemy with eligible loot. Overrides Enemy Drop Rate.";
        return false;
    }

    private static ConfigCommandType IdentifyCommand(ConfigCommandController? row)
    {
        if (row == null || row.commandsData == null)
        {
            return ConfigCommandType.None;
        }

        ConfigCommandType type = row.commandsData.configCommandType;
        return type switch
        {
            MultiplierType when row.gameObject.name != MultiplierObjectName => ConfigCommandType.None,
            GuaranteedType when row.gameObject.name != GuaranteedObjectName => ConfigCommandType.None,
            _ => type
        };
    }

    private static ConfigCommandType GetModCommandType(ConfigCommandController? row)
    {
        ConfigCommandType type = IdentifyCommand(row);
        return type is MultiplierType or GuaranteedType ? type : ConfigCommandType.None;
    }

    internal static string FormatMultiplier(double multiplier)
    {
        return "×" + MultiplierText.Format(multiplier);
    }

    internal static bool TryParseMultiplier(string label, out double multiplier)
    {
        multiplier = 0;
        return label.StartsWith('×') && MultiplierText.TryParse(label.AsSpan(1), out multiplier);
    }
}