using System.Text.Json.Serialization.Metadata;

namespace OpenSteamworks.Client.Config;

public interface IConfigFile<T> {
	public abstract static JsonTypeInfo<T> JsonTypeInfo { get; }
    public abstract static string ConfigName { get; }
    public abstract static bool PerUser { get; }
    public abstract static bool AlwaysSave { get; }
}