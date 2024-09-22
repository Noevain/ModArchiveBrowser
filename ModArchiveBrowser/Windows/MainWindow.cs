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
using System.Net.Http.Headers;
using ImGuizmoNET;

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
        : base("XIV Mod Archive Browser##modarchivebrowserhome")
    {
        modThumbs = WebClient.GetHomePageMods();
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 500),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Plugin = plugin;
    }

    public void Dispose() {

    }

    private void DrawHomePageTable()
    {
        int modCount = 0;
        foreach (ModThumb thumb in modThumbs)
            {
                ImGui.BeginGroup();
                var modThumbnail = Plugin.TextureProvider.GetFromFile(imageHandler.DownloadImage(thumb.url_thumb)).GetWrapOrDefault();
                if (modThumbnail != null)
                {
                    if (ImGui.ImageButton(modThumbnail.ImGuiHandle, new Vector2(modThumbnail.Width, modThumbnail.Height)))
                    {
                    try
                    {
                        Plugin.modWindow.ChangeMod(thumb);
                        if (!Plugin.modWindow.IsOpen)
                        {
                            Plugin.modWindow.Toggle();
                        }
                    }
                    catch(Exception e)
                    {
                        Plugin.Logger.Error($"Caught ex,changing window:{e}");
                    }
                    }
                }
                ImGui.TextWrapped(thumb.name);

                ImGui.Text($"By: {thumb.author}");

                ImGui.Text($"{thumb.type}");
                ImGui.Text($"Genders:{thumb.genders}");

                ImGui.SameLine(0, 100);  // Adjust the padding to float it to the right
                ImGui.Text($"9999 Views Today");

                ImGui.EndGroup();

            if ((modCount + 1) % 3 != 0)  //3 card layout like xivmodarchive
            {
                ImGui.SameLine();  
            }
            else
            {
                ImGui.NewLine();  
            }

            modCount++;

        }
       
    }

    public override void Draw()
    {
        DrawHomePageTable();  
    }
}
