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
        private HashSet<Types> selectedType = null;

        private string searchQuery = "";
        private string modName = "";
        private string modRaces = "";
        private string modAuthor = "";
        private string modTags = "";
        private string modAffects = "";
        private string modComments = "";

        public SearchWindow(Plugin plugin)
        : base("XIV Mod Archive Search##modarchivebrowsersearch")
        {
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

                Plugin.Logger.Debug(url); // For demonstration purposes
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
                    foreach(Types type in Enum.GetValues(typeof(Types)))
                    {
                        DrawTypeCheckbox(type);
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

        }
        public override void Draw()
        {
            DrawSearchHeader();
            DrawSearchResults();
        }
    }
}
