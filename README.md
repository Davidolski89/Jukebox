# Jukebox
Listen together to spotify playlists

# Description
React SPA Application which allows you to listen to Spotify playlists with anybody visiting tha page.
.NET backend to support functionanlity.

# Features 
- parse public spotify playlists
- start votes for playlists
- skip tracks
- vote next track in que
- search and add single tracks to current playlist
- playlists and tracks are stored on harddrive and database
- automatic WaveForm creation with ffmpeg for every track
- console with information of whats going on in the backend
    - somebody entering or leaving
    - playlist finished parsing
    - track/playlist added
- file compression with Opus    
## Usage
#### 1. Prerequisite
- ffmpeg/ffprobe binaries for your platform  
- Get access to Spotify API by registering an app in the developer section
#### 2. Main
1. configure paths and Spotify id/secret in appsetting.json

### Technologies
Frontend: React, [SignalR](https://github.com/SignalR/SignalR) (WebSockets)  
Backend: .NET, [SignalR](https://github.com/SignalR/SignalR), EntityFramework, [SpotifyAPI-NET](https://johnnycrazy.github.io/SpotifyAPI-NET/), FFmpeg, FFprobe

