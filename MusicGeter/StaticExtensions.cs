using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaManagement.MediaFiles;

namespace MusicGeter
{
    static class StaticExtensions
    {
        public static void InterpretTitleToSearchString(this MusicFileContainer container, bool oneInterpret = false)
        {
            if(container.SearchString.Length < 2)
            {
                if (oneInterpret)
                {
                    char[] seperators = { ',' };
                    string[] words = container.MusicFile.Interpret.Split(seperators).ToArray();
                    container.SearchString = words[0] + " " + container.MusicFile.Title;
                }
                else
                {
                    if (container.MusicFile != null)
                        container.SearchString = container.MusicFile.Interpret + " " + container.MusicFile.Title;
                }
            }
              
        }       
        public static void ReduceSearchWords(this MusicFileContainer container)
        {
            container.SearchWords = container.SearchWords.Take(container.SearchWords.Count()-1).ToArray();
        }
        public static object ToLoadNextTrack(this MusicFile musicFile , string playCommand)
        {
            if(!(musicFile is null))
            {
                if (musicFile.Downloaded)
                    return new object[] { musicFile.Id.ToString(), musicFile.Interpret, musicFile.Title, playCommand, new string[] { musicFile.Duration.ToString(), musicFile.Bitrate.ToString(), musicFile.Size.ToString() } };
                else
                    return new object[] { };
            }            
            else
                return new object[] { };
        }
        public static object ToGetCurrentQue(this List<MusicFile> musicFiles)
        {
            if (musicFiles.Count > 0)
            {                
                return musicFiles.Where(x => x.Downloaded == true).Select(x => new[] { x.Id.ToString(), x.Interpret, x.Title, x.Time, x.Samplerate.ToString(), x.Size.ToString(), x.Codec.ToString() });
            }
            else
                return new object[] { };            
        }
        public static string Cleanse(this string input)
        {
            return input.Replace("\\", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
        }

        public static void NameToInterpretTitle(this MusicFile container)
        {
            char[] seperatorsI = { '-' };
            string[] wordsI = container.Name.Split(seperatorsI).ToArray();
            if (wordsI.Length > 1)
            {
                container.Interpret = wordsI[0];
                container.Title = wordsI[1];
            }
            else
            {
                char[] seperators = { ' ' };
                string[] words = container.Name.Split(seperators).ToArray();
                container.Interpret = words[0];
                container.Title = String.Join(" ", words.Skip(1));
            }
        }
        public static void RemoveWrongFormating(this MusicFile container)
        {
            container.Name.Replace("&ndash;", "-");
        }
    }
}
