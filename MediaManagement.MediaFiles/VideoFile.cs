using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace MediaManagement.MediaFiles {
    public class VideoFile {
        //Infrastructure
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameWithoutExtension { get; set; }
        public string Path { get; set; }
        public bool Cached { get; set; }
        public DateTime TimeAdded { get; set; }
        

        //Meta
        public string Codec { get; set; }
        public int Duration { get; set; }
        public int Size { get; set; }
        public int Bitrate { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Genre { get; set; }
        //public Dictionary<string,int> Subtitels { get; set; }
        [NotMapped]
        public Dictionary<string,int> AudioStreams { get; set; }        
    }

    public enum VideoGenre {
        Horror,
        Action,
        SciFi,
        Thriller,
        Animation,
        Comedy,
        Romance
    }
}
