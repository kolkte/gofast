using Dalamud.Configuration;
using System;

namespace XIVSpeedTrainer;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public int SelectedSpeedOption { get; set; } = 0;

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}