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
            List<ModThumb> modthumbnails = new List<ModThumb>();
            HtmlNodeCollection titleNodes = homepage.DocumentNode.SelectNodes("//div[contains(@class, 'card-body')]//h5[contains(@class, 'card-title')]");
            HtmlNodeCollection urlNodes = homepage.DocumentNode.SelectNodes("//a[contains(@href, '/modid/')]//@href");
            HtmlNodeCollection thumbUrlNodes = homepage.DocumentNode.SelectNodes("//div[contains(@class, 'mod-card-img-container')]//img[contains(@class, 'card-img-top mod-card-img')]/@src");
            HtmlNodeCollection authorNameNodes = homepage.DocumentNode.SelectNodes("//div[contains(@class, 'card-body')]//p[contains(@class, 'card-text')]//a");
            HtmlNodeCollection authorUrlNodes = homepage.DocumentNode.SelectNodes("//div[contains(@class, 'card-body')]//p[contains(@class, 'card-text')]//a/@href");
            HtmlNodeCollection typeNodes = homepage.DocumentNode.SelectNodes("//div[contains(@class, 'card-body')]//p[contains(@class, 'card-text')]//code[contains(text(), 'Type')]");
            HtmlNodeCollection gendersNodes = homepage.DocumentNode.SelectNodes("//div[contains(@class, 'card-body')]//p[contains(@class, 'card-text')]//code[contains(text(), 'Genders')]");

            int size = titleNodes.Count;

            for (int i = 0; i < size; i++)
            {
                string title = titleNodes[i].InnerText;
                string modUrl = urlNodes[i].GetAttributeValue("href", "none");
                string thumbUrl = thumbUrlNodes[i].GetAttributeValue("src", "none");
                string authorName = authorNameNodes[i].InnerText;
                string authorUrl =  authorUrlNodes[i].GetAttributeValue("href", "none");
                string type = typeNodes[i].InnerText;
                string gender = gendersNodes[i].InnerText;
                modthumbnails.Add(new ModThumb(title, modUrl, authorName, thumbUrl, authorUrl));
            }

            return modthumbnails;
        }
        public WebClient() {
        
        }
    }
}
