using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using ScrapySharp;
using ScrapySharp.Html;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using MediaManagement.MediaFiles;

namespace StreamChat.Data.StreamingService
{
    public class Spotify
    {       
        public static List<MusicFile> ParseLink(string link)
        {
            ScrapingBrowser wowzer = new ScrapingBrowser() { Encoding = Encoding.UTF8 };
            List<MusicFile> Files = new List<MusicFile>();

            WebPage homePage = wowzer.NavigateToPage(new Uri(link));
            HtmlNode tracklistContainer = homePage.Find("div", By.Class("tracklist-container")).FirstOrDefault();
            //System.IO.File.WriteAllText(@"C:\Users\David\source\repos\finalresult.html", tracklistContainer.InnerHtml.ToString(), Encoding.UTF8);

            HtmlNode trackcontainerRoot = tracklistContainer.FirstChild;
            IEnumerable<HtmlNode> lis = trackcontainerRoot.ChildNodes;
            StringBuilder pint = new StringBuilder();

            foreach (HtmlNode li in lis)
            {
                var trackname = li.ChildNodes[1].ChildNodes[0].ChildNodes[0].InnerHtml.Replace("&amp;", "&").Replace("&#039;", "'");
                var interpret = li.ChildNodes[1].ChildNodes[0].ChildNodes[1].ChildNodes[0].ChildNodes[0].InnerHtml.Replace("&amp;", "&").Replace("&#039;", "'");
                var time = li.ChildNodes[3].ChildNodes[0].ChildNodes[0].InnerHtml;
                string date = DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day.ToString();
                Files.Add(new MusicFile { Interpret = interpret, Title = trackname ,Time = time,Date = date });
            }
            
            return Files;
        }
    }
}
