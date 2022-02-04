using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Jukebox.Hubs;
using Microsoft.Extensions.Configuration;
using MediaManagement.MediaFiles;
using Jukebox.Models;
using Jukebox.Data.Mirror;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MusicGeter;
using SpotifyApiCalls;
using System.Net;
using FFPempek;

namespace Jukebox.Data
{
    public class SongManager
    {
        #region Properties / Fields
        // Database
        //public MusicDb musicDb;
        // Current Playlist
        string rootPath;
        IHubContext<JukeboxHub> hubContext;
        public bool VoteInProgress;
        Thread VoteThread;
        
        public List<MusicFile> QueList = new List<MusicFile>();
        public MusicFile CurrentSong;
        public MusicFile LastSong;
        private readonly IServiceScopeFactory scopeFactory;
        // Player
        public bool enabled { get; set; } = true;
        public PlayerState Playerstate { get; set; } = PlayerState.paused;
        public Playlist Playlist= new Playlist();
        // Spotify
        Queue<PlaylistRequest> spotifyQue = new Queue<PlaylistRequest>();
        bool spotifyWorking = false;
        string spotifyAppId;
        string spotifyAppSecret;
        // Properties
        IConfiguration config;
        MirrorGod mirrorGod;
        Encoder encoder;
        public bool Busy { get; set; } = false;
        #endregion

        public SongManager(IConfiguration cf, IServiceScopeFactory scope,IHubContext<JukeboxHub> juke)
        {
            config = cf;            
            mirrorGod = new MirrorGod(config.GetSection("MGDownloadPath").Value);
            rootPath = config.GetSection("MGDownloadPath").Value;
            encoder = new Encoder(config.GetSection("FFmpegPath").Value, config.GetSection("FFProbePath").Value, config.GetSection("MGDownloadPath").Value);
            spotifyAppId = config.GetSection("SpotifyAppId").Value;
            spotifyAppSecret = config.GetSection("SpotifyAppSecret").Value;
            scopeFactory = scope;
            hubContext = juke;            
        }


        Dictionary<int, List<string>> NextSongVotes = new Dictionary<int, List<string>>();
        public void VoteSong(int songId, string userId)
        {
            bool sameInput = false;
            foreach (KeyValuePair<int,List<string>> item in NextSongVotes)
            {
                if (item.Value.Contains(userId))
                {
                    item.Value.Remove(userId);
                    if (item.Key == songId)
                        sameInput = true;
                    break;
                }                    
            }
            if (!sameInput)
            {
                if (NextSongVotes.ContainsKey(songId))
                {
                    NextSongVotes[songId].Add(userId);
                }
                else
                    NextSongVotes.Add(songId, new List<string>() { userId });
            }
            
        }
        string VoteCount(int id)
        {
            if (NextSongVotes.ContainsKey(id))
                return NextSongVotes[id].Count.ToString();
            else
                return "0";
        }
        public IEnumerable<string[]> ReturnQueList()
        {
            
            return QueList.Where(x => x.Downloaded == true).Select(x => new string[] { x.Id.ToString(), x.Interpret, x.Title, x.Time, VoteCount(x.Id), x.Samplerate.ToString(), x.Size.ToString(), x.Codec.ToString() });
        }
        public void StartTimer()
        {
            Thread thread = new Thread(new ThreadStart(PlaylistTimer));
            VoteThread = thread;
            thread.Start();            
        }
        public void StopTimer()
        {
            VoteThread.Abort();
        }
        public void PlaylistTimer()
        {
            Thread.Sleep(20000);
            if (VoteInProgress)
            {
                hubContext.Clients.All.SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : 10 seconds remaining");
                Thread.Sleep(10000);
                if (VoteInProgress)
                {
                    hubContext.Clients.All.SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : Playlist change didn't pass");
                    hubContext.Clients.All.SendAsync("PopVoteClose");
                    VoteInProgress = false;
                }
            }
        }      
        
