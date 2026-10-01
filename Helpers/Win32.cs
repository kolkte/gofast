using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace XIVSpeedTrainer.Helpers;

public static class Win32
{
    public static IntPtr GetGameHwnd()
    {
        return FindWindow(null, "FINAL FANTASY XIV");
    }

    public static IntPtr GetProcessHandle(IntPtr hwnd)
    {
        GetWindowThreadProcessId(hwnd, out var pid);
        return OpenProcess(0x10 | 0x20 | 0x8 | 0x2, false, (uint)pid);
    }

    public static IntPtr GetModuleBase()
    {
        return Process.GetProcessesByName("ffxiv_dx11")[0].MainModule!.BaseAddress;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(
        IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(
        IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);
}