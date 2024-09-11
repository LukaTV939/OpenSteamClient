using System;
using System.Runtime.InteropServices;
using OpenSteamworks.Attributes;
using OpenSteamworks.Data.Enums;
using OpenSteamworks.Data.Structs;
using OpenSteamworks.Data;

namespace OpenSteamworks.Callbacks.Structs;

[Callback(4505)]
[StructLayout(LayoutKind.Sequential, Pack = SteamClient.Pack)]
public struct HTML_URLChanged_t
{
	public HHTMLBrowser unBrowserHandle;
	public string pchURL;
	public string pchPostData;
	public bool bIsRedirect;
	public string pchPageTitle;
	public bool bNewNavigation;
}
