//==========================  AUTO-GENERATED FILE  ================================
//
// This file is partially auto-generated.
// If functions are removed, your changes to that function will be lost.
// Parameter types and names however are preserved if the function stays unchanged.
// Feel free to change parameters to be more accurate. 
//=============================================================================

using System;
using System.Text;
using OpenSteamworks.Attributes;
using OpenSteamworks.Enums;

using OpenSteamworks.Protobuf;
using OpenSteamworks.Structs;

namespace OpenSteamworks.Generated;

public unsafe interface IClientUtils
{
    public string GetInstallPath();  // argc: 0, index: 1, ipc args: [], ipc returns: [string]
    public string GetUserBaseFolderInstallImage();  // argc: 0, index: 2, ipc args: [], ipc returns: [string]
    public string GetUserBaseFolderPersistentStorage();  // argc: 0, index: 3, ipc args: [], ipc returns: [string]
    public string GetManagedContentRoot();  // argc: 0, index: 4, ipc args: [], ipc returns: [string]
    public RTime32 GetSecondsSinceAppActive();  // argc: 0, index: 5, ipc args: [], ipc returns: [bytes4]
    public RTime32 GetSecondsSinceComputerActive();  // argc: 0, index: 6, ipc args: [], ipc returns: [bytes4]
    public void SetComputerActive();  // argc: 0, index: 7, ipc args: [], ipc returns: []
    public EUniverse GetConnectedUniverse();  // argc: 0, index: 8, ipc args: [], ipc returns: [bytes4]
    public ESteamRealm GetSteamRealm();  // argc: 0, index: 9, ipc args: [], ipc returns: [bytes4]
    public RTime32 GetServerRealTime();  // argc: 0, index: 10, ipc args: [], ipc returns: [bytes4]
    public string GetIPCountry();  // argc: 0, index: 11, ipc args: [], ipc returns: [string]
    // WARNING: Arguments are unknown!
    public unknown_ret GetImageSize();  // argc: 3, index: 12, ipc args: [bytes4], ipc returns: [bytes1, bytes4, bytes4]
    // WARNING: Arguments are unknown!
    public unknown_ret GetImageRGBA();  // argc: 3, index: 13, ipc args: [bytes4, bytes4], ipc returns: [bytes1, bytes_length_from_mem]
    public unknown_ret GetNumRunningApps();  // argc: 0, index: 14, ipc args: [], ipc returns: [bytes4]
    public unknown_ret GetCurrentBatteryPower();  // argc: 0, index: 15, ipc args: [], ipc returns: [bytes1]
    // WARNING: Arguments are unknown!
    public unknown_ret GetBatteryInformation();  // argc: 2, index: 16, ipc args: [], ipc returns: [bytes1, bytes4, bytes1]
    // WARNING: Arguments are unknown!
    public unknown_ret SetOfflineMode(bool offline);  // argc: 1, index: 17, ipc args: [bytes1], ipc returns: []
    public bool GetOfflineMode();  // argc: 0, index: 18, ipc args: [], ipc returns: [bytes1]
    // WARNING: Arguments are unknown!
    public unknown_ret SetAppIDForCurrentPipe(AppId_t appid);  // argc: 2, index: 19, ipc args: [bytes4, bytes1], ipc returns: [bytes4]
    public AppId_t GetAppID();  // argc: 0, index: 20, ipc args: [], ipc returns: [bytes4]
    public void SetAPIDebuggingActive(bool active, bool verbose);  // argc: 2, index: 21, ipc args: [bytes1, bytes1], ipc returns: []
    public SteamAPICall_t AllocPendingAPICallHandle();  // argc: 0, index: 22, ipc args: [], ipc returns: [bytes8]
    // WARNING: Arguments are unknown!
    public bool IsAPICallCompleted(SteamAPICall_t handle, ref bool failed);  // argc: 3, index: 23, ipc args: [bytes8], ipc returns: [boolean, boolean]
    // WARNING: Arguments are unknown!
    public ESteamAPICallFailure GetAPICallFailureReason(SteamAPICall_t handle);  // argc: 2, index: 24, ipc args: [bytes8], ipc returns: [bytes4]
    /// <summary>
    /// Gets a result for an api call.
    /// </summary>
    // WARNING: Arguments are unknown!
    public bool GetAPICallResult(SteamAPICall_t handle, void* callbackData, int callbackDataMax, int expectedCallbackID, out bool failed);  // argc: 6, index: 25, ipc args: [bytes8, bytes4, bytes4], ipc returns: [bytes1, bytes_length_from_mem, bytes1]
    // WARNING: Arguments are unknown!
    public unknown_ret SetAPICallResultWithoutPostingCallback(SteamAPICall_t handle, byte[] responseData, int responseDataLen, int responseCallbackID);  // argc: 5, index: 26, ipc args: [bytes8, bytes4, bytes4, bytes_length_from_mem], ipc returns: []
    public unknown_ret SignalAppsToShutDown();  // argc: 0, index: 27, ipc args: [], ipc returns: [bytes1]
    public unknown_ret SignalServiceAppsToDisconnect();  // argc: 0, index: 28, ipc args: [], ipc returns: [bytes1]
    // WARNING: Arguments are unknown!
    public unknown_ret TerminateAllApps();  // argc: 1, index: 29, ipc args: [bytes1], ipc returns: [bytes1]
    public uint GetCellID();  // argc: 0, index: 30, ipc args: [], ipc returns: [bytes4]
    public bool BIsGlobalInstance();  // argc: 0, index: 31, ipc args: [], ipc returns: [boolean]
    // WARNING: Arguments are unknown!
    public SteamAPICall_t CheckFileSignature(string filename);  // argc: 1, index: 32, ipc args: [string], ipc returns: [bytes8]
    public unknown_ret GetBuildID();  // argc: 0, index: 33, ipc args: [], ipc returns: [bytes8]
    public void SetCurrentUIMode(EUIMode mode);  // argc: 1, index: 34, ipc args: [bytes4], ipc returns: []
    public EUIMode GetCurrentUIMode();  // argc: 0, index: 35, ipc args: [], ipc returns: [bytes4]
    public bool BIsWebBasedUIMode();  // argc: 0, index: 36, ipc args: [], ipc returns: [boolean]
    public void SetDisableOverlayScaling(bool val);  // argc: 1, index: 37, ipc args: [bytes1], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret ShutdownLauncher();  // argc: 2, index: 38, ipc args: [bytes1, bytes1], ipc returns: []
    // WARNING: Arguments are unknown!
    public void SetLauncherType(ELauncherType type);  // argc: 1, index: 39, ipc args: [bytes4], ipc returns: []
    public ELauncherType GetLauncherType();  // argc: 0, index: 40, ipc args: [], ipc returns: [bytes4]
    // WARNING: Arguments are unknown!
    public unknown_ret ShowGamepadTextInput();  // argc: 5, index: 41, ipc args: [bytes4, bytes4, string, bytes4, string], ipc returns: [bytes1]
    public unknown_ret GetEnteredGamepadTextLength();  // argc: 0, index: 42, ipc args: [], ipc returns: [bytes4]
    // WARNING: Arguments are unknown!
    public unknown_ret GetEnteredGamepadTextInput();  // argc: 2, index: 43, ipc args: [bytes4], ipc returns: [bytes1, bytes_length_from_mem]
    // WARNING: Arguments are unknown!
    public unknown_ret GamepadTextInputClosed();  // argc: 3, index: 44, ipc args: [bytes4, bytes1, string], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret ShowControllerLayoutPreview();  // argc: 3, index: 45, ipc args: [bytes4, bytes8], ipc returns: []
    // WARNING: Arguments are unknown!
    public void SetSpew(ESpewGroup group, int spewlevel, int loglevel);  // argc: 3, index: 46, ipc args: [bytes4, bytes4, bytes4], ipc returns: []
    public bool BDownloadsDisabled();  // argc: 0, index: 47, ipc args: [], ipc returns: [boolean]
    // WARNING: Arguments are unknown!
    public unknown_ret SetFocusedWindow(CGameID gameid, UInt64 windowId, bool unk);  // argc: 4, index: 48, ipc args: [bytes8, bytes1, bytes1, bytes4], ipc returns: []
    public string GetSteamUILanguage();  // argc: 0, index: 49, ipc args: [], ipc returns: [string]
    public SteamAPICall_t CheckSteamReachable();  // argc: 0, index: 50, ipc args: [], ipc returns: [bytes8]
    /// <summary>
    /// TODO: What does this do?
    /// </summary>
    /// <param name="launchmethod">EGameLaunchMethod</param>
    public void SetLastGameLaunchMethod(int launchmethod);  // argc: 1, index: 51, ipc args: [bytes4], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret SetVideoAdapterInfo();  // argc: 7, index: 52, ipc args: [bytes4, bytes4, bytes4, bytes4, bytes4, bytes4, string], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret SetOverlayWindowFocusForPipe();  // argc: 3, index: 53, ipc args: [bytes1, bytes1, bytes8], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret GetGameOverlayUIInstanceFocusGameID();  // argc: 3, index: 54, ipc args: [], ipc returns: [bytes8, bytes1, bytes4]
    // WARNING: Arguments are unknown!
    public unknown_ret GetFocusedGameWindow();  // argc: 3, index: 55, ipc args: [], ipc returns: [bytes8, bytes1, bytes4]
    // WARNING: Arguments are unknown!
    public unknown_ret SetControllerConfigFileForAppID(AppId_t appid, string config);  // argc: 2, index: 56, ipc args: [bytes4, string], ipc returns: [bytes1]
    // WARNING: Arguments are unknown!
    public unknown_ret GetControllerConfigFileForAppID(AppId_t appid, StringBuilder config, int configMax);  // argc: 3, index: 57, ipc args: [bytes4, bytes4], ipc returns: [bytes1, bytes_length_from_mem]
    public bool IsSteamRunningInVR();  // argc: 0, index: 58, ipc args: [], ipc returns: [boolean]
    public unknown_ret StartVRDashboard();  // argc: 0, index: 59, ipc args: [], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret IsVRHeadsetStreamingEnabled(AppId_t appid);  // argc: 1, index: 60, ipc args: [bytes4], ipc returns: [boolean]
    // WARNING: Arguments are unknown!
    public unknown_ret SetVRHeadsetStreamingEnabled();  // argc: 2, index: 61, ipc args: [bytes4, bytes1], ipc returns: []
    public SteamAPICall_t GenerateSupportSystemReport();  // argc: 0, index: 62, ipc args: [], ipc returns: [bytes8]
    public bool GetSupportSystemReport(StringBuilder str, int strMax, byte[]? bytes, int bytesMax);  // argc: 4, index: 63, ipc args: [bytes4, bytes4], ipc returns: [bytes1, bytes_length_from_mem, bytes_length_from_mem]
    // WARNING: Arguments are unknown!
    public unknown_ret GetAppIdForPid(AppId_t appid, bool unk);  // argc: 2, index: 64, ipc args: [bytes4, bytes1], ipc returns: [bytes4]
    public unknown_ret SetClientUIProcess();  // argc: 0, index: 65, ipc args: [], ipc returns: []
    public bool BIsClientUIInForeground();  // argc: 0, index: 66, ipc args: [], ipc returns: [boolean]
    // WARNING: Arguments are unknown!
    public void AllowSetForegroundThroughWebhelper(bool val);  // argc: 1, index: 67, ipc args: [bytes4], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret SetOverlayBrowserInfo();  // argc: 8, index: 68, ipc args: [bytes4, bytes4, bytes8, bytes4, bytes4, bytes4, bytes4, bytes4], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret ClearOverlayBrowserInfo(AppId_t appid);  // argc: 1, index: 69, ipc args: [bytes4], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret GetOverlayBrowserInfo();  // argc: 3, index: 70, ipc args: [bytes4], ipc returns: [bytes1, bytes_length_from_reg, bytes4]
    // WARNING: Arguments are unknown!
    public unknown_ret SetOverlayNotificationPosition(AppId_t appid, ENotificationPosition pos);  // argc: 2, index: 71, ipc args: [bytes4, bytes4], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret SetOverlayNotificationInset(AppId_t appid, int nHorizontalInset, int nVerticalInset);  // argc: 3, index: 72, ipc args: [bytes4, bytes4, bytes4], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret DispatchClientUINotification(EClientUINotificationType type, string data, uint unk);  // argc: 3, index: 73, ipc args: [bytes4, string, bytes4], ipc returns: [bytes1]
    // WARNING: Arguments are unknown!
    public unknown_ret RespondToClientUINotification();  // argc: 3, index: 74, ipc args: [bytes4, bytes1, bytes4], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret DispatchClientUICommand(string data, uint unk);  // argc: 2, index: 75, ipc args: [string, bytes4], ipc returns: []
    public unknown_ret DispatchComputerActiveStateChange();  // argc: 0, index: 76, ipc args: [], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret DispatchOpenURLInClient(string data, uint unk, bool unk2);  // argc: 3, index: 77, ipc args: [string, bytes4, bytes1], ipc returns: []
    // WARNING: Arguments are unknown!
    public SteamAPICall_t UpdateWideVineCDM(string maybePath);  // argc: 1, index: 78, ipc args: [string], ipc returns: [bytes8]
    public unknown_ret DispatchClearAllBrowsingData();  // argc: 0, index: 79, ipc args: [], ipc returns: []
    public unknown_ret DispatchClientSettingsChanged();  // argc: 0, index: 80, ipc args: [], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret DispatchClientPostMessage(string unk, string unk1, string unk2);  // argc: 3, index: 81, ipc args: [string, string, string], ipc returns: [bytes4]
    public bool IsSteamChina();  // argc: 0, index: 82, ipc args: [], ipc returns: [boolean]
    // WARNING: Arguments are unknown!
    public bool NeedsSteamChinaWorkshop(AppId_t app);  // argc: 1, index: 83, ipc args: [bytes4], ipc returns: [bytes1]
    public bool InitFilterText(AppId_t appid, uint filterOptions = 0);  // argc: 2, index: 84, ipc args: [bytes4, bytes4], ipc returns: [bytes1]
    // WARNING: Arguments are unknown!
    public int FilterText(AppId_t appid, ETextFilteringContext context, CSteamID senderSteamID, string msg, StringBuilder msgOut, int maxMsgOut);  // argc: 7, index: 85, ipc args: [bytes4, bytes4, uint64, string, bytes4], ipc returns: [bytes4, bytes_length_from_mem]
    // WARNING: Arguments are unknown!
    public unknown_ret GetIPv6ConnectivityState(ESteamIPv6ConnectivityProtocol protocol);  // argc: 1, index: 86, ipc args: [bytes4], ipc returns: [bytes4]
    // WARNING: Arguments are unknown!
    public unknown_ret ScheduleConnectivityTest();  // argc: 2, index: 87, ipc args: [bytes4, bytes1], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret GetConnectivityTestState();  // argc: 1, index: 88, ipc args: [], ipc returns: [bytes4, bytes16]
    public string GetCaptivePortalURL();  // argc: 0, index: 89, ipc args: [], ipc returns: [string]
    // WARNING: Arguments are unknown!
    public unknown_ret RecordSteamInterfaceCreation(string unk, string unk1);  // argc: 2, index: 90, ipc args: [string, string], ipc returns: []
    public ECloudGamingPlatform GetCloudGamingPlatform();  // argc: 0, index: 91, ipc args: [], ipc returns: [bytes4]
    // WARNING: Arguments are unknown!
    public unknown_ret BGetMacAddresses();  // argc: 3, index: 92, ipc args: [bytes4], ipc returns: [boolean, bytes_length_from_reg, bytes4]
    // WARNING: Arguments are unknown!
    public unknown_ret BGetDiskSerialNumber(StringBuilder builder, int maxOut);  // argc: 2, index: 93, ipc args: [bytes4], ipc returns: [boolean, bytes_length_from_mem]
    // WARNING: Arguments are unknown!
    public unknown_ret GetSteamEnvironmentForApp(AppId_t appid, StringBuilder buf, int bufMax);  // argc: 3, index: 94, ipc args: [bytes4, bytes4], ipc returns: [bytes4, bytes_length_from_mem]
    // WARNING: Arguments are unknown!
    public unknown_ret TestHTTP(string unk);  // argc: 1, index: 95, ipc args: [string], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret DumpJobs(string unk);  // argc: 1, index: 96, ipc args: [string], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret ShowFloatingGamepadTextInput();  // argc: 6, index: 97, ipc args: [bytes4, bytes4, bytes4, bytes4, bytes4, bytes4], ipc returns: [bytes1]
    // WARNING: Arguments are unknown!
    public unknown_ret DismissFloatingGamepadTextInput();  // argc: 1, index: 98, ipc args: [bytes4], ipc returns: [bytes1]
    // WARNING: Arguments are unknown!
    public unknown_ret DismissGamepadTextInput();  // argc: 1, index: 99, ipc args: [bytes4], ipc returns: [bytes1]
    public unknown_ret FloatingGamepadTextInputDismissed();  // argc: 0, index: 100, ipc args: [], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret SetGameLauncherMode();  // argc: 2, index: 101, ipc args: [bytes4, bytes1], ipc returns: []
    public unknown_ret ClearAllHTTPCaches();  // argc: 0, index: 102, ipc args: [], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret GetFocusedGameID();  // argc: 1, index: 103, ipc args: [], ipc returns: [bytes8]
    public uint GetFocusedWindowPID();  // argc: 0, index: 104, ipc args: [], ipc returns: [bytes4]
    // WARNING: Arguments are unknown!
    public unknown_ret SetWebUITransportWebhelperPID(uint pid);  // argc: 1, index: 105, ipc args: [bytes4], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret GetWebUITransportInfo();  // argc: 1, index: 106, ipc args: [], ipc returns: [bytes1, protobuf]
    // WARNING: Arguments are unknown!
    public unknown_ret RecordFakeReactRouteMetric(string unk);  // argc: 1, index: 107, ipc args: [string], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret SteamRuntimeSystemInfo([IPCOut] CUtlBuffer* data);  // argc: 1, index: 108, ipc args: [], ipc returns: [bytes8, utlbuffer]
    // WARNING: Arguments are unknown!
    public unknown_ret DumpHTTPClients();  // argc: 1, index: 109, ipc args: [bytes4], ipc returns: []
    // WARNING: Arguments are unknown!
    public unknown_ret BGetMachineID([IPCOut] CUtlBuffer* data);  // argc: 1, index: 110, ipc args: [], ipc returns: [boolean, utlbuffer]
    public unknown_ret NotifyMissingInterface(string interfaceName);  // argc: 1, index: 111, ipc args: [string], ipc returns: []
    public unknown_ret IsSteamInTournamentMode();  // argc: 0, index: 112, ipc args: [], ipc returns: [boolean]
}