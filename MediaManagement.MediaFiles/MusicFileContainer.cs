using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MediaManagement.MediaFiles
{
    public class MusicFileContainer
    {
        public bool OnHDD { get; set; }
        public MusicFile MusicFile { get; set; }
        string searchString;
        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value;
                string search = value.Replace("-", " ").Replace("(", " ").Replace(")", " ");
                char[] seperators = { ' ', '-' };
                string[] words = search.Split(seperators).Where(x => x != "" && x != " ").ToArray();
                SearchWords = words;
            }
        }
        public string[] SearchWords { get; set; }
        public string Interpret { get; set; }
        public string Title { get; set; }
        //public Dictionary<MirrorName, List<MusicFile>> Mirrors { get; set; } = new Dictionary<MirrorName, List<MusicFile>>();
    }
}
