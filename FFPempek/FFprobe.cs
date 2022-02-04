using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using MediaManagement.MediaFiles;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace FFPempek
{
    public class FFprobe
    {
        public string Path { get; set; }
        public FFprobe(string ffprobepath)
        {
            Path = ffprobepath;
        }
        public MusicFile GetTrackMeta(MusicFile musicfile)
        {
            Process process = new Process();
            process.StartInfo.FileName = Path;
            process.StartInfo.Arguments = $"-v quiet -print_format json -i \"{musicfile.Path}\" -show_streams -show_format";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            StreamReader stream = process.StandardOutput;
            string jsonstring = stream.ReadToEnd();

            JsonTextReader reader = new JsonTextReader(new StringReader(jsonstring));

            JObject jObject = JObject.Parse(jsonstring);

            // Streams und Format werden aufgelistet
            foreach (KeyValuePair<string, JToken> topkey in jObject)
            {
                if (topkey.Key == "streams")
                {
                    foreach (JObject index in topkey.Value)
                    { // index == stream
                        if ((string)index["codec_type"] == "audio")
                        {
                            foreach (KeyValuePair<string, JToken> streaminfo in index)
                            {
                                //Console.WriteLine(streaminfo.Key + " ist gleich " + streaminfo.Value);
                                switch (streaminfo.Key)
                                {
                                    //case "index":
                                    //    musicfile.Index = (int)streaminfo.Value;
                                    //    break;
                                    case "codec_name":
                                        musicfile.Codec = (string)streaminfo.Value;
                                        break;
                                    case "codec_type":
                                        musicfile.Type = (string)streaminfo.Value;
                                        break;
                                    case "sample_rate":
                                        musicfile.Samplerate = (int)streaminfo.Value / 1000;
                                        break;
                                    case "bit_rate":
                                        musicfile.Bitrate = (int)streaminfo.Value / 1000;
                                        break;
                                    case "duration":
                                        musicfile.Duration = (int)(float)streaminfo.Value;
                                        break;
                                    default:
                                        break;
                                }
                                // Opus Tags
                                if (streaminfo.Key.ToLower() == "tags")
                                {
                                    foreach (KeyValuePair<string,JToken> tag in (JObject)streaminfo.Value)
                                    {
                                        switch (tag.Key.ToLower())
                                        {
                                            case "album":
                                                musicfile.Album = (string)tag.Value;
                                                break;
                                            case "title":
                                                musicfile.Title = (string)tag.Value;
                                                break;
                                            case "artist":
                                                musicfile.Interpret = (string)tag.Value;
                                                break;
                                            case "genre":
                                                musicfile.Genre = (string)tag.Value;
                                                break;
                                            case "date":
                                                musicfile.Date = (string)tag.Value;
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                                //{
                                //    foreach (KeyValuePair<string,JToken> item in streaminfo.Value)
                                //    {
                                //        switch (item.Key)
                                //        {
                                //            case "album":
                                //                musicfile.Album = (string)item.Value;
                                //                break;
                                //            case "title":
                                //                musicfile.Title = (string)item.Value;
                                //                break;
                                //            case "artist":
                                //                musicfile.Artist = (string)item.Value;
                                //                break;
                                //            case "date":
                                //                musicfile.Date = (string)item.Value;
                                //                break;
                                //            case "genre":
                                //                musicfile.Genre = (string)item.Value;
                                //                break;
                                //            default:
                                //                break;
                                //        }
                                //    }
                                //}
                            }
                        }
                        
                       
                        //if ((string)index["codec_type"] == "video")
                        //    musicfile.CoverFormat = (string)index["codec_name"];
                    }
                }

                if (topkey.Key == "format")
                {
                    foreach (JProperty secondary in topkey.Value)
                    {
                        int a = secondary.Count;
                        if (secondary.Name == "size")
                            musicfile.Size = (float)Math.Round((float)secondary.Value / 1024 / 1024, 2);
                        if (secondary.Name == "bit_rate")
                        {
                            if (musicfile.Codec == "flac")
                            {
                                musicfile.Bitrate = (int)secondary.Value / 1000;
                            }
                            if (musicfile.Codec == "opus")
                            {
                                musicfile.Bitrate = (int)secondary.Value / 1000;
                            }

                        }
                        if (secondary.Name == "tags")
                        {
                            foreach (JProperty mp3tag in secondary.Value)
                            {
                                switch (mp3tag.Name.ToLower())
                                {
                                    case "album":
                                        musicfile.Album = (string)mp3tag.Value;
                                        break;
                                    case "title":
                                        musicfile.Title = (string)mp3tag.Value;
                                        break;
                                    case "artist":
                                        musicfile.Interpret = (string)mp3tag.Value;
                                        break;
                                    case "date":
                                        musicfile.Date = (string)mp3tag.Value;
                                        break;
                                    case "genre":
                                        musicfile.Genre = (string)mp3tag.Value;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return musicfile;
        }

        public void ExtractTitleArtistFromName(MusicFile track)
        {
            string patternArtist = @"[\w_\s]+-";
            string patternTitle = @"-[\s\w]+";
            string patternOnlyTitleCheckAgainst = @"[\d]+[\s|.|-]+[\w\s]+[-]";
            string patternOnlyTitle = @"[\d]+[\s|.|-]+[\w\s]+";
            if (Regex.Match(track.Name, patternArtist).Success && Regex.Match(track.Name, patternTitle).Success)
            {
                string artist = @"[^\d][\w]+[\w\s]+";
                track.Interpret = Regex.Match(track.Name, patternArtist).Value;
                track.Interpret = Regex.Match(track.Interpret, artist).Value;
                track.Interpret = track.Interpret.TrimStart('_').TrimStart(' ').TrimEnd('_').TrimEnd(' ').Replace('_', ' ');


                string title = @"[\w]+[\w\s]+";
                track.Title = Regex.Match(track.Name, patternTitle).Value;
                track.Title = Regex.Match(track.Title, title).Value;
                track.Title = track.Title.TrimStart('_').TrimStart(' ').TrimEnd('_').TrimEnd(' ').Replace('_', ' ');                          
            }

            if (Regex.Match(track.Name, patternOnlyTitle).Success && !Regex.Match(track.Name, patternOnlyTitleCheckAgainst).Success)
            {
                string title = @"(?!\d)[\w\s]+";
                track.Title = Regex.Match(track.Name, patternOnlyTitle).Value;
                track.Title = Regex.Match(track.Title, title).Value;
                track.Title = track.Title.TrimStart('_').TrimStart(' ').TrimEnd('_').TrimEnd(' ').Replace('_', ' ');                

                string AlbumArtist = @"[\w\s]+(?=-)";                
                if (Regex.Match(Directory.GetParent(track.Path).Name, AlbumArtist).Success)
                {
                    track.Interpret = Regex.Match(Directory.GetParent(track.Path).Name, AlbumArtist).Value;
                    track.Interpret = Regex.Match(track.Interpret, title).Value;
                    track.Interpret = track.Interpret.TrimEnd('-').TrimEnd(' ');
                }
                if(!Regex.Match(Directory.GetParent(track.Path).Name, AlbumArtist).Success)
                {
                    track.Interpret = Regex.Match(Directory.GetParent(track.Path).Name, AlbumArtist).Value;
                }               
            }           
        }

        public VideoFile CreateVideoMeta(string path)
        {
            Process process = new Process();
            process.StartInfo.FileName = Path;
            process.StartInfo.Arguments = $"-v quiet -print_format json -i \"{path}\" -show_streams -show_format";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            StreamReader stream = process.StandardOutput;
            string jsonstring = stream.ReadToEnd();

            JsonTextReader reader = new JsonTextReader(new StringReader(jsonstring));

            JObject jObject = JObject.Parse(jsonstring);

            // Streams und Format werden aufgelistet
            VideoFile vidoefile = new VideoFile();
            vidoefile.TimeAdded = DateTime.Now;
            vidoefile.Path = path;
            vidoefile.Name = System.IO.Path.GetFileName(path);
            vidoefile.NameWithoutExtension = System.IO.Path.GetFileName(path).NameWithoutExtension();
            foreach (KeyValuePair<string, JToken> topkey in jObject)
            {
                if (topkey.Key == "streams")
                {
                    foreach (JObject index in topkey.Value)
                    { // index == stream
                        if ((string)index["codec_type"] == "video")
                        {
                            foreach (KeyValuePair<string, JToken> streaminfo in index)
                            {
                                //Console.WriteLine(streaminfo.Key + " ist gleich " + streaminfo.Value);
                                switch (streaminfo.Key)
                                {

                                    case "codec_name":
                                        vidoefile.Codec = (string)streaminfo.Value;
                                        break;
                                    case "width":
                                        vidoefile.Width = (int)streaminfo.Value;
                                        break;
                                    case "height":
                                        vidoefile.Height = (int)streaminfo.Value;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        //if ((string)index["codec_type"] == "video")
                        //    musicfile.CoverFormat = (string)index["codec_name"];
                    }
                }

                if (topkey.Key == "format")
                {
                    foreach (JProperty secondary in topkey.Value)
                    {
                        int a = secondary.Count;
                        if (secondary.Name == "size")
                            vidoefile.Size = (int)(float)Math.Round((float)secondary.Value / 1024 / 1024, 2);
                        if (secondary.Name == "bit_rate")
                        {
                            vidoefile.Bitrate = (int)(float)secondary.Value / 1024;
                        }
                        if (secondary.Name == "duration")
                        {
                            vidoefile.Duration = (int)(float)secondary.Value;
                        }
                    }
                }
            }
            return vidoefile;
        }

        public void GetIFrames(VideoFile file,string outputPath)
        {
            Process process = new Process();
            process.StartInfo.FileName = Path;
            process.StartInfo.Arguments = $"-loglevel error -skip_frame nokey -select_streams v:0 -show_entries frame=pkt_pts_time -of csv=print_section=0 {file.Path}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            StreamReader stream = process.StandardOutput;
            string csvString = stream.ReadToEnd();
            //string[] lines = csvString.Split(new[] { Environment.NewLine },StringSplitOptions.None);
            System.IO.File.WriteAllText(System.IO.Path.Combine(outputPath, file.Id.ToString()), csvString);
        }
    }
}
