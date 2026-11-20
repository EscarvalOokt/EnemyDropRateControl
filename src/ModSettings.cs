using System;
using System.IO;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace EnemyDropRateControl;

internal static class ModSettings
{
    internal const double DefaultMultiplier = 1.0;

    private static ConfigEntry<string> _dropRateMultiplier = null!;
    private static ConfigEntry<bool> _guaranteedEnemyDrops = null!;

    internal static double DropRateMultiplier
    {
        get => MultiplierText.TryParse(_dropRateMultiplier.Value, out double value) ? value : DefaultMultiplier;
        set => _dropRateMultiplier.Value = MultiplierText.Format(value);
    }

    internal static bool GuaranteedEnemyDrops
    {
        get => _guaranteedEnemyDrops.Value;
        set => _guaranteedEnemyDrops.Value = value;
    }

    internal static void Bind(ConfigFile config, ManualLogSource log)
    {
        // BepInEx can save while Bind is still loading the entries from disk.
        bool autoSave = config.SaveOnConfigSet;
        config.SaveOnConfigSet = false;
        try
        {
            _dropRateMultiplier = config.Bind(
                "Drops",
                "DropRateMultiplier",
                MultiplierText.Format(DefaultMultiplier),
                "Multiplies the chance of regular enemy drops. 1 uses the game's default rates and 0 disables regular drops. " +
                "Use a decimal point for fractional values, for example 0.5. " +
                "Changes in Config → Boost take effect immediately. Restart the game after editing this file.");

            _guaranteedEnemyDrops = config.Bind(
                "Drops",
                "GuaranteedEnemyDrops",
                false,
                "Guarantees one item from each eligible enemy's drop pool. Overrides DropRateMultiplier. " +
                "Changes in Config → Boost take effect immediately. Restart the game after editing this file.");
        }
        finally
        {
            config.SaveOnConfigSet = autoSave;
        }

        if (!MultiplierText.TryParse(_dropRateMultiplier.Value, out _))
        {
            log.LogWarning(
                "DropRateMultiplier must be a finite, non-negative number with a decimal point for fractions " +
                "(for example 0.5). Using the game's default rates.");
        }

        if (autoSave)
        {
            try
            {
                config.Save();
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                log.LogWarning(
                    $"Could not write {config.ConfigFilePath}. The loaded settings are active. {error.Message}");
            }
        }
    }
}