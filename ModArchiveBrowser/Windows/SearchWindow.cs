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
using Dalamud.Interface.Textures;
using System.Collections.Concurrent;

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
        private int page = 1;
        private Task buildingTask = null;
        private List<ModThumb> modThumbs = new List<ModThumb>();
        ConcurrentDictionary<string, ISharedImmediateTexture> images = new ConcurrentDictionary<string, ISharedImmediateTexture>();
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

        public void UpdateSearch(List<ModThumb> searchRes)
        {
            this.modThumbs=searchRes;
            RebuildSharedTextures();
        }

        private async void RebuildSharedTextures()
        {
            ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 4 };
            await Parallel.ForEachAsync(modThumbs,parallelOptions, async (modThumb,token) =>
            {
                string path = await plugin.imageHandler.DownloadImage(modThumb.url_thumb);
                ISharedImmediateTexture sharedTexture = Plugin.TextureProvider.GetFromFile(path);
                images.TryAdd(modThumb.url_thumb, sharedTexture);
            });
        }

        public void DrawSearchHeader()
        {
            ImGui.Text("Search for Mods");
            ImGui.SameLine();
            if(ImGui.Button("Go back to homepage"))
            {
                plugin.MainWindow.Toggle();
                plugin.MainWindow.BringToFront();
                plugin.searchWindow.Toggle();
            }
            ImGui.Separator();

            // Search Form
            ImGui.InputText("Search for mods...", ref searchQuery, 100);
            ImGui.SameLine();
            if (ImGui.Button("Search"))
            {
                page = 1;
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
                buildingTask = Task.Run((() => {UpdateSearch(WebClient.DoSearch(url)); }));
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
                    string[] genderOptions = { "Male", "Female", "Unisex", "Any" };
                    int genderIndex = selectedGender.HasValue ? (int)selectedGender.Value : 3; // 'Any' is the last option
                    if (ImGui.Combo("Gender", ref genderIndex, genderOptions, genderOptions.Length))
                    {
                        if (genderIndex < 3) // Valid gender selected
                            selectedGender = (Gender)genderIndex;
                        else
                            selectedGender = null; // None selected
                    }

                    // NSFW Toggle
                    bool nsfwSelected = selectedNSFW == NSFW.True;
                    ImGui.BeginDisabled();
                    if (ImGui.Checkbox("NSFW", ref nsfwSelected))
                    {
                        selectedNSFW = nsfwSelected ? NSFW.True : NSFW.False;
                    }
                    ImGui.EndDisabled();
                    // DT Compatibility Dropdown
                    string[] dtCompatOptions = { "Compatible", "Tex Tools partial","Partial Compatibility","Not compatible" };
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
                    string[] sortByOptions = { "Relevance", "Release Date", "Name", "Last Version Update", "Views","Views Today", "Downloads","Followers" };
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
                if (buildingTask != null && buildingTask.IsCompleted)
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
            float windowWidth = ImGui.GetWindowWidth();
            float buttonWidth = 100;

            float centerOffset = (windowWidth - buttonWidth) * 0.5f;

            // Set the cursor position to the calculated offset to center the button
            ImGui.SetCursorPosX(centerOffset);
            
            if (page > 1)
            {
                if (ImGui.ArrowButton("SearchGoBack", ImGuiDir.Left))
                {
                    page = page - 1;
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
                        types: selectedType,
                        page: page
                    );

                    Plugin.Logger.Debug(url);
                    buildingTask = Task.Run((() => {UpdateSearch(WebClient.DoSearch(url)); }));
                }
                ImGui.SameLine();
            }
            
            if (ImGui.ArrowButton("SearchGoForward", ImGuiDir.Right))
            {
                page = page + 1;
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
                    types: selectedType,
                    page: page
                );

                Plugin.Logger.Debug(url);
                buildingTask = Task.Run((() => {UpdateSearch(WebClient.DoSearch(url)); }));
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
