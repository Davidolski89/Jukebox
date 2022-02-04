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
        public void MusicCreateTrackCover(MusicFile file, string outputPath, int scale = 200)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = Path;                
                process.StartInfo.Arguments = $"-i \"{file.Path}\" -an -vf scale={scale}:-1 \"{System.IO.Path.Combine(outputPath, file.Id + ".jpg")}\" -y";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardError = false;
                process.Start();
                process.PriorityClass = ProcessPriorityClass.Idle;
                process.WaitForExit();
            }           
        }
        public void MusicCreateTrack(MusicFile track,string outputPath, int bitrate = 96,Codec codec = Codec.Opus)
        {
            using (Process process = new Process())
            {                
                process.StartInfo.FileName = Path;                

                switch (codec)
                {
                    case Codec.Opus:
                        process.StartInfo.Arguments = $"-i \"{track.Path}\" -c:a libopus -b:a {bitrate}k -vbr on -compression_level 10 \"{System.IO.Path.Combine(outputPath, track.Id + "." + codec.ToString().ToLower())}\" -y";
                        break;
                    case Codec.Mp3:
                        process.StartInfo.Arguments = $"-i \"{track.Path}\" -c:a libmp3lame -b:a {bitrate}k -vbr on -compression_level 10 \"{System.IO.Path.Combine(outputPath, track.Id + "." + codec.ToString().ToLower())}\" -y";
                        break;
                    default:
                        break;
                }
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardError = false;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.PriorityClass = ProcessPriorityClass.Idle;
                process.WaitForExit();
                
                OnProcessed(new FFmpegEventArgs() { ExitCode = process.ExitCode, Interpret = track.Interpret,Title = track.Title,Message = ""});
            }        
        }
        public void MusicCreateWaveForm(MusicFile file, string outputPath, int height = 140, int width = 1280 , string color = "0x5F9EA0",bool rotate = false)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = Path;
                if (rotate)
                    process.StartInfo.Arguments = $"-i \"{file.Path}\" -filter_complex \"showwavespic=s={width}x{height}:colors={color}[picture];[picture]transpose=3[ready]\" -map \"[ready]\" -frames:v 1 \"{System.IO.Path.Combine(outputPath, file.Id + ".png")}\" -y";                
                else
                    process.StartInfo.Arguments = $"-i \"{file.Path}\" -filter_complex \"showwavespic=s={width}x{height}:colors={color}\" -frames:v 1 \"{System.IO.Path.Combine(outputPath, file.Id + ".png")}\" -y";                
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardError = false;
                process.StartInfo.UseShellExecute = false;

                process.Start();
                process.PriorityClass = ProcessPriorityClass.Idle;
                process.WaitForExit();
            }
        }
    }
}
