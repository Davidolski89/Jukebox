using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using StreamChat.Data;
using System.Threading;
using MediaManagement.MediaFiles;
using Jukebox.Data.Mirror;
using Jukebox.Models;
using MusicGeter;
using Jukebox.Data;

namespace Jukebox.Hubs
{
    public class JukeboxHub : Hub
    {
        IConfiguration config;
        string downloadRootFolder;
        SongManager songManager;        
        static DateTime playtime = DateTime.Now;
        string ffmpegPath;
        string ffprobePath;
        static DateTime LastVoteTime = DateTime.Now;

        public static List<JUser> Users = new List<JUser>();
        
       
        MusicDb ReturnDbContext()
        {
            return new MusicDbContextFactory() { }.CreateDbContext(new string[] { });
        }

        #region Constructor
        public JukeboxHub(IConfiguration configuration, SongManager man)
        {
            config = configuration;            
            downloadRootFolder = config.GetSection("MGDownloadPath").Value;
            songManager = man;
            if (System.IO.File.Exists(config.GetSection("FFProbe").Value))
                ffprobePath = config.GetSection("FFProbe").Value;
            if (System.IO.File.Exists(config.GetSection("FFmpegPath").Value))
            {
                ffmpegPath = config.GetSection("FFmpegPath").Value;
                if(!Directory.Exists(Path.Combine(downloadRootFolder, "opus")))
                    Directory.CreateDirectory(Path.Combine(downloadRootFolder, "opus"));
                if(!Directory.Exists(Path.Combine(downloadRootFolder, "waveForm")))
                    Directory.CreateDirectory(Path.Combine(downloadRootFolder, "waveForm"));
            }           
        }
        #endregion
        public async Task SearchSong(string songName)
        {
            JUser user = Users.Where(x => x.Id == Context.ConnectionId).FirstOrDefault();
            MusicFileContainer container = new MusicFileContainer() { SearchString = songName };            
            user.MirrorGod.SearchMusicFile(container, MirrorName.All, 2);
            //songManager.searchDb(user.MusicGeter.MusicFileContainer);            
            await Clients.Caller.SendAsync("EnableSearch");
            List<object> temp = new List<object>();
            foreach (var mirror in user.MirrorGod.Mirrors)
            {
                int counter = 0;
                foreach (var song in mirror.CurrentSongList)
                {
                    temp.Add(new object[] { song.Interpret, song.Title, song.Time, mirror.MirrorName ,counter });
                    counter++;
                }                
            }
            await Clients.Caller.SendAsync("ReceiveSearchResult", temp);             
        }
        public async Task AddToQueue(MirrorName mirrorName, int songNumber)
        {
            JUser user = Users.Where(x => x.Id == Context.ConnectionId).FirstOrDefault();

            if ((DateTime.Now - user.LastQue).TotalSeconds > 60)
            {
                DateTime temp = user.LastQue;
                user.LastQue = DateTime.Now;
                while (songManager.Busy)
                {
                    Thread.Sleep(1000);
                }
                try
                {
                    songManager.Busy = true;
                    user.MirrorGod.DownloadMusicFile(mirrorName, songNumber);
                    MusicFile file = user.MirrorGod.MusicFileContainer.MusicFile;
                    file.Date = DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day.ToString();
                    songManager.AddUserSong(user.MirrorGod.MusicFileContainer.MusicFile);           
                    await Clients.All.SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : {file.Interpret +" - " +file.Title } has been added to queue");

                    
                    if (songManager.Playerstate == PlayerState.paused) //  && songManager.QueList.IndexOf(songManager.CurrentSong) == songManager.QueList.Count-2
                    {
                        //songManager.NextTrack();
                        await Clients.All.SendAsync("LoadNextTrack", songManager.CurrentSong.ToLoadNextTrack("pause"));
                    }
                    await Clients.All.SendAsync("GetCurrentQue", songManager.ReturnQueList()); //
                }
                catch(Exception x)
                {
                    Console.WriteLine(x.Message);
                    await Clients.Caller.SendAsync("Console", DateTime.Now.ToLongTimeString() + " : Something went wrong. Choose another track");
                    user.LastQue = temp;                    
                }
                finally {
                    songManager.Busy = false;
                    await Clients.Caller.SendAsync("EnableSearch");
                }
            }
            else
            {                
                await Clients.Caller.SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : You have to wait another {60-(DateTime.Now - user.LastQue).Seconds} seconds to add a new Song");
                await Clients.Caller.SendAsync("EnableSearch");
            }                
        }
        

