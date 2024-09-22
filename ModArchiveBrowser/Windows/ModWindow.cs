using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Penumbra.Api.IpcSubscribers;
using Penumbra.Api.Enums;
namespace ModArchiveBrowser.Windows
{
    public class ModWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private Mod? mod;
        private ModHandler modHandler = new ModHandler("./ModDownloads");
        private ImageHandler imageHandler = new ImageHandler("./DownloadCache");
        public ModWindow(Plugin plugin): base("Mod view window##")
        {
            Plugin = plugin;

            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(375, 330),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
        }

        public void ChangeMod(ModThumb modThumb)
        {
            this.mod = WebClient.GetModPage(modThumb);
        }
        public void Dispose()
        {

        }

        private void DrawModPage()
        {

            // DT compatiblity
            ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), "DT Compatibility: ✅ This mod is compatible with Dawntrail.");

            ImGui.Columns(2, "Columns", true);

            // Left Column (Mod Information)
            {
                // Mod Title
                ImGui.BeginChild("LeftColumn", new Vector2(0, 0), true);
                ImGui.Text(mod.Value.modThumb.name);

                ImGui.Separator();

                // Author
                ImGui.Text($"{mod.Value.modThumb.type} by {mod.Value.modThumb.author}");

                // Image Carousel Placeholder
                ImGui.Text("Mod Preview Image");
                var modThumbnail = Plugin.TextureProvider.GetFromFile(imageHandler.DownloadImage(mod.Value.modThumb.url_thumb)).GetWrapOrDefault();
                if (modThumbnail != null)
                {
                    ImGui.Image(modThumbnail.ImGuiHandle, new Vector2(300, 200)); // Placeholder for image carousel
                }
                else
                {
                    ImGui.Button("Failed to load thumbnail", new Vector2(300, 200));
                }

                // Tabs (Info, Files, History)
                ImGui.TextWrapped(mod.Value.modMeta.description);

                ImGui.EndChild();
            }

            ImGui.NextColumn(); // Move to right column

            // Right Column (Author Info, Download, Stats)
            {
                ImGui.BeginChild("RightColumn", new Vector2(0, 0), true);

                // Author Card
                ImGui.Text(mod.Value.modThumb.author);
                var authorpicThumbnail = Plugin.TextureProvider.GetFromFile(imageHandler.DownloadImage(mod.Value.url_author_profilepic)).GetWrapOrDefault();
                if (authorpicThumbnail != null)
                {
                    ImGui.Image(authorpicThumbnail.ImGuiHandle, new Vector2(100, 100));
                }
                else
                {
                    ImGui.Button("Failed to get user avatar", new Vector2(100, 100)); // Placeholder for avatar
                }
                ImGui.Separator();

                // Download button
                    if (ImGui.Button("Install using Penumbra"))
                    {
                        string modpath = modHandler.DownloadMod(WebClient.xivmodarchiveRoot + mod.Value.url_download_button);
                        PenumbraApiEc res = Plugin.penumbra.InstallMod(modpath);
                        if (res != PenumbraApiEc.Success)
                        {
                            Plugin.Logger.Error($"Failed to install mod,code:{res.ToString()}");
                        }
                        else
                        {
                            Plugin.penumbra.OpenModWindow();
                        }

                    }
               

                ImGui.Separator();

                // Stats
                ImGui.Text($"Views: {mod.Value.modMeta.views}");
                ImGui.Text($"Downloads: {mod.Value.modMeta.downloads}");
                ImGui.Text($"Followers: {mod.Value.modMeta.pins}");

                ImGui.Separator();

                // Metadata
                var race_str = string.Empty;
                for (int i = 0; i < mod.Value.modMeta.races.Length; i++)
                {
                    race_str = race_str + mod.Value.modMeta.races[i] + " ,";
                }
                var tag_str = string.Empty;
                for (int i = 0; i < mod.Value.modMeta.tags.Length; i++)
                {
                    tag_str = tag_str + mod.Value.modMeta.tags[i]+ " ,";
                }
                ImGui.Text($"Last Version Update: {mod.Value.modMeta.last_update}");
                ImGui.Text($"Affects / Replaces: {mod.Value.modMeta.affectReplace}");
                ImGui.Text($"Races: {race_str}");
                ImGui.TextWrapped($"Genders: {mod.Value.modThumb.genders}");
                ImGui.TextWrapped($"Tags: {tag_str}");

                ImGui.EndChild();
            }

            ImGui.Columns(1); // End columns
        }
        public override void Draw()
        {
           if(mod is not null)
            {
                DrawModPage();
            }
            else
            {
                ImGui.Text("No mod selected,use the main window to browse some mods");
            }
           

        }
    }
}
