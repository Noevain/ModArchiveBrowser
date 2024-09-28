using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;

namespace ModArchiveBrowser;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public int CacheSize { get; set; } = 2000;

    public string CacheModPath { get; set; } = Path.Combine(System.IO.Path.GetTempPath(), "modarchivebrowser\\modCache");
    public string CacheImagePath { get; set; } = Path.Combine(System.IO.Path.GetTempPath(), "modarchivebrowser\\imageCache");

    public string ThumbnailsFolder { get; set; } = Path.Combine(System.IO.Path.GetTempPath(), "modarchivebrowser\\thumbnails");
    public HashSet<string> CacheFiles { get; set; } = new HashSet<string>();
    public Dictionary<string, string> modNameToThumbnail = new Dictionary<string, string>();
    public bool penumbraDispThumb = true;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
