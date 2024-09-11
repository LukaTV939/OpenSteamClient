using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using OpenSteamworks.Data.Enums;
using OpenSteamworks.Generated;
using OpenSteamworks.Utils;

namespace OpenSteamClient;

/// <summary>
/// The entry point of MainExe.
/// </summary>
public static unsafe class Entry
{
    public static PinnedUTF8String LoggingDir = new();
    public static PinnedUTF8String InstallDir = new();  
    public static PinnedUTF8String CurrentBeta = new();

    public delegate int MainDelegate(int argc, byte** argv);
    public static int Main(int argc, byte** argv) {
        Console.WriteLine("EntryMain called");
        
        // TODO: Duplicate of InstallManager
        var localShare = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        InstallDir.CurrentString = Path.Combine(localShare, "OpenSteam");
        LoggingDir.CurrentString = Path.Combine(InstallDir.CurrentString, "logs");

        CurrentBeta.CurrentString = "clientbeta";

        string[] args = new string[argc];
        for (int i = 0; i < argc; i++)
        {
            if (OperatingSystem.IsLinux()) {
                // Assume UTF8
                args[i] = Marshal.PtrToStringAuto((nint)((argv)[i])) ?? string.Empty;
            } else {
                // Assume UTF16
                args[i] = Marshal.PtrToStringAuto((nint)(((char**)argv)[i])) ?? string.Empty;
            }
            
        }

        Program.Main(args);
        return 0;
    }
    
    public unsafe delegate void* SteamBootstrapper_GetInstallDirDelegate();
    public static unsafe void* SteamBootstrapper_GetInstallDir() {
        Console.WriteLine("SteamBootstrapper_GetInstallDir called");
        return InstallDir.CurrentPtr;
    }

    public unsafe delegate void* SteamBootstrapper_GetLoggingDirDelegate();
    public static unsafe void* SteamBootstrapper_GetLoggingDir() {
        Console.WriteLine("SteamBootstrapper_GetLoggingDir called");
        return LoggingDir.CurrentPtr;
    }

    public unsafe delegate void PermitDownloadClientUpdatesDelegate(bool permit);
    public static unsafe void PermitDownloadClientUpdates(bool permit) {
        Console.WriteLine("PermitDownloadClientUpdates called with (permit: " + permit + ")");
    }

    public unsafe delegate uint GetBootstrapperVersionDelegate();
    public static unsafe uint GetBootstrapperVersion() {
        Console.WriteLine("GetBootstrapperVersion called");
        return VersionInfo.STEAM_MANIFEST_VERSION;
    }

    public unsafe delegate void* GetCurrentClientBetaDelegate();
    public static unsafe void* GetCurrentClientBeta() {
        Console.WriteLine("GetCurrentClientBeta called");
        return CurrentBeta.CurrentPtr;
    }

    public unsafe delegate int SteamBootstrapper_GetEUniverseDelegate();
    public static unsafe int SteamBootstrapper_GetEUniverse() {
        Console.WriteLine("SteamBootstrapper_GetEUniverse called");
        return (int)EUniverse.Public;
    }

    public unsafe delegate byte StartCheckingForUpdatesDelegate();
    public static unsafe byte StartCheckingForUpdates() {
        Console.WriteLine("StartCheckingForUpdates called");
        return 1;
    }

    public unsafe delegate void ClientUpdateRunFrameDelegate();
    public static unsafe void ClientUpdateRunFrame() {
        Console.WriteLine("ClientUpdateRunFrame called");
    }

    public unsafe delegate byte IsClientUpdateAvailableDelegate();
    public static unsafe byte IsClientUpdateAvailable() {
        Console.WriteLine("IsClientUpdateAvailable called");
        return 0;
    }

    public unsafe delegate byte CanSetClientBetaDelegate();
    public static unsafe byte CanSetClientBeta() {
        Console.WriteLine("CanSetClientBeta called");
        return 1;
    }

    public unsafe delegate uint GetClientUpdateBytesDownloadedDelegate();
    public static unsafe uint GetClientUpdateBytesDownloaded() {
        Console.WriteLine("GetClientUpdateBytesDownloaded called");
        return 0;
    }

    public unsafe delegate uint GetClientUpdateBytesToDownloadDelegate();
    public static unsafe uint GetClientUpdateBytesToDownload() {
        Console.WriteLine("GetClientUpdateBytesToDownload called");
        return 0;
    }

    public unsafe delegate void SetClientBetaDelegate(void* ptr);
    public static unsafe void SetClientBeta(void* ptr) {
        Console.WriteLine("SetClientBeta called");
        CurrentBeta.CurrentString = Marshal.PtrToStringUTF8((nint)ptr) ?? string.Empty;
    }

    public unsafe delegate int GetClientLauncherTypeDelegate();
    public static unsafe int GetClientLauncherType() {
        Console.WriteLine("GetClientLauncherType called");
        return (int)ELauncherType.Clientui;
    }

    public unsafe delegate void ForceUpdateNextRestartDelegate();
    public static unsafe void ForceUpdateNextRestart() {
        Console.WriteLine("ForceUpdateNextRestart called");
    }

    public unsafe delegate byte IsClientUpdateOutOfDiskSpaceDelegate();
    public static unsafe byte IsClientUpdateOutOfDiskSpace() {
        Console.WriteLine("IsClientUpdateOutOfDiskSpace called");
        return 0;
    }
}
