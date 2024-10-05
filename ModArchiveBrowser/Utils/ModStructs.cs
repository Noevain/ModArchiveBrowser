using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModArchiveBrowser.Utils
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

        public ModThumb(string name, string url, string author, string url_thumb, string author_url, string type, string genders, string views)
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

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            var other = (ModThumb)obj;
            return (this.url == other.url);
        }
    }

    public struct Mod//full representation,ik this is awful I am just trying to test if this is even doable
    {
        public ModThumb modThumb;
        public string url_download_button;//url that points to the mod dl link,can be external
        public string url_author_profilepic;//url to the profile pic
        public ModMetadata modMeta;

        public Mod(ModThumb modThumb, string url_download_button, string url_author_profilepic, ModMetadata modMeta)
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
        public DTCompatibility dTCompatibility;

        public ModMetadata(string views, string downloads, string pins, string last_update, string release_date,
            string[] races, string[] tags, string description, string affectReplace,DTCompatibility dT)
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
            this.dTCompatibility = dT;
        }
    }

    public enum Gender
    {
        Male,
        Female,
        Unisex
    }

    public enum Types
    {
        Gear = 1,
        Body = 2,
        Face = 3,
        Hair = 4,
        ReshadePreset = 5,
        Other = 6,
        Minion = 7,
        Mount = 8,
        Furniture = 9,
        Skin = 10,
        //Type11 = 11,//possibly unused?if passed alone no results and if refreshed it will be scrubbed
        RacialScaling = 12,
        Pose = 13,
        VFX = 14,
        Animation = 15,
        Sound = 16,
    }

    public enum NSFW
    {
        True,
        False
    }

    public enum DTCompatibility
    {
        FullyCompatible = 0,
        TexToolsCompatible = 1,
        PartiallyCompatible = 2,
        NotCompatible = 3
    }

    public enum SortBy
    {
        Rank,
        Time_posted,
        Name
    }

    public enum SortOrder
    {
        Asc,
        Desc
    }

}
