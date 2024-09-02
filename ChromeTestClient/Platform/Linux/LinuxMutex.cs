
namespace ipctest.Platform.Linux;

public sealed unsafe class LinuxMutex : IMutex {
    private readonly IPCImpl.Entry_Mutex* entry;

    private IPCImpl ipcImpl => (Globals.IPCImpl as IPCImpl)!;
    public LinuxMutex(string name, out bool bCreated) {
        entry = (IPCImpl.Entry_Mutex*)ipcImpl.FindOrCreateEntry(IPCImpl.EntryType.Mutex, name, out bCreated);
    }

    public void Dispose() {
        ipcImpl.Deref(entry);
    }

    public IDisposable Lock(uint timeoutMS)
    {
        return new MutexDisposable(&entry->Mutex, timeoutMS);
    }
}