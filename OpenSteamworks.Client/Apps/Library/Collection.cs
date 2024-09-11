using System.Diagnostics.CodeAnalysis;
using OpenSteamClient.Logging;
using OpenSteamworks.Client.Enums;
using OpenSteamworks.Client.Utils;
using OpenSteamworks.Data;
using OpenSteamworks.Data.Structs;
using OpenSteamworks.Utils;

namespace OpenSteamworks.Client.Apps.Library;

public class Collection
{
    public string ID { get; private set; }
    public string Name { get; set; }

    [MemberNotNullWhen(true, nameof(StateFilter))]
    [MemberNotNullWhen(true, nameof(FeatureAndSupportFilter))]
    [MemberNotNullWhen(true, nameof(StoreTagsFilter))]
    [MemberNotNullWhen(true, nameof(FriendsInCommonFilter))]
    public bool IsDynamic { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsPartner { get; private set; }
    public bool HasFilters
    {
        get
        {
            ThrowIfStatic();
            if (this.StateFilter!.FilterOptions.Count > 0)
                return true;

            if (this.FeatureAndSupportFilter!.FilterOptions.Count > 0)
                return true;

            if (this.StoreTagsFilter!.FilterOptions.Count > 0)
                return true;

            if (this.FriendsInCommonFilter!.FilterOptions.Count > 0)
                return true;

            return false;
        }
    }
    public FilterGroup<ELibraryAppStateFilter>? StateFilter { get; set; }
    public FilterGroup<ELibraryAppFeaturesFilter>? FeatureAndSupportFilter { get; set; }
    public FilterGroup<int>? StoreTagsFilter { get; set; }
    public FilterGroup<uint>? FriendsInCommonFilter { get; set; }

    internal HashSet<AppId_t> explicitlyAddedApps = new();
    internal HashSet<AppId_t> explicitlyRemovedApps = new();

    internal HashSet<AppId_t> dynamicCollectionAppsCached = new();

    private readonly LibraryManager libraryManager;

    internal Collection(LibraryManager libraryManager, string name, string id, bool system = false)
    {
        this.libraryManager = libraryManager;
        this.Name = name;
        this.ID = id;
        this.IsSystem = system;
        this.IsPartner = id.StartsWith("partner-");
        this.IsDynamic = false;
    }

    public static Collection FromJSONCollection(LibraryManager libraryManager, JSONCollection json)
    {
        Collection collection = new(libraryManager, json.name, json.id);
        // Determine if this is a dynamic collection
        collection.IsDynamic = json.filterSpec != null;

        if (collection.IsDynamic)
        {
            // No need to do any special processing here, just save the filter specs
            UtilityFunctions.AssertNotNull(json.filterSpec);
            
            if (json.filterSpec.filterGroups.Length < 7)
            {
                libraryManager.Logger.Warning("There are less filter groups than 7. Possibly missing or misinterpreting some filters. New length is: " + json.filterSpec.filterGroups.Length);
            }

            if (json.filterSpec.filterGroups.Length > 7)
            {
                libraryManager.Logger.Warning("There are more filter groups than 7. The filters have changed. New length is: " + json.filterSpec.filterGroups.Length);
            }

            if (json.filterSpec.filterGroups.Length > 1) {
                collection.StateFilter = FilterGroup<ELibraryAppStateFilter>.FromJSONFilterGroup(json.filterSpec.filterGroups[1]);
            } else {
                collection.StateFilter = new();
            }

            if (json.filterSpec.filterGroups.Length > 2) {
                collection.FeatureAndSupportFilter = FilterGroup<ELibraryAppFeaturesFilter>.FromJSONFilterGroup(json.filterSpec.filterGroups[2]);
            } else {
                collection.FeatureAndSupportFilter = new();
            }

            if (json.filterSpec.filterGroups.Length > 4) {
                collection.StoreTagsFilter = FilterGroup<int>.FromJSONFilterGroup(json.filterSpec.filterGroups[4]);
            } else {
                collection.StoreTagsFilter = new();
            }

            if (json.filterSpec.filterGroups.Length > 6) {
                collection.FriendsInCommonFilter = FilterGroup<uint>.FromJSONFilterGroup(json.filterSpec.filterGroups[6]);
            } else {
                collection.FriendsInCommonFilter = new();
            }
        }
        
        if (json.added == null)
        {
            json.added = new List<uint>();
        }

        if (json.removed == null)
        {
            json.removed = new List<uint>();
        }

        collection.explicitlyAddedApps = json.added.Select(x => (AppId_t)x).ToHashSet();
        collection.explicitlyRemovedApps = json.removed.Select(x => (AppId_t)x).ToHashSet();

        return collection;
    }

    /// <summary>
    /// Adds an app to this collection.
    /// </summary>
    public void AddApp(AppId_t appid)
    {
        this.explicitlyAddedApps.Add(appid);
    }

    /// <summary>
    /// Adds an app to this collection.
    /// </summary>
    public void AddApp(CGameID gameid)
    {
        if (!gameid.IsSteamApp()) {
            //TODO: shortcuts support (need to be locally persisted)
            return;
        }

        this.explicitlyAddedApps.Add(gameid.AppID);
    }

    /// <summary>
    /// Adds multiple apps to this collection.
    /// </summary>
    public void AddApps(IEnumerable<AppId_t> appids) {
        foreach (var item in appids)
        {
            this.explicitlyAddedApps.Add(item);
        }
    }

    /// <summary>
    /// Adds multiple apps to this collection.
    /// </summary>
    public void AddApps(IEnumerable<CGameID> appids) {
        foreach (var item in appids)
        {
            if (!item.IsSteamApp()) {
                //TODO: shortcuts support (need to be locally persisted)
                continue;
            }

            this.explicitlyAddedApps.Add(item.AppID);
        }
    }

    /// <summary>
    /// Removes an app from the collection.
    /// If the collection is dynamic, it will blacklist the app from being visible in the collection
    /// </summary>
    public void RemoveApp(AppId_t appid)
    {
        if (this.IsDynamic)
        {
            this.explicitlyRemovedApps.Add(appid);
        }
        this.explicitlyAddedApps.Remove(appid);
    }

    /// <summary>
    /// Removes an exclusion for an app from a dynamic collection.
    /// </summary>
    public void RemoveExcludedApp(AppId_t appid)
    {
        this.ThrowIfStatic();
        this.explicitlyRemovedApps.Remove(appid);
    }

    internal void ThrowIfDynamic()
    {
        if (this.IsDynamic)
        {
            throw new InvalidOperationException("This operation is invalid for dynamic collections.");
        }
    }

    internal void ThrowIfStatic()
    {
        if (!this.IsDynamic)
        {
            throw new InvalidOperationException("This operation is invalid for static collections.");
        }
    }

    internal void ThrowIfSystem()
    {
        if (this.IsSystem)
        {
            throw new InvalidOperationException("This operation is invalid for system collections.");
        }
    }

    internal JSONCollection ToJSON()
    {
        JSONCollection json = new()
        {
            id = this.ID,
            name = this.Name,
            added = this.explicitlyAddedApps.Select(x => (uint)x).ToList(),
            removed = this.explicitlyRemovedApps.Select(x => (uint)x).ToList()
        };

        if (this.IsDynamic)
        {
            json.filterSpec = new JSONFilterSpec();
            json.filterSpec.filterGroups = new JSONFilterGroup[] {
                new JSONFilterGroup(),
                this.StateFilter.ToJSON(),
                this.FeatureAndSupportFilter.ToJSON(),
                new JSONFilterGroup(),
                this.StoreTagsFilter.ToJSON(),
                new JSONFilterGroup(),
                this.FriendsInCommonFilter.ToJSON()
            };
        }

        return json;
    }

    public void MergeFrom(Collection collection)
    {
        this.explicitlyAddedApps = collection.explicitlyAddedApps;
        this.explicitlyRemovedApps = collection.explicitlyRemovedApps;
        this.dynamicCollectionAppsCached = collection.dynamicCollectionAppsCached;
        this.Name = collection.Name;

        // Dynamic filters
        this.StateFilter = collection.StateFilter;
        this.StoreTagsFilter = collection.StoreTagsFilter;
        this.FriendsInCommonFilter = collection.FriendsInCommonFilter;
        this.FeatureAndSupportFilter = collection.FeatureAndSupportFilter;
    }
}