using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MediaManagement.MediaFiles
{
    public interface IFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int Size { get; set; }
        public FileType Type { get; set; }
    }
}





