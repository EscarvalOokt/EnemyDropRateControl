using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnemyDropRateControl.Tests;

[TestClass]
[DoNotParallelize]
public class ConfigFileTests
{
    private static string _directory = null!;
    private string _path = null!;

    [ClassInitialize]
    public static void InitializeBepInEx(TestContext _)
    {
        _directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
        Paths.SetExecutablePath(Path.Combine(_directory, "ConfigTest.exe"));
    }

    [TestInitialize]
    public void CreateConfigPath() => _path = Path.Combine(_directory, Guid.NewGuid() + ".cfg");

    [ClassCleanup]
    public static void Cleanup() => Directory.Delete(_directory, recursive: true);

    [TestMethod]
    public void ReadsExistingValuesFromReadOnlyFile()
    {
        const string contents = "[Drops]\nDropRateMultiplier = 1.37\nGuaranteedEnemyDrops = true\n";
        File.WriteAllText(_path, contents);
        File.SetAttributes(_path, FileAttributes.ReadOnly);
        try
        {
            var config = new ConfigFile(_path, saveOnInit: false);
            using var log = new ManualLogSource("Test");
            ModSettings.Bind(config, log);

            Assert.AreEqual(1.37, ModSettings.DropRateMultiplier);
            Assert.IsTrue(ModSettings.GuaranteedEnemyDrops);
            Assert.IsTrue(config.SaveOnConfigSet);
            Assert.AreEqual(contents, File.ReadAllText(_path));
        }
        finally
        {
            File.SetAttributes(_path, FileAttributes.Normal);
        }
    }

    [TestMethod]
    public void NewConfigAndSubsequentChangesPersistAcrossLoads()
    {
        using var log = new ManualLogSource("Test");
        ModSettings.Bind(new ConfigFile(_path, saveOnInit: false), log);
        Assert.IsTrue(File.Exists(_path));
        Assert.AreEqual(1.0, ModSettings.DropRateMultiplier);
        Assert.IsFalse(ModSettings.GuaranteedEnemyDrops);

        ModSettings.DropRateMultiplier = 1.37;
        ModSettings.GuaranteedEnemyDrops = true;
        ModSettings.Bind(new ConfigFile(_path, saveOnInit: false), log);

        Assert.AreEqual(1.37, ModSettings.DropRateMultiplier);
        Assert.IsTrue(ModSettings.GuaranteedEnemyDrops);
    }

    [TestMethod]
    public void CommaInExistingConfigFallsBackToDefault()
    {
        File.WriteAllText(_path, "[Drops]\nDropRateMultiplier = 1,37\nGuaranteedEnemyDrops = true\n");
        using var log = new ManualLogSource("Test");
        ModSettings.Bind(new ConfigFile(_path, saveOnInit: false), log);

        Assert.AreEqual(1.0, ModSettings.DropRateMultiplier);
        Assert.IsTrue(ModSettings.GuaranteedEnemyDrops);
        StringAssert.Contains(File.ReadAllText(_path), "1,37");
    }
}