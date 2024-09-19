using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ModArchiveBrowser
{
    public struct ModThumb//partial representation 
    {
        public string name;
        public string url;//url to the modpage:/modid/XXXXX
        public string author;
        public string url_thumb;//url to the thumbnail of the mod
        public string author_url;//url to the xivmodarchive author profile:/user/XXXXXX

        public ModThumb(string name, string url, string author, string url_thumb, string author_url)
        {
            this.name = name;
            this.url = url;
            this.author = author;
            this.url_thumb = url_thumb;
            this.author_url = author_url;
        }
    }
    internal class WebClient
    {
        public const string xivmodarchiveRoot = "https://www.xivmodarchive.com/";
        private static HtmlWeb clientInstance = null;

        public static HtmlWeb ClientInstance
        {
            get
            {
                if (clientInstance == null)
                {
                    clientInstance = new HtmlWeb();
                    return clientInstance;
                }
                else
                {
                    return clientInstance;
                }
            }
        }

        public static List<ModThumb> GetHomePageMods()
        {
            HtmlDocument homepage = ClientInstance.Load(xivmodarchiveRoot);
            Plugin.Logger.Debug("Request made");
            return ParseHomePage(homepage);
        }

        public static List<ModThumb> ParseHomePage(HtmlDocument homepage)
        {
            var modLists = homepage.DocumentNode.SelectNodes("//ul[contains(@class, 'glide__slides')]");
            List<ModThumb> modthumbnails = new List<ModThumb>();
            foreach (var modList in modLists)
            {
                Plugin.Logger.Debug("Going through modList");
                var mods = modList.SelectNodes(".//li");

                foreach (var mod in mods)
                {
                    Plugin.Logger.Debug("Going through a mod");
                    string name = string.Empty;
                    string url = string.Empty;
                    string url_thumb = string.Empty;
                    string author_url = string.Empty;
                    string author = string.Empty;
                    // mod title
                    var h5node = mod.SelectSingleNode(".//div[1]//h5");
                    if (h5node != null)
                    {
                        name = h5node.InnerText;
                        Console.WriteLine("H5 Text: " + name);
                    }
                    else//caught an item that is not a mod?Didnt find one in the raw html but there are empty entries so w/e
                    {
                        break;
                    }

                    // modurl and thumbnail url
                    var anchorNode = mod.SelectSingleNode(".//div[2]//a[starts-with(@href, '/modid/')]");
                    if (anchorNode != null)
                    {
                        url = anchorNode.GetAttributeValue("href", string.Empty);
                        Console.WriteLine("Filtered Href: " + url);

                        var imgNode = anchorNode.SelectSingleNode(".//img");
                        if (imgNode != null)
                        {
                            url_thumb = imgNode.GetAttributeValue("src", string.Empty);
                            Console.WriteLine("Nested img src: " + url_thumb);
                        }
                    }

                    //Author infos
                    var nestedAnchorInP = mod.SelectSingleNode(".//div[3]//p//a");
                    if (nestedAnchorInP != null)
                    {
                        author_url = nestedAnchorInP.GetAttributeValue("href", string.Empty);
                        author = nestedAnchorInP.InnerText;
                    }

                    // Categories
                    /* var codeNodes = mod.SelectNodes(".//div[4]//p//code");
                     if (codeNodes != null && codeNodes.Count >= 2)
                     {
                         Console.WriteLine("First <code> tag: " + codeNodes[0].InnerText);
                         Console.WriteLine("Second <code> tag: " + codeNodes[1].InnerText);
                     }*/
                    Plugin.Logger.Debug("Going through modList");
                    modthumbnails.Add(new ModThumb(name, url, author, url_thumb, author_url));
                }

            }
            return modthumbnails;
        }
        public WebClient() {
        
        }
    }
}
