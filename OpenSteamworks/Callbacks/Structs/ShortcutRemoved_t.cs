using System;
using System.Runtime.InteropServices;
using OpenSteamworks.Attributes;
using OpenSteamworks.Data;

namespace OpenSteamworks.Callbacks.Structs;

[Callback(1130002)]
[StructLayout(LayoutKind.Sequential, Pack = SteamClient.Pack)]
public struct ShortcutRemoved_t
{
	public AppId_t m_nAppID;
    public bool m_bRemote;
}