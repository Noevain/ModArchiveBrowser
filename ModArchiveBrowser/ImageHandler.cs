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

namespace ModArchiveBrowser
{

    //because we are going to display image from urls in a draw loop
    //need to handle caching,no multiple requests on same ressources,avoid 429 errors
    internal class ImageHandler : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ConcurrentDictionary<string, Task<Bitmap>> _imageCache;

        public ImageHandler()
        {
            _httpClient = new HttpClient();
            _imageCache = new ConcurrentDictionary<string, Task<Bitmap>>();
        }

        public Task<Bitmap> GetImageAsync(string url)
        {
            // Check if the image is already cached
            if (_imageCache.TryGetValue(url, out var cachedImage))
            {
                return cachedImage; // Return cached image Task
            }

            // Download the image and cache it asynchronously
            var downloadTask = DownloadImageAsync(url);
            _imageCache.TryAdd(url, downloadTask); // Cache the download task

            return downloadTask;
        }

        private async Task<Bitmap> DownloadImageAsync(string url)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                await using var responseStream = await response.Content.ReadAsStreamAsync();
                var bitmap = new Bitmap(responseStream);

                // You might want to resize or process the image here if necessary.
                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading image from {url}: {ex.Message}");
                return null; // Handle failure gracefully (null bitmap)
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            foreach (var kvp in _imageCache)
            {
                kvp.Value.Dispose(); // Dispose of cached bitmaps
            }
            _imageCache.Clear();
        }
    }
}
