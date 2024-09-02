namespace ipctest.Platform.Linux;

using System.IO.Hashing;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;

public unsafe class IPCImpl : IIPCImpl
{
    public enum EntryType : int
    {
        None,
        Mutex,
        Event,
        SharedMemory,
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Entry_Mutex {
		public Entry Header;
        public SteamLinuxMutex Mutex;

		public override string ToString()
		{
			return $"{Header}: {Mutex}";
		}
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Entry_Event {
		public Entry Header;
		public uint unk;
		public uint isSignaled;
        public uint sequence;
		public uint isManualReset;

		public override string ToString()
		{
			return $"{Header}: {unk} {isSignaled} {sequence} {isManualReset}";
		}
	
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct Entry {
        public EntryType Type;
        public uint NameCRC;
        public int RefCount;

        public const int PID_ARR_SIZE = 16;
        public fixed int ReferencingPIDS[PID_ARR_SIZE];

        public static void Init(Entry* pThis, EntryType type, uint nameCRC, bool AddReference = true) {
            pThis->NameCRC = nameCRC;
            if (AddReference) {
                Entry.AddReference(pThis);
            }

            pThis->Type = type;
        }

        public static void AddReference(Entry* pThis) {
            AddReferenceCore(pThis);
        }

        private static void AddReferenceCore(Entry* pThis, bool didCleanup = false) {
            var pid = Environment.ProcessId;
            for (int i = 0; i < PID_ARR_SIZE; i++)
            {
                if (pThis->ReferencingPIDS[i] == 0) {
                    pThis->ReferencingPIDS[i] = pid;
                    pThis->RefCount++;
                    return;
                }
            }

            // No problem, try clearing out unused references and try again
            if (!didCleanup) {
                CleanupDeadReferences(pThis);
                AddReferenceCore(pThis, true);
                return;
            }

            throw new Exception("PID array ran out");
        }

        public static bool RemoveReference(Entry* pThis, int pid) {
            for (int i = 0; i < PID_ARR_SIZE; i++)
            {
                var elem = pThis->ReferencingPIDS[i];
                if (elem == pid) {
                    elem = pid;
                    pThis->RefCount--;
                    return true;
                }
            }

            return false;
        }

        public static bool AreAnyReferencesAlive(Entry* pThis) {
            for (int i = 0; i < PID_ARR_SIZE; i++)
            {
                var elem = pThis->ReferencingPIDS[i];
                if (elem != 0) {
                    if (LinuxConsts.kill(elem, 0) == 0) {
                        return true;
                    } else {
                        // Clear it from the list if not alive
                        pThis->ReferencingPIDS[i] = 0;
                        pThis->RefCount--;
                    }
                }
            }

            return false;
        }

        public static void CleanupDeadReferences(Entry* pThis) {
            for (int i = 0; i < PID_ARR_SIZE; i++)
            {
                var elem = pThis->ReferencingPIDS[i];
                if (elem != 0) {
                    if (LinuxConsts.kill(elem, 0) != 0) {
                        // Clear it from the list if not alive
                        pThis->ReferencingPIDS[i] = 0;
                        pThis->RefCount--;
                    }
                }
            }
        }

        public static void Destruct(Entry* pThis)
        {
			//TODO: Get this info from the SharedObjectManagerHeader
            NativeMemory.Clear(pThis, MGR_ELEM_SIZE);
        }

        public static bool HasAnyReferences(Entry* pThis)
        {
            return pThis->RefCount != 0;
        }

        public override string ToString()
        {
            return $"T: {Type}, CRC: {NameCRC}, C: {RefCount}";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SharedObjectManagerHeader
    {
        public int version;
        public int elemSize;
        public int length;
        public int mgrMutexOwner;
        public int unused;
        public int unused2;
        public SteamLinuxMutex sysMgrMutex;


        public override string ToString()
        {
            return $"v: {version} elemSize: {elemSize} totalSize: {length} currentOwner: {mgrMutexOwner} {unused} {unused2} currentLockingThreadID: {sysMgrMutex.threadID}";
        }
    }

    public const int MGR_SHM_SIZE = 0x200000;
    public const int MGR_ELEM_SIZE = 256;
    public const int MGR_VERSION = 4;

    private void* mmapTarget;
    private SharedObjectManagerHeader* header => (SharedObjectManagerHeader*)mmapTarget;

    public IPCImpl()
    {
        int shm_fd = -1;
        bool isHoldingSysMgrMutex = false;

        try {
            var ipcName = GetIPCObjName();
            var shmFilepath = $"/dev/shm{ipcName}";
            Console.WriteLine("IPC obj at " + shmFilepath);

            shm_fd = LinuxConsts.shm_open(ipcName, 66, 511);
            Console.WriteLine("open result: " + shm_fd);

            if (shm_fd < 0) {
                throw new Exception($"Process {Environment.ProcessId} failed to shm_open {ipcName}");
            }

            if (LinuxConsts.flock(shm_fd, 2) != 0) {
                throw new Exception($"Error {Marshal.GetLastWin32Error()} locking shared memory file");
            }

            var allocated = new FileInfo(shmFilepath).Length;
            Console.WriteLine("Bytes Allocated: " + allocated);
            if (allocated < MGR_SHM_SIZE)
            { 
                Console.WriteLine("ftruncate: " + LinuxConsts.ftruncate(shm_fd, MGR_SHM_SIZE));
                var newsize = new FileInfo(shmFilepath).Length;
                Console.WriteLine("New Size: " + newsize);
            }

            mmapTarget = LinuxConsts.mmap((void*)0x0, MGR_SHM_SIZE, 3, 1, shm_fd, 0);
            Console.WriteLine("mmap result: " + (nint)mmapTarget);
            Console.WriteLine(header->ToString());

            if (header->version == 0) {
                // Need to initialize
                header->length = MGR_SHM_SIZE;
                header->elemSize = MGR_ELEM_SIZE;
                header->version = MGR_VERSION;
            }

            if (header->version != MGR_VERSION && header->version != 0) {
                throw new NotImplementedException("version upgrade detected, new version is " + header->version);
            }

			if (header->elemSize != MGR_ELEM_SIZE) {
                throw new NotImplementedException($"element size change detected, expected {MGR_ELEM_SIZE}, got {header->elemSize}");
            }
        }
        finally
        {
            if (isHoldingSysMgrMutex && header != null)
            {
                SteamLinuxMutex.IPCMutexUnlock(&header->sysMgrMutex);
            }
            
            if (shm_fd != -1) {
                LinuxConsts.flock(shm_fd, 8);
            }
            
        }
    }

    public ISharedMemory CreateSharedMemory(string name, uint size, out bool bCreated) {
        Console.WriteLine("CreateSharedMemory: " + name);

        using var l = Lock();
        return new LinuxSharedMemory(name, size, out bCreated);
    }

    public IEvent CreateEvent(string name, out bool bCreated, bool isManualReset, bool initiallySignaled) {
        Console.WriteLine("CreateEvent: " + name);

        using var l = Lock();
        return new LinuxEvent(name, isManualReset, initiallySignaled, out bCreated);
    }

    public IMutex CreateMutex(string name, out bool bCreated) {
        Console.WriteLine("CreateMutex: " + name);

        SteamLinuxMutex.IPCMutexUnlock(&header->sysMgrMutex);
        using var l = Lock();
        return new LinuxMutex(name, out bCreated);
    }

    private MutexDisposable Lock() {
        return new MutexDisposable(&header->sysMgrMutex, 5000, () => { header->mgrMutexOwner = Environment.ProcessId; }, () => {header->mgrMutexOwner = 0; });
    }

    public static string HashName(string name) {
        return Convert.ToHexString(BitConverter.GetBytes(HashNameUInt32(name)).Reverse().ToArray());
    }

    public static uint HashNameUInt32(string name) {
        return Crc32.HashToUInt32(Encoding.UTF8.GetBytes(name));
    }

    public void* DynamicStart => (void*)((nint)mmapTarget + sizeof(SharedObjectManagerHeader) + 32);
    public Entry* FindEntry(EntryType type, string name) {
        uint nameCRC = HashNameUInt32(name);
        int sizeOfEntry = header->elemSize;
        void* current = DynamicStart;
        void* last = (void*)((nint)header + header->length);
        int entryCount = 0;
        while (current < last)
        {
            Entry* pEntry = (Entry*)current;

            if (pEntry->Type == 0) {
                goto loopEnd;
            }

            if (pEntry->Type == type && pEntry->NameCRC == nameCRC) {
                Console.WriteLine($"Match, found '{name}'");
                return pEntry;
            } else {
                Console.WriteLine("Not a match: a: " + pEntry->Type + ", w: " + type);
                Console.WriteLine("Not a match: a: " + pEntry->NameCRC + ", w: " + nameCRC);
            }

        loopEnd:
            entryCount++;
            current = (void*)((nint)current + sizeOfEntry);
        }

        //throw new Exception("Did not find entry, searched " + entryCount);

        return null;
    }

    public Entry* CreateEntry(EntryType type, string name) {
        uint nameCRC = HashNameUInt32(name);
        int sizeOfEntry = header->elemSize;
        void* current = DynamicStart;
        void* last = header + header->length;
		int entryCount = 0;
        while (current < last)
        {
            Entry* pEntry = (Entry*)current;
            if (pEntry->Type == 0) {
				// Non initialized entry, mark this as ours and return it
				Console.WriteLine("Init entry at pos " + entryCount);
				Entry.Init(pEntry, type, nameCRC);
                return pEntry;
            } 

            current = (void*)((nint)current + sizeOfEntry);
			entryCount++;
		}
        
        throw new Exception("Ran out of IPC SharedObjectManager entries!");
    }

    private static string GetIPCObjName()
    {
        return $"/u{LinuxConsts.getuid()}-ValveIPCSharedObj-Steam";
    }

    /// <summary>
    /// LOCK BEFORE USING!
    /// </summary>
    public Entry* FindOrCreateEntry(EntryType type, string name, out bool bCreated)
    {
        Entry* entry = FindEntry(type, name);
        if (entry != null) {
            bCreated = false;
            IPCImpl.Entry.AddReference(entry);
        } else {
            bCreated = true;
            entry = CreateEntry(type, name);
        }

        return entry;
    }
    
    public void Deref(void* entry)
    {
        using var l = Lock();

        ArgumentNullException.ThrowIfNull(entry);
        if (!IPCImpl.Entry.RemoveReference((Entry*)entry, Environment.ProcessId)) {
            throw new Exception("Dereferencing failed");
        }

        if (!IPCImpl.Entry.HasAnyReferences((Entry*)entry)) {
            Entry.Destruct((Entry*)entry);
        }
    }
}