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
using Dalamud.Utility;
using ModArchiveBrowser.Utils;
using System.IO;
using HtmlAgilityPack;
using Dalamud.Interface.Utility.Raii;
namespace ModArchiveBrowser.Windows
{
    public class ModWindow : Window, IDisposable
    {
        private Plugin plugin;
        private Mod? mod;
        private HtmlNodeCollection descriptionNodes;
        private bool failedAvatarUrl = false;
        private bool _isLoading = false;
        private string _statusMessage = string.Empty;
        public ModWindow(Plugin plugin): base("Mod view window##")
        {
            this.plugin = plugin;
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(375, 330),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
        }

        public void ChangeMod(ModThumb modThumb)
        {
            (this.mod,this.descriptionNodes) = WebClient.GetModPage(modThumb);
            failedAvatarUrl = false ;
        }
        public void Dispose()
        {

        }

        private void DrawDescHtmlFromNode(HtmlNode node)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Text:
                    // Reached the text of the node
                    ImGui.TextWrapped(node.InnerText.Trim());
                    break;

                case HtmlNodeType.Element:
                    if (node.Name == "p")
                    {
                        // Paragraphs
                        foreach (var child in node.ChildNodes)
                        {
                            DrawDescHtmlFromNode(child);
                        }
                        ImGui.NewLine(); // Add space after paragraphs
                    }
                    else if (node.Name == "br")
                    {
                        // Line break
                        ImGui.NewLine();
                    }
                    else if (node.Name == "a")
                    {
                        DrawLink(node);
                    }
                    else
                    {
                        // Others html elements for later
                        foreach (var child in node.ChildNodes)
                        {
                            DrawDescHtmlFromNode(child);
                        }
                    }
                    break;

                default:
                    // Keep going if node is not recognized
                    foreach (var child in node.ChildNodes)
                    {
                        DrawDescHtmlFromNode(child);
                    }
                    break;
            }
        }

        private void DrawLink(HtmlNode node)
        {
            string url = node.GetAttributeValue("href", string.Empty);
            string linkText = node.InnerText.Trim();

            // Render link text as a clickable item
            ImGui.TextColored(new Vector4(0.1f, 0.4f, 1.0f, 1.0f), linkText);
            if (ImGui.IsItemClicked())
            {
                //later
            }

            ImGui.SameLine(); // Ensure links are inline
        }

        private void StartInstall()
        {
            _isLoading = true;
            Task.Run(() =>
            {
                _statusMessage = "Downloading...";
                string modpath = plugin.modHandler.DownloadModAsync(WebClient.xivmodarchiveRoot + mod.Value.url_download_button).Result;
                _statusMessage = "Installing...";
                plugin.modHandler.InstallMod(modpath, plugin.imageHandler.DownloadImage(mod.Value.modThumb.url_thumb));

            }).ContinueWith(task => { _isLoading = false; });
        }

        private void DrawLoading()
        {
            using var loadingChild = ImRaii.Child("###modbrowserinstallingLoadingFrame", new Vector2(-1, -1), false);
            if (loadingChild)
            {
                ImGui.GetWindowDrawList().PushClipRectFullScreen();
                ImGui.GetWindowDrawList().AddRectFilled(
                    ImGui.GetWindowPos() + new Vector2(0, (ImGui.GetFontSize() + (ImGui.GetStyle().FramePadding.Y * 2))),
                    ImGui.GetWindowPos() + ImGui.GetWindowSize(),
                    0xCC000000,
                    ImGui.GetStyle().WindowRounding,
                    ImDrawFlags.RoundCornersBottom);
                ImGui.PopClipRect();

                ImGui.SetCursorPosY(ImGui.GetWindowSize().Y / 2);
                StaticHelpers.CenteredText(_statusMessage);
            }
        }

        private void DrawModPage()
        {
            if (_isLoading)
            {
                DrawLoading();
            }

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
                var modThumbnail = Plugin.TextureProvider.GetFromFile(plugin.imageHandler.DownloadImage(mod.Value.modThumb.url_thumb)).GetWrapOrDefault();
                if (modThumbnail != null)
                {
                    ImGui.Image(modThumbnail.ImGuiHandle, new Vector2(300, 200)); // Placeholder for image carousel
                }
                else
                {
                    ImGui.Button("Failed to load thumbnail", new Vector2(300, 200));
                }

                // Tabs (Info, Files, History)
                DrawDescHtmlFromNode(descriptionNodes.First());

                ImGui.EndChild();
            }

            ImGui.NextColumn(); // Move to right column

            // Right Column (Author Info, Download, Stats)
            {
                ImGui.BeginChild("RightColumn", new Vector2(0, 0), true);

                // Author Card
                ImGui.Text(mod.Value.modThumb.author);
                if (!failedAvatarUrl)
                {
                    var authorpicpath = plugin.imageHandler.DownloadImage(mod.Value.url_author_profilepic);
                    if (!authorpicpath.IsNullOrEmpty())
                    {
                        var authorpicThumbnail = Plugin.TextureProvider.GetFromFile(authorpicpath).GetWrapOrDefault();
                        if (authorpicThumbnail != null)
                        {
                            ImGui.Image(authorpicThumbnail.ImGuiHandle, new Vector2(100, 100));
                        }
                        else
                        {
                            ImGui.Button("Failed to turn user avatar into texture", new Vector2(100, 100)); // Placeholder for avatar
                        }
                    }
                    else
                    {
                        failedAvatarUrl = true;
                    }
                }
                else
                {
                    ImGui.Button("Failed to GET authorprofile", new Vector2(100, 100));
                }
                ImGui.Separator();

                // Download button
                if (mod.Value.url_download_button.Contains("private"))//keep it simple now but will need to be updated
                {
                    if (ImGui.Button("Install using Penumbra"))
                    {
                        StartInstall();

                    }
                }
                else
                {
                    ImGui.Button("Not available(redirect to unsupported 3rd party)");
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
