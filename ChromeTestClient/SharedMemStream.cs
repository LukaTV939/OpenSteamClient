using System.IO;
using System.Runtime.InteropServices;
using ipctest.Platform;

namespace ipctest;

public unsafe sealed class SharedMemStream : IDisposable
{
    public struct SharedMemHeader {
        public uint totalRead;
        public uint totalWritten;
        public uint connSize;
        public uint availableBytes;

        public override string ToString()
        {
            return $"{totalRead} {totalWritten} {connSize} {availableBytes}";
        }
    }

    private readonly ISharedMemory sharedMemory;
    private readonly IEvent writtenEvent;
    private readonly IEvent availEvent;
    private readonly IMutex mutex;
    private bool isDisposed = false;
    public SharedMemHeader* HeaderPtr => (SharedMemHeader*)sharedMemory.Data;
    private byte* DataPtr => (byte*)((nint)sharedMemory.Data + sizeof(SharedMemHeader));
    private byte* EndPtr => (byte*)((nint)sharedMemory.Data + HeaderPtr->connSize);

    // Stream compat
    public int WriteTimeout { get; set; }
    public int ReadTimeout { get; set; }


    public SharedMemStream(string name, out bool bCreated, uint wantedSize = 0x1900000, int timeoutMS = 1000) {
		this.WriteTimeout = timeoutMS;
		this.ReadTimeout = timeoutMS;

		mutex = Globals.IPCImpl.CreateMutex($"{name}_mutex", out bool bCreatedMutex);
        IDisposable? constructLock = null;
        if (bCreatedMutex) {
            constructLock = mutex.Lock();
        }

        try
        {
            writtenEvent = Globals.IPCImpl.CreateEvent($"{name}_written", out _);
            availEvent = Globals.IPCImpl.CreateEvent($"{name}_avail", out _);
            var allocatedSize = (uint)(wantedSize + sizeof(SharedMemHeader));
            sharedMemory = Globals.IPCImpl.CreateSharedMemory($"{name}_mem", allocatedSize, out bCreated);
            Console.WriteLine($"Got ptr {(nint)sharedMemory.Data}");
            if (!bCreated) {
                // Check validity of existing stream
                if (HeaderPtr->connSize != wantedSize) {
                    throw new Exception($"Size on connection to existing SharedMemStream doesn't match actual: {name}, {allocatedSize}, {HeaderPtr->connSize}");
                }
            } else {
                NativeMemory.Clear(sharedMemory.Data, allocatedSize);
                HeaderPtr->connSize = wantedSize;
            }

			ReadPosition = HeaderPtr->totalRead;
			
		}
        finally
        {
            constructLock?.Dispose();
        }
    }

    public void Dispose() {
        if (isDisposed) {
            return;
        }

        isDisposed = true;
        mutex.Dispose();
        sharedMemory.Dispose();
        writtenEvent.Dispose();
        availEvent.Dispose();
    }

    public bool BWaitForDataToGet(uint timeoutMS = 0) {
        ObjectDisposedException.ThrowIf(this.isDisposed, this);
        if (WritePosition > ReadPosition) {
            return true;
        }

        writtenEvent.WaitForStateChange(timeoutMS);
		Console.WriteLine("HDR: " + HeaderPtr->ToString());
		Console.WriteLine("AvailableToRead: " + AvailableToRead);
		return AvailableToRead > 0;
    }

    public long Capacity => HeaderPtr->connSize;
    public long ReadPosition { get; set; }

	public long WritePosition { 
        get => HeaderPtr->totalWritten; 
        set => HeaderPtr->totalWritten = (uint)value; 
    }

	public long AvailableToRead
		=> HeaderPtr->totalWritten - ReadPosition;

	public long RemainingCapacity
		=> this.EndPtr - (this.DataPtr + HeaderPtr->availableBytes);

	public long GetOffsetReadPosition(int offset) {
        return ReadPosition + offset;
    }

    public long GetOffsetWritePosition(int offset) {
        return WritePosition + offset;
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        ObjectDisposedException.ThrowIf(this.isDisposed, this);
        ThrowIfArrayDoesntMatchSize(buffer, count, nameof(count));

        Console.WriteLine("LockR");
        using var l = mutex.Lock((uint)this.ReadTimeout);
        Console.WriteLine("LockedR");
        
		long toRead = long.Clamp(count, 0, Capacity);
		toRead = long.Clamp(toRead, 0, buffer.Length);
		toRead = long.Clamp(toRead, 0, HeaderPtr->totalWritten - GetOffsetReadPosition(offset));
		
		Console.WriteLine("HDR: " + HeaderPtr->ToString());
		fixed (byte* ptr = buffer) {
            NativeMemory.Copy(DataPtr + GetOffsetReadPosition(offset), ptr, (nuint)toRead);
        }
		
		if (HeaderPtr->totalRead > ReadPosition) {
			// Only update the read count and available bytes if we're reading at the head
			HeaderPtr->totalRead += (uint)toRead;

			if (toRead >= HeaderPtr->availableBytes) {
				// Only update available bytes if we're reading at the top
				HeaderPtr->availableBytes -= (uint)toRead;
				availEvent.Signal();
			}
		}

		ReadPosition += (uint)toRead;

		// This API is terrible. Why do we need so many conversions?
		return (int)toRead;
    }

    public void Write(byte[] buffer, int offset, int count)
    {
        ObjectDisposedException.ThrowIf(this.isDisposed, this);
        ThrowIfArrayDoesntMatchSize(buffer, count, nameof(count));

        Console.WriteLine("LockW");
        using var l = mutex.Lock((uint)this.WriteTimeout);
        Console.WriteLine("LockedW");

        long maxWritable = long.Clamp(count, 0, Capacity - HeaderPtr->totalWritten - offset);
        if (maxWritable < count) {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        fixed (byte* ptr = buffer) {
            NativeMemory.Copy(ptr, DataPtr + GetOffsetWritePosition(offset), (nuint)maxWritable);
            HeaderPtr->availableBytes += (uint)maxWritable;
            HeaderPtr->totalWritten += (uint)maxWritable;
        }

		this.writtenEvent.Signal();
	}

    private static void ThrowIfArrayDoesntMatchSize(byte[] arr, long size, string argumentName) {
        if (size > arr.LongLength) {
            throw new ArgumentOutOfRangeException(argumentName);
        }
    }
}