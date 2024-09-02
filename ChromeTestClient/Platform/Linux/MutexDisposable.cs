namespace ipctest.Platform.Linux;

public unsafe sealed class MutexDisposable : IDisposable
{
    private SteamLinuxMutex* mutex;
    private Action? destructExtraFunc;
    public MutexDisposable(SteamLinuxMutex* mutex, uint timeoutMS = 5000, Action? acquireExtraFunc = null, Action? destructExtraFunc = null) {
        this.destructExtraFunc = destructExtraFunc;
        this.mutex = mutex;
        if (timeoutMS != 0) {
            // Wait a specified timeout
            timespec timeout = new()
            {
                tv_sec = timeoutMS / 1000
            };
            
            ErrnoException.ThrowIfNonZero(SteamLinuxMutex.IPCMutexLock(mutex, &timeout));
            acquireExtraFunc?.Invoke();
        } else {
            // Wait forever
            ErrnoException.ThrowIfNonZero(SteamLinuxMutex.IPCMutexLock(mutex, null));
            acquireExtraFunc?.Invoke();
        }
    }

    public void Dispose()
    {
        destructExtraFunc?.Invoke();
        SteamLinuxMutex.IPCMutexUnlock(mutex);
    }
}