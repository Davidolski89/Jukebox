using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaManagement.MediaFiles;
using FFPempek;

namespace Jukebox.Data.Mirror
{
    public class Encoder
    {
        FFmpeg ffmpeg;
        FFprobe ffprobe;
        string rootPath = "";
        public string CurrentRequester { get; set; }

        public Encoder(string ffmpegPath, string ffprobePath, string path)
        {
            ffmpeg = new FFmpeg(ffmpegPath);
            ffprobe = new FFprobe(ffprobePath);
            rootPath = path;
        }
        public event EventHandler<FFmpegEventArgs> Processed;
        protected virtual void OnProcessed(FFmpegEventArgs e)
        {
            EventHandler<FFmpegEventArgs> handler = Processed;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        void EncoderOnProcessed(object theThing, FFmpegEventArgs args)
        {
            EventHandler<FFmpegEventArgs> handler = Processed;
            if (handler != null)
            {
                handler(this, args);
            }
        }


        public void CreateOpusWaveForm(MusicFileContainer container)
        {
            ffmpeg.Processed += EncoderOnProcessed;
            try
            {
                if (!container.OnHDD)
                {
                    var musicfile = container.MusicFile;
                    ffprobe.GetTrackMeta(musicfile);
                    //musicfile.DownloadPathOpus = Path.Combine(rootPath, "opus", musicfile.Date, musicfile.Id.ToString() + ".opus");                
                    if (!Directory.Exists(Path.Combine(rootPath, "opus", musicfile.Date)))
                        Directory.CreateDirectory(Path.Combine(rootPath, "opus", musicfile.Date));
                    ffmpeg.MusicCreateTrack(musicfile, Path.Combine(rootPath, "opus", musicfile.Date), 144);
                    if (!Directory.Exists(Path.Combine(rootPath, "waveForm", musicfile.Date)))
                        Directory.CreateDirectory(Path.Combine(rootPath, "waveForm", musicfile.Date));
                    ffmpeg.MusicCreateWaveForm(musicfile, Path.Combine(rootPath, "waveForm", musicfile.Date), 150, 900, "#ffa500", true);
                    musicfile.Cached = true;
                }
            }
            catch
            {

            }
            finally
            {
                ffmpeg.Processed -= EncoderOnProcessed;
            }
            
        }
        public void CreateOpusWaveForm(MusicFile musicfile)
        {
            ffprobe.GetTrackMeta(musicfile);
            musicfile.Cached = true;
            if (!Directory.Exists(Path.Combine(rootPath, "opus", musicfile.Date)))
                Directory.CreateDirectory(Path.Combine(rootPath, "opus", musicfile.Date));
            ffmpeg.MusicCreateTrack(musicfile, Path.Combine(rootPath, "opus", musicfile.Date), 144);
            if (!Directory.Exists(Path.Combine(rootPath, "waveForm", musicfile.Date)))
                Directory.CreateDirectory(Path.Combine(rootPath, "waveForm", musicfile.Date));
            ffmpeg.MusicCreateWaveForm(musicfile, Path.Combine(rootPath, "waveForm", musicfile.Date), 150, 900, "#ffa500", true);
            musicfile.Cached = true;
        }
    }
    public class EncoderEventArgs : EventArgs
    {
        public bool Successfull { get; set; }
        public string Message { get; set; }
        public string Interpret { get; set; }
        public string Title { get; set; }
        public int ExitCode { get; set; }
    }
}
