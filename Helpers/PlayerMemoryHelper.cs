using System;
using XIVSpeedTrainer.Enums;

namespace XIVSpeedTrainer.Helpers;

public static class PlayerMemoryHelper
{
    private static IntPtr _cachedBaseAddress = IntPtr.Zero;
    private static IntPtr _cachedSpeedAddress = IntPtr.Zero;
    private static readonly byte[] _readBuffer = new byte[4];
    private static byte[] _writeBuffer = new byte[4];

    private static IntPtr GetSpeedAddress()
    {
        if (_cachedSpeedAddress == IntPtr.Zero)
        {
            _cachedBaseAddress = Win32.GetModuleBase();
            _cachedSpeedAddress = IntPtr.Add(_cachedBaseAddress, (int)Constants.OffsetPlayerSpeed);
        }
        return _cachedSpeedAddress;
    }

    public static float ReadMemAtPlayerSpeed(IntPtr hProcess)
    {
        if (hProcess == IntPtr.Zero) return -1.0f;
        if (Win32.ReadProcessMemory(hProcess, GetSpeedAddress(), _readBuffer, 4, out _))
            return BitConverter.ToSingle(_readBuffer, 0);
        return -1.0f;
    }

    public static bool WriteMemAtPlayerSpeed(IntPtr hProcess, float speedRate)
    {
        if (hProcess == IntPtr.Zero) return false;

        var speedAddress = GetSpeedAddress();

        // Read the current value first so we don't spam writes when it already matches.
        if (!Win32.ReadProcessMemory(hProcess, speedAddress, _readBuffer, 4, out _))
            return false;

        float current = BitConverter.ToSingle(_readBuffer, 0);
        float target = 6.0f * speedRate;

        // If the value is already at our target, do nothing.
        if (Math.Abs(current - target) < 0.01f)
            return true;

        // Sanity check — if the value is absurdly high, the game may have just written
        // something unrelated. Skip.
        if (current > 100f)
            return true;

        _writeBuffer = BitConverter.GetBytes(target);
        return Win32.WriteProcessMemory(hProcess, speedAddress, _writeBuffer, 4, out _);
    }
}