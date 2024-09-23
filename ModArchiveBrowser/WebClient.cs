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
        public string type;
        public string genders;
        public string views;

        public ModThumb(string name, string url, string author, string url_thumb, string author_url,string type,string genders,string views)
        {
            this.name = name;
            this.url = url;
            this.author = author;
            this.url_thumb = url_thumb;
            this.author_url = author_url;
            this.type = type;
            this.genders = genders;
            this.views = views;
        }
    }

    public struct Mod//full representation,ik this is awful I am just trying to test if this is even doable
    {
        public ModThumb modThumb;
        public string url_download_button;//url that points to the mod dl link,can be external
        public string url_author_profilepic;//url to the profile pic
        public ModMetadata modMeta;

        public Mod(ModThumb modThumb, string url_download_button,string url_author_profilepic, ModMetadata modMeta)
        {
            this.modThumb = modThumb;
            this.url_author_profilepic = url_author_profilepic;
            this.url_download_button = url_download_button;
            this.modMeta = modMeta;
        }
    }
    public struct ModMetadata//misc data about the mod,mostly so that the Mod ct isnt a billion params
    {
        public string views;
        public string downloads;
        public string pins;
        public string last_update;
        public string release_date;
        public string[] races;
        public string[] tags;
        public string description;
        public string affectReplace;

        public ModMetadata(string views, string downloads, string pins, string last_update, string release_date, 
            string[] races, string[] tags, string description, string affectReplace)
        {
            this.views = views;
            this.downloads = downloads;
            this.pins = pins;
            this.last_update = last_update;
            this.release_date = release_date;
            this.races = races;
            this.tags = tags;
            this.description = description;
            this.affectReplace = affectReplace;
        }
    }
    internal class WebClient
    {
        public const string xivmodarchiveRoot = "https://www.xivmodarchive.com";
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
        //param url should be in the format of xivmodarchive aka /modid/XXXX and not absolutes
        public static Mod GetModPage(ModThumb modThumb)
        {
            string url = xivmodarchiveRoot + modThumb.url;
            Plugin.Logger.Debug($"{url}");
            HtmlDocument page = ClientInstance.Load(url);
            Plugin.Logger.Debug("Request made");
            return ParseModPage(page,modThumb);
        }

        public static Mod ParseModPage(HtmlDocument page,ModThumb thumb)//I know,I know,this is ugly
        {
            string profile_pic;
            string download_url;
            string affectReplace;
            string[] races;
            string[] tags;
            string views;
            string downloads;
            string pins;
            string lastVersionUpdate;
            string originalReleaseDate;
            HtmlNodeCollection authorProfilePictureNodes = page.DocumentNode.SelectNodes("//div[contains(@class, 'user-card')]//img[contains(@class, 'rounded-circle')]/@src");
            HtmlNodeCollection downloadModButtonNodes = page.DocumentNode.SelectNodes("//a[@id='mod-download-link']/@href");
            HtmlNodeCollection affectsReplacesNodes = page.DocumentNode.SelectNodes("//div[contains(@class, 'mod-meta-block')][contains(text(),'Affects')]//code[contains(text(), '')]");
            HtmlNodeCollection racesNodes = page.DocumentNode.SelectNodes("//div[contains(@class, 'mod-meta-block')]//code[contains(@class, 'text-light')]//a[contains(@href, '/search?races=')]");
            HtmlNodeCollection tagsNodes = page.DocumentNode.SelectNodes("//div[contains(@class, 'mod-meta-block')]//code[contains(@class, 'text-light')]//a[contains(@href, '/search?tags=')]");
            HtmlNodeCollection viewsNodes = page.DocumentNode.SelectNodes("//span[contains(@class, 'emoji-block') and contains(@title, 'Views')]//span[contains(@class, 'count')]");
            HtmlNodeCollection downloadsNodes = page.DocumentNode.SelectNodes("//span[contains(@class, 'emoji-block') and contains(@title, 'Downloads')]//span[contains(@class, 'count')]");
            HtmlNodeCollection pinsNodes = page.DocumentNode.SelectNodes("//span[contains(@class, 'emoji-block') and contains(@title, 'Followers')]//span[contains(@class, 'count')]");
            HtmlNodeCollection lastVersionUpdateNodes = page.DocumentNode.SelectNodes("//div[contains(@class, 'mod-meta-block')]//code[contains(@class, 'server-date')][1]");
            profile_pic = authorProfilePictureNodes[0].GetAttributeValue("src", "none");
            download_url = downloadModButtonNodes[0].GetAttributeValue("href", "none");
            if (affectsReplacesNodes != null)
            {
                affectReplace = affectsReplacesNodes[0].InnerText;
            }
            else
            {
                affectReplace = "N/A";
            }
            races = new string[racesNodes.Count];
            for (int i = 0; i < racesNodes.Count; i++)
            {
                races[i] =  racesNodes[i].InnerText;
            }
            tags = new string[tagsNodes.Count];
            for (int i = 0;i < tagsNodes.Count; i++)
            {
                tags[i] = tagsNodes[i].InnerText;
            }
            views = viewsNodes[0].InnerText;
            downloads = downloadsNodes[0].InnerText;
            pins = pinsNodes[0].InnerText;
            lastVersionUpdate = lastVersionUpdateNodes[0].InnerText;
            originalReleaseDate = lastVersionUpdateNodes[1].InnerText;
            //lastVersionUpdate = "N/A";
            //originalReleaseDate = "N/A";
            string description = "I will implement description parsing/rendering,later";
            ModMetadata modMetadata = new ModMetadata(views,downloads,pins,lastVersionUpdate,originalReleaseDate, 
                races,tags,description,affectReplace);
            return (new Mod(thumb, download_url,profile_pic, modMetadata));


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
            HtmlNodeCollection viewsNodes = homepage.DocumentNode.SelectNodes("//div[contains(@class, 'card-body')]//p[contains(@class, 'card-text')]//em[contains(text(), 'Views')]");

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
                string views = viewsNodes[i].InnerText;
                modthumbnails.Add(new ModThumb(title, modUrl, authorName, thumbUrl, authorUrl,type,gender,views));
            }

            return modthumbnails;
        }
        public WebClient() {
        
        }
    }
}
