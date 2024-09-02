using System.Runtime.InteropServices;

namespace ipctest.Platform.Linux;

public unsafe static class LinuxConsts {
    public const int SYS_gettid = 186;
    public const int SYS_futex = 202;
    public const int SYS_set_robust_list = 273;
    public const int SYS_get_robust_list = 274;
    public const int FUTEX_WAIT = 0;
    public const int FUTEX_WAKE = 1;
    public const int FUTEX_CMP_REQUEUE = 4;
    public const int O_RDWR = 2;
    public const int O_CREAT = 100;
    public const int PROT_READ = 1;
    public const int PROT_WRITE = 2;
    public const int PROT_EXEC = 4;
    public const int MAP_SHARED = 1;
    public const int MAP_PRIVATE = 2;
    public const int MAP_SHARED_VALIDATE = 3;
    public const int MAP_TYPE = 0x0f;

    // This can't be const due to a dumb C# moment
    public static void* MAP_FAILED => (void*)-1;



    [DllImport("c", EntryPoint = "syscall", SetLastError = true)]
    public static extern long syscall_futex(long num, uint* uaddr, int futex_op, uint val, timespec* timeout, uint* uaddr2, uint val3);

    [DllImport("c", EntryPoint = "syscall", SetLastError = true)]
    public static extern long syscall_get_robust_list(long num, int pid, robust_list_head** head_ptr, UIntPtr* len_ptr);

    [DllImport("c", EntryPoint = "syscall", SetLastError = true)]
    public static extern long syscall_set_robust_list(long num, robust_list_head* head, UIntPtr len);

    [DllImport("c", EntryPoint = "syscall", SetLastError = true)]
    public static extern long syscall_gettid(long num);

    [DllImport("c", EntryPoint = "kill", SetLastError = true)]
    public static extern int kill(int pid, int op);

    [DllImport("c")]
    public static extern int getuid();

    [DllImport("c", SetLastError = true)]
    public static extern void* mmap(void* addr, uint length, int prot, int flags, int fd, uint offset);

    [DllImport("c", SetLastError = true)]
    public static extern int munmap(void* addr, uint length);

    [DllImport("c", SetLastError = true)]
    public static extern int ftruncate(int fd, uint length);

    [DllImport("c", SetLastError = true)]
    public static extern int shm_open([MarshalAs(UnmanagedType.LPUTF8Str)] string name, int oflag, int mode);

    [DllImport("c", SetLastError = true)]
    public static extern int shm_unlink([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport("c", SetLastError = true)]
    public static extern int flock(int fd, int op);
}

public unsafe static class Errno {
    public const int EWOULDBLOCK = 11;
    public const int EINTR = 4;
    public const int ENOLCK = 37;
    public const int EPERM = 1;
    public const int EOWNERDEAD = 130;
    public const int EEXIST = 17;
    public const int EAGAIN = 11;
	public const int ETIMEDOUT = 110;
}

public struct timespec {
    public long	tv_sec;		/* seconds */
    public long tv_nsec;	/* nanoseconds */
}

public unsafe struct robust_list {
    public robust_list* next;
}

public unsafe struct robust_list_head {
    public robust_list list;
    public long futex_offset;
    public robust_list* list_op_pending;
}