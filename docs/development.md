# Building from source

## Prerequisites

Install a .NET SDK that can target `net6.0` and Final Fantasy IV Pixel Remaster from Steam on Windows x64. Follow [BepInEx setup](../README.md#set-up-bepinex) through the first launch. The build uses assemblies from `BepInEx/core` and the `BepInEx/interop` folder generated on first launch.

## Configure the game directory

If you do not have a local settings file yet, copy the [example file](EnemyDropRateControl.Local.props.example) to `src/EnemyDropRateControl.Local.props` manualy or run the folowing PowerShell command from the repository root:

```powershell
Copy-Item -LiteralPath .\docs\EnemyDropRateControl.Local.props.example -Destination .\src\EnemyDropRateControl.Local.props
```

In `src/EnemyDropRateControl.Local.props`, set `GameDirectory` to the full path of the folder containing `FINAL FANTASY IV.exe`. Steam can open this folder from the game's installed files settings. If the path contains an ampersand, write it as `&amp;` in the XML. Both projects read this file, and Git ignores it so each developer can use their own path.

## Build

Build the solution with the saved path.

```powershell
dotnet build .\src\EnemyDropRateControl.sln -c Release
```

For a one-off build, pass `-p:GameDirectory` to use a different path or build without creating the local settings file. Replace the example path with your game directory. Keep the quotes around the argument in PowerShell.

```powershell
dotnet build .\src\EnemyDropRateControl.sln -c Release '-p:GameDirectory=C:\Games\FINAL FANTASY IV PR'
```

The build produces `src/bin/Release/net6.0/EnemyDropRateControl.dll`. Run the [automated tests](testing.md#automated-tests) or [install the DLL you built](../README.md#install-the-mod) for in-game checks.