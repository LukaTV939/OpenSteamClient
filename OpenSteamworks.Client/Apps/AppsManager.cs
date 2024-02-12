
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenSteamworks;
using OpenSteamworks.Attributes;
using OpenSteamworks.Callbacks.Structs;
using OpenSteamworks.Client.Apps.Assets;
using OpenSteamworks.Client.Apps.Compat;
using OpenSteamworks.Client.Config;
using OpenSteamworks.Client.Managers;
using OpenSteamworks.Client.Utils;
using OpenSteamworks.Client.Utils.DI;
using OpenSteamworks.ClientInterfaces;
using OpenSteamworks.Enums;
using OpenSteamworks.Generated;
using OpenSteamworks.KeyValue;
using OpenSteamworks.KeyValue.ObjectGraph;
using OpenSteamworks.KeyValue.Deserializers;
using OpenSteamworks.KeyValue.Serializers;
using OpenSteamworks.Messaging;
using OpenSteamworks.Structs;
using OpenSteamworks.Utils;
using static OpenSteamworks.Callbacks.CallbackManager;

namespace OpenSteamworks.Client.Apps;

public class AppPlaytimeChangedEventArgs : EventArgs {
    public AppPlaytimeChangedEventArgs(uint appid, AppPlaytime_t playtime) { AppID = appid; PlaytimeAllTime = TimeSpan.FromMinutes(playtime.AllTime); PlaytimeLastTwoWeeks = TimeSpan.FromMinutes(playtime.LastTwoWeeks); }
    public uint AppID { get; }
    public TimeSpan PlaytimeAllTime { get; }
    public TimeSpan PlaytimeLastTwoWeeks { get; }
}

public class AppLastPlayedChangedEventArgs : EventArgs {
    public AppLastPlayedChangedEventArgs(uint appid, UInt32 lastPlayed) { AppID = appid; LastPlayed = DateTimeOffset.FromUnixTimeSeconds(lastPlayed).DateTime; }
    public uint AppID { get; }
    public DateTime LastPlayed { get; }
}

public class AppsManager : ILogonLifetime
{
    private readonly ISteamClient steamClient;
    private readonly ClientMessaging clientMessaging;
    private readonly Logger logger;
    private readonly InstallManager installManager;
    private readonly Container container;
    private CompatManager compatManager => container.Get<CompatManager>();

    public readonly ClientApps ClientApps;

    public EventHandler<AppPlaytimeChangedEventArgs>? AppPlaytimeChanged;
    public EventHandler<AppLastPlayedChangedEventArgs>? AppLastPlayedChanged;

    // These apps cause issues. They have no appinfo sections, so they're blacklisted. Apps that fail to initialize during startup are also automatically added to this list.
    private static readonly HashSet<AppId_t> appsFilter = new() { 5, 7, 753, 3482, 346790, 375350, 470950, 472500, 483470, 503590, 561370, 957691, 957692, 972340, 977941, 1275680, 1331320, 2130210, 2596140 };

    /// <summary>
    /// Gets ALL owned AppIDs of the current user. Includes all configs. Will probably show 1000+ apps.
    /// </summary>
    public HashSet<AppId_t> OwnedAppIDs {
        get {
            IncrementingUIntArray iua = new(256);
            iua.RunToFit(() => this.steamClient.IClientUser.GetSubscribedApps(iua.Data, iua.UIntLength, false));
            return iua.Data.Select(a => (AppId_t)a).Except(appsFilter).ToHashSet();
        }
    }

    /// <summary>
    /// Gets all the user's Games, Tools, etc. Does not include DLC, Configs and other backend types
    /// </summary>
    public HashSet<AppId_t> OwnedApps {
        get {
            return GetAllUserApps().Where(a => steamClient.IClientUser.BIsSubscribedApp(a.AppID)).Select(a => a.AppID).ToHashSet();
        }
    }

    public HashSet<AppId_t> InstalledApps {
        get {
            var len = this.steamClient.IClientAppManager.GetNumInstalledApps();
            var arr = new uint[len];
            this.steamClient.IClientAppManager.GetInstalledApps(arr, len);
            return arr.Select(a => (AppId_t)a).ToHashSet();
        }
    }

