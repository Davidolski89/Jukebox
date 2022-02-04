using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace MediaManagement.MediaFiles {    
    public class MusicFile {
        //Infrastructure
        
        public int Id { get; set; }
        public string NameExtension { get; set; }
        public string Name { get; set; }
        public string Time { get; set; }
        public string Path { get; set; }
        public string DownloadPath { get; set; }
        public string DownloadPathOpus { get; set; }
        public string DownloadLink { get; set; }        
        public string UsersId { get; set; }
        public bool Cached { get; set; }
        public bool Downloaded { get; set; }
        public bool Cover { get; set; }
        public AudioGenre Tag  { get; set; }
        public DateTime TimeAdded { get; set; }

        //Stream
        
        public string Codec { get; set; } //codec_name
        public string Type { get; set; } //codec_type
        public int Samplerate { get; set; } // sample_rate
        public int Bitrate { get; set; } //bit_rate
        public int Duration { get; set; } //duration in sekunden

        //Format
        public float Size { get; set; } // size
        public string Album { get; set; } // album
        public string Title { get; set; } // titel
        public string Interpret { get; set; } // artist
        public string Date { get; set; } // date
        public string Genre { get; set; }
        public List<PlaylistMusicFile> PlaylistMusicFiles { get; set; }
        [NotMapped]

        public ICollection<Playlist> Playlists { get; set; }
    }

    public enum AudioGenre {
        Rock,
        Electro,
        Pop,
        Classic,
        RNB,
        DNB,
        Dubstep
    }
}
