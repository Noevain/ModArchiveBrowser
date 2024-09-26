using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.IO;

namespace ModArchiveBrowser;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public int CacheSize { get; set; } = 2000;

    public string CacheModPath { get; set; } = Path.Combine(System.IO.Path.GetTempPath(), "modarchivebrowser\\modCache");
    public string CacheImagePath { get; set; } = Path.Combine(System.IO.Path.GetTempPath(), "modarchivebrowser\\imageCache");

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
