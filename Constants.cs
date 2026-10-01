using System;

namespace XIVSpeedTrainer.Enums;

public static class Constants
{
    public static readonly IntPtr OffsetPlayerSpeed = 0x2A9FE74;

    private static readonly float[] Rates = { 1.0f, 1.05f, 1.15f, 3.0f, 9.99f };
    public static readonly string[] OptionLabels = { "1.00", "1.05", "1.15", "3.00", "9.99" };

    public static float GetSelectedOption(int index)
    {
        if (index < 0 || index >= Rates.Length) return 1.0f;
        return Rates[index];
    }
}