using Dalamud.Configuration;
using System;

namespace XIVSpeedTrainer;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    // This will be controlled by the UI checkbox.
    public bool EnableMovementSpeedHack { get; set; } = false;

    // This will be controlled by the UI slider.
    public float MovementSpeedMultiplier { get; set; } = 1.0f;

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}