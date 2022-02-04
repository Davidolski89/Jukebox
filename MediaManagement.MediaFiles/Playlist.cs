using MediaManagement.MediaFiles;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaManagement.MediaFiles
{
    public class Playlist
    {
        public int Id { get; set; }
        public int CD { get; set; }
        public string Name { get; set; }
        public int Files { get; set; }
        public List<PlaylistMusicFile> PlaylistMusicFiles { get; set; }
        [NotMapped]

        public ICollection<MusicFile> MusicFiles { get; set; }
        public string Path { get; set; }
        public bool Cover { get; set; }
        // public string UsersId { get; set; }
        public bool Cached { get; set; }
        public DateTime TimeAdded { get; set; }
        public string Genre { get; set; }
        public string Uploader { get; set; }
    }
}
