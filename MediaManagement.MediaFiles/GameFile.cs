using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaManagement.MediaFiles
{
    public class GameFile    
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Size { get; set; }
        public DateTime Date { get; set; }
        public string ImgUrl { get; set; }
    }
}
