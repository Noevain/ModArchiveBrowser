using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface.Textures.TextureWraps;


namespace ModArchiveBrowser.Utils
{
    internal static class StaticHelpers
    {
        public static double CalculateFolderSizeInMB(string path)
        {
            if (!Directory.Exists(path))
            {
                //Plugin.ReportError("Directory does not exist.",null);
                return 0;
            }

            // Get all files in the directory and sum up their sizes
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            long totalSizeBytes = files.Select(file => new FileInfo(file)).Sum(fileInfo => fileInfo.Length);

            // Convert the size from bytes to megabytes (1 MB = 1024 * 1024 bytes)
            double totalSizeMB = totalSizeBytes / (1024.0 * 1024.0);
            return totalSizeMB;
        }

        public static void ClearCacheFully(string path)
        {
            try
            {
                // Get all files in the download directory
                var files = Directory.GetFiles(path);
                int howmuch = 0;
                foreach (var file in files)
                {
                    File.Delete(file);
                    howmuch++;
                }
                Plugin.Logger.Debug($"Deleted {howmuch} files");
            }
            catch (Exception ex)
            {
                Plugin.Logger.Debug("Error deleting files: " + ex.Message);
            }
        }

        public static void CenteredText(string text)
        {
            CenterCursorForText(text);
            ImGui.TextUnformatted(text);
        }
        //from https://github.com/heliosphere-xiv/plugin/blob/dev/Util/ImGuiHelper.cs#L114
        //
        public static void ImageFullWidth(IDalamudTextureWrap wrap, float maxHeight = 0f, bool centred = false)
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

        /// <summary>
        /// Center the ImGui cursor for a certain text.
        /// </summary>
        /// <param name="text">The text to center for.</param>
        public static void CenterCursorForText(string text) => CenterCursorFor(ImGui.CalcTextSize(text).X);

        /// <summary>
        /// Center the ImGui cursor for an item with a certain width.
        /// </summary>
        /// <param name="itemWidth">The width to center for.</param>
        public static void CenterCursorFor(float itemWidth) =>
            ImGui.SetCursorPosX((int)((ImGui.GetWindowWidth() - itemWidth) / 2));
    }
}
