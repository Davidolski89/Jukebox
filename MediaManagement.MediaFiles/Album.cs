using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaManagement.MediaFiles{
    
    public class Album {
        public int Id { get; set; }       
        public int CD { get; set; }
        public string Name { get; set; }
        public int Files { get; set; }
        public ICollection<MusicFile> MusicFiles { get; set; }        
        public string Path { get; set; }        
        public bool Cover { get; set; }
        // public string UsersId { get; set; }
        public bool Cached { get; set; }
        public DateTime TimeAdded { get; set; }
        public string Genre { get; set; }
    }
}
