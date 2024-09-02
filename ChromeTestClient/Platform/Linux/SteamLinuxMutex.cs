using System.Runtime.InteropServices;

namespace ipctest.Platform.Linux;

//TODO: We could use managed thread ID's instead of native thread id's, however I'm pretty sure the code-executing thread may switch around every now and then when running managed code, so it may cause problems
[StructLayout(LayoutKind.Sequential)]
public unsafe struct SteamLinuxMutex
{
    public int threadID;
    public uint unk1;
    public uint unk2;
    public uint unk3;
    public uint unk4;
    //public uint unk5;
    //public uint** anotherField;
    public robust_list robust_list;

    // This function is almost directly copied from Ghidra. It needs cleanup.
    public static int IPCMutexLock(SteamLinuxMutex* mutex, timespec* timeout)
    {
        int iVar1 = 0;
        uint actualThreadHolder = 0;
        long lVar2 = 0;
        uint ourThreadID = 0;
        uint expectedThreadID = 0;
        uint ownerThread = 0;
        uint fastAcquireRun = 0;
        uint uVar4 = 0;
        bool bVar5 = false;
        uint someLocal = 0;
        robust_list_head* robust_list = null;
        nuint len = 0;
        robust_list* ppuVar1 = null;

        var ret = LinuxConsts.syscall_get_robust_list(LinuxConsts.SYS_get_robust_list, 0, &robust_list, &len);
        if (robust_list == null || len != 24)
        {
            throw new Exception("Fatal error: futex robust_list not initialized by pthreads");
        }

        if (robust_list->futex_offset != -32)
        {
            throw new Exception("Fatal error: futex robust_list not pthreads-compatible");
        }

        robust_list->list_op_pending = (robust_list*)&mutex->robust_list;

        if (ourThreadID == 0)
        {
            ourThreadID = (uint)LinuxConsts.syscall_gettid(LinuxConsts.SYS_gettid);
        }

        ourThreadID = ourThreadID & 0x1fffffff;
        ownerThread = (uint)Interlocked.CompareExchange(ref mutex->threadID, (int)ourThreadID, 0);
        if (ownerThread != 0)
        {
            bool anyConditionMatches = false;
            if ((ourThreadID != (ownerThread & 0x1fffffff)))
            {
                anyConditionMatches = true;
            }
            else if ((ownerThread & 0x40000000) != 0)
            {
                anyConditionMatches = true;
                iVar1 = 0x23;
            }

            if (anyConditionMatches)
            {
                if ((timeout == (timespec*)0x0) || ((timeout->tv_sec != 0 || (timeout->tv_nsec != 0))))
                {
                    fastAcquireRun = 0;
                    someLocal = ownerThread & 0x40000000;
                    expectedThreadID = ownerThread;
                    uVar4 = ourThreadID;
                    if (someLocal == 0) {
                        do
                        {
                            bool doubleJump = false;

                            uVar4 = ourThreadID;
                            if (ownerThread == 0)
                            {
                                expectedThreadID = 0;
                                someLocal = 0;
                            }
                            else if (fastAcquireRun < 100)
                            {
                                ownerThread = 0;
                                fastAcquireRun = fastAcquireRun + 1;
                                expectedThreadID = 0;
                            }
                            else
                            {
                                ourThreadID = ourThreadID | 0x80000000;
                                expectedThreadID = ownerThread;
                                uVar4 = ourThreadID;
                                if (-1 < (int)ownerThread)
                                {
                                    expectedThreadID = ownerThread | 0x80000000;
                                    actualThreadHolder = (uint)Interlocked.CompareExchange(ref mutex->threadID, (int)expectedThreadID, (int)ownerThread);
                                    if (((ownerThread ^ actualThreadHolder) & 0x7fffffff) != 0)
                                    {
                                        fastAcquireRun = fastAcquireRun + 1;
                                        doubleJump = true;
                                        goto breakOutOfIf;
                                        //goto LAB_010438bb;
                                    }
                                }
                                lVar2 = LinuxConsts.syscall_futex(LinuxConsts.SYS_futex, (uint*)&mutex->threadID, 0, expectedThreadID, timeout, null, 0);
                                if (lVar2 < 0)
                                {
                                    iVar1 = Marshal.GetLastWin32Error();
                                    if ((iVar1 != 0xb) && (iVar1 != 4))
                                    {
                                        if (iVar1 == 0) goto uncontestedLock;
                                        goto endOfFunction;
                                    }
                                }
                                fastAcquireRun = fastAcquireRun + 1;
                                expectedThreadID = 0;
                                ownerThread = 0;
                            }

                            breakOutOfIf:

                            while (true)
                            {
                                if (doubleJump) {
                                    doubleJump = false;
                                    goto LAB_010438bb;
                                }

                                actualThreadHolder = (uint)Interlocked.CompareExchange(ref mutex->threadID, (int)ourThreadID, (int)expectedThreadID);
                                if (actualThreadHolder == ownerThread)
                                {
                                    if (someLocal == 0) goto uncontestedLock;
                                    iVar1 = 130;
                                    goto endOfFunction;
                                }
                            LAB_010438bb:
                                someLocal = actualThreadHolder & 0x40000000;
                                ownerThread = actualThreadHolder;
                                expectedThreadID = actualThreadHolder;
                                ourThreadID = uVar4;
                                if (someLocal == 0) break;
                                goto LAB_010438cd;
                            }
                        } while (true);
                    }

                    // someLocal != 0
                LAB_010438cd:
                    ourThreadID = expectedThreadID & 0x80000000 | uVar4;
                    ownerThread = expectedThreadID;

                }

                if ((ownerThread & 0x40000000) != 0)
                {
                    bVar5 = Interlocked.CompareExchange(ref mutex->threadID, (int)(ourThreadID | ownerThread & 0x80000000), (int)ownerThread) == ownerThread;
                    if (bVar5)
                    {
                        iVar1 = 130;
                        goto endOfFunction;
                    }
                }

                iVar1 = 110;
            }

            goto endOfFunction;
        }

    uncontestedLock:     
        // ownerThread == 0
        ppuVar1 = &robust_list->list;
        (mutex->robust_list).next = ppuVar1->next;
        ppuVar1->next = &mutex->robust_list;

    endOfFunction:
        robust_list->list_op_pending = (robust_list*)0x0;
        return iVar1;
    }

    /// <summary>
    /// Unlocks the mutex. Make sure you actually own it before freeing, as there are no checks done.
    /// </summary>
    /// <param name="mutex"></param>
    /// <returns></returns>
    public static int IPCMutexUnlock(SteamLinuxMutex* mutex)
    {
		//Console.WriteLine("Unlocking mutex: " + (nint)mutex);
		var xchgResult = Interlocked.Exchange(ref mutex->threadID, 0);
		if (xchgResult < 0) {
			LinuxConsts.syscall_futex(LinuxConsts.SYS_futex, (uint*)&mutex->threadID, LinuxConsts.FUTEX_WAKE, 1, null, null, 0);
		}

		return xchgResult;
	}

	public override string ToString()
	{
		return $"{threadID} {unk1} {unk2} {unk3} {unk4}";
	}
}