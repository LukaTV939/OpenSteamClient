using System.Reflection;
using System.Text.Json;
using OpenSteamworks.Extensions;
using OpenSteamworks.Client.Managers;
using OpenSteamworks.Client.Utils;
using OpenSteamClient.DI;
using OpenSteamworks.Utils;
using Profiler;
using OpenSteamClient.DI.Lifetime;
using OpenSteamClient.Logging;

namespace OpenSteamworks.Client.Config;

public class ConfigManager : IClientLifetime, ILogonLifetime {
    private readonly InstallManager installManager;
    private readonly IContainer container;
    private readonly ILogger logger;
    private readonly static JsonSerializerOptions jsonOpts = new()
    {
        WriteIndented = true
    };

    private readonly MethodInfo saveAsyncCached;
    private readonly Dictionary<Type, object> loadedConfigs = new();
    private readonly Dictionary<Type, object> loadedUserConfigs = new();
    private readonly List<Type> registeredConfigs = new();
    public ReadOnlyCollectionEx<Type> RegisteredConfigs => new(registeredConfigs);

    public ConfigManager(IContainer container, InstallManager installManager) {
        this.installManager = installManager;
        this.container = container;
        this.logger = Logger.GetLogger("ConfigManager", this.installManager.GetLogPath("ConfigManager"));
        container.RegisterFactoryMethod<AdvancedConfig>(() => Get<AdvancedConfig>());
        container.RegisterFactoryMethod<BootstrapperState>(() => Get<BootstrapperState>());
        container.RegisterFactoryMethod<GlobalSettings>(() => Get<GlobalSettings>());
        container.RegisterFactoryMethod<LoginUsers>(() => Get<LoginUsers>());
        container.RegisterFactoryMethod<UserSettings>(() => Get<UserSettings>());
        saveAsyncCached = this.GetType().GetMethod(nameof(SaveAsync))!;
    }

    private string GetPathForConfig<T>() where T: IConfigFile {
        string basePath;
        if (T.PerUser) {
            if (!this.container.TryGet(out LoginManager? loginManager))
            {
                throw new InvalidOperationException("Attempted to get per-user config before user has logged on");
            }

            if (!loginManager.IsLoggedOn())
            {
                throw new InvalidOperationException("Attempted to get per-user config before user has logged on");
            }

            basePath = loginManager.GetUserConfigDirectory();
        } else {
            basePath = this.installManager.ConfigDir;
        }

        return Path.Combine(basePath, T.ConfigName + ".json");
    }

    /// <summary>
    /// Registers a config file for use by editor GUIs.
    /// Automatically called in Get as well.
    /// </summary>
    public void Register<T>() where T: IConfigFile {
        if (!this.registeredConfigs.Contains(typeof(T))) {
            this.registeredConfigs.Add(typeof(T));
        }
    }

    public T Get<T>() where T: IConfigFile, new() {
        if (loadedConfigs.TryGetValue(typeof(T), out object? val)) {
            return (T)val;
        }

        if (loadedUserConfigs.TryGetValue(typeof(T), out object? valUser)) {
            return (T)valUser;
        }

        var fullPath = GetPathForConfig<T>();
        T? result = new();
        logger.Info("Attempting to load config " + T.ConfigName + " from path: " + fullPath);
        if (File.Exists(fullPath) && new FileInfo(fullPath).Length > 0) {
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                result = JsonSerializer.Deserialize<T>(stream);
            }
        } else {
            logger.Warning("File did not exist or it was empty");
        }

        if (result == null) {
            throw new JsonException("Deserializing config file failed");
        }

        if (T.PerUser) {
            loadedUserConfigs[typeof(T)] = result;
        } else {
            loadedConfigs[typeof(T)] = result;
        }

        Register<T>();
        
        return result;
    }
    
    private static MemoryStream HasChanged<T>(T instance, out bool hasChanged) where T: IConfigFile, new() {
        using var defaultstream = new MemoryStream();
        var instancestream = new MemoryStream();
        
        JsonSerializer.Serialize(defaultstream, new T(), jsonOpts);
        defaultstream.Position = 0;

        JsonSerializer.Serialize(instancestream, instance, jsonOpts);
        instancestream.Position = 0;
        
        hasChanged = !defaultstream.AreEqual(instancestream);
        return instancestream;
    }

    public void Save<T>(T instance) where T: IConfigFile, new() {
        var fullPath = GetPathForConfig<T>();

        using (var stream = HasChanged(instance, out bool hasChanged))
        {
            if (!(hasChanged || T.AlwaysSave)) {
                logger.Info("Not saving " + T.ConfigName + ", all config values are still set to defaults");
                return;
            }

            using (Stream file = File.Open(fullPath, FileMode.Create))
            {
                stream.CopyTo(file);
            }
        }
    }

    public async Task SaveAsync<T>(T instance) where T: IConfigFile, new() {
        using var subScope = CProfiler.CurrentProfiler?.EnterScope("ConfigManager.SaveAsync");

        var fullPath = GetPathForConfig<T>();
        
        using (var stream = HasChanged(instance, out bool hasChanged))
        {
            if (!(hasChanged || T.AlwaysSave)) {
                logger.Info("Not saving " + T.ConfigName + ", all config values are still set to defaults");
                return;
            }

            logger.Info("Saving config " + T.ConfigName + " to path: " + fullPath);
            using (Stream file = File.Open(fullPath, FileMode.Create))
            {
                await stream.CopyToAsync(file);
            }
        }
    }

    async Task IClientLifetime.RunStartup(IProgress<OperationProgress> progress)
    {
        await Task.CompletedTask;
    }

    async Task IClientLifetime.RunShutdown(IProgress<OperationProgress> operation)
    {
        operation.Report(new("Saving configs"));

        foreach (var item in loadedConfigs)
        {
            // TODO: This is icky
            await (Task)saveAsyncCached.MakeGenericMethod(item.Key).Invoke(this, new[] { item.Value })!;
        }

        loadedConfigs.Clear();
        await Task.CompletedTask;
    }

    async Task ILogonLifetime.RunLogon(IProgress<OperationProgress> progress)
    {
        await Task.CompletedTask;
    }

    async Task ILogonLifetime.RunLogoff(IProgress<OperationProgress> progress)
    {
        progress.Report(new("Saving user configs"));
        
        foreach (var item in loadedUserConfigs)
        {
            // TODO: This is icky
            await (Task)saveAsyncCached.MakeGenericMethod(item.Key).Invoke(this, new[] { item.Value })!;
        }

        loadedUserConfigs.Clear();
    }
}