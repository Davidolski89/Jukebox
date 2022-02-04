using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using MediaManagement.MediaFiles;

namespace FFPempek
{
    public partial class FFmpeg
    {
        public string Path { get; set; }

        public FFmpeg(string ffmpegpath)
        {
            Path = ffmpegpath;
        }
        public void Convert(string inputPath, string outputPath, string codec, int crf, string container)
        {
            Process process = new Process();
            process.StartInfo.FileName = Path;            
            process.StartInfo.Arguments = "-i " + inputPath + " -c:v " + codec + " -crf " + crf.ToString() + " -c:a copy"  + " " + outputPath + container;            
            process.Start();
            process.PriorityClass = ProcessPriorityClass.Idle;
            process.WaitForExit();
            process.Dispose();
        }

        public void CreateVideoPreview(VideoFile video,string outputPath, string codec = "libvpx-vp9", int cuts = 5, int cutdur = 3, int resolution = 320, int crf = 25)
        {
            StringBuilder s = new StringBuilder();
            s.Append(Path + " ");
            for (int i = 0; i < cuts; i++)
            {
                int startingtime = video.Duration / 5 * (i + 1);
                s.Append($"-ss {startingtime} -t {cutdur} -i \"{video.Path}\" ");
            }
            s.Append("-filter_complex \"");
            for (int i = 0; i < cuts; i++)
            {
                s.Append($"[{i}:v:0]");
            }
            s.Append($"concat=n={cuts}:v=1:a=0[outv];");
            s.Append($"[outv]scale=-1:{resolution}[outv2]\" ");
            s.Append($"-c:v {codec} -crf {crf} -b:v 0 \"{System.IO.Path.Combine(outputPath, video.Id.ToString() + ".webm")}\"");



            Process process = new Process();
            process.StartInfo.FileName = Path;
            process.StartInfo.Arguments = s.ToString();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();
        }

        public void CreateThumbnail(VideoFile file, string outputpath, int quality = 5)
        {
            string arguments = Path + " " + $" -ss {file.Duration/5} -i input -vframes 1 -q:v {quality} {System.IO.Path.Combine(outputpath, file.Id.ToString())}.jpg";
            Process process = new Process();
            process.StartInfo.FileName = Path;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();
        }
    }
}