        #region Spotify Playlist
        public async Task SpotifyAddPlaylistRequest(PlaylistRequest request)
        {
            await Task.Run(()=> spotifyQue.Enqueue(request));
            //spotifyQue.Enqueue(request);
            spotifyProcessList();
        }
        async void spotifyProcessList()
        {
            if (!spotifyWorking)
            {
                int playlistToDelete = -1;
                try
                {
                    spotifyWorking = true;
                    PlaylistRequest playListRequest = spotifyQue.Dequeue();
                    
                    SpotifyApiRequest spotifyApiRequest = new SpotifyApiRequest(playListRequest.Url, spotifyAppId, spotifyAppSecret);
                    await spotifyApiRequest.ParseLink();
                    if (String.IsNullOrEmpty(playListRequest.PlaylistName))
                        playListRequest.PlaylistName = spotifyApiRequest.AlbumName;

                    List<MusicFileContainer> containerList = new List<MusicFileContainer>();
                    spotifyApiRequest.Tracks.ForEach(x => containerList.Add(new MusicFileContainer() { MusicFile = x }));
                    //await hubContext.Clients.Client(playListRequest.RequesterHubId).SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : Processing {containerList.Count} titles ");
                    await hubContext.Clients.All.SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : Processing \"{playListRequest.PlaylistName +"\" with "+  containerList.Count} titles ");
                    mirrorGod.DownloadedTrack += handleMusicFileReturn;
                    encoder.Processed += handleMusicDownload;
                    mirrorGod.CurrentRequesterId = playListRequest.RequesterHubId;
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var musicDb = scope.ServiceProvider.GetRequiredService<MusicDb>();
                        containerList.ForEach(x => searchDb(x, musicDb));
                        await hubContext.Clients.Client(playListRequest.RequesterHubId).SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : Found {containerList.Where(x => x.MusicFile.Downloaded == true).Count()} tracks in database");
                        List<MusicFile> musicNotYetInDb = new List<MusicFile>();
                        await hubContext.Clients.Client(playListRequest.RequesterHubId).SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : Starting search and download");
                        containerList.ForEach(x => { mirrorGod.SpotifySearchDownload(x); });
                        containerList.ForEach(x => { if (!x.OnHDD) musicNotYetInDb.Add(x.MusicFile); });
                        musicDb.AddRange(musicNotYetInDb);
                        musicDb.SaveChanges();
                        musicNotYetInDb.ForEach(x => downloadCover(x));
                        containerList.ForEach(x => { if (x.OnHDD) musicNotYetInDb.Add(x.MusicFile); });
                        await hubContext.Clients.Client(playListRequest.RequesterHubId).SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : Converting {containerList.Where(x => x.MusicFile.Downloaded == true && x.OnHDD == false).Count()} tracks to Opus");
                        containerList.ForEach(x => { if (!x.OnHDD && x.MusicFile.Downloaded) encoder.CreateOpusWaveForm(x); });
                        Playlist spotiPlaylist = new Playlist() { Name = playListRequest.PlaylistName, Uploader = playListRequest.Requester, TimeAdded = DateTime.Now };
                        musicDb.Playlists.Add(spotiPlaylist);
                        musicDb.SaveChanges();
                        playlistToDelete = spotiPlaylist.Id;
                        List<PlaylistMusicFile> dbAddition = new List<PlaylistMusicFile>();
                        musicNotYetInDb.ForEach(x => dbAddition.Add(new PlaylistMusicFile { MusicFile = x, Playlist = spotiPlaylist }));
                        musicDb.AddRange(dbAddition);
                        musicDb.SaveChanges();
                        await hubContext.Clients.AllExcept(playListRequest.RequesterHubId).SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : Playlist \"{spotiPlaylist.Name}\" with {containerList.Where(x => x.MusicFile.Downloaded == true).Count()}/{spotifyApiRequest.Tracks.Count} tracks has been added by {playListRequest.Requester}!");
                        await hubContext.Clients.Client(playListRequest.RequesterHubId).SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : Playlist \"{spotiPlaylist.Name}\" with {containerList.Where(x => x.MusicFile.Downloaded == true).Count()}/{spotifyApiRequest.Tracks.Count} tracks has been added!");
                    }
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                    await hubContext.Clients.All.SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : {x.Message}");
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var musicDb = scope.ServiceProvider.GetRequiredService<MusicDb>();
                        if(playlistToDelete != -1)
                        {
                            var playlist = musicDb.Playlists.Where(x => x.Id == playlistToDelete).FirstOrDefault();
                            if (!(playlist is null))
                                musicDb.Playlists.Remove(playlist);
                        }                           
                    }
                }
                finally
                {
                    mirrorGod.DownloadedTrack -= handleMusicFileReturn;
                    encoder.Processed -= handleMusicDownload;
                    spotifyWorking = false;
                    if (spotifyQue.Count > 0)
                    {
                        spotifyProcessList();
                    }
                }
            }
        }

        void downloadCover(MusicFile file)
        {
            string coverPathRoot = Path.Combine(rootPath, "cover", file.Date);
            if (!Directory.Exists(coverPathRoot))
                Directory.CreateDirectory(coverPathRoot);
            using (WebClient client = new WebClient()){
                try
                {
                    client.DownloadFile(file.DownloadPathOpus, Path.Combine(coverPathRoot, file.Id.ToString() + ".jpg"));
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }                
            }
        }
        #endregion



        public void searchDb(MusicFileContainer container,MusicDb musicDb)
        {                     
                List<MusicFile> files = musicDb.Tracks.Where(x => x.Title.Contains(container.MusicFile.Title) && x.Interpret.Contains(container.MusicFile.Interpret)).ToList();

                if (files.Count > 0)
                {                    
                    container.MusicFile = files.FirstOrDefault();
                    container.OnHDD = true;
                }            
        }

        public void AddUserSong(MusicFile god)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MusicDb>();
                //MusicFile file = god.Mirrors.Where(x => x.MirrorName == mirrorName).First().CurrentSongList[songNumber];                
                MusicFile fileCopy = new MusicFile() { Path = god.Path, Title = god.Title, Interpret = god.Interpret, Date = god.Date, Time = god.Time ,Downloaded = god.Downloaded};
                db.Tracks.Add(fileCopy);
                db.SaveChanges();
                //encoder.CreateOpusWaveForm(container.MusicFile);
                encoder.CreateOpusWaveForm(fileCopy);
                db.SaveChanges();
                god = fileCopy;
            }
            AddSong(god);
        }
        public Playlist GetPlaylist(int id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var musicDb = scope.ServiceProvider.GetRequiredService<MusicDb>();
                return musicDb.Playlists.Where(x => x.Id == id).FirstOrDefault();
            }
        }

        #region Current Playlist
        public void AddSong(MusicFile musicFile)
        {

            QueList.Add(musicFile);
            if (CurrentSong is null)
            {
                LastSong = musicFile;
                CurrentSong = musicFile;
            }            
        }

        public void NextTrack()
        {           
            if (NextSongVotes.Count > 0)
            {
                int highestSongId = NextSongVotes.OrderByDescending(x => x.Value.Count).First().Key;
                LastSong = CurrentSong;
                CurrentSong = QueList.Where(x => x.Id == highestSongId).First();
                
            }
            else if (!(CurrentSong is null))
            {
                int curentPosition = QueList.FindIndex(x => x.Id == CurrentSong.Id);
                try
                {
                    if (curentPosition < QueList.Count - 1)
                    {
                        MusicFile file = QueList.ElementAt(curentPosition + 1);
                        LastSong = CurrentSong;
                        CurrentSong = file;
                    }
                    else
                        LastSong = CurrentSong;
                }
                catch
                {
                    
                }
                
            }
            NextSongVotes = new Dictionary<int, List<string>>();
        }
        public void ChangePlaylist(int id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var musicDb = scope.ServiceProvider.GetRequiredService<MusicDb>();
                var musicFiles = musicDb.PlaylistTracks.Include(x => x.MusicFile).Where(x => x.PlaylistId == id).Select(x => x.MusicFile).ToList();
                if(musicFiles.Count > 0)
                {
                    QueList = musicFiles.Where(x => x.Downloaded == true).ToList();
                    LastSong = QueList.ElementAt(0);
                    CurrentSong = QueList.ElementAt(0);
                    NextSongVotes = new Dictionary<int, List<string>>();
                }               
            }
        }
        async void handleMusicFileReturn(object sender, DownloadedTrackArgs e)
        {
            string response = e.Downloaded ? "\""+e.Interpret + " - " + e.Title + "\" [found]" : "\""+e.Interpret + " - " + e.Title + "\" [failed]";
            await hubContext.Clients.Client(mirrorGod.CurrentRequesterId).SendAsync("Console", response);          
        }
        async void handleMusicDownload(object sender, FFmpegEventArgs e)
        {
            string response = "\""+e.Interpret+" - "+e.Title +"\" encoded [ExitCode="+e.ExitCode+"]";
            await hubContext.Clients.Client(mirrorGod.CurrentRequesterId).SendAsync("Console", response);
        }
        #endregion
    }

    public class PlaylistRequest
    {
        string requester;
        public string Requester
        {
            get { return requester; }
            set
            {
                string name = value.ToString();
                requester = name.Substring(0, 1).ToUpper() + name.Substring(1, name.Length - 1).ToLower();
            }
        }
        public string PlaylistName { get; set; }
        public string Url { get; set; }
        public string RequesterHubId { get; set; }
    }

    public enum PlayerState
    {
        paused = 1,
        playing = 2,
        loading = 3,
        stoped = 4
    }
}
