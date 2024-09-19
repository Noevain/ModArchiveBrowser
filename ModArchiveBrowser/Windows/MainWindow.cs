using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using System.Net.Http;
using ImGuiNET;
using HtmlAgilityPack;
using Dalamud.Interface.Textures;

namespace ModArchiveBrowser.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private List<ModThumb> modThumbs;

    private ImageHandler imageHandler = new ImageHandler("./DownloadCache");
    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("My Amazing Window##With a hidden ID")
    {
        modThumbs = WebClient.GetHomePageMods();
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        foreach (ModThumb thumb in modThumbs)
        {

            ImGui.Text($"Mod title:{thumb.name}");
            ImGui.Text($"Mod author:{thumb.author}");
            ImGui.Separator();
            //Plugin.Logger.Debug($"Starting image dl task for{thumb.url_thumb}");
            var modThumbnail = Plugin.TextureProvider.GetFromFile(imageHandler.DownloadImage(thumb.url_thumb)).GetWrapOrDefault();
            if (modThumbnail != null)
            {
                ImGui.Image(modThumbnail.ImGuiHandle, new Vector2(modThumbnail.Width, modThumbnail.Height));
            }
            ImGui.Separator();
        }
    }
}
