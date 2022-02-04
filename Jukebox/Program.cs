using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jukebox.Hubs;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using StreamChat.Data;
using Jukebox.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Jukebox.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
MusicDb.CheckAndCreateDatabase();
builder.Services.AddRazorPages();
var GetDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
builder.Services.AddDbContext<MusicDb>(options => options.UseSqlite($"Data Source={Path.Combine(GetDirectory, "Database", "Music.db")}"));

builder.Services.AddSignalR();
builder.Services.AddSingleton<SongManager>();
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "jukebox-app/build";
});
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseSpaStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    //endpoints.MapRazorPages();
    endpoints.MapControllers();    
    endpoints.MapHub<JukeboxHub>("/jukeboxHub");
});

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "trololo";/*"jukebox-app";*/

    if (app.Environment.IsDevelopment())
    {
        spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");

    }
});
app.Run();
