using Dalamud.Configuration;
using System;

namespace XIVSpeedTrainer;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool EnableMovementSpeedHack { get; set; } = false;
    public float MovementSpeedMultiplier { get; set; } = 1.0f;

    // Kept for the old MainWindow UI. Not used by the hook.
    public int SelectedSpeedOption { get; set; } = 0;

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}