using OpenSteamClient.DI;
using OpenSteamClient.DI.Attributes;
using OpenSteamClient.DI.Lifetime;
using OpenSteamClient.Logging;
using OpenSteamworks.Client.Managers;

namespace OpenSteamworks.Client.DI;

[DIRegisterInterface<ILifetimeManager>]
public class LifetimeManager : ILifetimeManager {
	private readonly IContainer container;
	private readonly ILogger logger;

	public LifetimeManager(IContainer container) {
		this.container = container;
		this.logger = container.Get<ILoggerFactory>().CreateLogger("LifetimeManager");
	}

	private class LifetimeObject {
		public object? Object { get; set; }
		public Type? ContainerType { get; set; }
		public Type RealType {
			get {
				if (Object != null) {
					return Object.GetType();
				} else if (ContainerType != null) {
					return ContainerType;
				}

				throw new InvalidDataException("Neither Object nor ContainerType is specified");
			}
		}

		private readonly IContainer container;
		public LifetimeObject(IContainer container) {
			this.container = container;
		}

		public override string ToString()
		{
			if (Object != null) {
				return Object.GetType().Name;
			} else if (ContainerType != null) {
				return ContainerType.Name;
			}

			throw new InvalidDataException("Neither Object nor ContainerType is specified");
		}

		public async Task RunClientStartup(IProgress<OperationProgress> progress) {
			IClientLifetime? obj = (IClientLifetime?)Object;
			if (obj == null && ContainerType != null) {
				obj = (IClientLifetime?)container.Get(ContainerType);
			}

			if (obj == null) {
				throw new InvalidDataException("Neither Object nor ContainerType is specified");
			}

			await obj.RunStartup(progress);
		}

		public async Task RunClientShutdown(IProgress<OperationProgress> progress) {
			IClientLifetime? obj = (IClientLifetime?)Object;
			if (obj == null && ContainerType != null) {
				obj = (IClientLifetime?)container.Get(ContainerType);
			}

			if (obj == null) {
				throw new InvalidDataException("Neither Object nor ContainerType is specified");
			}

			await obj.RunShutdown(progress);
		}

		public async Task RunLogon(IProgress<OperationProgress> progress) {
			ILogonLifetime? obj = (ILogonLifetime?)Object;
			if (obj == null && ContainerType != null) {
				obj = (ILogonLifetime?)container.Get(ContainerType);
			}

			if (obj == null) {
				throw new InvalidDataException("Neither Object nor ContainerType is specified");
			}

			await obj.RunLogon(progress);
		}

		public async Task RunLogoff(IProgress<OperationProgress> progress) {
			ILogonLifetime? obj = (ILogonLifetime?)Object;
			if (obj == null && ContainerType != null) {
				obj = (ILogonLifetime?)container.Get(ContainerType);
			}

			if (obj == null) {
				throw new InvalidDataException("Neither Object nor ContainerType is specified");
			}

			await obj.RunLogoff(progress);
		}
	}

	private readonly List<LifetimeObject> clientLifetimeOrder = new();
    private readonly List<LifetimeObject> logonLifetimeOrder = new();
	private bool hasRanStartup;
	public bool IsShuttingDown => !hasRanStartup;

	public void RegisterForClientLifetime(IClientLifetime obj) {
		clientLifetimeOrder.Add(new(container) { Object = obj });
		logger.Debug("Registered factory of type '" + obj.GetType().Name + "' for client lifetime at index " + this.clientLifetimeOrder.Count);
	}

	public void RegisterForLogonLifetime(ILogonLifetime obj) {
		logonLifetimeOrder.Add(new(container) { Object = obj });
		logger.Debug("Registered factory of type '" + obj.GetType().Name + "' for logon lifetime at index " + this.logonLifetimeOrder.Count);
	}

	public void RegisterContainerType(Type type) {
		if (type.IsAssignableTo(typeof(IClientLifetime)))
		{
			clientLifetimeOrder.Add(new(container) { ContainerType = type });
			logger.Debug("Registered factory of type '" + type.Name + "' for client lifetime at index " + this.clientLifetimeOrder.Count);
		}
		
		if (type.IsAssignableTo(typeof(ILogonLifetime)))
		{
			logonLifetimeOrder.Add(new(container) { ContainerType = type });
			logger.Debug("Registered factory of type '" + type.Name + "' for logon lifetime at index " + this.logonLifetimeOrder.Count);
		}
	}

	private readonly SemaphoreSlim clientLifetimeLock = new(1, 1);
	public async Task RunClientStartup(IProgress<OperationProgress> progress)
    {
		await clientLifetimeLock.WaitAsync();
		try
		{
			foreach (var component in clientLifetimeOrder)
			{
				logger.Info("Running startup for " + component.ToString());
				await component.RunClientStartup(progress);
				logger.Info("Startup for " + component.ToString() + " finished");
			}

			hasRanStartup = true;
		}
		finally
		{
			clientLifetimeLock.Release();
		}
    }

    public async Task RunClientShutdown(IProgress<OperationProgress> progress)
    {
		await clientLifetimeLock.WaitAsync();
		try
		{
			if (!hasRanStartup)
			{
				return;
			}

			hasRanStartup = false;
			foreach (var component in clientLifetimeOrder)
			{
				logger.Info("Running shutdown for " + component.ToString());
				await component.RunClientShutdown(progress);
				logger.Info("Shutdown for " + component.ToString() + " finished");
			}
		}
		finally
		{
			clientLifetimeLock.Release();
		}
    }

	private readonly SemaphoreSlim logonLock = new(1, 1);
    public async Task RunLogon(IProgress<OperationProgress> progress)
    {
		await logonLock.WaitAsync();
		try
		{
			foreach (var component in logonLifetimeOrder)
			{
				logger.Info("Running logon for component " + component.ToString());
				await component.RunLogon(progress);
			}
		}
		finally
		{
			logonLock.Release();
		}
    }

    public async Task RunLogoff(IProgress<OperationProgress> progress)
    {
		await logonLock.WaitAsync();
		try
		{
			foreach (var component in logonLifetimeOrder)
			{				
				logger.Info("Running logoff for component " + component.ToString());
				await component.RunLogoff(progress);
				logger.Info("Logoff for component " + component.ToString() + " finished");
			}
		}
		finally
		{
			logonLock.Release();
		}
    }

	public bool IsClientLifetimeRegistered(Type type)
		=> clientLifetimeOrder.Any(p => p.RealType == type);

	public bool IsLogonLifetimeRegistered(Type type)
		=> logonLifetimeOrder.Any(p => p.RealType == type);
}