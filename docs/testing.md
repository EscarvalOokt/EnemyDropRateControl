# Testing

## Automated tests

From the project root, run the unit tests with a .NET SDK that can target `net6.0` and the .NET 6 runtime. The empty `GameDirectory` override lets the tests run without the game or BepInEx, even if you have a local settings file.

```powershell
dotnet test .\tests\EnemyDropRateControl.Tests.csproj -c Release '-p:GameDirectory='
```

The unit tests cover drop-rate rounding, the 100% cap, story rates, drop pools and custom multipliers. For mixed pools they verify the weights passed to the selector and supply a fixed selection result.

To include the config-file integration tests, follow the [build prerequisites](development.md#prerequisites) and [configure the game directory](development.md#configure-the-game-directory). With `GameDirectory` set in `src/EnemyDropRateControl.Local.props`, run the solution tests from the project root. These tests use BepInEx and temporary files to check defaults, saved values, malformed decimals and read-only configs.

```powershell
dotnet test .\src\EnemyDropRateControl.sln -c Release
```

## In-game checks

Follow [Building from source](development.md) if you're testing a local build. [Set up BepInEx](../README.md#set-up-bepinex) and [install the mod](../README.md#install-the-mod). After launching the game, check that the `Loading [FFIV PR Enemy Drop Rate Control ...]` entry in the BepInEx log (usually `BepInEx/LogOutput.log`) shows the version you installed. Disable other loot modifiers for the battle checks.

| Area | Procedure and expected result |
| --- | --- |
| Boost menu and persistence | Close the game and set `DropRateMultiplier = 1.37` and `GuaranteedEnemyDrops = false` in the [config file](../README.md#configuration). Start the game and open **Config → Boost** twice. Each visit shows one **Enemy Drop Rate** row at ×1.37 followed by one **Guaranteed Enemy Drops** row at Off. Switch the guarantee to On and confirm the multiplier still shows ×1.37. Select ×2, restart the game and verify ×2 and On. |
| Regular battle rewards | Look for battles with only Goblins on the world map outside Baron. Win a separate battle for each combination and check the item rewards after each victory. Expect at least one item at ×20 with the guarantee Off, no items at ×0 with the guarantee Off, and at least one item at ×0 with the guarantee On. |
| Story reward | Load a save from before the Dr. Lugae fight in the Tower of Babel. Set **Enemy Drop Rate** to ×0 and **Guaranteed Enemy Drops** to Off, then defeat him. Check that his key appears in Key Items. Reload the same save, switch the guarantee to On and repeat the fight. The key appears in both runs. |