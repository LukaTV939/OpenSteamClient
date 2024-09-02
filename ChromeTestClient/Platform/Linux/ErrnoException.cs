using System.Runtime.InteropServices;

namespace ipctest.Platform.Linux;

[System.Serializable]
public class ErrnoException : System.Exception
{
    public ErrnoException() { }
    public ErrnoException(int errno) : base($"Linux system call failed with {errno}") { }
    public ErrnoException(int errno, System.Exception inner) : base($"Linux system call failed with {errno}", inner) { }

    public static void ThrowIfNonZero(int value) {
        if (value != 0) {
            throw new ErrnoException(value);
        }
    }

    public static void ThrowCurrentMarshaledIfNegative(int value) {
        if (value < 0) {
            throw new ErrnoException(Marshal.GetLastWin32Error());
        }
    }
}