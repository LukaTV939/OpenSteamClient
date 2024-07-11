using System;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenSteamworks.Utils;

public unsafe sealed class PinnedUTF8String : IDisposable {
    private readonly PinnedByteArray underlyingBytes = new(256);
    public string CurrentString {
        get => Marshal.PtrToStringUTF8((nint)underlyingBytes.CurrentPtr) ?? string.Empty;
        set => underlyingBytes.SetData(Encoding.UTF8.GetBytes(value + "\0"));
    }

    public byte* CurrentPtr => underlyingBytes.CurrentPtr;

    public PinnedUTF8String() { }

    public PinnedUTF8String(string str) {
        this.CurrentString = str;
    }

    public void Dispose()
    {
        underlyingBytes.Dispose();
    }
}