        async Task SetNextTrack()
        {
            songManager.NextTrack();
            await Clients.All.SendAsync("GetCurrentQue", songManager.ReturnQueList());
            if (!(songManager.CurrentSong is null))
            {
                if (songManager.CurrentSong.Id != songManager.LastSong.Id)
                {
                    await Clients.All.SendAsync("LoadNextTrack", songManager.CurrentSong.ToLoadNextTrack("pause"));
                    Users.ForEach(x => x.TrackFinished = false);
                    songManager.Playerstate = PlayerState.loading;
                }// que empty
                else
                {
                    await Clients.All.SendAsync("Console", DateTime.Now.ToLongTimeString() + " : Que is empty");
                    songManager.Playerstate = PlayerState.paused;
                }
            }           
        }
        public async Task TrackFinished()
        {// Fired after song finished
            JUser user = Users.Where(x => x.Id == Context.ConnectionId).FirstOrDefault();
            user.TrackFinished = true;
            if(Users.Where(x=>x.TrackFinished == true).Count() == Users.Count)
            {
                // next track available
                await SetNextTrack();
            }
        }
        public async Task StartNextTrack()
        {
            JUser user = Users.Where(x => x.Id == Context.ConnectionId).FirstOrDefault();
            user.TrackLoaded = true;
            if (Users.Where(x => x.TrackLoaded == true).Count() == Users.Count)
            {                
                await Clients.All.SendAsync("StartNextTrack");
                songManager.Playerstate = PlayerState.playing;
                playtime = DateTime.Now;
                Users.ForEach(x => x.TrackLoaded = false);
            }
        }

        public async Task AddSpotifyPlaylist(string link, string name,string requester)
        {
            if (String.IsNullOrEmpty(requester))
               requester = "Anonymous";
            PlaylistRequest request = new PlaylistRequest { Url = link, PlaylistName = name, Requester = requester ,RequesterHubId = Context.ConnectionId };
            
            await Clients.Caller.SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : Playlist has been added to processing queue");            
            await songManager.SpotifyAddPlaylistRequest(request);            
        }

        #region Votes       

        public async Task VoteStartPlaylist(string zahl)
        {
            int plalistId = int.Parse(zahl);
            try
            {
                if (!songManager.VoteInProgress && (DateTime.Now - LastVoteTime).TotalSeconds > 33)
                {
                    LastVoteTime = DateTime.Now;
                    songManager.StartTimer();
                    songManager.VoteInProgress = true;
                    JUser user = Users.Where(x => x.Id == Context.ConnectionId).FirstOrDefault();
                    user.Votes[Vote.ChangePlaylist] = true;
                    songManager.Playlist = songManager.GetPlaylist(plalistId);
                    await calculateChangePlaylist();
                    await Clients.Others.SendAsync("PopVote", $"Request to load Playlist {songManager.Playlist.Name}");
                    await Clients.Caller.SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : Voting in progress");
                }
                else
                    await Clients.Caller.SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : Wait another {33 - (int)(DateTime.Now - LastVoteTime).TotalSeconds} seconds");
            }
            catch (Exception c)
            {
                Console.WriteLine(c.Message);
            }
        }

        async Task calculateSkipVotes()
        {
            float persons = Users.Where(x => x.Votes[Vote.Skip] == true).Count();
            float gap = persons / Users.Count * 100;
            //return (int)gap;
            
            if (gap > 65)
            {
                await Clients.All.SendAsync("Console", DateTime.Now.ToLongTimeString() + " : Skip vote has passed");
                await Clients.All.SendAsync("SkipTrack");                
                ResetVotes();
                await Clients.All.SendAsync("getVotes", 0);
            }
            else
                await Clients.All.SendAsync("getVotes", (int)gap);
        }
        public async Task VoteSkip()
        {
            if (!songManager.VoteInProgress)
            {
                JUser user = Users.Where(x => x.Id == Context.ConnectionId).FirstOrDefault();
                if (user.Votes[Vote.Skip] == true)
                    user.Votes[Vote.Skip] = false;
                else
                    user.Votes[Vote.Skip] = true;
                await calculateSkipVotes();
            }
            else
                await Clients.Caller.SendAsync("Console", DateTime.Now.ToLongTimeString() + " : There's already a Vote in progress");
        }

        public async Task VoteChangePlaylist(string somebool)
        {
            await Clients.Caller.SendAsync("PopVoteClose");
            if (songManager.VoteInProgress)
            {
                bool decision = bool.Parse(somebool);
                JUser user = Users.Where(x => x.Id == Context.ConnectionId).FirstOrDefault();
                user.Votes[Vote.ChangePlaylist] = decision;
                await calculateChangePlaylist();
            }                      
        }
        async Task calculateChangePlaylist()
        {
            float persons = Users.Where(x => x.Votes[Vote.ChangePlaylist] == true).Count();
            float gap = persons / Users.Count * 100;
            if (gap > 65)
            {
                await Clients.All.SendAsync("Console", DateTime.Now.ToLongTimeString() + $" : Changing to playlist {songManager.Playlist.Name}");
                songManager.VoteInProgress = false;
                await Clients.All.SendAsync("PopVoteClose");
                ResetVotes();
                songManager.ChangePlaylist(songManager.Playlist.Id);                
                await Clients.All.SendAsync("LoadNextTrack", songManager.CurrentSong.ToLoadNextTrack("pause"));                
                await Clients.All.SendAsync("GetCurrentQue", songManager.ReturnQueList());                
            }
        }

