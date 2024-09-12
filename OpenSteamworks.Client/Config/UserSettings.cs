using OpenSteamworks.Data.Enums;
using OpenSteamworks.Client.Config.Attributes;
using OpenSteamworks.Client.Enums;
using System.Text.Json.Serialization.Metadata;

namespace OpenSteamworks.Client.Config;

public class UserSettings: IConfigFile<UserSettings> {
	static JsonTypeInfo<UserSettings> IConfigFile<UserSettings>.JsonTypeInfo => ConfigJsonContext.Default.UserSettings;
    static string IConfigFile<UserSettings>.ConfigName => "UserSettings_001";
    static bool IConfigFile<UserSettings>.PerUser => true;
    static bool IConfigFile<UserSettings>.AlwaysSave => false;

    [ConfigName("Language", "#UserSettings_Language")]
    [ConfigDescription("Sets the language of the client.", "#UserSettings_LanguageDesc")]
    public ELanguage Language { get; set; } = ELanguage.None;

    [ConfigName("Auto-login to friends network", "#UserSettings_AutologinFriendsNetwork")]
    [ConfigDescription("Automatically login to the friends network when logging in", "#UserSettings_AutologinFriendsNetworkDesc")]
    public bool LoginToFriendsNetworkAutomatically { get; set; } = true;

    [ConfigName("Data-rate units", "#UserSettings_DownloadUnit")]
    [ConfigDescription("The data-rate units to use for displaying download speeds", "#UserSettings_DownloadUnitDesc")]
    public DataRateUnit DownloadDataRateUnit { get; set; } = DataRateUnit.Auto_Gbps_Mbps_Kbps_bits;
}