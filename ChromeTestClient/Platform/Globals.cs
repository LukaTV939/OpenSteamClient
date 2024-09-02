namespace ipctest.Platform;

public static class Globals {
    public static IIPCImpl IPCImpl { get; }
    static Globals() {
        if (OperatingSystem.IsLinux()) {
            IPCImpl = new Linux.IPCImpl();
            return;
        }

        if (OperatingSystem.IsWindows()) {
            //IPCImpl = new Windows.IPCImpl();
            //return;
        }

        throw new NotImplementedException("unsupported os");
    }
}