    //TODO: streamable apps
    public HashSet<AppId_t> ReadyToPlayApps {
        get {
            var apps = InstalledApps;
            Console.WriteLine("apps: " + apps.Count);
            apps = apps.Where(this.ClientApps.BIsAppUpToDate).ToHashSet();
            Console.WriteLine("apps2: " + apps.Count);
            return apps;
        }
    }
    
    private Dictionary<AppId_t, RTime32> appLastPlayedMap = new();
    private Dictionary<AppId_t, AppPlaytime_t> appPlaytimeMap = new();

    public HashSet<AppId_t> PlayedApps {
        get {
            HashSet<AppId_t> apps = new();
            //TODO: There's probably a better way
            foreach (var item in appLastPlayedMap)
            {
                if (item.Value == 0) {
                    continue;
                }

                apps.Add(item.Key);
            }

            return apps;
        }
    }

    public HashSet<AppId_t> UnplayedApps {
        get {
            HashSet<AppId_t> apps = OwnedApps;
            apps = apps.Except(PlayedApps).ToHashSet();
            return apps;
        }
    }

    public AppsManager(ISteamClient steamClient, Container container, ClientApps clientApps, ClientMessaging clientMessaging, InstallManager installManager) {
        this.logger = Logger.GetLogger("AppsManager", installManager.GetLogPath("AppsManager"));
        this.container = container;
        this.ClientApps = clientApps;
        this.steamClient = steamClient;
        this.clientMessaging = clientMessaging;
        this.installManager = installManager;
        steamClient.CallbackManager.RegisterHandler<AppMinutesPlayedDataNotice_t>(OnAppMinutesPlayedDataNotice);
        steamClient.CallbackManager.RegisterHandler<AppLastPlayedTimeChanged_t>(OnAppLastPlayedTimeChanged);
    }

    public void OnAppMinutesPlayedDataNotice(CallbackHandler<AppMinutesPlayedDataNotice_t> handler, AppMinutesPlayedDataNotice_t notice) {
        UInt32 allTime = 0;
        UInt32 lastTwoWeeks = 0;
        if (steamClient.IClientUser.BGetAppMinutesPlayed(notice.m_nAppID, ref allTime, ref lastTwoWeeks))
        {
            this.AppPlaytimeChanged?.Invoke(this, new AppPlaytimeChangedEventArgs(notice.m_nAppID, new AppPlaytime_t(allTime, lastTwoWeeks)));
        }
    }

    public void OnAppLastPlayedTimeChanged(CallbackHandler<AppLastPlayedTimeChanged_t> handler, AppLastPlayedTimeChanged_t lastPlayedTimeChanged) {
        AppLastPlayedChanged?.Invoke(this, new AppLastPlayedChangedEventArgs(lastPlayedTimeChanged.m_nAppID, lastPlayedTimeChanged.m_lastPlayed));
    }

    public async Task OnLoggedOn(IExtendedProgress<int> progress, LoggedOnEventArgs e) {
        foreach (var item in OwnedAppIDs)
        {
            try
            {
                GetApp(new CGameID(item));
            }
            catch (System.Exception e2)
            {
                appsFilter.Add(item);
                logger.Warning("Failed to initialize owned app " + item + " at logon time");
                logger.Warning(e2);
            }
        }

        if (steamClient.ConnectedWith == ConnectionType.NewClient) {
            unsafe {
                CUtlMap<AppId_t, RTime32> mapn = new(0, 4096);
                if (steamClient.IClientUser.BGetAppsLastPlayedMap(&mapn)) {
                    appLastPlayedMap = mapn.ToManagedAndFree();
                } else {
                    mapn.Free();
                }
            }

            unsafe {
                CUtlMap<AppId_t, AppPlaytime_t> mapn = new(0, 4096);
                if (steamClient.IClientUser.BGetAppPlaytimeMap(&mapn)) {
                    appPlaytimeMap = mapn.ToManagedAndFree();
                } else {
                    mapn.Free();
                }
            }
        } else {
            foreach (var ownedAppID in OwnedApps)
            {
                var lastPlayed = steamClient.IClientUser.GetAppLastPlayedTime(ownedAppID);
                if (lastPlayed != 0) {
                    appLastPlayedMap[ownedAppID] = lastPlayed;
                }

                var playtime = this.steamClient.ClientConfigStore.GetInt(EConfigStore.UserLocal, $"Software\\Valve\\Steam\\Apps\\{ownedAppID}\\Playtime") ?? 0;
                var playtime2wks = this.steamClient.ClientConfigStore.GetInt(EConfigStore.UserLocal, $"Software\\Valve\\Steam\\Apps\\{ownedAppID}\\Playtime2wks") ?? 0;
                if (playtime != 0 || playtime2wks != 0) {
                    appPlaytimeMap[ownedAppID] = new AppPlaytime_t((uint)playtime, (uint)playtime2wks);
                }
            }
        }
    }

