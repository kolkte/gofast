using System;
using XIVSpeedTrainer.Enums;

namespace XIVSpeedTrainer.Helpers;

public static class PlayerMemoryHelper
{
    private static IntPtr _cachedBaseAddress = IntPtr.Zero;
    private static IntPtr _cachedSpeedAddress = IntPtr.Zero;
    private static byte[] _readBuffer = new byte[4];
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
        const float normalSpeed = 6.0f;
        float target = speedRate * normalSpeed;

        // Skip if the current value already matches (approximately).
        if (Win32.ReadProcessMemory(hProcess, GetSpeedAddress(), _readBuffer, 4, out _))
        {
            float current = BitConverter.ToSingle(_readBuffer, 0);
            if (Math.Abs(current - target) < 0.01f)
                return true;
        }

        _writeBuffer = BitConverter.GetBytes(target);
        return Win32.WriteProcessMemory(hProcess, GetSpeedAddress(), _writeBuffer, 4, out _);
    }
}