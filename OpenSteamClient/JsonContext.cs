using System.Text.Json.Serialization;

namespace OpenSteamClient;

[JsonSerializable(typeof(Translation.Translation))]
internal partial class JsonContext : JsonSerializerContext
{
}