using System;
using System.Collections.Generic;
using System.Text;

namespace MediaManagement.MediaFiles
{
    public enum Assignment
    {
        ConvertAudio,
        ConvertVideo,
        CreateCacheVideo,
        CreateThumbnailVideo,
        CreateCacheTrack,
        CreateCacheCover,
        CreateWaveForm
    }
    public enum Codec
    {
        X264,
        X265,
        AV1,
        Mp3,
        Opus,
    }
    public enum Preset
    {
        Fast,
        Medium,
        Slow
    }
    public class VideoEncodingSettings
    {
        public int Id { get; set; }
        public string FFmpegPath { get; set; }
        public int Bitrate { get; set; }
        public int Crf { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Codec Codec { get; set; }
        public Preset Preset { get; set; }
        public int Threads { get; set; }
        public string OutputPath { get; set; }
    }
    public class AudioEncodingSettings
    {
        public AudioEncodingSettings(Codec codec = Codec.Opus, int bitrate = 128)
        {
            Bitrate = bitrate;
            Codec = codec;
        }
        public int Id { get; set; }
        public string FFmpegPath { get; set; }
        public int Bitrate { get; set; }
        public Codec Codec { get; set; }
        public string OutputPath { get; set; }
    }
}
