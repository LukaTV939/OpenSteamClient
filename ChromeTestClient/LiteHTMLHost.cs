using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

namespace ipctest;

public static class LiteHTMLHost {
	private static readonly object CurrentHTMLHostLock = new();
	private static Process? CurrentHTMLHost;
	private static string InstallDir =>  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenSteam");
	private static Thread? WatcherThread;

	[SupportedOSPlatform("windows")]
    private static string GetWindowsCEFPath() {
        string baseDir = Path.Combine(InstallDir, "bin", "cef");
        
        bool TryVersion(string ver, [NotNullWhen(true)] out string? path) {
            path = Path.Combine(baseDir, ver);
            bool isValid = Directory.Exists(path) && File.Exists(Path.Combine(path, "steamwebhelper.exe"));
            if (!isValid) {
                Console.WriteLine($"Wanted CEF version {ver}, but it doesn't exist.");
            }

            return isValid;
        }

        
        if (OperatingSystem.IsWindowsVersionAtLeast(10) && TryVersion("cef.win10x64", out string? path)) {
            return path;
        } else {
            if (TryVersion("cef.win7x64", out path)) {
                return path;
            } else if (TryVersion("cef.win7", out path)) {
                return path;
            } else {
                throw new Exception("No CEF available");
            }
        }
    }

	private static bool expectedToStop = false;

	[SupportedOSPlatform("linux")]
    [SupportedOSPlatform("windows")]
    public static void StartHTMLHost()
    {
        lock (CurrentHTMLHostLock)
        {
            Console.WriteLine("Creating steamwebhelper process");

            CurrentHTMLHost = new Process();
            if (OperatingSystem.IsLinux()) {
                CurrentHTMLHost.StartInfo.WorkingDirectory = Path.Combine(InstallDir, "ubuntu12_64");

				// Strace
				// CurrentHTMLHost.StartInfo.FileName = "/usr/bin/strace";
				// CurrentHTMLHost.StartInfo.ArgumentList.Add("-efutex");
				// CurrentHTMLHost.StartInfo.ArgumentList.Add("--detach-on=execve");
				// CurrentHTMLHost.StartInfo.ArgumentList.Add("--interruptible=never");
				// CurrentHTMLHost.StartInfo.ArgumentList.Add(Path.Combine(InstallDir, "ubuntu12_64", "steamwebhelper"));

				CurrentHTMLHost.StartInfo.FileName = Path.Combine(InstallDir, "ubuntu12_64", "steamwebhelper");
                CurrentHTMLHost.StartInfo.EnvironmentVariables["LD_LIBRARY_PATH"] = $".:{Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")}";
                CurrentHTMLHost.StartInfo.ArgumentList.Add("--disable-seccomp-filter-sandbox");
            } else if (OperatingSystem.IsWindows()) {
                string basePath = GetWindowsCEFPath();
                CurrentHTMLHost.StartInfo.WorkingDirectory = basePath;
                CurrentHTMLHost.StartInfo.FileName = Path.Combine(basePath, "steamwebhelper.exe");
            }
            
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"-lang=en-US");

            CurrentHTMLHost.StartInfo.ArgumentList.Add($"-cachedir={Path.Combine(InstallDir, "appcache", "htmlcache")}");

            // This could technically be improved by reading from the steam.pid file, but no need since this code always assumes we're the master
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"-steampid={Environment.ProcessId}");

            CurrentHTMLHost.StartInfo.ArgumentList.Add($"-buildid={1716242052}");

            // Don't know our SteamID at this point.
            CurrentHTMLHost.StartInfo.ArgumentList.Add("-steamid=0");
			var logsDir = Path.Combine(InstallDir, "logs");
			CurrentHTMLHost.StartInfo.ArgumentList.Add($"-logdir={logsDir}");
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"-uimode=7");

            // We don't track this.
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"-startcount=0");

            CurrentHTMLHost.StartInfo.ArgumentList.Add($"-steamuniverse=1");
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"-realm=1");

            // Doesn't exist, but we pass it anyway.
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"-clientui={Path.Combine(InstallDir, "clientui")}");

            string steampath;
            if (OperatingSystem.IsLinux())
            {
                steampath = Directory.ResolveLinkTarget("/proc/self/exe", false)!.FullName;
            }
            else
            {
                steampath = Environment.ProcessPath!;
            }

            CurrentHTMLHost.StartInfo.ArgumentList.Add($"-steampath={steampath}");

            // No idea what this means or does.
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"-launcher=0");

            // This should only be passed if we're in debug mode, but that info isn't really passed through to us in any way.
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"-dev");

            CurrentHTMLHost.StartInfo.ArgumentList.Add($"-no-restart-on-ui-mode-change");
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"--enable-media-stream");

            CurrentHTMLHost.StartInfo.ArgumentList.Add($"--enable-smooth-scrolling");
            
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"--password-store=basic");
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"--log-file={Path.Combine(logsDir, "cef_log.txt")}");
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"--disable-quick-menu");
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"--disable-features=SameSiteByDefaultCookies");
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"--enable-blink-features=ResizeObserver,Worklet,AudioWorklet");
            CurrentHTMLHost.StartInfo.ArgumentList.Add($"--disable-blink-features=Badging");

            Console.WriteLine("Starting steamwebhelper process");
            CurrentHTMLHost.Start();

            if (WatcherThread == null)
            {
                Console.WriteLine("Creating watcher thread");
                WatcherThread = new Thread(() =>
                {
                    do
                    {
                        if (CurrentHTMLHost.HasExited && !expectedToStop)
                        {
                            Console.WriteLine($"steamwebhelper crashed (exit code {CurrentHTMLHost.ExitCode})! Restarting in 1s.");
                            Thread.Sleep(1000);
                            StartHTMLHost();
                        }
                        Thread.Sleep(50);
                    } while (true);
                });

                Console.WriteLine("Starting watcher thread");
                WatcherThread.Start();
            }
        }
    }

	public static void KillHTMLHost()
	{
		expectedToStop = true;

		var procs = Process.GetProcessesByName("steamwebhelper");
		for (int i = 0; i < procs.Length; i++)
		{
			procs[i].Kill();
		}
	}
}