using MediaManagement.MediaFiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaManagement.MediaFiles
{
    public class File : IFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int Size { get; set; }
        public FileType Type { get; set; }
    }
}
