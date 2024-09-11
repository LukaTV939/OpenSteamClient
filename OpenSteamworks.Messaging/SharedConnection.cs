using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSteamClient.Logging;
using OpenSteamworks.Data;
using OpenSteamworks.Generated;

using OpenSteamworks.Protobuf;
using OpenSteamworks.Utils;
using OpenSteamworks.Utils.Enum;

namespace OpenSteamworks.Messaging;

public sealed class SharedConnection : IDisposable {
    public class StoredMessage {
        public const uint PROTOBUF_MASK = 0x80000000;
        public EMsg eMsg;
        public CMsgProtoBufHeader header;
        public byte[] fullMsg;
        public DateTime removalTime;
        internal StoredMessage(byte[] fullMsg) {
            this.fullMsg = fullMsg;
            this.removalTime = DateTime.UtcNow.AddMinutes(1);
            using (var stream = new MemoryStream(fullMsg)) {
                // The steamclient is a strange beast. A 64-bit library compiled for little endian.
                using (var reader = new EndianAwareBinaryReader(stream, Encoding.UTF8, Endianness.Little))
                {
                    this.eMsg = (EMsg)(~PROTOBUF_MASK & reader.ReadUInt32());

                    // Read the header
                    var header_size = reader.ReadUInt32();
                    Logging.MessagingLogger.Debug("header_size: " + header_size);
                    byte[] header_binary = reader.ReadBytes((int)header_size);

                    // Parse the header
                    this.header = CMsgProtoBufHeader.Parser.ParseFrom(header_binary);

                    this.fullMsg = fullMsg;
                }
            }
        }
    }
    private readonly uint nativeConnection;
    private readonly IClientSharedConnection iSharedConnection;
    private readonly IClientUser clientUser;

    private SharedConnection(IClientSharedConnection iSharedConnection, IClientUser clientUser) {
        this.nativeConnection = iSharedConnection.AllocateSharedConnection();
        this.iSharedConnection = iSharedConnection;
        this.clientUser = clientUser;
        this.StartPollThread();
    }

    public void Dispose()
    {
        this.storedMessages.Clear();
		this.StopPollThread();
		this.iSharedConnection.ReleaseSharedConnection(this.nativeConnection);
    }

    /// <summary>
    /// Sends a oneshot protobuf message. Awaits for results.
    /// </summary>
    public async Task<ProtoMsg<TResult>> ProtobufSendMessageAndAwaitResponse<TResult, TMessage>(ProtoMsg<TMessage> msg, EMsg expectedResponseMsg = EMsg.Invalid) 
    where TResult: Google.Protobuf.IMessage<TResult>, new()
    where TMessage: Google.Protobuf.IMessage<TMessage>, new() {
        return await Task.Run(async () =>
        {
            if (!clientUser.BConnected()) {
                clientUser.EConnect();
                while (!clientUser.BConnected())
                {
                    System.Threading.Thread.Sleep(50);
                }
            }
            var resultMsg = new ProtoMsg<TResult>();

            msg.Header.Steamid = clientUser.GetSteamID();

            // Register a service method handler if we have a jobname
            if (!string.IsNullOrEmpty(msg.JobName)) {
                this.RegisterServiceMethodHandler(msg.JobName);
                this.RegisterEMsgHandler(EMsg.ServiceMethodResponse);
            }

            unsafe {
                byte[] serialized = msg.Serialize();

                fixed (void* pointerToFirst = serialized)
                {
                    uint size = (uint)serialized.Length * sizeof(byte);
                    this.iSharedConnection.SendMessageAndAwaitResponse(this.nativeConnection, pointerToFirst, size);
                }
            }

            if (!string.IsNullOrEmpty(msg.JobName)) {
                // Waiting for a job 
                var recvd = await WaitForServiceMethod(msg.JobName);
				using var stream = new MemoryStream(recvd.fullMsg);
				resultMsg.FillFromStream(stream);
            } else {
                // Waiting for an emsg
                if (expectedResponseMsg == EMsg.Invalid) {
                    throw new ArgumentException("Sending non-service method but didn't indicate response EMsg.");
                }

                var recvd = await WaitForEMsg(expectedResponseMsg);
                using var stream = new MemoryStream(recvd.fullMsg);
				resultMsg.FillFromStream(stream);
            }

            return resultMsg;
        });
    }

