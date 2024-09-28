using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using ImGuiNET;
using Dalamud.Interface.Textures.TextureWraps;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.GroupPoseModule;

namespace ModArchiveBrowser.Interop.Penumbra
{
    internal class PenumbraWindowIntegration
    {
        private Plugin plugin;
        public PenumbraWindowIntegration(Plugin plugin) 
        {
            this.plugin = plugin;
        }

        public void Dispose()
        {
            
        }

        //from https://github.com/heliosphere-xiv/plugin/blob/dev/Util/ImGuiHelper.cs#L114
        //
        internal static void ImageFullWidth(IDalamudTextureWrap wrap, float maxHeight = 0f, bool centred = false)
        {
            // get the available area
            var widthAvail = centred && ImGui.GetScrollMaxY() == 0
                ? ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ScrollbarSize
                : ImGui.GetContentRegionAvail().X;
            widthAvail = Math.Max(0, widthAvail);

            // set max height to image height if unspecified
            if (maxHeight == 0f)
            {
                maxHeight = wrap.Height;
            }

            // clamp height at the actual image height
            maxHeight = Math.Min(wrap.Height, maxHeight);

            // for the width, either use the whole space available
            // or the actual image's width, whichever is smaller
            var width = widthAvail == 0
                ? wrap.Width
                : Math.Min(widthAvail, wrap.Width);
            // determine the ratio between the actual width and the
            // image's width and multiply the image's height by that
            // to determine the height
            var height = wrap.Height * (width / wrap.Width);

            // check if the height is greater than the max height,
            // in which case we'll have to scale the width down
            if (height > maxHeight)
            {
                width *= maxHeight / height;
                height = maxHeight;
            }

            if (centred && width < widthAvail)
            {
                var cursor = ImGui.GetCursorPos();
                ImGui.SetCursorPos(cursor with
                {
                    X = widthAvail / 2 - width / 2,
                });
            }

            ImGui.Image(wrap.ImGuiHandle, new Vector2(width, height));
        }

        public void PreSettingsTabBarDraw(string moddir,float width,float titleWidth)
        {
            if (!plugin.Configuration.penumbraDispThumb)
            {
                return;
            }
            if (plugin.modHandler._thumbnailToTextures.ContainsKey(moddir))
            {
                    var thumb = plugin.modHandler._thumbnailToTextures[moddir].GetWrapOrDefault();
                if (thumb != null)
                    {
                    //ImGui.Image(thumb.ImGuiHandle, new Vector2(thumb.Width, thumb.Height));
                    ImageFullWidth(thumb, 0, true);
                }
                }
        }
           
    }
}
