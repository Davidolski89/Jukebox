using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.Sqlite;

namespace Jukebox.Models
{
    public class MusicDbContextFactory : IDesignTimeDbContextFactory<MusicDb>
    {
        public MusicDb CreateDbContext(string[] args)
        {
            var GetDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var optionsBuilder = new DbContextOptionsBuilder<MusicDb>();
            optionsBuilder.UseSqlite($"Data Source={Path.Combine(GetDirectory, "Database", "Music.db")}");

            return new MusicDb(optionsBuilder.Options);
        }
    }
}
