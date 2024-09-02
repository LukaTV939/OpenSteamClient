using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Text;

namespace ipctest.Platform.Linux;

public unsafe sealed class LinuxSharedMemory : ISharedMemory
{
    public void* Data { get; private set; }
    public uint Length { get; private set; }

    public void Dispose()
    {
        (Globals.IPCImpl as IPCImpl)!.Deref(entry);
        LinuxConsts.munmap(Data, Length);
    }

    private readonly IPCImpl.Entry* entry;

    public LinuxSharedMemory(string name, uint size, out bool bCreated) {
        bCreated = false;
        Length = size;

        var hash = IPCImpl.HashName(name);
        string memname = $"/u{LinuxConsts.getuid()}-Shm_{hash.ToLowerInvariant()}";
        string filename = $"/dev/shm{memname}";

        entry = (Globals.IPCImpl as IPCImpl)!.FindEntry(IPCImpl.EntryType.SharedMemory, name);

		int fd;
		if (entry != null) {
            bCreated = false;
            IPCImpl.Entry.AddReference(entry);
			Console.WriteLine("LinuxSharedMemory, opening: " + filename);
			fd = LinuxConsts.shm_open(memname, LinuxConsts.O_RDWR, 511);
        } else {
			Console.WriteLine("LinuxSharedMemory, creating: " + filename);
			bCreated = true;
			fd = LinuxConsts.shm_open(memname, LinuxConsts.O_RDWR | LinuxConsts.O_CREAT, 511);
			entry = (Globals.IPCImpl as IPCImpl)!.CreateEntry(IPCImpl.EntryType.SharedMemory, name);
		}

        try
		{
			if (fd < 0) {
				throw new Exception("Opening shared memory failed with " + Marshal.GetLastWin32Error());
			}
	
			var allocated = new FileInfo(filename).Length;
			Console.WriteLine("Bytes Allocated: " + allocated);
			if (allocated < size)
			{
				Console.WriteLine("ftruncate: " + LinuxConsts.ftruncate(fd, size));
				var newsize = new FileInfo(filename).Length;
				Console.WriteLine("New Size: " + newsize);
			}
	
			var ret = LinuxConsts.mmap(null, size, LinuxConsts.PROT_READ | LinuxConsts.PROT_WRITE, LinuxConsts.MAP_SHARED, fd, 0);
			if (ret == LinuxConsts.MAP_FAILED) {
				throw new Exception("mmap failed with " + Marshal.GetLastWin32Error());
			}
	
			Data = ret;
		}
		catch (System.Exception)
		{
			if (entry != null) {
				IPCImpl.Entry.RemoveReference(entry, Environment.ProcessId);
			}

			throw;
		}
    }
}