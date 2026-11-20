using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace EnemyDropRateControl;

[BepInPlugin(PluginId, "FFIV PR Enemy Drop Rate Control", "1.0.0")]
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Plugin : BasePlugin
{
    private const string PluginId = "ffivpr.enemy-drop-rate-control";

    public override void Load()
    {
        ModSettings.Bind(Config, Log);

        var harmony = new Harmony(PluginId);
        DropRatePatch.Apply(harmony);
        BoostMenuPatch.Apply(harmony, Log);
    }
}