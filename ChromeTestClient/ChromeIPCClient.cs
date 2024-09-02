using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Google.Protobuf;
using ipctest.Platform;
using OpenSteamworks.Protobuf;
using OpenSteamworks.Utils;

namespace ipctest;

public unsafe class ChromeIPCClient {
	private struct ChromeConnectionHelloResponse {
		public int m_eCmd;
		public int m_iServerPID;
		public fixed byte ResponseStreamName[64];
	}

	private struct ChromeConnectionHello {
		public int unk;
		public int unk1;
		public int m_iClientPID;
		public fixed byte ResponseStreamName[64];
	}



	private readonly IEvent managerEvent;
	public ChromeIPCClient() {
		using var masterStream = new SharedMemStream($"SteamChrome_MasterStream_uid{Platform.Linux.LinuxConsts.getuid()}_spid{GetSteamPID()}", out bool bCreated);
		if (bCreated) {
			throw new Exception("Failed to connect to master html process, created shared memory");
		}
		
        Console.WriteLine("created: " + bCreated);
        Console.WriteLine("capacity: " + masterStream.Capacity);
        Console.WriteLine("AvailableToRead: " + masterStream.AvailableToRead);

		var randomNumber = Random.Shared.NextInt64(0, 65535);
		string newClientName = $"SteamChrome_MasterStream_{GetSteamPID()}_{randomNumber}";
		using var childStream = new SharedMemStream(newClientName, out bool bChildCreated);
		if (bChildCreated == false) {
			throw new Exception("Collided with existing master response stream");
		}

		byte[] buf = new byte[sizeof(ChromeConnectionHello)];
		var hello = new ChromeConnectionHello();
		hello.unk = 1;
		hello.unk1 = 1;
		hello.m_iClientPID = Environment.ProcessId;
		Encoding.UTF8.GetBytes(newClientName).CopyTo(new Span<byte>(hello.ResponseStreamName, 64));

		new ReadOnlySpan<byte>(&hello, sizeof(ChromeConnectionHello)).CopyTo(buf);

		masterStream.Write(buf, 0, sizeof(ChromeConnectionHello));

		managerEvent = Globals.IPCImpl.CreateEvent($"SteamChrome_MasterStream_Event_uid{Platform.Linux.LinuxConsts.getuid()}_spid{GetSteamPID()}", out bool bEventCreated);
		Console.WriteLine("Signaling event");
		managerEvent.Signal();

		Console.WriteLine("Waiting for response");
		var gotData = childStream.BWaitForDataToGet();

		Console.WriteLine("HDR: " + childStream.HeaderPtr->ToString());
        Console.WriteLine("gotData: " + gotData);

		ChromeConnectionHelloResponse resp;
		if (gotData) {
            buf = new byte[sizeof(ChromeConnectionHelloResponse)];
            var actual = buf[0..childStream.Read(buf, 0, buf.Length)];
			if (actual.Length < sizeof(ChromeConnectionHelloResponse)) {
				throw new Exception($"not enough data, got {actual.Length}, but expected {sizeof(ChromeConnectionHelloResponse)}, exiting");
			}
			
			Console.WriteLine("actual: " + string.Join(" ", actual) + ", read: " + actual.Length);
			fixed (byte* bptr = actual) {
				resp = *(ChromeConnectionHelloResponse*)bptr;
			}
		} else {
			throw new Exception("got no data, exiting");
		}

		string? responseStreamName = Marshal.PtrToStringUTF8((nint)resp.ResponseStreamName);
		if (string.IsNullOrEmpty(responseStreamName)) {
			throw new Exception("got no response stream, exiting");
		}

		Console.WriteLine("CMD: " + resp.m_eCmd);
		Console.WriteLine("Server PID: " + resp.m_iServerPID);
		Console.WriteLine("ResponseStreamName: " + responseStreamName);
		CreateStreams(responseStreamName);

		CMsgBrowserCreate proto = new();
		proto.RequestId = 50;
		proto.BrowserType = 1;
		proto.InitialUrl = "https://google.com";
		proto.Useragent = "Valve Steam Client";
		PushCommand(10, 0, proto.ToByteArray());
	}

	private SharedMemStream sendStream;
	private SharedMemStream recvStream;
	
	[MemberNotNull(nameof(sendStream))]
	[MemberNotNull(nameof(recvStream))]
	private void CreateStreams(string responseStreamName, bool bIsServer = false) {
		string sendStreamName = responseStreamName + (bIsServer ? "_server" : "_client");
		string recvStreamName = responseStreamName + (bIsServer ? "_client" : "_server");

		sendStream = new SharedMemStream(sendStreamName, out _);
		sendStream.WriteTimeout = sendStream.ReadTimeout = 1000;

		recvStream = new SharedMemStream(recvStreamName, out _);
		recvStream.WriteTimeout = recvStream.ReadTimeout = 1000;
	}

	private int GetSteamPID() {
		var homevar = Environment.GetEnvironmentVariable("HOME");
		if (string.IsNullOrEmpty(homevar)) {
			throw new NullReferenceException("HOME not set, please set it");
		}

		return int.Parse(File.ReadAllText(Path.Combine(homevar, ".steam/steam.pid")));
    }

	public void PushCommand(int eMsg, int iBrowser, byte[] msg) {
		// Wire format is not known
		using var stream = new MemoryStream();
		using var writer = new EndianAwareBinaryWriter(stream, OpenSteamworks.Utils.Enum.Endianness.Little);
		// writer.WriteInt32(eMsg);
		// writer.WriteInt32(iBrowser);
		writer.WriteInt32(msg.Length);
		writer.Write(msg);

		var msgSerialized = stream.ToArray();
		sendStream.Write(msgSerialized, 0, msgSerialized.Length);
		
		Thread.Sleep(50);
		managerEvent.Signal();
	}
}