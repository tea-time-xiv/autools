using Dalamud.Configuration;
using System;

namespace Autools;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool AutoPriorityPassEnabled { get; set; }
    public bool NoJogEnabled { get; set; }

    // The below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
