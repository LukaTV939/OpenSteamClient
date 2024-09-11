using OpenSteamClient.Logging;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleToAttribute("OpenSteamworks.Messaging")]
[assembly: InternalsVisibleToAttribute("OpenSteamworks.IPC")]
[assembly: InternalsVisibleToAttribute("OpenSteamworks.ConCommands")]

namespace OpenSteamworks;

internal static class Logging {
    public static ILogger GeneralLogger { get; set; } = new ConsoleLogger();
    /// <summary>
    /// The logger used explicitly for messages coming straight from the underlying steamclient library.
    /// Or in the case of IPCClient, messages from IPCClient
    /// </summary>
    public static ILogger NativeClientLogger { get; set; } = new ConsoleLogger();
    /// <summary>
    /// Logs all IPC activity from IPCClient
    /// </summary>
    public static ILogger IPCLogger { get; set; } = new ConsoleLogger();
    public static ILogger CallbackLogger { get; set; } = new ConsoleLogger();
    public static ILogger ConCommandsLogger { get; set; } = new ConsoleLogger();
    public static ILogger MessagingLogger { get; set; } = new ConsoleLogger();
    /// <summary>
    /// The logger used for CUtl types
    /// </summary>
    public static ILogger CUtlLogger { get; set; } = new ConsoleLogger();

	public static bool LogIncomingCallbacks { get; set; } = false;
	public static bool LogCallbackContents { get; set; } = false;
}