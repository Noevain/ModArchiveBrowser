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
using ModArchiveBrowser.Utils;

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

    public class ImageHandler : IDisposable

    {
    public readonly string _downloadDirectory;
    private readonly HttpClient _httpClient;
    public HashSet<string> _downloadedFilenames;

    public ImageHandler(string downloadDirectory)
    {
        _downloadDirectory = downloadDirectory;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "DalamudPluginModBrowser");
        _downloadedFilenames = new HashSet<string>(); //maybe fill this with cache dir or load from config later

        // Check if it exist first
        if (!Directory.Exists(_downloadDirectory))
        {
            Directory.CreateDirectory(_downloadDirectory);
        }
    }

    public string GetImage(string filename)
    {
        try
        {
            // Extract the file name from the URL (e.g. a41de820-fb64-4eb7-9995-ad9953dbf5e8.jpg)
            string fileName = Path.GetFileName(new Uri(filename).AbsolutePath);

            if (_downloadedFilenames.Contains(fileName))
            {
                return Path.Combine(_downloadDirectory, fileName);
            }

            return "thumbnail.jpg"; //loading icon?later
        }
        catch (Exception ex)
        {
            return "thumbnail.jpg"; //default missing thumbnail icon,probably best to include with manifest later
        }
    }

    public async Task<string> DownloadImage(string imageUrl)
    {
        try
        {
            // Extract the file name from the URL (e.g. a41de820-fb64-4eb7-9995-ad9953dbf5e8.jpg)
            string fileName = Path.GetFileName(new Uri(imageUrl).AbsolutePath);

            if (_downloadedFilenames.Contains(fileName))
            {
                return Path.Combine(_downloadDirectory, fileName);
            }


            byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);


            string filePath = Path.Combine(_downloadDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, imageBytes);


            _downloadedFilenames.Add(fileName);
            return filePath;
        }
        catch (Exception ex)
        {
            Plugin.ReportError($"Failed to download image: {imageUrl}. Error: {ex.Message}", ex);
            return "thumbnail.jpg";
        }
    }

    public void Dispose()
    {
        StaticHelpers.ClearCacheFully(_downloadDirectory);
    }
    }


}
