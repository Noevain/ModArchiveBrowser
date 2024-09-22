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
namespace ModArchiveBrowser.Windows
{
    public class ModWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private readonly string modurl;
        private readonly Mod mod;
        public ModWindow(Plugin plugin,ModThumb modThumb): base($"Mod view window##{modThumb.name}")
        {
            Plugin = plugin;
            Plugin.Logger.Debug(modThumb.url);
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(375, 330),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
            this.mod = WebClient.GetModPage(modThumb);
        }
        public void Dispose()
        {
            Plugin.WindowSystem.RemoveWindow(this);
        }

        public override void Draw()
        {
            
            ImGui.Text("Mod name:"+mod.modThumb.name);
            ImGui.Text("Mod author:"+mod.modThumb.author);
            ImGui.Text("Mod type:"+mod.modThumb.type);
            ImGui.Text("Mod gender:"+mod.modThumb.genders);
            ImGui.Text("Mod:"+mod.modMeta.views);
            ImGui.Text("Mod:"+mod.modMeta.downloads);
            ImGui.Text("Mod:"+mod.modMeta.last_update);
            var race_str = string.Empty;
            for(int i = 0;i<mod.modMeta.races.Length;i++)
            {
                race_str = race_str + mod.modMeta.races[i];
            }
            var tag_str = string.Empty;
            for (int i = 0; i < mod.modMeta.tags.Length; i++)
            {
                tag_str = tag_str + mod.modMeta.tags[i];
            }
            ImGui.Text(race_str);
            ImGui.Text(tag_str);

            ImGui.Separator();
            if (ImGui.Button("Install using Penumbra"))
            {
                
            }
            

        }
    }
}
