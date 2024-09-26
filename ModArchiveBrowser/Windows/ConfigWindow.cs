using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace ModArchiveBrowser.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private Plugin plugin;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("A Wonderful Configuration Window###With a constant ID")
    {
        this.plugin = plugin;
        Size = new Vector2(400, 400);

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {

    }

    public override void Draw()
    {
        var cacheSize = Configuration.CacheSize;
        if(ImGui.InputInt("Cache Size", ref cacheSize))
        {
            Configuration.CacheSize = cacheSize;
            Configuration.Save();
        }
        var modCachePath = Configuration.CacheModPath;
        if (ImGui.InputText("Mod cache path",ref modCachePath,300))
        {
            Configuration.CacheModPath = modCachePath;
            plugin.modHandler = new ModHandler(modCachePath,plugin);
        }
        ImGui.SameLine();
        if(ImGui.Button("Select Path....")){
            plugin.fileDialogManager.OpenFolderDialog("Pick mod cache folder", (bool somebool, string somestring) =>
            {
                Plugin.Logger.Debug(somebool.ToString());
                Plugin.Logger.Debug(somestring);
            },string.Empty,true);
        }
        var imageCachePath = Configuration.CacheImagePath;
        if (ImGui.InputText("Image cache part", ref imageCachePath, 300))
        {
            Configuration.CacheModPath = imageCachePath;
            plugin.imageHandler = new ImageHandler(imageCachePath);
        }

        ImGui.NewLine();
        ImGui.Text($"Current Image cache size:{plugin.imageHandler.CalculateFolderSizeInMB():F2}");//:F2 disp up to 2 after float point
        ImGui.Text($"Current Mod cache size:{plugin.modHandler.CalculateFolderSizeInMB():F2}");
        // can't ref a property, so use a local copy
    }
}
