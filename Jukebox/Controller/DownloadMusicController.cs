using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Jukebox.Data;
using Jukebox.Models;
using MediaManagement.MediaFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace Jukebox.Controller
{
    [ApiController]
    //[Route("[controller]")]
    [Route("jukebox/api/[controller]/[action]")]
    public class DownloadMusicController : ControllerBase
    {
        SongManager manager;
        string rootPath;
        bool enabled;        
        MusicDb musicDb;
        string envCurrentDirectory;

        public DownloadMusicController(IConfiguration configuration, IWebHostEnvironment webenv, SongManager songManager, MusicDb db)
        {               
            manager = songManager;
            rootPath = configuration.GetSection("MGDownloadPath").Value;
            envCurrentDirectory = webenv.WebRootPath;
            enabled = songManager.enabled;
            musicDb = db;
        }
        


        [HttpGet("{id}")] /*("{id}")*/
        public IActionResult DownloadMp3(int id)
        {
            if (enabled)
            {                
                MusicFile file = musicDb.Tracks.Where(x => x.Id == id).FirstOrDefault();
                //if (!file.Cached)
                //{
                //    PhysicalFileResult result = new PhysicalFileResult(file.DownloadPath, "audio/mpeg")
                //    {
                //        FileDownloadName = id.ToString()
                //    };
                //    return result;
                //}
                //else
                {
                    PhysicalFileResult result = new PhysicalFileResult(Path.Combine(rootPath,"opus",file.Date,file.Id.ToString()+".opus"), "audio/ogg")
                    {
                        FileDownloadName = id.ToString()
                    };
                    return result;
                }
            }
            else return NotFound();
        }
        [HttpGet("{id}")]
        public IActionResult DownloadWaveForm(int id)           
        {
            MusicFile file = musicDb.Tracks.Where(x => x.Id == id).FirstOrDefault();
            if (enabled)
            {
                PhysicalFileResult result = new PhysicalFileResult(System.IO.Path.Combine(rootPath, "waveForm", file.Date ,id + ".png"), "image/png");
                return result;
            }
            else return NotFound();
            
        }
        [HttpGet("{id}")]
        public IActionResult DownloadCover(int id)
        {
            MusicFile file = musicDb.Tracks.Where(x => x.Id == id).FirstOrDefault();
            if (enabled)
            {
                if (System.IO.File.Exists(Path.Combine(rootPath, "cover", file.Date, id + ".jpg")))
                {
                    PhysicalFileResult result = new PhysicalFileResult(Path.Combine(rootPath, "cover", file.Date, id + ".jpg"), "image/jpg");
                    return result;
                }                     
                else
                {
                    PhysicalFileResult result = new PhysicalFileResult(Path.Combine(envCurrentDirectory, "media","nocover.jpg"),"image/jpg");
                    return result;
                }
            }
            else return NotFound();

        }

        [HttpPost]
        //public IActionResult GetPlaylist(string uploader, string date, string searchString)
        public IActionResult GetPlaylist([FromBody] PlaylistRequest request)
        {
            int take = 30;
            List<Playlist> playlists = new List<Playlist>();
            if (!String.IsNullOrEmpty(request.uploader) && !String.IsNullOrEmpty(request.searchString))
                playlists = musicDb.Playlists.Where(x => x.Uploader.ToLower() == request.uploader.ToLower()).Where(c => c.Name.ToLower().Contains(request.searchString.ToLower())).ToList();
            else if (!String.IsNullOrEmpty(request.uploader) && String.IsNullOrWhiteSpace(request.searchString))
                playlists = musicDb.Playlists.Where(x => x.Uploader.ToLower() == request.uploader.ToLower()).ToList();
            else if (String.IsNullOrEmpty(request.uploader) && !String.IsNullOrEmpty(request.searchString))
                playlists = musicDb.Playlists.Where(c => c.Name.ToLower().Contains(request.searchString.ToLower())).ToList();
            else
                playlists = musicDb.Playlists.ToList();
            if (request.date == "asc")
                return new JsonResult(playlists.OrderBy(x => x.TimeAdded).Take(take).Select(x => new string[] { x.Id.ToString(),x.Name, x.Uploader, x.TimeAdded.ToShortDateString() }));
            else
                return new JsonResult(playlists.OrderByDescending(x => x.TimeAdded).Take(take).Select(x => new string[] { x.Id.ToString(), x.Name, x.Uploader, x.TimeAdded.ToShortDateString() }));
        }
        [HttpGet]
        public IActionResult GetRequesters()
        {
            var requesters = musicDb.Playlists.Select(x => x.Uploader).Distinct().ToList();
            return new JsonResult(requesters);
        }

        [HttpGet]
        public IActionResult DisableThis()
        {
            manager.enabled = false;
            return Ok("Successfully disabled");
        }
        public IActionResult EnableThis()
        {
            manager.enabled = true;
            return Ok("Successfully enabled");
        }
    }
    public class PlaylistRequest
    {
        public string uploader { get; set; }
        public string date { get; set; }
        public string searchString { get; set; }
    }
}
