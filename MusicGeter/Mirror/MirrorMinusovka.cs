using HtmlAgilityPack;
using MediaManagement.MediaFiles;
using ScrapySharp.Extensions;
using ScrapySharp.Html;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MusicGeter
{
    public class MirrorMinusovka : IMirror
    {
        public MirrorName MirrorName { get; } = MirrorName.Mirror1;
        readonly string mirrorDomain = "https://x-minusovka.ru";
        readonly ScrapingBrowser wowzer = new ScrapingBrowser() { Encoding = Encoding.UTF8 };
        public string RootPath { get; set; }
        public List<MusicFile> CurrentSongList { get; set; } = new List<MusicFile>();
        public List<string> FilterWords { get; set; }

        //MusicFileContainer _spotiContainer = new MusicFileContainer();
        public MusicFile SpotiFile { get; set; }

        public MirrorMinusovka(string path)
        {
            RootPath = path;
        }


        #region Get Request Url
        public string CreateRequestUrl(string searchString)
        {
            StringBuilder requestString = new StringBuilder();
            char[] seperators = { ' ', '-' };
            string[] words = searchString.Split(seperators);
            requestString.Append(mirrorDomain);
            requestString.Append("/?song=");
            foreach (string item in words)
            {
                if (item != "")
                {
                    requestString.Append(item);
                    requestString.Append("+");
                }
            }
            requestString.Remove(requestString.Length - 1, 1);
            requestString.Append("&artist=0");
            return requestString.ToString();
        }
        public string CreateRequestUrl(MusicFileContainer container)
        {
            StringBuilder requestString = new StringBuilder();
            requestString.Append(mirrorDomain);
            requestString.Append("/?song=");
            foreach (string item in container.SearchWords)
            {
                if (item != "")
                {
                    requestString.Append(item);
                    requestString.Append("+");
                }
            }
            requestString.Remove(requestString.Length - 1, 1);
            requestString.Append("&artist=0");
            return requestString.ToString();
        }
        #endregion

        public void GetMusicFiles(MusicFileContainer container, int amount, bool oneInterpret = false, bool anyTime = false, bool rmWord = false)
        {
            if (!container.OnHDD)
            {
                //container.InterpretTitleToSearchString(oneInterpret);
                if (rmWord)
                    container.ReduceSearchWords();
                string requestUrl = CreateRequestUrl(container);
                List<MusicFile> Lieder = new List<MusicFile>();
                wowzer.Encoding = Encoding.Default;
                WebPage homePage;
                try
                {
                    homePage = wowzer.NavigateToPage(new Uri(requestUrl));
                    IEnumerable<HtmlNode> chkds = homePage.Find("div", By.Class("chkd"));

                    foreach (var item in chkds)
                    {
                        MusicFile file = new MusicFile
                        {
                            Interpret = container.MusicFile != null ? container.MusicFile.Interpret : "",
                            Title = container.MusicFile != null ? container.MusicFile.Title : "",
                            Name = item.ChildNodes[2].ChildNodes[0].InnerText,
                            DownloadLink = mirrorDomain + item.Elements("a").ElementAt(1).GetAttributeValue("href"),
                            DownloadPathOpus = container.MusicFile != null ? container.MusicFile.DownloadPathOpus : "",
                            Time = item.SelectSingleNode("span").InnerText,
                            Date = DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day.ToString()
                        };
                        file.RemoveWrongFormating();
                        file.NameToInterpretTitle();

                        //new 
                        if (container.MusicFile is null)
                        {
                            Lieder.Add(file);
                        }
                        else if (!anyTime && spotiTimeComparer(container.MusicFile, file))
                            Lieder.Add(file);
                        else if (anyTime)
                            Lieder.Add(file);
                    }
                    if (Lieder.Count > 0)
                    {
                        if (!(CurrentSongList is null))
                            if (CurrentSongList.Count > 1)
                                CurrentSongList.RemoveRange(0, CurrentSongList.Count);
                        CurrentSongList = Lieder.ToList();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.Write("Catch in R_minusovka");
                }
            }
        }
        public bool DownloadMusicFile(int number)
        {
            bool returnBool = false;
            foreach (MusicFile musicfile in CurrentSongList)
            {
                try
                {
                    Uri uri = new Uri(musicfile.DownloadLink);
                    var musicFile = wowzer.DownloadWebResource(uri).Content;
                    byte[] byteFile = musicFile.ToArray();
                    string mp3Path = Path.Combine(RootPath, "mp3", musicfile.Date, musicfile.Interpret.Cleanse() + " - " + musicfile.Title.Cleanse() + ".mp3");
                    System.IO.File.WriteAllBytes(mp3Path, byteFile);
                    musicfile.Path = mp3Path;
                    musicfile.Downloaded = true;
                    musicfile.DownloadLink = "";
                    returnBool = true;
                    SpotiFile = musicfile;
                    if (CurrentSongList.Count > 1)
                        CurrentSongList.RemoveRange(0, CurrentSongList.Count);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.Write(" Catch in D_minusovka");
                    returnBool = false;
                }
            }
            return returnBool;
        }
        public void SpotifyGetMusicFiles(MusicFileContainer container, int amount, bool oneInterpret = false, bool anyTime = false, bool rmWord = false)
        {
            if (!container.OnHDD)
            {
                container.InterpretTitleToSearchString(oneInterpret);
                if (rmWord)
                    container.ReduceSearchWords();
                string requestUrl = CreateRequestUrl(container);
                List<MusicFile> Lieder = new List<MusicFile>();
                wowzer.Encoding = Encoding.UTF8;
                WebPage homePage;
                try
                {
                    homePage = wowzer.NavigateToPage(new Uri(requestUrl));
                    IEnumerable<HtmlNode> chkds = homePage.Find("div", By.Class("chkd"));

                    foreach (var item in chkds)
                    {
                        MusicFile file = new MusicFile
                        {
                            Interpret = container.MusicFile.Interpret,
                            Title = container.MusicFile.Title,
                            DownloadLink = mirrorDomain + item.Elements("a").ElementAt(1).GetAttributeValue("href"),
                            DownloadPathOpus = container.MusicFile.DownloadPathOpus,
                            Time = item.SelectSingleNode("span").InnerText,
                            Date = DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day.ToString()
                        };
                        if (!anyTime && spotiTimeComparer(container.MusicFile, file))
                            Lieder.Add(file);
                        else if (anyTime)
                            Lieder.Add(file);
                    }
                    if (Lieder.Count > 0)
                    {
                        if (!(CurrentSongList is null))
                            if (CurrentSongList.Count > 1)
                                CurrentSongList.RemoveRange(0, CurrentSongList.Count);
                        CurrentSongList = Lieder.ToList();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.Write("Catch in R_minusovka");
                }
            }
        }
        public bool SpotifyDownloadMusicFile()
        {
            bool returnBool = false;
            foreach (MusicFile musicfile in CurrentSongList)
            {
                try
                {
                    Uri uri = new Uri(musicfile.DownloadLink);
                    var musicFile = wowzer.DownloadWebResource(uri).Content;
                    byte[] byteFile = musicFile.ToArray();
                    string mp3Path = Path.Combine(RootPath, "mp3", musicfile.Date, musicfile.Interpret.Cleanse() + " - " + musicfile.Title.Cleanse() + ".mp3");
                    System.IO.File.WriteAllBytes(mp3Path, byteFile);
                    musicfile.Path = mp3Path;
                    musicfile.Downloaded = true;
                    musicfile.DownloadLink = "";
                    returnBool = true;
                    SpotiFile = musicfile;
                    if (CurrentSongList.Count > 1)
                        CurrentSongList.RemoveRange(0, CurrentSongList.Count);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.Write(" Catch in D_minusovka");
                    returnBool = false;
                }
            }
            return returnBool;
        }
        bool spotiTimeComparer(MusicFile desired, MusicFile actual)
        {
            if (desired != null)
            {
                TimeSpan soll = new TimeSpan(0, DateTime.Parse(desired.Time).Hour, DateTime.Parse(desired.Time).Minute);
                TimeSpan ist;
                if (TimeSpan.TryParse(actual.Time, out ist))
                {
                    ist = new TimeSpan(0, DateTime.Parse(actual.Time).Hour, DateTime.Parse(actual.Time).Minute);
                    double wert2 = (ist - soll).TotalSeconds;
                    if (wert2 < 3 && wert2 > -3)
                        return true;
                    else return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

        //old
        //public void GetMusicFiles(MusicFileContainer container, int amount = 6, bool force = false)
        //{
        //    if (!(CurrentSongList is null))
        //        if (CurrentSongList.Count > 0)
        //            CurrentSongList.RemoveRange(0, CurrentSongList.Count);
        //    if (!container.OnHDD || force)
        //    {
        //        string requestUrl = CreateRequestUrl(container);
        //        List<MusicFile> Lieder = new List<MusicFile>();
        //        wowzer.Encoding = Encoding.UTF8;
        //        WebPage homePage = wowzer.NavigateToPage(new Uri(requestUrl));
        //        IEnumerable<HtmlNode> chkds = homePage.Find("div", By.Class("chkd"));

        //        foreach (var item in chkds)
        //        {
        //            MusicFile file = new MusicFile
        //            {
        //                Interpret = item.SelectSingleNode("h2").FirstChild.FirstChild.FirstChild.InnerText,
        //                Title = item.SelectSingleNode("h2").FirstChild.ChildNodes[2].InnerText,
        //                DownloadLink = mirrorDomain + item.Elements("a").ElementAt(1).GetAttributeValue("href"),
        //                Time = item.SelectSingleNode("span").InnerText,
        //                Date = DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day.ToString()
        //            };
        //            Lieder.Add(file);
        //        }
        //        CurrentSongList = Lieder.Take(amount).ToList();
        //    }
        //}
        //public void DownloadMusicFile(int number)
        //{
        //    MusicFile file = CurrentSongList[number];
        //    Uri uri = new Uri(file.DownloadLink);
        //    var musicFile = wowzer.DownloadWebResource(uri).Content;
        //    byte[] byteFile = musicFile.ToArray();
        //    string mp3Path = Path.Combine(RootPath, "mp3", file.Date, file.Interpret.Cleanse() + " - " + file.Title.Cleanse() + ".mp3");
        //    System.IO.File.WriteAllBytes(mp3Path, byteFile);
        //    file.Path = mp3Path;
        //    file.Downloaded = true;
        //}

    }
}
