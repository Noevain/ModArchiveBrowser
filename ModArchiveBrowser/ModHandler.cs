using ModArchiveBrowser.Interop.Penumbra;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;

using SharpCompress.Common;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Textures;
namespace ModArchiveBrowser
{
    public class ModHandler : IDisposable
    {
        private readonly string _downloadDirectory;
        private readonly string _thumbnailDirectory;
        private readonly HttpClient _httpClient;
        private readonly HashSet<string> _downloadedFilenames;
        public  Dictionary<string, string> _modNameToThumbnail;
        public Dictionary<string,ISharedImmediateTexture> _thumbnailToTextures = new Dictionary<string, ISharedImmediateTexture>();
        private Plugin plugin;

        public ModHandler(string downloadDirectory,string thumbnailsDirectory, Plugin plugin)
        {
            _downloadDirectory = downloadDirectory;
            _thumbnailDirectory = thumbnailsDirectory;
            _httpClient = new HttpClient();
            _downloadedFilenames = plugin.Configuration.CacheFiles;
            _modNameToThumbnail = plugin.Configuration.modNameToThumbnail;
            this.plugin = plugin;
            // Check if it exist first
            if (!Directory.Exists(_downloadDirectory))
            {
                Directory.CreateDirectory(_downloadDirectory);
            }
            if (!Directory.Exists(_thumbnailDirectory))
            {
                Directory.CreateDirectory(_thumbnailDirectory);
            }
            UpdateTextures();
        }

        private void UpdateTextures()//Cant call TextureProvider in PenumbraAPI so need the textures to be ready in advance
        {
            foreach(string mod in _modNameToThumbnail.Keys)
            {
                if (!_thumbnailToTextures.ContainsKey(mod))
                {
                    var tex = Plugin.TextureProvider.GetFromFile(_modNameToThumbnail[mod]);
                    Plugin.Logger.Debug($"Tex updated for:{mod}");
                    _thumbnailToTextures.Add(mod, tex);
                }
            }
        }

        public double CalculateFolderSizeInMB()
        {
            if (!Directory.Exists(_downloadDirectory))
            {
                Plugin.Logger.Error("Directory does not exist.");
                return 0;
            }

            // Get all files in the directory and sum up their sizes
            var files = Directory.GetFiles(_downloadDirectory, "*", SearchOption.AllDirectories);
            long totalSizeBytes = files.Select(file => new FileInfo(file)).Sum(fileInfo => fileInfo.Length);

            // Convert the size from bytes to megabytes (1 MB = 1024 * 1024 bytes)
            double totalSizeMB = totalSizeBytes / (1024.0 * 1024.0);
            return totalSizeMB;
        }

        public void Dispose()
        {
            plugin.Configuration.modNameToThumbnail = this._modNameToThumbnail;
            plugin.Configuration.CacheFiles = this._downloadedFilenames;
            plugin.Configuration.Save();
            
        }

        public string DownloadMod(string modUrl)
        {
            try
            {
                string fileName = Path.GetFileName(new Uri(modUrl).AbsolutePath);

                if (_downloadedFilenames.Contains(fileName))
                {
                    return Path.Combine(_downloadDirectory, fileName);
                }
                byte[] modBytes = _httpClient.GetByteArrayAsync(modUrl).Result;


                string filePath = Path.Combine(_downloadDirectory, Uri.UnescapeDataString(fileName));
                File.WriteAllBytes(filePath, modBytes);


                _downloadedFilenames.Add(fileName);
                return filePath;
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"Failed to download mod: {modUrl}. Error: {ex.Message}");
                return null;
            }
        }

        public void InstallMod(string filePath,string imagepath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Plugin.Logger.Error("Invalid file path or file does not exist.");
                return;
            }

            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            //.ttmp2 or .pmp - Direct install
            if (extension == ".ttmp2" || extension == ".pmp")
            {
                Plugin.Logger.Debug($"Installing mod directly: {filePath}");
                plugin.penumbra.InstallMod(filePath);
                _modNameToThumbnail.Add(Path.GetFileNameWithoutExtension(filePath), imagepath);//the penumbra mod directory will have the same name as the file
                UpdateTextures();
                plugin.penumbra.OpenModWindow();
            }
            //Extract .ttmp2 and .pmp files, queue everything
            else if (extension == ".zip" || extension == ".rar" || extension == ".7z")
            {
                Plugin.Logger.Debug($"Extracting mod from archive: {filePath}");
                List<string> modFiles = ExtractModFiles(filePath);

                // Install each extracted mod file
                foreach (var modFile in modFiles)
                {
                    Plugin.Logger.Debug($"Installing extracted mod: {modFile}");
                    plugin.penumbra.InstallMod(modFile);
                    _modNameToThumbnail.Add(Path.GetFileNameWithoutExtension(filePath), imagepath);
                    UpdateTextures();
                    plugin.penumbra.OpenModWindow();
                }
            }
            else
            {
                Plugin.Logger.Error($"Unsupported file format: {extension}");
            }
        }



        private List<string> ExtractModFiles(string archivePath)
        {
            string extension = Path.GetExtension(archivePath).ToLowerInvariant();
            List<string> modFiles = new List<string>();

            if (extension == ".zip")
            {
                using (ZipArchive archive = ZipFile.OpenRead(archivePath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".ttmp2", StringComparison.OrdinalIgnoreCase) ||
                            entry.FullName.EndsWith(".pmp", StringComparison.OrdinalIgnoreCase))
                        {
                            string destinationPath = Path.Combine(_downloadDirectory, entry.FullName);
                            entry.ExtractToFile(destinationPath, true);
                            modFiles.Add(destinationPath);
                        }
                    }
                }
            }
            else if (extension == ".rar")
            {
                using (var archive = RarArchive.Open(archivePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory && (entry.Key.EndsWith(".ttmp2", StringComparison.OrdinalIgnoreCase) ||
                                                   entry.Key.EndsWith(".pmp", StringComparison.OrdinalIgnoreCase)))
                        {
                            string destinationPath = Path.Combine(_downloadDirectory, entry.Key);
                            entry.WriteToFile(destinationPath);
                            modFiles.Add(destinationPath);
                        }
                    }
                }
            }
            else if (extension == ".7z")
            {
                using (var archive = SevenZipArchive.Open(archivePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory && (entry.Key.EndsWith(".ttmp2", StringComparison.OrdinalIgnoreCase) ||
                                                   entry.Key.EndsWith(".pmp", StringComparison.OrdinalIgnoreCase)))
                        {
                            string destinationPath = Path.Combine(_downloadDirectory, entry.Key);
                            entry.WriteToFile(destinationPath);
                            modFiles.Add(destinationPath);
                        }
                    }
                }
            }

            return modFiles;
        }

    }
}