    /// <summary>
    /// Sends a oneshot protobuf message.
    /// </summary>
    public async void ProtobufSendMessage<TMessage>(ProtoMsg<TMessage> msg) 
    where TMessage: Google.Protobuf.IMessage<TMessage>, new() {
        await Task.Run(() =>
        {
            if (!clientUser.BConnected()) {
                clientUser.EConnect();
                while (!clientUser.BConnected())
                {
                    System.Threading.Thread.Sleep(50);
                }
            }

			msg.Header.Steamid = clientUser.GetSteamID();

            // Register a service method handler if we have a jobname
            if (!string.IsNullOrEmpty(msg.JobName)) {
                this.iSharedConnection.RegisterServiceMethodHandler(this.nativeConnection, msg.JobName);
                this.iSharedConnection.RegisterEMsgHandler(this.nativeConnection, (uint)EMsg.ServiceMethodResponse);
            }

            unsafe {
                byte[] serialized = msg.Serialize();

                fixed (void* pointerToFirst = serialized)
                {
                    uint size = (uint)serialized.Length * sizeof(byte);
                    this.iSharedConnection.SendMessage(this.nativeConnection, pointerToFirst, size);
                }
            }
        });
    }

    public void RegisterServiceMethodHandler(string serviceMethod) {
        this.iSharedConnection.RegisterServiceMethodHandler(this.nativeConnection, serviceMethod);
    }

    public void RegisterEMsgHandler(EMsg emsg) {
        this.iSharedConnection.RegisterEMsgHandler(this.nativeConnection, (uint)emsg);
    }

    private bool shouldPoll = false;
    private Task? pollThread;
    private readonly List<StoredMessage> storedMessages = new();
    private readonly Dictionary<EMsg, Delegate> eMsgHandlers = new();
    private readonly Dictionary<string, Delegate> serviceMethodHandlers = new();
    public void StartPollThread() {
        if (shouldPoll) {
            throw new InvalidOperationException("Already polling");
        }
        
        shouldPoll = true;
        pollThread = Task.Run(() =>
        {
            using var buffer = new CUtlBuffer(1024, 1024);

            double secondsWaited = 0;
            bool hasMessage = false;

            while (shouldPoll)
            {
                unsafe
                {
                    hasMessage = this.iSharedConnection.BPopReceivedMessage(this.nativeConnection, &buffer, out uint callOut);
                    if (hasMessage)
                    {
                        Logging.MessagingLogger.Debug("Got message: " + callOut + ", size: " + buffer.m_Put + ", waited " + secondsWaited + "ms");
                        var sm = new StoredMessage(buffer.ToManaged());
                        if (eMsgHandlers.TryGetValue(sm.eMsg, out Delegate? eMsgHandler)) {
                            eMsgHandler.DynamicInvoke(sm);
                        } else if (serviceMethodHandlers.TryGetValue(sm.header.TargetJobName, out Delegate? serviceMethodHandler)) {
                            serviceMethodHandler.DynamicInvoke(sm);
                        } else {
                            storedMessages.Add(sm);
                        }
                        
                        secondsWaited = 0;

                        buffer.SeekToBeginning();
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(100);
                        secondsWaited += 0.100;
                    }
                }

                // Remove messages that haven't been retrieved in one minute
                this.storedMessages.RemoveAll(item => DateTime.UtcNow > item.removalTime);
            }
        });
    }

    public void StopPollThread() {
        shouldPoll = false;
    }

