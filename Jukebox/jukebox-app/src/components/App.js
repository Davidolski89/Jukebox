import React, { useState, useEffect } from "react";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
// import Controlls from "./Controlls";
import Playlist from "./Playlist";
import Search from "./Controls/Search";
import Player from "./Player";
import JukeConsole from "./JukeConsole";
import "../Css/global.css";
import Controls from "./Controls/Controls";
import PopPlaylist from "./PopPlaylist";

const App = () => {
  const selection = {
    Search: "search",
    Spotify: "spotofy",
    PlayistControlls: "playlistcontolls",
  };
  const [currentQue, setCurrentQue] = useState([]);
  const [currentSong, setCurrentSong] = useState([]);
  const [searchResults, setSearchResults] = useState([]);
  const [votes, setVotes] = useState(0);
  const [popPlaylistEnabled, setPopPlaylistEnabled] = useState(false);
  const [voteMessage, setVoteMessage] = useState("");
  const [disableSearch, setDisableSearch] = useState(false);
  const [startListening, setStartListening] = useState(false);
  const [consoleQue, setConsoleQue] = useState([[1, "Hello there"]]);
  const [playerCommand, setPlayerCommand] = useState("pause");
  const [connection, setConnecion] = useState(null);

  useEffect(() => {
    if (startListening == true) {
      const connect = new HubConnectionBuilder()
        .withUrl("/jukeboxHub")
        .withAutomaticReconnect()
        .build();
      setConnecion(connect);
    }
  }, [startListening]);

  useEffect(() => {
    if (connection) {
      connection.start().then(() => {
        connection.on("GetCurrentQue", (que) => {
          if (que.length > 0) {
            setCurrentQue(que);
          }
        });

        connection.on("LoadNextTrack", (nextTrack) => {
          setPlayerCommand(nextTrack[3]);
          setCurrentSong(nextTrack);
        });

        connection.on("StartNextTrack", () => {
          setPlayerCommand("play");
        });

        connection.on("ReceiveSearchResult", (songlist) => {
          setSearchResults(songlist);
        });
        setDisableSearch(false);

        connection.on("EnableSearch", () => {
          setDisableSearch(false);
        });
        connection.on("getVotes", (vote) => {
          setVotes(vote);
        });
        connection.on("SkipTrack", () => {
          sendTrackFinished();
        });
        connection.on("PopVote", (message) => {
          setPopPlaylistEnabled(true);
          setVoteMessage(message);
        });
        connection.on("PopVoteClose", () => {
          setPopPlaylistEnabled(false);
        });
        connection.on("Console", (message) => {
          setConsoleQue((odlConsoel) => {
            const lastNumber = odlConsoel[0][0];
            const brum = new Array(lastNumber + 1, message);
            const copy = odlConsoel.slice();
            if (copy.length > 4) {
            }
            copy.splice(0, 0, brum);
            return copy;
          });
        });
      });
      // if (connection.state == "Disconnected") {

      // }
    }
  }, [connection]);

  const sendSearchSong = function (songName) {
    setDisableSearch(true);
    connection.invoke("SearchSong", songName);
  };
  const sendTrackFinished = function () {
    connection.invoke("TrackFinished");
    setPlayerCommand("pause");
  };
  const sendTrackLoaded = function () {
    connection.invoke("StartNextTrack");
  };
  const sendAddToQueue = function (mirror, index) {
    connection.invoke("AddToQueue", mirror, index);
    // setSearchResults([]);
    setDisableSearch(true);
  };
  const sendVote = function () {
    connection.invoke("VoteSkip");
  };
  const voteNextSong = function (id) {
    connection.invoke("VoteNextSong", id);
  };
  const voteStartPlaylist = function (playlistId) {
    connection.invoke("VoteStartPlaylist", playlistId);
  };
  const sendPlaylistVote = function (vote) {
    connection.invoke("VoteChangePlaylist", vote);
  };
  const sendAddSpotifyPlaylist = function (
    playlistLink,
    playlistName,
    playlistRequester
  ) {
    connection.invoke(
      "AddSpotifyPlaylist",
      playlistLink,
      playlistName,
      playlistRequester
    );
  };

  return (
    <div id="mainContainer">
      {popPlaylistEnabled ? (
        <PopPlaylist
          voteMessage={voteMessage}
          popPlaylistEnabled={popPlaylistEnabled}
          setPopPlaylistEnabled={setPopPlaylistEnabled}
          sendPlaylistVote={sendPlaylistVote}
        ></PopPlaylist>
      ) : (
        <></>
      )}

      <Playlist
        currentQue={currentQue}
        currentSong={currentSong}
        voteNextSong={voteNextSong}
      ></Playlist>

      <Player
        currentSong={currentSong}
        playerCommand={playerCommand}
        sendTrackFinished={sendTrackFinished}
        sendTrackLoaded={sendTrackLoaded}
        setStartListening={setStartListening}
        startListening={startListening}
        votes={votes}
        sendVote={sendVote}
      ></Player>

      <div id="consoleSearch">
        <Controls
          voteStartPlaylist={voteStartPlaylist}
          sendAddSpotifyPlaylist={sendAddSpotifyPlaylist}
          sendSearchSong={sendSearchSong}
          searchResults={searchResults}
          sendAddToQueue={sendAddToQueue}
          disableSearch={disableSearch}
        ></Controls>
        <JukeConsole consoleQue={consoleQue}></JukeConsole>
      </div>
    </div>
  );
};

export default App;
