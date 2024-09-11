using System;
using System.Runtime.InteropServices;
using OpenSteamworks.Attributes;
using OpenSteamworks.Data.Enums;
using OpenSteamworks.Data.Structs;
using OpenSteamworks.Data;

namespace OpenSteamworks.Callbacks.Structs;

[Callback(4511)]
[StructLayout(LayoutKind.Sequential, Pack = SteamClient.Pack)]
public struct HTML_HorizontalScroll_t
{
	public HHTMLBrowser unBrowserHandle;
	public UInt32 unScrollMax;
	public UInt32 unScrollCurrent;
	public float flPageScale;
	public bool bVisible;
	public UInt32 unPageSize;
}
