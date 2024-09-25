using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using ModArchiveBrowser.Utils;

namespace ModArchiveBrowser.Windows
{
    public class SearchWindow : Window, IDisposable
    {
        private SortBy selectedSortBy = SortBy.Rank;
        private SortOrder selectedSortOrder = SortOrder.Desc;
        private Gender? selectedGender = null;
        private NSFW selectedNSFW = NSFW.False;
        private DTCompatibility selectedDTCompat = DTCompatibility.TexToolsCompatible;
        private HashSet<Types> selectedType = new HashSet<Types>();
        private Plugin plugin;

        private string searchQuery = "";
        private string modName = "";
        private string modRaces = "";
        private string modAuthor = "";
        private string modTags = "";
        private string modAffects = "";
        private string modComments = "";

        private List<ModThumb> modThumbs = new List<ModThumb>();

        public SearchWindow(Plugin plugin)
        : base("XIV Mod Archive Search##modarchivebrowsersearch")
        {
            this.plugin = plugin;
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(600, 500),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
        }
        public void Dispose()
        {

        }

        public void DrawSearchHeader()
        {
            ImGui.Text("Search for Mods");
            ImGui.Separator();

            // Search Form
            ImGui.InputText("Search for mods...", ref searchQuery, 100);
            ImGui.SameLine();
            if (ImGui.Button("Search"))
            {
                string url = WebClient.BuildSearchURL(
                    selectedSortBy,
                    selectedSortOrder,
                    basicText: searchQuery,
                    nsfw: selectedNSFW,
                    name: modName,
                    author: modAuthor,
                    gender: selectedGender,
                    race: modRaces,
                    tags: modTags,
                    affects: modAffects,
                    comments: modComments,
                    dtCompatibility: selectedDTCompat,
                    types: selectedType
                );

                Plugin.Logger.Debug(url);
                this.modThumbs = WebClient.DoSearch(url);
            }

            // Advanced Search Toggle
            if (ImGui.CollapsingHeader("Advanced Search Options"))
            {
                if (ImGui.BeginChild("leftsearch", new Vector2(200, 0), true))
                {
                    ImGui.InputText("Name", ref modName, 100);
                    ImGui.InputText("Races", ref modRaces, 100);
                    ImGui.InputText("Author", ref modAuthor, 100);
                    ImGui.InputText("Affects", ref modAffects, 100);
                    ImGui.InputText("Tags", ref modTags, 100);
                    ImGui.InputText("Comments", ref modComments, 100);
                    ImGui.EndChild();
                }
                ImGui.SameLine();
                if (ImGui.BeginChild("rightsearch", new Vector2(200, 0), true))
                {
                    // Gender Selection using Enum
                    string[] genderOptions = { "Male", "Female", "Other", "None" };
                    int genderIndex = selectedGender.HasValue ? (int)selectedGender.Value : 3; // 'None' is the last option
                    if (ImGui.Combo("Gender", ref genderIndex, genderOptions, genderOptions.Length))
                    {
                        if (genderIndex < 3) // Valid gender selected
                            selectedGender = (Gender)genderIndex;
                        else
                            selectedGender = null; // None selected
                    }

                    // NSFW Toggle
                    bool nsfwSelected = selectedNSFW == NSFW.True;
                    if (ImGui.Checkbox("NSFW", ref nsfwSelected))
                    {
                        selectedNSFW = nsfwSelected ? NSFW.True : NSFW.False;
                    }

                    // DT Compatibility Dropdown
                    string[] dtCompatOptions = { "Compatible", "Not Compatible" };
                    int dtCompatIndex = (int)selectedDTCompat;
                    ImGui.Combo("DT Compatibility", ref dtCompatIndex, dtCompatOptions, dtCompatOptions.Length);
                    selectedDTCompat = (DTCompatibility)dtCompatIndex;

                    // Mod Types using Enum
                    ImGui.Text("Types:");
                    int i = 0;
                    foreach(Types type in Enum.GetValues(typeof(Types)))
                    {
                        if(i%2 == 0)
                        {
                            ImGui.SameLine();
                        }
                        DrawTypeCheckbox(type);
                        i++;
                    }

                    // Sorting Options
                    string[] sortByOptions = { "Rank", "Date", "Name" };
                    int sortByIndex = (int)selectedSortBy;
                    ImGui.Combo("Sort By", ref sortByIndex, sortByOptions, sortByOptions.Length);
                    selectedSortBy = (SortBy)sortByIndex;

                    string[] sortOrderOptions = { "Ascending", "Descending" };
                    int sortOrderIndex = (int)selectedSortOrder;
                    ImGui.Combo("Sort Order", ref sortOrderIndex, sortOrderOptions, sortOrderOptions.Length);
                    selectedSortOrder = (SortOrder)sortOrderIndex;
                    ImGui.EndChild();
                }
            }


        }

        public void DrawTypeCheckbox(Types type)
        {
            bool isSelected = selectedType.Contains(type);
            if (ImGui.Checkbox(type.ToString(),ref isSelected))
            {
                if (isSelected)
                {
                    selectedType.Add(type);
                }
                else
                {
                    selectedType.Remove(type);
                }
            }
        }

        public void DrawSearchResults()
        {
            int modCount = 0;
            foreach (ModThumb thumb in modThumbs)
            {
                ImGui.BeginGroup();
                var modThumbnail = Plugin.TextureProvider.GetFromFile(plugin.imageHandler.DownloadImage(thumb.url_thumb)).GetWrapOrDefault();
                if (modThumbnail != null)
                {
                    if (ImGui.ImageButton(modThumbnail.ImGuiHandle, new Vector2(modThumbnail.Width, modThumbnail.Height)))
                    {
                        try
                        {
                            plugin.modWindow.ChangeMod(thumb);
                            if (!plugin.modWindow.IsOpen)
                            {
                                plugin.modWindow.Toggle();
                            }
                        }
                        catch (Exception e)
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
            DrawSearchHeader();
            if (modThumbs.Count > 0)
            {
                DrawSearchResults();
            }
        }
    }
}
