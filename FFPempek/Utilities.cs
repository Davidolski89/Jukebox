using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FFPempek
{
    public static class Utilities
    {
        public static string NameWithoutExtension(this string name)
        {
            FileInfo file = new FileInfo(name);
            return name.Remove(name.Length - file.Extension.Length);
        }
    }
}
