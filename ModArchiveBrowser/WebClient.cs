using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModArchiveBrowser.Utils;
using HtmlAgilityPack;
using System.Net;
using System.IO;

namespace ModArchiveBrowser
{
    
    internal class WebClient
    {
        public const string xivmodarchiveRoot = "https://www.xivmodarchive.com";
        public const string new_and_updated_from_patreon_subs = "search?nsfl=false&sponsored=true&dt_compat=1&sortby=time_edited&sortorder=desc";
        public const string today_most_viewed = "search?nsfl=false&dt_compat=1&sortby=views_today&sortorder=desc";
        public const string newest_mods_from_all_users = "search?nsfl=false&dt_compat=1&sortby=time_published&sortorder=desc";
        private static HtmlWeb clientInstance = null;
        public static HtmlWeb ClientInstance
        {
            get
            {
                if (clientInstance == null)
                {
                    clientInstance = new HtmlWeb();
                    clientInstance.CachePath = Path.Combine(System.IO.Path.GetTempPath(), "modarchivebrowser\\htmlcache");
                    clientInstance.UsingCache = true;
                    clientInstance.UserAgent = "DalamudPluginModBrowser";
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
        public static (Mod,HtmlNodeCollection) GetModPage(ModThumb modThumb)
        {
            string url = xivmodarchiveRoot + modThumb.url;
            Plugin.Logger.Debug($"{url}");
            HtmlDocument page = ClientInstance.Load(url);
            HtmlNodeCollection descriptionNodeStart = page.DocumentNode.SelectNodes("//div[@id='info']");
            Plugin.Logger.Debug("Request made");
            return (ParseModPage(page,modThumb),descriptionNodeStart);
        }

        public static (Mod,HtmlNodeCollection) GetModPage(string modId)
        {
            string url = xivmodarchiveRoot+"/modid/"+modId;
            Plugin.Logger.Debug($"{url}");
            HtmlDocument page = ClientInstance.Load(url);
            HtmlNodeCollection descriptionNodeStart = page.DocumentNode.SelectNodes("//div[@id='info']");
            Plugin.Logger.Debug("Request made");
            ModThumb mdThumb = GetModThumbFromFullPage(page,modId);
            return(ParseModPage(page,mdThumb),descriptionNodeStart);
        }

        public static List<ModThumb> DoSearch(string searchUrl)
        {
            string url = xivmodarchiveRoot + '/' + searchUrl;
            HtmlDocument page = ClientInstance.Load(url);
            Plugin.Logger.Debug("Request made");
            return ParseSearchResults(page);
        }

        public static ModThumb GetModThumbFromFullPage(HtmlDocument page,string url)
        {
            string title;
            string thumbUrl;
            string authorName;
            string type;
            string gender;
            string views;
            HtmlNodeCollection titleNode = page.DocumentNode.SelectNodes("//h1[contains(@class, 'display-5')]");
            HtmlNodeCollection imageNode = page.DocumentNode.SelectNodes("//img[contains(@class, 'mod-carousel-image')]/@src");
            HtmlNodeCollection authorNode = page.DocumentNode.SelectNodes("//a[contains(@class, 'user-card-link')]");
            HtmlNodeCollection typeNodes = page.DocumentNode.SelectNodes("//div[contains(@class, 'col-8')]//p[contains(@class, 'lead')]");
            HtmlNodeCollection genderNodes = page.DocumentNode.SelectNodes("/html/body/div[2]/div[2]/div[2]/div[1]/div[3]/div[6]/code/a");
            HtmlNodeCollection viewsNodes = page.DocumentNode.SelectNodes("/html/body/div[2]/div[2]/div[2]/div[1]/div[3]/div[1]/div/span[1]/div/span[2]");
            title = titleNode[0].InnerText;
            thumbUrl = imageNode[0].GetAttributeValue("src", "none");
            authorName = authorNode[0].InnerText;
            type = typeNodes[0].InnerText;
            gender = genderNodes[0].InnerText;
            views = viewsNodes[0].InnerText;
            return new ModThumb(title, url, authorName,thumbUrl,"none",type,gender,views);


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
            HtmlNode dtCompatible = page.DocumentNode.SelectSingleNode(".//div[contains(@class, 'alert-success')]");
            DTCompatibility dTCompatibility = DTCompatibility.FullyCompatible;
            if(dtCompatible is null)
            {
                HtmlNode dtTexTools = page.DocumentNode.SelectSingleNode(".//div[contains(@class, 'alert-info')]");
                dTCompatibility = DTCompatibility.TexToolsCompatible;
                if(dtTexTools is null)
                {
                    HtmlNode dtPartial = page.DocumentNode.SelectSingleNode(".//div[contains(@class, 'alert-warning')]");
                    dTCompatibility = DTCompatibility.PartiallyCompatible;
                    if(dtPartial is null)
                    {
                        HtmlNode dtFucked = page.DocumentNode.SelectSingleNode(".//div[contains(@class, 'alert-danger')]");
                        dTCompatibility = DTCompatibility.NotCompatible;
                    }
                    else
                    {
                        dTCompatibility = DTCompatibility.PartiallyCompatible;//should never happen but you never know
                    }
                }
            }
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
                races,tags,description,affectReplace,dTCompatibility);
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
                string title = WebUtility.HtmlDecode(titleNodes[i].InnerText);
                string modUrl = urlNodes[i].GetAttributeValue("href", "none");
                string thumbUrl = thumbUrlNodes[i].GetAttributeValue("src", "none");
                string authorName = WebUtility.HtmlDecode(authorNameNodes[i].InnerText);
                string authorUrl =  authorUrlNodes[i].GetAttributeValue("href", "none");
                string type = typeNodes[i].InnerText;
                string gender = gendersNodes[i].InnerText;
                string views = viewsNodes[i].InnerText;
                modthumbnails.Add(new ModThumb(title, modUrl, authorName, thumbUrl, authorUrl,type,gender,views));
            }

            return modthumbnails;
        }

        public static List<ModThumb> ParseSearchResults(HtmlDocument searchpage)
        {
            List<ModThumb> modthumbnails = new List<ModThumb>();
            HtmlNodeCollection titleNodes = searchpage.DocumentNode.SelectNodes("//div[contains(@class, 'mod-card')]//h5[contains(@class, 'card-title')]");
            HtmlNodeCollection authorNameNodes = searchpage.DocumentNode.SelectNodes("//div[contains(@class, 'mod-card')]//p[contains(@class, 'card-text')]//a[contains(@href, '/user/')]");
            HtmlNodeCollection urlNodes = searchpage.DocumentNode.SelectNodes("//a[contains(@href, '/modid/')]/@href");
            HtmlNodeCollection thumbUrlNodes = searchpage.DocumentNode.SelectNodes("//div[contains(@class, 'mod-card-img-container')]//img[contains(@class, 'mod-card-img')]/@src");
            HtmlNodeCollection typeNodes = searchpage.DocumentNode.SelectNodes("//div[contains(@class, 'mod-card')]//code[contains(text(), 'Type')]");
            HtmlNodeCollection gendersNodes = searchpage.DocumentNode.SelectNodes("//div[contains(@class, 'mod-card')]//code[contains(text(), 'Genders')]");
            HtmlNodeCollection viewsNodes = searchpage.DocumentNode.SelectNodes("//div[contains(@class, 'mod-card')]//span[contains(@title, 'Lifetime Views')]");
            /*HtmlNodeCollection downloadsNodes = searchpage.DocumentNode.SelectNodes("//span[contains(@class, 'emoji-block') and contains(@title, 'Downloads')]//span[contains(@class, 'count')]");
            HtmlNodeCollection pinsNodes = searchpage.DocumentNode.SelectNodes("//span[contains(@class, 'emoji-block') and contains(@title, 'Followers')]//span[contains(@class, 'count')]");*/
            int size = titleNodes.Count;
            for (int i = 0; i < size; i++)
            {
                string title = WebUtility.HtmlDecode(titleNodes[i].InnerText);
                string modUrl = urlNodes[i].GetAttributeValue("href", "none");
                string thumbUrl = thumbUrlNodes[i].GetAttributeValue("src", "none");
                string authorName = WebUtility.HtmlDecode(authorNameNodes[i].InnerText);
                string type = typeNodes[i].InnerText;
                string gender = gendersNodes[i].InnerText;
                string views = viewsNodes[i].InnerText;
                modthumbnails.Add(new ModThumb(title, modUrl, authorName, thumbUrl, "", type, gender, views));
            }

            return modthumbnails;

        }

        public static string BuildSearchURL(
            SortBy sortBy,
        SortOrder sortOrder,
        string basicText = null,
        NSFW nsfw = NSFW.False,
        string name = null,
        string author = null,
        Gender? gender = null,
        string race = null,
        string tags = null,
        string affects = null,
        string comments = null,
        DTCompatibility dtCompatibility = DTCompatibility.TexToolsCompatible,
        HashSet<Types> types = null,
        int page = 1)
        {
            var queryParams = new Dictionary<string, string>();

            // Required Parameters
            queryParams["sortby"] = sortBy.ToString().ToLower();
            queryParams["sortorder"] = sortOrder.ToString().ToLower();
            queryParams["nsfw"] = nsfw == NSFW.True ? "true" : "false";
            queryParams["dt_compat"] = ((int)dtCompatibility).ToString();

            // Optional Parameters
            if (!string.IsNullOrEmpty(basicText)) queryParams["basic_text"] = basicText;
            if (!string.IsNullOrEmpty(name)) queryParams["name"] = name;
            if (!string.IsNullOrEmpty(author)) queryParams["author"] = author;
            if (gender.HasValue) queryParams["genders"] = gender.ToString().ToLower();
            if (!string.IsNullOrEmpty(race)) queryParams["races"] = race;
            if (!string.IsNullOrEmpty(tags)) queryParams["tags"] = tags;
            if (!string.IsNullOrEmpty(affects)) queryParams["affects"] = affects;
            if (!string.IsNullOrEmpty(comments)) queryParams["comments"] = comments;
            
            if (types != null && types.Count > 0)
            {
                // comma-separated string for url
                var typesString = string.Join("%2C", types.Select(t => ((int)t).ToString()));
                queryParams["types"] = typesString;
            }
            queryParams["page"] = page.ToString();
            // Construct the URL
            var sb = new StringBuilder("search?");
            foreach (var param in queryParams)
            {
                sb.Append($"{param.Key}={param.Value}&");
            }

            // Remove the last '&'
            sb.Length--;

            return sb.ToString();
        }
        public WebClient() {
        
        }
    }
}
