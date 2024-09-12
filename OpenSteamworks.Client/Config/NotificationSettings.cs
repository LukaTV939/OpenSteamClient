using OpenSteamworks.Data.Enums;
using OpenSteamworks.Client.Config.Attributes;
using System.Text.Json.Serialization.Metadata;

namespace OpenSteamworks.Client.Config;

public class NotificationSettings: IConfigFile<NotificationSettings> {
	static JsonTypeInfo<NotificationSettings> IConfigFile<NotificationSettings>.JsonTypeInfo => ConfigJsonContext.Default.NotificationSettings;
    static string IConfigFile<NotificationSettings>.ConfigName => "NotificationSettings_001";
    static bool IConfigFile<NotificationSettings>.PerUser => true;
    static bool IConfigFile<NotificationSettings>.AlwaysSave => false;

    [ConfigName("Enable notifications", "#NotificationSettings_EnableNotifications")]
    [ConfigDescription("Allows notifications to be sent", "#NotificationSettings_EnableNotificationsDesc")]
    public ELanguage Language { get; set; } = ELanguage.None;
}