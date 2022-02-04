using System;
using System.Collections.Generic;
using System.Text;

namespace FFPempek
{
    public partial class FFmpeg
    {
        public event EventHandler<FFmpegEventArgs> Processed;

       
        protected virtual void OnProcessed(FFmpegEventArgs e)
        {
            EventHandler<FFmpegEventArgs> handler = Processed;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }


    public class FFmpegEventArgs : EventArgs
    {
        public bool Successfull { get; set; }
        public string Message { get; set; }
        public string Interpret { get; set; }
        public string Title { get; set; }
        public int ExitCode { get; set; }       
    }
}
