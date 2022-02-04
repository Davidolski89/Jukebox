using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MediaManagement.MediaFiles;

namespace Jukebox.Models
{
    public class MusicDb : DbContext
    {
        public MusicDb(DbContextOptions<MusicDb> options) : base(options)
        {
            //if (!TableExists())
            //    GenerateDatabase();
        }

        public DbSet<MusicFile> Tracks { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistMusicFile> PlaylistTracks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Playlist>().HasKey(c => c.Id);
            modelBuilder.Entity<MusicFile>().HasKey(c => c.Id);
            modelBuilder.Entity<PlaylistMusicFile>().HasKey(x => new { x.MusicFileId, x.PlaylistId });

            modelBuilder.Entity<PlaylistMusicFile>()
                .HasOne(x => x.MusicFile)
                .WithMany(y => y.PlaylistMusicFiles)
                .HasForeignKey(z => z.MusicFileId);
            modelBuilder.Entity<PlaylistMusicFile>()
               .HasOne(x => x.Playlist)
               .WithMany(y => y.PlaylistMusicFiles)
               .HasForeignKey(z => z.PlaylistId);
        }

        public bool TableExists()
        {
            string path = Path.Combine(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName, "Database", "Music.db");
            SqliteConnection sqlite = new SqliteConnection("Data Source=" + path);
            SqliteCommand checkfortable = new SqliteCommand("select * from Playlists;", sqlite);
            try
            {
                sqlite.Open();
                checkfortable.ExecuteNonQuery();
                sqlite.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void GenerateDatabase()
        {
            string dbDirectory = Path.Combine(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName, "Database");
            string creationString;
            using (StreamReader reader = new StreamReader(Path.Combine(dbDirectory, "MusicDbCreationString")))
            {
                creationString = reader.ReadToEnd();
            }
            string path = Path.Combine(dbDirectory, "Music.db");
            SqliteConnection sqlite = new SqliteConnection("Data Source=" + path);
            SqliteCommand createAllTables = new SqliteCommand(creationString, sqlite);
            sqlite.Open();
            createAllTables.ExecuteNonQuery();
            sqlite.Close();
        }
        public static void CheckAndCreateDatabase()
        {
            if (!check())
                generate();
        }
        static bool check()
        {
            string path = Path.Combine(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName, "Database", "Music.db");
            SqliteConnection sqlite = new SqliteConnection("Data Source=" + path);
            SqliteCommand checkfortable = new SqliteCommand("select * from Playlists;", sqlite);
            try
            {
                sqlite.Open();
                checkfortable.ExecuteNonQuery();
                sqlite.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        static void generate()
        {
            string dbDirectory = Path.Combine(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName, "Database");
            string creationString;
            using (StreamReader reader = new StreamReader(Path.Combine(dbDirectory, "MusicDbCreationString")))
            {
                creationString = reader.ReadToEnd();
            }
            string path = Path.Combine(dbDirectory, "Music.db");
            SqliteConnection sqlite = new SqliteConnection("Data Source=" + path);
            SqliteCommand createAllTables = new SqliteCommand(creationString, sqlite);
            sqlite.Open();
            createAllTables.ExecuteNonQuery();
            sqlite.Close();
        }

    }
}
