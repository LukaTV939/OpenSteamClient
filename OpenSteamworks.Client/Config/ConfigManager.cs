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
	private interface IConfigFileInternal 
	{
		public object Value { get; }
		public Task SaveAsync();
	}

	private class ConfigFileInternal<T> : IConfigFileInternal where T: IConfigFile<T>, new()
	{
		public object Value { get; }

		private readonly ConfigManager configManager;
		public ConfigFileInternal(ConfigManager configManager, object value)
		{
			this.configManager = configManager;
			this.Value = value;
		}

		public Task SaveAsync()
		{
			if (Value is not T asT) {
				throw new ArgumentException("Invalid type", nameof(Value));
			}

			return configManager.SaveAsync(asT);
		}
	}

	private readonly InstallManager installManager;
    private readonly IContainer container;
    private readonly ILogger logger;
    private readonly static JsonSerializerOptions jsonOpts = new()
    {
        WriteIndented = true
    };

    private readonly Dictionary<Type, IConfigFileInternal> loadedConfigs = new();
    private readonly Dictionary<Type, IConfigFileInternal> loadedUserConfigs = new();
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
    }

    private string GetPathForConfig<T>() where T: IConfigFile<T> {
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
    public void Register<T>() where T: IConfigFile<T> {
        if (!this.registeredConfigs.Contains(typeof(T))) {
            this.registeredConfigs.Add(typeof(T));
        }
    }

    public T Get<T>() where T: IConfigFile<T>, new() {
        if (loadedConfigs.TryGetValue(typeof(T), out IConfigFileInternal? val)) {
            return (T)val.Value;
        }

        if (loadedUserConfigs.TryGetValue(typeof(T), out IConfigFileInternal? valUser)) {
            return (T)valUser.Value;
        }

        var fullPath = GetPathForConfig<T>();
        T? result = new();
        logger.Info("Attempting to load config " + T.ConfigName + " from path: " + fullPath);
        if (File.Exists(fullPath) && new FileInfo(fullPath).Length > 0) {
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                result = JsonSerializer.Deserialize<T>(stream, T.JsonTypeInfo);
            }
        } else {
            logger.Warning("File did not exist or it was empty");
        }

        if (result == null) {
            throw new JsonException("Deserializing config file failed");
        }

        if (T.PerUser) {
            loadedUserConfigs[typeof(T)] = new ConfigFileInternal<T>(this, result);
        } else {
            loadedConfigs[typeof(T)] = new ConfigFileInternal<T>(this, result);
        }

        Register<T>();
        
        return result;
    }
    
    private static MemoryStream HasChanged<T>(T instance, out bool hasChanged) where T: IConfigFile<T>, new() {
        using var defaultstream = new MemoryStream();
        var instancestream = new MemoryStream();
        
        JsonSerializer.Serialize(defaultstream, new T(), T.JsonTypeInfo);
        defaultstream.Position = 0;

        JsonSerializer.Serialize(instancestream, instance, T.JsonTypeInfo);
        instancestream.Position = 0;
        
        hasChanged = !defaultstream.AreEqual(instancestream);
        return instancestream;
    }

    public void Save<T>(T instance) where T: IConfigFile<T>, new() {
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

    public async Task SaveAsync<T>(T instance) where T: IConfigFile<T>, new() {
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

		await Task.WhenAll(loadedConfigs.Select(c => c.Value.SaveAsync()));

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
        
        await Task.WhenAll(loadedUserConfigs.Select(c => c.Value.SaveAsync()));

        loadedUserConfigs.Clear();
    }
}