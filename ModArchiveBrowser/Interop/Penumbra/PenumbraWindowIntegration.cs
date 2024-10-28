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
                    Utils.StaticHelpers.ImageFullWidth(thumb, 0, true);
                }
                }
        }
           
    }
}
