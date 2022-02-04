using System;
using System.Collections.Generic;
using System.Text;

namespace MediaManagement.MediaFiles
{
    public class PlaylistMusicFile
    {
        public int MusicFileId { get; set; }
        public int PlaylistId { get; set; }
        public MusicFile MusicFile { get; set; }
        public Playlist Playlist { get; set; }
    }
}
