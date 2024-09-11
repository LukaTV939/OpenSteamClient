using OpenSteamworks.Data.Enums;
using OpenSteamworks.Client.Config.Attributes;

namespace OpenSteamworks.Client.Config;

public class BootstrapperState : IConfigFile
{
    static string IConfigFile.ConfigName => "BootstrapperState_001";
    static bool IConfigFile.PerUser => false;
    static bool IConfigFile.AlwaysSave => false;

    [ConfigNeverVisible]
    public uint InstalledVersion { get; set; } = 0;

    [ConfigNeverVisible]
    public string CommitHash { get; set; } = "";

    [ConfigName("Skip file verification", "#BootstrapperState_SkipVerification")]
    [ConfigDescription("Skips all file verification in the bootstrapper.", "#BootstrapperState_SkipVerificationDesc")]
    [ConfigCategory("Bootstrapper", "#BootstrapperState_Category_Bootstrapper")]
    [ConfigAdvanced]
    public bool SkipVerification { get; set; } = false;

    [ConfigName("Custom natives path", "#BootstrapperState_CustomNativesPath")]
    [ConfigDescription("Sets the path where natives are copied from. Leave empty to use default.", "#BootstrapperState_CustomNativesPathDesc")]
    [ConfigCategory("Bootstrapper", "#BootstrapperState_Category_Bootstrapper")]
    [ConfigAdvanced]
    public string CustomNativesPath { get; set; } = string.Empty;

    [ConfigNeverVisible]
    public Dictionary<string, long> InstalledFiles { get; set; } = new();

    [ConfigNeverVisible]
    public Dictionary<string, string> LinuxRuntimeChecksums { get; set; } = new();

    [ConfigNeverVisible]
    public bool LinuxPermissionsSet { get; set; } = false;

    [ConfigNeverVisible]
    public bool LastConfigLinkSuccess { get; set; } = false;

    [ConfigName("Local package server URL", "#BootstrapperState_LocalPackageServerURL")]
    [ConfigDescription("Download package files from this package server if the versions are compatible.", "#BootstrapperState_LocalPackageServerURLDesc")]
    [ConfigCategory("Bootstrapper", "#BootstrapperState_Category_Bootstrapper")]
    [ConfigAdvanced]
    public string LocalPackageServerURL { get; set; } = "http://localhost:8125/client/";
}