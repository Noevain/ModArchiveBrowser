using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModArchiveBrowser.Utils
{
    internal static class StaticHelpers
    {
        public static double CalculateFolderSizeInMB(string path)
        {
            if (!Directory.Exists(path))
            {
                Plugin.Logger.Error("Directory does not exist.");
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
                foreach (var file in files)
                {
                    File.Delete(file);
                    Plugin.Logger.Debug("Deleted file: " + Path.GetFileName(file));
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.Debug("Error deleting files: " + ex.Message);
            }
        }
    }
}
