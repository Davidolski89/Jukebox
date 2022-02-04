using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaManagement.MediaFiles;


namespace MusicGeter
{
    public interface IMirror
    {
        public MirrorName MirrorName { get; }
        public string RootPath { get; set; }

        public List<MusicFile> CurrentSongList { get; set; }
        public string CreateRequestUrl(MusicFileContainer container);
        public void GetMusicFiles(MusicFileContainer container, int amount, bool oneInterpret = false, bool anyTime = false, bool rmWord = false);
        public void SpotifyGetMusicFiles(MusicFileContainer container, int amount, bool oneInterpret, bool anyTime ,bool shortenTitle);
        public bool DownloadMusicFile(int number);
        public bool SpotifyDownloadMusicFile();
        public MusicFile SpotiFile { get; set; }
        public List<string> FilterWords { get; set; }
    }

    public enum MirrorName
    {
        All,
        HDD,
        Mirror1,
        Mirror2      
    }
}
