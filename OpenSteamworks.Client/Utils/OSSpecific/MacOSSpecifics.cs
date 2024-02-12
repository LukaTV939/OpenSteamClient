using System.Runtime.Versioning;

namespace OpenSteamworks.Client.Utils.OSSpecific;

[SupportedOSPlatform("osx")]
public class MacOSSpecifics : IOSSpecifics {
    public string SteamClientBinaryName => "steamclient.dylib";
    public string SteamClientManifestName => "steam_client_osx";
}