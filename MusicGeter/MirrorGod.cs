using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MediaManagement.MediaFiles;

namespace MusicGeter
{
    public class MirrorGod
    {
        string rootPath;
        public List<IMirror> Mirrors = new List<IMirror>();
        public MusicFileContainer MusicFileContainer;
        public string CurrentRequesterId { get; set; }
        //Dictionary<MirrorName, ScrapingBrowser> Browsers = new Dictionary<MirrorName, ScrapingBrowser>();
        public event EventHandler<DownloadedTrackArgs> DownloadedTrack;
        
        public MirrorGod(string path)
        {
            rootPath = path;
            Mirrors.Add(new MirrorMinusovka(rootPath));
            //mirrors.ForEach(x => Browsers.Add(x.MirrorName,new ScrapingBrowser() { Encoding = Encoding.UTF8 }));
        }
        protected virtual void OnDownloadedTrack(DownloadedTrackArgs e)
        {
            EventHandler<DownloadedTrackArgs> handler = DownloadedTrack;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void SearchMusicFile(MusicFileContainer container, MirrorName mirrorName = MirrorName.All, int amount = 6)
        {            
            MusicFileContainer = container;
            foreach (IMirror mirror in Mirrors)
            {
                if (mirror.MirrorName != MirrorName.HDD)
                    mirror.GetMusicFiles(container, amount, true); // was false
            }
        }       
        public void DownloadMusicFile(MirrorName mirrorName, int number = 0)
        {           
            IMirror mirror = Mirrors.Where(x=>x.MirrorName == mirrorName).FirstOrDefault();
            MusicFileContainer.MusicFile = mirror.CurrentSongList[number];            
            if (!Directory.Exists(Path.Combine(rootPath, "mp3", MusicFileContainer.MusicFile.Date)))
                Directory.CreateDirectory(Path.Combine(rootPath, "mp3", MusicFileContainer.MusicFile.Date));
            mirror.DownloadMusicFile(number);            
        }

        bool TryDownload(MusicFileContainer container,bool oneInterpret, bool any, bool rmWord)
        {
            bool downloaded = false;
            foreach (IMirror mirror in Mirrors)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (mirror.MirrorName != MirrorName.HDD)
                        mirror.SpotifyGetMusicFiles(container, 30, oneInterpret, any, rmWord);

                    if (!Directory.Exists(Path.Combine(rootPath, "mp3", container.MusicFile.Date)))
                        Directory.CreateDirectory(Path.Combine(rootPath, "mp3", container.MusicFile.Date));

                    if (mirror.CurrentSongList.Count > 0)
                    {
                        // use bool for deep search
                        if (mirror.SpotifyDownloadMusicFile())
                        {                           
                            if(!oneInterpret && !any && !rmWord)
                                Console.WriteLine($"{container.MusicFile.Interpret+" - "+container.MusicFile.Title+": "}a-{i} succsess");
                            if(oneInterpret && !any && !rmWord)
                                Console.WriteLine($"{container.MusicFile.Interpret + " - " + container.MusicFile.Title + ": "}b-{i} succsess");
                            if (oneInterpret && any && !rmWord)
                                Console.WriteLine($"{container.MusicFile.Interpret + " - " + container.MusicFile.Title + ": "}c-{i} succsess");
                            if (oneInterpret && any && rmWord)
                                Console.WriteLine($"{container.MusicFile.Interpret + " - " + container.MusicFile.Title + ": "}d-{i} succsess");

                            container.MusicFile = mirror.SpotiFile;
                            OnDownloadedTrack(new DownloadedTrackArgs() { TimeReached = DateTime.Now, Interpret = container.MusicFile.Interpret, Title = container.MusicFile.Title, Downloaded = true, CurrentRequester = CurrentRequesterId });
                            downloaded = true;
                            break;
                        }
                        else {
                            if (!oneInterpret && !any && !rmWord)
                                Console.WriteLine($"{container.MusicFile.Interpret + " - " + container.MusicFile.Title + ": "}a-{i} failed");
                            if (oneInterpret && !any && !rmWord)
                                Console.WriteLine($"{container.MusicFile.Interpret + " - " + container.MusicFile.Title + ": "}b-{i} failed");
                            if (oneInterpret && any && !rmWord)
                                Console.WriteLine($"{container.MusicFile.Interpret + " - " + container.MusicFile.Title + ": "}c-{i} failed");
                            if (oneInterpret && any && rmWord)
                                Console.WriteLine($"{container.MusicFile.Interpret + " - " + container.MusicFile.Title + ": "}d-{i} failed");                      
                            if (oneInterpret && any && rmWord && i==1)
                                OnDownloadedTrack(new DownloadedTrackArgs() { TimeReached = DateTime.Now, Interpret = container.MusicFile.Interpret, Title = container.MusicFile.Title, Downloaded = false, CurrentRequester = CurrentRequesterId });
                        }                          
                    }                    
                }
                if (downloaded)
                    break;
            }
            return downloaded;
        }
        public void SpotifySearchDownload(MusicFileContainer container)
        {
            if (!container.OnHDD)
            {
                if (!TryDownload(container, false, false, false))
                    if (!TryDownload(container, true, false, false))
                        if (!TryDownload(container, true, true, false))
                            TryDownload(container, true, true, true);
            }            
        }
    }
    
    public enum State
    {
        ready = 1,
        aborted = 2,
        downloading = 3
    }
    public class DownloadedTrackArgs : EventArgs
    {
        public string Interpret { get; set; }
        public string Title { get; set; }
        public bool Downloaded { get; set; }
        public DateTime TimeReached { get; set; }
        public string CurrentRequester { get; set; }
    }
}