        public async Task VoteNextSong(string songId)
        {
            int intId = int.Parse(songId);
            songManager.VoteSong(intId, Context.ConnectionId);
            await Clients.All.SendAsync("GetCurrentQue", songManager.ReturnQueList());
        }
        public async Task VoteReplay(Vote voteCategory)
        {
            JUser user = Users.Where(x => x.Id == Context.ConnectionId).FirstOrDefault();
            if (user.Votes[Vote.Replay] == true)
                user.Votes[Vote.Replay] = false;
            else
                user.Votes[Vote.Replay] = true;

            float gap = Users.Where(x => x.Votes[Vote.Replay] == true).Count() / Users.Count;
            if (gap > 0.65 && true)
            {
                await Clients.All.SendAsync("ReplayTrack");
                ResetVotes();
            }           
        }
        void ResetVotes()
        {
            foreach (JUser usr in Users)
            {
                usr.Votes[Vote.ChangePlaylist] = false;
                usr.Votes[Vote.Play] = false;
                usr.Votes[Vote.Replay] = false;
                usr.Votes[Vote.Skip] = false;
            }
        }
        #endregion

        #region Connect / Disconnect
        public override async Task OnConnectedAsync()
        {
            await Clients.Others.SendAsync("Console", DateTime.Now.ToLongTimeString() + " : somebody entered the room");
            Users.Add(new JUser(Context.ConnectionId, downloadRootFolder) );           
            
            if (Users.Count == 1)
                playtime = DateTime.Now;
            await calculateSkipVotes();
            await Clients.Caller.SendAsync("Console", "Connected");
            
            if (!(songManager.CurrentSong is null) && (songManager.Playerstate == PlayerState.paused))
            {
                await Clients.Caller.SendAsync("LoadNextTrack", songManager.CurrentSong.ToLoadNextTrack("pause"));
            }
            if(!(songManager.CurrentSong is null) && (songManager.Playerstate == PlayerState.playing))
            {
                var time = Convert.ToInt32((DateTime.Now - playtime).TotalSeconds);
                var fullPlaytime = TimeSpan.Parse(songManager.CurrentSong.Time);
                if(time > (fullPlaytime.Hours +60 + fullPlaytime.Minutes))
                {
                    time = 0;
                }
                await Clients.Caller.SendAsync("LoadNextTrack", songManager.CurrentSong.ToLoadNextTrack("play"));
                await Clients.Caller.SendAsync("StartNextTrack");
            }
            if (!(songManager.CurrentSong is null) && (songManager.Playerstate == PlayerState.loading))
            {                
                await Clients.Caller.SendAsync("LoadNextTrack", songManager.CurrentSong.ToLoadNextTrack("play"));
                await Clients.Caller.SendAsync("StartNextTrack");
            }
            await Clients.Caller.SendAsync("GetCurrentQue", songManager.ReturnQueList());
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await Clients.Caller.SendAsync("Console", DateTime.Now.ToLongTimeString() + " : somebody left");
            
            Users.Remove(Users.Where(x => x.Id == Context.ConnectionId).FirstOrDefault());   
            if(Users.Count == 0)
            {
                songManager.Playerstate = PlayerState.paused;               
            }
            if (Users.Where(x => x.TrackFinished == true).Count() == Users.Count)
            {              
                await SetNextTrack();
            }
            await calculateSkipVotes();
            await base.OnDisconnectedAsync(ex);
        }
        #endregion
        
    }


    public class JUser
    {
        public JUser(string conId,string rootpath)
        {
            Id = conId;
            MirrorGod = new MirrorGod(rootpath);                      
        }       
        
        public string Id { get; set; }
        public MirrorGod MirrorGod { get; set; }
        public DateTime LastQue { get; set; }       
        public Dictionary<Vote, bool> Votes { get; set; } = new Dictionary<Vote, bool>()
        {
            { Vote.Skip, false },
            { Vote.ChangePlaylist, false },
            { Vote.Play, false },
            { Vote.Replay, false }          
        };
        public bool TrackFinished { get; set; } = false;
        public bool TrackLoaded { get; set; } = false;
    }
    
   
    public enum Vote
    {
        Skip = 1,
        Replay = 2,
        ChangePlaylist = 3,
        Play = 4
    }    
}
