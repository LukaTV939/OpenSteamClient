namespace ipctest.Platform;

public unsafe interface IIPCImpl {
    public ISharedMemory CreateSharedMemory(string name, uint size, out bool bCreated);
    public IEvent CreateEvent(string name, out bool bCreated, bool isManualReset = false, bool initiallySignaled = false);
    public IMutex CreateMutex(string name, out bool bCreated);
}

public unsafe interface ISharedMemory : IDisposable {
    public void* Data { get; }
    public uint Length { get; }
}

public unsafe interface IEvent : IDisposable {
    public void Signal();
    public void Reset();

	/// <summary>
	/// 
	/// </summary>
	/// <param name="timeoutMS"></param>
	/// <returns>True if the state has changed, false if an error occurred.</returns>
    public bool WaitForStateChange(uint timeoutMS);
}

public unsafe interface IMutex : IDisposable {
    public IDisposable Lock(uint timeoutMS = 5000);
}