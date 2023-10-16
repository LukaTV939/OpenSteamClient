using System;
using System.Runtime.InteropServices;
using OpenSteamworks.Enums;
using OpenSteamworks.Structs;

namespace OpenSteamworks.Callbacks.Structs;

[StructLayout(LayoutKind.Sequential, Pack = SteamClient.Pack)]
public unsafe struct HTML_BrowserRestarted_t
{
	public HHTMLBrowser unBrowserHandle;
	public HHTMLBrowser unOldBrowserHandle;
};