    private readonly List<AppBase> apps = new();
        
    /// <summary>
    /// Gets all "user apps", which includes games, apps, betas, tools
    /// </summary>
    public IEnumerable<AppBase> GetAllUserApps() {
        return GetAllAppsOfTypes(new[] {EAppType.Application, EAppType.Beta, EAppType.Game, EAppType.Tool});
    }

    public IEnumerable<AppBase> GetAllAppsOfTypes(EAppType[] types) {
        return GetAllApps().Where(a => types.Contains(a.Type));
    }

    public IEnumerable<AppBase> GetAllApps() {
        return apps.ToList();
    }

    /// <summary>
    /// Gets an app. Initializes it synchronously if not previously initialized.
    /// </summary>
    /// <param name="gameid"></param>
    /// <returns></returns>
    public AppBase GetApp(CGameID gameid) {
        var existingApp = apps.Where((a) => a.GameID == gameid).FirstOrDefault();
        if (existingApp != null) {
            return existingApp;
        }

        var app = AppBase.CreateSteamApp(gameid.AppID);

        // There's too many configs and demo's to store them all in memory, so let's not (and demos as "demo" types seems to be deprecated anyway)
        if (!(app.Type == EAppType.Config || app.Type == EAppType.Demo)) {
            apps.Add(app);
        }
        
        return app;
    }

    public async Task OnLoggingOff(IExtendedProgress<int> progress) {
        appLastPlayedMap.Clear();
        appPlaytimeMap.Clear();
        apps.Clear();
        await Task.CompletedTask;
    }

    public void Kill(CGameID gameid) {
        this.steamClient.IClientUser.TerminateGame(gameid, true);
    }

    /// <summary>
    /// May return '' or 'public' depending on the phase of the moon, angle of the sun and some other unknown factors (public seems to be the correct behaviour, does '' stand for failure?)
    /// </summary>
    public string GetBetaForApp(AppId_t appid) {
        IncrementingStringBuilder betaName = new();
        betaName.RunUntilFits(() => steamClient.IClientAppManager.GetActiveBeta(appid, betaName.Data, betaName.Length));
        return betaName.ToString();
    }

    /// <summary>
    /// Gets all owned apps for a SteamID. 
    /// Will not work in offline mode. Yet. TODO: we need a robust caching system.
    /// </summary>
    /// <param name="steamid">SteamID of the user to request apps for</param>
    public async Task<HashSet<AppId_t>> GetAppsForSteamID(CSteamID steamid, bool includeSteamPackageGames = false, bool includeFreeGames = true) {
        logger.Debug("Attempting to get owned apps for " + steamid);
        ProtoMsg<Protobuf.CPlayer_GetOwnedGames_Request> request = new("Player.GetOwnedGames#1");
        request.body.Steamid = steamid;
        request.body.IncludeAppinfo = false;
        request.body.IncludeExtendedAppinfo = false;
        request.body.IncludeFreeSub = includeSteamPackageGames;
        request.body.IncludePlayedFreeGames = includeFreeGames;

        ProtoMsg<Protobuf.CPlayer_GetOwnedGames_Response> response;
        HashSet<AppId_t> ownedApps = new();
        using (var conn = this.clientMessaging.AllocateConnection())
        {
            response = await conn.ProtobufSendMessageAndAwaitResponse<Protobuf.CPlayer_GetOwnedGames_Response, Protobuf.CPlayer_GetOwnedGames_Request>(request);
        }

        foreach (var protoApp in response.body.Games)
        {
            // Why the fuck is the AppID field an int here?????
            ownedApps.Add((uint)protoApp.Appid);
        }

        logger.Debug(steamid + " owns " + ownedApps.Count + " games");
        return ownedApps;
    }

