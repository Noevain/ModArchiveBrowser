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
using Dalamud.Utility;
using ImGuizmoNET;
using System.Drawing.Text;
using System.Linq;
using ModArchiveBrowser.Utils;
using System.Threading;
using Penumbra.Api.IpcSubscribers;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ModArchiveBrowser.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;
    private List<ModThumb> modThumbs;
    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    private Task refreshTask = null;
    ConcurrentDictionary<string,ISharedImmediateTexture> images = new ConcurrentDictionary<string, ISharedImmediateTexture>();
    ConcurrentDictionary<string,Task> imagesTasks = new ConcurrentDictionary<string,Task>();
    public MainWindow(Plugin plugin)
        : base("XIV Mod Archive Browser##modarchivebrowserhome")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 500),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.plugin = plugin;
        refreshTask = Task.Run(Refresh);
    }

    public void Dispose() {

    }

    private void Refresh()
    {
        modThumbs = WebClient.GetHomePageMods();
        modThumbs = modThumbs.Distinct().ToList();
        RebuildSharedTextures();
    }

    private async void RebuildSharedTextures()
    {
        imagesTasks.Clear();
        foreach (ModThumb modThumb in modThumbs)
        {
            Task thumbnailTask = Task.Run((async () =>
                                              {
                                                  string path = await plugin.imageHandler.DownloadImage(modThumb.url_thumb);
                                                  ISharedImmediateTexture sharedTexture = Plugin.TextureProvider.GetFromFile(path);
                                                  images.TryAdd(modThumb.url_thumb, sharedTexture);
                                              }));
            imagesTasks.TryAdd(modThumb.url_thumb, thumbnailTask);
        }
    }
    private void DrawHomePageTable()
    {
        if (ImGui.Button("Search")){
            plugin.searchWindow.IsOpen = true;
            plugin.searchWindow.BringToFront();
            this.IsOpen = false;
        }
        ImGui.SameLine();
        if(ImGui.Button("New and Updated from Patreon Subscribers"))
        {
            plugin.searchWindow.IsOpen = true;
            plugin.searchWindow.BringToFront();
            Plugin.Logger.Debug(WebClient.new_and_updated_from_patreon_subs);
            plugin.searchWindow.UpdateSearch(WebClient.new_and_updated_from_patreon_subs);
            this.IsOpen = false;
        }
        ImGui.SameLine();
        if (ImGui.Button("Today Most Viewed Mods"))
        {
            plugin.searchWindow.IsOpen = true;
            plugin.searchWindow.BringToFront();
            Plugin.Logger.Debug(WebClient.today_most_viewed);
            plugin.searchWindow.UpdateSearch(WebClient.today_most_viewed);
            this.IsOpen = false;
        }
        ImGui.SameLine();
        if (ImGui.Button("Newest Mods from All Users"))
        {
            plugin.searchWindow.IsOpen = true;
            plugin.searchWindow.BringToFront();
            Plugin.Logger.Debug(WebClient.newest_mods_from_all_users);
            plugin.searchWindow.UpdateSearch(WebClient.newest_mods_from_all_users);
            this.IsOpen = false;
        }
        if (ImGui.Button("Refresh homepage"))
        {
             refreshTask = Task.Run(() =>
            {
                    Refresh();
            });
        }
        int modCount = 0;
        foreach (ModThumb thumb in modThumbs)
            {
                ImGui.BeginGroup();
                if (refreshTask != null && refreshTask.IsCompleted)
                {
                    var modThumbnail = images[thumb.url_thumb].GetWrapOrDefault();
                    if (modThumbnail != null)
                    {
                        if (ImGui.ImageButton(modThumbnail.ImGuiHandle,
                                              new Vector2(modThumbnail.Width, modThumbnail.Height)))
                        {
                            try
                            {
                                plugin.modWindow.ChangeMod(thumb);
                                if (!plugin.modWindow.IsOpen)
                                {
                                    plugin.modWindow.Toggle();
                                }

                                plugin.modWindow.BringToFront();
                            }
                            catch (Exception e)
                            {
                                Plugin.ReportError("Error while loading mod,check /xllog for details", e);
                            }
                        }
                    }
                }
                else
                {
                    ImGui.Button("Loading....", new Vector2(355, 200));
                }

                ImGui.TextWrapped(thumb.name);

                ImGui.Text($"By: {thumb.author}");

                ImGui.Text($"{thumb.type}");
                ImGui.Text($"{thumb.genders}");

                ImGui.SameLine(0, 100);  // Adjust the padding to float it to the right
                ImGui.Text($"{thumb.views}");

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
