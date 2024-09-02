using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;

namespace ipctest;

public unsafe class Program
{
    private static void Main(string[] args)
    {
		// PrintInfo();

		// var thread = new Thread(() =>
		// {
		//     PrintInfo();
		// });

		// thread.Start();
		// Thread.Sleep(50);

		// thread = new Thread(() =>
		// {
		//     PrintInfo();
		// });

		// thread.Start();
		// Thread.Sleep(50);

		// thread = new Thread(() =>
		// {
		//     PrintInfo();
		// });

		// thread.Start();
		// Thread.Sleep(50);
		bool hasSteam = Process.GetProcessesByName("steam").FirstOrDefault() != null;
		if (OperatingSystem.IsLinux() && !hasSteam) {
			var homevar = Environment.GetEnvironmentVariable("HOME");
            if (string.IsNullOrEmpty(homevar)) {
                throw new NullReferenceException("HOME not set, please set it");
            }

            File.WriteAllText(Path.Combine(homevar, ".steam/steam.pid"), Environment.ProcessId.ToString());
            Console.WriteLine("Set steam PID to " + Environment.ProcessId.ToString());
		}

		if (!hasSteam) {
			Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) =>
			{
				LiteHTMLHost.KillHTMLHost();
			};

			LiteHTMLHost.StartHTMLHost();
			Thread.Sleep(1000);
		}

		var chromeIPCClient = new ChromeIPCClient();

		while (true)
		{
			Thread.Sleep(10);
		}

        // sharedMemoryIPC.CreateSharedMemory($"SteamChrome_MasterStream_uid{SharedMemoryIPC.getuid()}_spid{GetSteamPID()}_mem");
        // sharedMemoryIPC.CreateSharedMemory($"SteamChrome_MasterStream_3137_28054_mem");
    }
}