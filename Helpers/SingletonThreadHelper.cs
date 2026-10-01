using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace XIVSpeedTrainer.Helpers;

public static class SingletonThreadHelper
{
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentThread();

    [DllImport("kernel32.dll")]
    private static extern bool SetThreadPriority(IntPtr hThread, int nPriority);

    private const int THREAD_PRIORITY_TIME_CRITICAL = 15;

    public static volatile bool IsModifying;
    private static Thread? _thread;
    private static volatile bool _isRunning;
    private static IntPtr _processHandle = IntPtr.Zero;
    private static volatile float _rate = 1.0f;

    private static void ThreadFunc()
    {
        // Crank this thread to maximum priority so it wins the race against
        // the game's own speed-writing thread.
        var hThread = GetCurrentThread();
        SetThreadPriority(hThread, THREAD_PRIORITY_TIME_CRITICAL);

        while (_isRunning)
        {
            if (IsModifying && _processHandle != IntPtr.Zero)
            {
                // Only write when the player is actually moving.
                // When standing still, the game's own "0" value stays untouched.
                if (Plugin.IsPlayerMoving)
                {
                    PlayerMemoryHelper.WriteMemAtPlayerSpeed(_processHandle, _rate);
                }
            }

            // Tight loop — pins a CPU core, but wins the race against the game's thread.
            Thread.SpinWait(0);
        }
    }

    public static void InitThread(IntPtr hProcess)
    {
        if (_processHandle != IntPtr.Zero) return;
        _processHandle = hProcess;
        _isRunning = true;
        _thread = new Thread(ThreadFunc)
        {
            IsBackground = true,
            Priority = ThreadPriority.Highest
        };
        _thread.Start();
        IsModifying = true;
    }

    public static void Dispose()
    {
        _isRunning = false;
        _thread?.Join(500);
    }

    public static void SetRate(float rate) => _rate = rate;
}