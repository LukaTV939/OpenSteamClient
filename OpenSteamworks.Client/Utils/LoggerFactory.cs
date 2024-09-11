using OpenSteamClient.DI.Attributes;
using OpenSteamClient.Logging;
using OpenSteamworks.Client.Managers;

namespace OpenSteamworks.Client.Utils;

[DIRegisterInterface<ILoggerFactory>]
public sealed class LoggerFactory : ILoggerFactory
{
	private readonly InstallManager installManager;

	public LoggerFactory(InstallManager installManager)
	{
		this.installManager = installManager;
	}

	public ILogger CreateLogger(string name)
		=> Logger.GetLogger(name, installManager.GetLogPath(name));
}