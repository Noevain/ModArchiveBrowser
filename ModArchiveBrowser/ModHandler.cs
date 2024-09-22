using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ModArchiveBrowser
{
    public class ModHandler
    {
        private readonly string _downloadDirectory;
        private readonly HttpClient _httpClient;
        private readonly HashSet<string> _downloadedFilenames;

        public ModHandler(string downloadDirectory)
        {
            _downloadDirectory = downloadDirectory;
            _httpClient = new HttpClient();
            _downloadedFilenames = new HashSet<string>();//maybe fill this with cache dir or load from config later

            // Check if it exist first
            if (!Directory.Exists(_downloadDirectory))
            {
                Directory.CreateDirectory(_downloadDirectory);
            }
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


                string filePath = Path.Combine(_downloadDirectory, fileName);
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
        
    }
}
