using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using OpenSteamworks.Enums;
using OpenSteamworks.Structs;

namespace OpenSteamworks.Client.Apps;

/// <summary>
/// Provides the base class for all kinds of apps (and configs, etc other steam types).
/// </summary>
public abstract class AppBase
{
    /// <summary>
    /// Generic launch option for implementation and display purposes
    /// </summary>
    public interface ILaunchOption {
        public int ID { get; }
        public string Name { get; }
        public string Description { get; }
    }

    public abstract EAppState State { get; }
    public abstract EAppType Type { get; }
    public abstract IEnumerable<ILaunchOption> LaunchOptions { get; }
    public abstract int? DefaultLaunchOptionID { get; }
    public abstract Task<EAppUpdateError> Launch(string userLaunchOptions, int launchOptionID);

    protected abstract string ActualName { get; }
    protected abstract string ActualHeroURL { get; }
    protected abstract string ActualLogoURL { get; }
    protected abstract string ActualIconURL { get; }
    protected abstract string ActualPortraitURL { get; }
    
    public string Name => GetValueOverride(NameOverride, ActualName);
    public string HeroURL => GetValueOverride(HeroOverrideURL, ActualHeroURL);
    public string LogoURL => GetValueOverride(LogoOverrideURL, ActualLogoURL);
    public string IconURL => GetValueOverride(IconOverrideURL, ActualIconURL);
    public string PortraitURL => GetValueOverride(PortraitOverrideURL, ActualPortraitURL);

    public abstract int ChangeNumber { get; }
    public CGameID GameID { get; protected set; } = CGameID.Zero;
    public AppId_t AppID
    {
        get
        {
            return GameID.AppID;
        }
    }

    /// <summary>
    /// Use this to set a custom name. <br/> 
    /// It will override the name defined in the app's appdata sections, or in the case of mods it will override the mod's name (from it's gameinfo.txt)
    /// </summary>
    public string NameOverride { get; set; } = "";

    /// <summary>
    /// Use this to set a custom hero artwork url. <br/> 
    /// It will override the artwork defined in the app's appdata sections, or in the case of mods it will override the mod's parent's hero art. 
    /// </summary>
    public string HeroOverrideURL { get; set; } = "";

    /// <summary>
    /// Use this to set a custom logo url. <br/> 
    /// It will override the artwork defined in the app's appdata sections, or in the case of mods it will override the mod's parent's logo. 
    /// </summary>
    public string LogoOverrideURL { get; set; } = "";

    /// <summary>
    /// Use this to set a custom icon url. <br/> 
    /// It will override the icon defined in the app's appdata sections, or in the case of mods it will override the mod's (gameinfo.txt) icon. 
    /// </summary>
    public string IconOverrideURL { get; set; } = "";

    /// <summary>
    /// Use this to set a custom portrait artwork url. <br/> 
    /// It will override the artwork defined in the app's appdata sections, or in the case of mods it will override the default grey portrait. 
    /// </summary>
    public string PortraitOverrideURL { get; set; } = "";
    
    public string? LocalIconPath { get; protected set; }
    public string? LocalLogoPath { get; protected set; }
    public string? LocalHeroPath { get; protected set; }
    public string? LocalPortraitPath { get; protected set; }

    public async Task UpdateLibraryAssets() {
        var HeroURI = new Uri(HeroURL);
        if (HeroURI.IsFile) {
            LocalHeroPath = HeroURI.LocalPath;
        } else {
            int lastAssetsChangeNumber = 0;
            string oldPath = Path.Combine(this.AppsManager.LibraryAssetsPath, $"{this.AppID}_change");
            string targetPath = Path.Combine(this.AppsManager.LibraryAssetsPath, $"{this.AppID}_Hero");
            if (File.Exists(oldPath)) {
                lastAssetsChangeNumber = int.Parse(await File.ReadAllTextAsync(oldPath));
            }

            //TODO: how should we check for the correct version of the assets without redownloading each time
            if (!File.Exists(targetPath) || ChangeNumber > lastAssetsChangeNumber) {
                using (var response = await Client.HttpClient.GetStreamAsync(HeroURI))
                {
                    using var file = File.OpenWrite(targetPath);
                    await response.CopyToAsync(file);
                }
            }

            LocalHeroPath = targetPath;
        }

        var IconURI = new Uri(IconURL);
        if (IconURI.IsFile) {
            LocalHeroPath = IconURI.LocalPath;
        } else {
            int lastAssetsChangeNumber = 0;
            string oldPath = Path.Combine(this.AppsManager.LibraryAssetsPath, $"{this.AppID}_change");
            string targetPath = Path.Combine(this.AppsManager.LibraryAssetsPath, $"{this.AppID}_Icon");
            if (File.Exists(oldPath)) {
                lastAssetsChangeNumber = int.Parse(await File.ReadAllTextAsync(oldPath));
            }

            //TODO: how should we check for the correct version of the assets without redownloading each time 
            if (!File.Exists(targetPath) || ChangeNumber > lastAssetsChangeNumber) {
                using (var response = await Client.HttpClient.GetStreamAsync(IconURI))
                {
                    using var file = File.OpenWrite(targetPath);
                    await response.CopyToAsync(file);
                }
            }
            
            LocalIconPath = targetPath;
        }

        LibraryAssetsUpdated?.Invoke(this, EventArgs.Empty);
    }

    public abstract void PauseUpdate();
    public abstract void Update();

    public event EventHandler? LibraryAssetsUpdated;
    public bool IsMod => this.GameID.IsMod();
    public bool IsShortcut => this.GameID.IsShortcut();
    public bool IsSteamApp => this.GameID.IsSteamApp();

    protected AppsManager AppsManager { get; init; }
    public AppBase(AppsManager appsManager)
    {
        AppsManager = appsManager;
    }

    public void Kill() {
        AppsManager.Kill(GameID);
    }

    protected static string GetValueOverride(string? overridestr, string? valuestr)
    {
        if (!string.IsNullOrEmpty(overridestr))
        {
            return overridestr;
        }

        if (string.IsNullOrEmpty(valuestr))
        {
            return string.Empty;
        }

        return valuestr;
    }

    public static SteamApp CreateSteamApp(AppsManager appsManager, AppId_t appid) {
        return new SteamApp(appsManager, appid);
    }

    public static ShortcutApp CreateShortcut(AppsManager appsManager, string name, string exe, string workingDir) {
        return new ShortcutApp(appsManager, name, exe, workingDir);
    }

    public static SourcemodApp CreateSourcemod(AppsManager appsManager, string sourcemodDir, uint modid) {
        return new SourcemodApp(appsManager, sourcemodDir, modid);
    }

    protected AppBase? GetAppIfValidGameID(CGameID gameid) {
        if (!gameid.IsValid()) {
            return null;
        }

        if (AppsManager == null) {
            throw new InvalidOperationException("AppsManager was null when getting gameid " + gameid);
        }

        return AppsManager.GetApp(gameid);
    }
}