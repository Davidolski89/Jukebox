using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediaManagement.MediaFiles;
using SpotifyAPI.Web;

namespace SpotifyApiCalls
{
    public class SpotifyApiRequest
    {
        string incomingUrl { get; set; }
        public string AlbumName = "";
        public List<MusicFile> Tracks = new List<MusicFile>();
        string applicationId = "Your application Id";
        string applicationSecret = "Your application Secret";

        public SpotifyApiRequest(string url, string appId,string appSecret)
        {
            incomingUrl = url;
            applicationId = appId;
            applicationSecret = appSecret;
        }

        public async Task ParseLink()
        {
            Uri uri = new Uri(incomingUrl);
            string playlistId = uri.AbsolutePath.Substring(uri.AbsolutePath.LastIndexOf("/") + 1);

            var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(applicationId, applicationSecret));

            var spotify = new SpotifyClient(config);

            //old
            List<MusicFile> tracksToReturn = new List<MusicFile>();
            FullPlaylist tracks = await spotify.Playlists.Get(playlistId);
            AlbumName = tracks.Name;

            int total = 0;
            if (tracks.Tracks.Total is int isint)
                total = isint;


            foreach (PlaylistTrack<IPlayableItem> item in tracks.Tracks.Items)
            {
                if (item.Track is FullTrack track)
                {
                    string title = track.Name;
                    string interpret;
                    string uril = "";
                    if (track.Album.Images.Count == 3)
                        uril = track.Album.Images[2].Url;
                    
                    string timeString = TimeSpan.FromMilliseconds(track.DurationMs).Minutes + ":" + TimeSpan.FromMilliseconds(track.DurationMs).Seconds;
                    StringBuilder builder = new StringBuilder();
                    foreach (SimpleArtist aname in track.Artists)
                    {
                        builder.Append(aname.Name + ",");
                    }
                    interpret = builder.ToString().Substring(0, builder.ToString().Length - 1);

                    string date = DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day.ToString();
                    
                        
                    tracksToReturn.Add(new MusicFile() { Interpret = interpret, Title = title, Time = timeString, Date = date, DownloadPathOpus = uril });
                }
            }
            int counter = 1;
            while (total > 100)
            {
                var playlistGetItemsRequest = new PlaylistGetItemsRequest();
                playlistGetItemsRequest.Offset = 100 * counter;
                var tracks_new = await spotify.Playlists.GetItems(playlistId, playlistGetItemsRequest);
                foreach (var item in tracks_new.Items)
                {
                    if (item.Track is FullTrack track)
                    {
                        string title = track.Name;
                        string interpret;
                        string timeString = TimeSpan.FromMilliseconds(track.DurationMs).Minutes + ":" + TimeSpan.FromMilliseconds(track.DurationMs).Seconds;
                        StringBuilder builder = new StringBuilder();
                        foreach (SimpleArtist aname in track.Artists)
                        {
                            builder.Append(aname.Name + ",");
                        }
                        interpret = builder.ToString().Substring(0, builder.ToString().Length - 1);

                        string date = DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day.ToString();
                        tracksToReturn.Add(new MusicFile() { Interpret = interpret, Title = title, Time = timeString, Date = date });
                    }
                }

                counter++;
                total -= 100;
            }
            Tracks = tracksToReturn;
        }
    }
}
