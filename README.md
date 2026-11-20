# FFIV PR Enemy Drop Rate Control

Adjust enemy item drop rates or guarantee ordinary drops in Final Fantasy IV Pixel Remaster.

## Requirements

- Final Fantasy IV Pixel Remaster on Steam for Windows x64.
- BepInEx 6 for Unity IL2CPP (Unity.IL2CPP-win-x64).

## Installation

### Set up BepInEx

1. Follow the [official Unity IL2CPP installation guide](https://docs.bepinex.dev/master/articles/user_guide/installation/unity_il2cpp.html). Choose the `Unity.IL2CPP-win-x64` build and extract it into the folder that contains `FINAL FANTASY IV.exe`.
2. If this is your first launch with BepInEx, start the game once to let it create its folders, then close the game.

### Install the mod

1. Download the mod ZIP from [GitHub Releases](https://github.com/EscarvalOokt/EnemyDropRateControl/releases) or from [Nexus Mods](https://www.nexusmods.com/terraria/mods/245)
2. With the game closed, extract the ZIP into `BepInEx/plugins` in the game directory.

The installed layout should look like this:

```
FINAL FANTASY IV PR\
└── BepInEx\
    └── plugins\
        └── EnemyDropRateControl\
            └── EnemyDropRateControl.dll[/code]
```
3. Start the game. The mod creates `BepInEx/config/ffivpr.enemy-drop-rate-control.cfg`.

For a source build, create the `BepInEx/plugins/EnemyDropRateControl` folder and copy the DLL there.

## Configuration

Open **Config → Boost** to adjust enemy drops. Changes apply to the next loot roll and are saved in the BepInEx config for all save slots.

- **Enemy Drop Rate** offers ×0, ×0.5, ×1, ×2, ×5, ×10 and ×20. The default is ×1. With the guarantee off, ×0 disables ordinary drops and ×2 doubles the base chance. The resulting chance is rounded to a whole percent and capped at 100%. Even at 100%, a roll can still select an empty slot in the enemy's drop list.
- **Guaranteed Enemy Drops** defaults to Off. When On, it overrides the multiplier. Each enemy that gets a loot roll yields one entry from its available drops, in the quantity listed for that entry. Available drops keep their relative chances. Enemies with no available drops still drop nothing. Turning the guarantee off restores the selected multiplier.

Guaranteed story rewards are preserved in both modes.

If you prefer to edit the settings by hand or the mod's options are missing from Boost, close the game and open `BepInEx/config/ffivpr.enemy-drop-rate-control.cfg`. Set `DropRateMultiplier` and `GuaranteedEnemyDrops` under `[Drops]`, then restart the game. Use a finite, non-negative multiplier with a decimal point for fractions (for example, `0.5`). Set the guarantee to `true` or `false`. An invalid multiplier such as `0,5` uses ×1 and produces a warning in the BepInEx log (usually `BepInEx/LogOutput.log`). A valid custom multiplier stays active until you choose a preset in the menu.

A read-only config still loads. Changes made in Boost apply for the current session. Make the file writable to save changes across restarts.

## Updating and uninstalling

To update, close the game and extract the new ZIP into `BepInEx/plugins`, replacing the existing files. If you installed the DLL directly in `BepInEx/plugins`, remove that copy before extracting the ZIP.

To restore the original drop rates, set **Enemy Drop Rate** to ×1 and **Guaranteed Enemy Drops** to Off. To uninstall, close the game and remove the `BepInEx/plugins/EnemyDropRateControl` folder. You can optionally delete the configuration file.

## For developers

- [Building from source](docs/development.md)
- [Testing](docs/testing.md)