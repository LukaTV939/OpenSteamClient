using System.Runtime.InteropServices;
using ipctest.Platform;

namespace ipctest.Platform.Linux;

public unsafe sealed class LinuxEvent : IEvent
{
    private readonly IPCImpl.Entry_Event* entry;

    private IPCImpl ipcImpl => (Globals.IPCImpl as IPCImpl)!;
    public LinuxEvent(string name, bool isManualReset, bool initiallySignaled, out bool bCreated) {
        entry = (IPCImpl.Entry_Event*)ipcImpl.FindOrCreateEntry(IPCImpl.EntryType.Event, name, out bCreated);
        if (bCreated) {
            entry->isManualReset = isManualReset ? 1u : 0u;
			entry->isSignaled = initiallySignaled ? 1u : 0u;
		}
    }

    public void Dispose() {
        ipcImpl.Deref(entry);
    }

    public bool WaitForStateChange(uint timeoutMS)
    {
		bool isSet = Interlocked.CompareExchange(ref entry->isSignaled, entry->isManualReset, 1) == 1;
		if (!isSet) {
			timespec* timeoutPtr = null;
			timespec timeout;
			if (timeoutMS > 0) {
				timeout = new timespec() { tv_sec = timeoutMS / 1000 };
				timeoutPtr = &timeout;
			}
			
			while (!isSet)
			{
				var ret = LinuxConsts.syscall_futex(LinuxConsts.SYS_futex, &entry->isSignaled, LinuxConsts.FUTEX_WAIT, 0, timeoutPtr, null, 0);
				Console.WriteLine("ret: " + ret + ", val: " + entry->ToString());
				
				if (ret == -1) {
					var err = Marshal.GetLastWin32Error();
					if (err == Errno.ETIMEDOUT) {
						return isSet;
					}

					if ((err != Errno.EAGAIN) && (err != Errno.EINTR)) {
						return isSet;
					}
				}

				isSet = Interlocked.CompareExchange(ref entry->isSignaled, entry->isManualReset, 1) == 1;
			}
		}

		return true;
	}

    public void Signal() {
		Interlocked.Increment(ref entry->sequence);
		var isSignaled = Interlocked.CompareExchange(ref entry->isSignaled, 1, 0) == 0;
		if (isSignaled) {
			// It's not safe to release all waiters if it's a manual reset signal
			uint numToWake = entry->isManualReset == 0 ? int.MaxValue : 1u;

			LinuxConsts.syscall_futex(LinuxConsts.SYS_futex, &entry->isSignaled, LinuxConsts.FUTEX_WAKE, numToWake, null, null, 0);
		}
	}

    public void Reset()
    {
		Interlocked.Exchange(ref entry->isSignaled, 0);
    }
}