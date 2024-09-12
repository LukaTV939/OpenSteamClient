using OpenSteamworks.Data.Enums;
using OpenSteamworks.Client.Config.Attributes;
using OpenSteamworks.Data;
using System.Text.Json.Serialization.Metadata;

namespace OpenSteamworks.Client.Config;

public class LibrarySettings: IConfigFile<LibrarySettings> {
	static JsonTypeInfo<LibrarySettings> IConfigFile<LibrarySettings>.JsonTypeInfo => ConfigJsonContext.Default.LibrarySettings;
    static string IConfigFile<LibrarySettings>.ConfigName => "LibrarySettings_001";
    static bool IConfigFile<LibrarySettings>.PerUser => true;
    static bool IConfigFile<LibrarySettings>.AlwaysSave => true;

    [ConfigNeverVisible]
    public AppId_t LastSelectedAppID { get; set; } = 0;
}