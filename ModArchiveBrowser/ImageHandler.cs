using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Net;

namespace ModArchiveBrowser
{

    //because we are going to display image from urls in a draw loop
    //need to handle caching,no multiple requests on same ressources,avoid 429 errors
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class ImageHandler
    {
        private readonly string _downloadDirectory;
        private readonly HttpClient _httpClient;
        private readonly HashSet<string> _downloadedFilenames;

        public ImageHandler(string downloadDirectory)
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

        public string DownloadImage(string imageUrl)
        {
            try
            {
                // Extract the file name from the URL (e.g. a41de820-fb64-4eb7-9995-ad9953dbf5e8.jpg)
                string fileName = Path.GetFileName(new Uri(imageUrl).AbsolutePath);

                if (_downloadedFilenames.Contains(fileName))
                {
                    return Path.Combine(_downloadDirectory, fileName);
                }


                byte[] imageBytes = _httpClient.GetByteArrayAsync(imageUrl).Result;


                string filePath = Path.Combine(_downloadDirectory, fileName);
                File.WriteAllBytes(filePath, imageBytes);

               
                _downloadedFilenames.Add(fileName);
                return filePath; 
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"Failed to download image: {imageUrl}. Error: {ex.Message}");
                return string.Empty;
            }
        }
    }


}
