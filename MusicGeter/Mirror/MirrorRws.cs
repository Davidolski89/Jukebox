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

namespace MusicGeter
{
    class MirrorRws
    {
        public MirrorName MirrorName { get; } = MirrorName.Mirror2;
        public List<MusicFile> CurrentSongList { get; set; }
        readonly string host = "https://music.xn--41a.ws";
        readonly List<string> songs = new List<string>();

        ScrapingBrowser wowzer = new ScrapingBrowser() { Encoding = Encoding.UTF8 };

        public string CreateRequestUrl(string searchExpression)
        {
            StringBuilder requestString = new StringBuilder();
            requestString.Append(host);
            requestString.Append("/search/");
            char[] seperators = { ' ', '-' };
            string[] words = searchExpression.Split(seperators);

            foreach (string word in words)
            {
                if (word != "" && word != " ")
                {
                    requestString.Append(word);
                    requestString.Append("-");
                }
            }
            requestString.Remove(requestString.Length - 1, 1);
            return requestString.ToString();
        }
        public string CreateRequestUrl(MusicFileContainer container)
        {
            StringBuilder requestString = new StringBuilder();
            requestString.Append(host);
            requestString.Append("/search/");           

            foreach (string word in container.SearchWords)
            {
                if (word != "" && word != " ")
                {
                    requestString.Append(word);
                    requestString.Append("-");
                }
            }
            requestString.Remove(requestString.Length - 1, 1);
            return requestString.ToString();
        }


        public void SearchMusic(string songName)
        {
            WebPage homePage = wowzer.NavigateToPage(new Uri(CreateRequestUrl(songName)));
            HtmlNode tracklistContainer = homePage.Find("ul", By.Class("playlist")).FirstOrDefault();
            System.IO.File.WriteAllText(@"C:\Users\David\source\repos\ul.html", tracklistContainer.OuterHtml, Encoding.UTF8);
            IEnumerable<HtmlNode> lis = tracklistContainer.ChildNodes;

            foreach (HtmlNode li in lis)
            {
                if(li.Name == "li")
                {
                    string downloadlink = host + li.ChildAttributes("data-mp3").FirstOrDefault().Value;
                    string interpret = li.ChildNodes[5].ChildNodes[1].InnerText;
                    string title = li.ChildNodes[5].ChildNodes[3].InnerText;
                    Console.WriteLine(interpret + " - "+title);
                }
                
            }
        }
        public void GetMusicFiles(MusicFileContainer container, int amount ,bool force)
        {

        }
        
    }
}