    public async Task RunInstallScriptAsync(AppId_t appid, bool uninstall = false) {
        await Task.Run(() => RunInstallScriptSync(appid, uninstall));
    }

    public void RunInstallScriptSync(AppId_t appid, bool uninstall = false) {
        //TODO: we still aren't 100% sure about the second arg.
        steamClient.IClientUser.RunInstallScript(appid, "english", false);
        while (steamClient.IClientUser.IsInstallScriptRunning() != 0)
        {
            Thread.Sleep(30);
        }
    }

    public void SetDefaultCompatToolForApp(CGameID gameid) {
        string defaultCompatTool = this.steamClient.IClientCompat.GetCompatToolName(0);
        if (string.IsNullOrEmpty(defaultCompatTool)) {
            throw new InvalidOperationException("Can't set default config tool for app " + gameid + ", since no default compat tool has been specified!");
        }

        this.SetCompatToolForApp(gameid, defaultCompatTool);
    }

    public void DisableCompatToolForApp(CGameID gameid) {
        this.steamClient.IClientCompat.SpecifyCompatTool(gameid.AppID, "", "", 0);
    }

    public string GetCurrentCompatToolForApp(CGameID gameid) {
        if (!this.steamClient.IClientCompat.BIsCompatibilityToolEnabled(gameid.AppID)) {
            return "";
        }
        
        return this.steamClient.IClientCompat.GetCompatToolName(gameid.AppID);
    }

    public void SetCompatToolForApp(CGameID gameid, string compatToolName) {
        this.steamClient.IClientCompat.SpecifyCompatTool(gameid.AppID, compatToolName, "", 250);
    }

    public void SetDefaultCompatTool(string compatToolName) {
        this.steamClient.IClientCompat.SpecifyCompatTool(0, compatToolName, "", 75);
    }

    public bool IsAppInstalled(AppId_t appid) {
        return InstalledApps.Contains(appid);
    }

    public async Task<EAppUpdateError> LaunchApp(AppId_t app, int launchOption = -1, string userLaunchOptions = "") {
        return await LaunchApp(GetApp(new CGameID(app)), launchOption, userLaunchOptions);
    }

    public async Task<EAppUpdateError> LaunchApp(AppBase app, int launchOption, string userLaunchOptions) {
        return await app.Launch(userLaunchOptions, launchOption);
    }
    
    public Logger GetLoggerForApp(AppBase app) {
        string name;
        if (app.IsSteamApp) {
            name = "SteamApp";
        } else if (app.IsMod) {
            name = "Mod";
        } else if (app.IsShortcut) {
            name = "Shortcut";
        } else {
            name = "App";
        }

        name += app.GameID;
        return Logger.GetLogger(name, installManager.GetLogPath(name));
    }

    public UInt64 StartCompatSession(AppId_t appID) => this.steamClient.IClientCompat.StartSession(appID);

    /// <summary>
    /// Gets the current effective OS for an app. 
    /// If the game has compat tools enabled, it will use the compat tool's target OS, otherwise the current os.
    /// </summary>
    /// <param name="appID"></param>
    /// <returns></returns>
    public string GetCurrentEffectiveOSForApp(AppId_t appID)
    {
        if (compatManager.IsCompatEnabled && compatManager.IsCompatEnabledForApp(appID)) {
            return compatManager.GetPlatformStringForCompatTool(compatManager.GetCompatToolForApp(appID));
        }

        return UtilityFunctions.GetSteamPlatformString();
    }
}