    /// <summary>
    /// Adds a handler to be called when a specific EMsg is received. If existing messages are stored, the callback will be retroactively called for all the fitting stored messages.
    /// </summary>
    /// <param name="emsg">EMsg to handle</param>
    /// <param name="callback">Callback to call when the EMsg is received</param>
    /// <param name="oneShot">Whether the handler should automatically be removed once it is called</param>
    public void AddEMsgHandler(EMsg emsg, Action<StoredMessage> callback, bool oneShot = false) {
        Action<StoredMessage> realCallback = callback;
        if (oneShot) {
            realCallback = (StoredMessage msg) =>
            {
                RemoveEMsgHandler(emsg, realCallback);
                callback.Invoke(msg);
            };
        }

        if (!eMsgHandlers.TryGetValue(emsg, out Delegate? value)) {
            eMsgHandlers.Add(emsg, realCallback);
        } else {
            eMsgHandlers[emsg] = Delegate.Combine(value, realCallback);
        }

        foreach (var item in storedMessages)
        {
            if (item.eMsg == emsg) {
                storedMessages.Remove(item);
                realCallback(item);
                if (oneShot) {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Adds a handler to be called when a specific service method is received. If existing messages are stored, the callback will be retroactively called for all the fitting stored messages.
    /// </summary>
    /// <param name="method">Service method to handle</param>
    /// <param name="callback">Callback to call when the service method is received</param>
    /// <param name="oneShot">Whether the handler should automatically be removed once it is called</param>
    public void AddServiceMethodHandler(string method, Action<StoredMessage> callback, bool oneShot = false) {
        this.iSharedConnection.RegisterServiceMethodHandler(this.nativeConnection, method);
        this.iSharedConnection.RegisterEMsgHandler(this.nativeConnection, (uint)EMsg.ServiceMethodResponse);

        Action<StoredMessage> realCallback = callback;
        if (oneShot) {
            realCallback = (StoredMessage msg) =>
            {
                RemoveServiceMethodHandler(method, realCallback);
                callback.Invoke(msg);
            };
        }

        if (!serviceMethodHandlers.TryGetValue(method, out Delegate? value)) {
            serviceMethodHandlers.Add(method, realCallback);
        } else {
            serviceMethodHandlers[method] = Delegate.Combine(value, realCallback);
        }

        foreach (var item in storedMessages)
        {
            if (item.header.TargetJobName == method) {
                storedMessages.Remove(item);
                realCallback(item);
                if (oneShot) {
                    break;
                }
            }
        }
    }

    public void RemoveEMsgHandler(EMsg emsg, Action<StoredMessage> callback) {
        Delegate.Remove(eMsgHandlers[emsg], callback);
        eMsgHandlers.Remove(emsg);
    }

    public void RemoveServiceMethodHandler(string method, Action<StoredMessage> callback) {
        Delegate.Remove(serviceMethodHandlers[method], callback);
        serviceMethodHandlers.Remove(method);
    }

    /// <summary>
    /// Waits until a message with a specific EMsg is received. Will use stored messages if they exist
    /// </summary>
    /// <param name="emsg">EMsg to wait for</param>
    /// <returns>The StoredMessage object for further parsing into a ProtoMsg</returns>
    public async Task<StoredMessage> WaitForEMsg(EMsg emsg) {
        var tcs = new TaskCompletionSource<StoredMessage>();

        AddEMsgHandler(emsg, msg => {
            tcs.TrySetResult(msg);
        }, true);

        return await tcs.Task;
    }

    /// <summary>
    /// Waits until a message with a specific TargetJobName(service method name) is received. Will use stored messages if they exist
    /// </summary>
    /// <param name="method">Service method to  wait for</param>
    /// <returns>The StoredMessage object for further parsing into a ProtoMsg</returns>
    public async Task<StoredMessage> WaitForServiceMethod(string method) {        
        var tcs = new TaskCompletionSource<StoredMessage>();

        AddServiceMethodHandler(method, msg => {
            tcs.TrySetResult(msg);
        }, true);

        return await tcs.Task;
    }

	public static SharedConnection AllocateConnection()
		=> new(SteamClient.GetIClientSharedConnection(), SteamClient.GetIClientUser());
}