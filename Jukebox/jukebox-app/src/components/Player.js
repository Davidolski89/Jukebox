import { useEffect, useState, componentDidMount, useRef } from "react";
import "../Css/player.css";
import "../Css/skipButton.css";

const Player = (props) => {
  const audioPlayer = useRef();
  const progressBar = useRef();
  const [currentTrack, setCurrentTrack] = useState("");
  const [currentWaveForm, setCurrentWaveForm] = useState("");
  const [volume, setVolume] = useState(30);

  useEffect(() => {
    audioPlayer.current.volume = volume / 100;
  }, []);
  // change source file event
  useEffect(() => {
    if (props.currentSong.length > 2) {
      setCurrentTrack("");
      audioPlayer.current.removeAttribute("src");
      audioPlayer.current.load();
      setCurrentTrack(
        "jukebox/api/downloadmusic/DownloadMp3/" + props.currentSong[0]
      );
      setCurrentWaveForm(
        "jukebox/api/downloadmusic/DownloadWaveForm/" + props.currentSong[0]
      );
      //audioPlayer.current.preload = "auto";
      audioPlayer.current.load();
      if (props.currentSong.length == 6) {
        audioPlayer.current.currentTime = props.currentSong[5];
      }
      if (props.currentSong[3] === "play") {
        audioPlayer.current.play();
      }
    }
  }, [props.currentSong]);
  useEffect(() => {
    if (props.playerCommand === "play" && props.currentSong[3] === "play") {
      audioPlayer.current.play();
    }
  }, [currentTrack]);

  // play pause
  useEffect(() => {
    if (props.playerCommand === "play") {
      audioPlayer.current.play();
    }
    if (props.playerCommand === "pause") {
      audioPlayer.current.pause();
    }
  }, [props.playerCommand]);

  // start music first time

  function handlePlay(e) {
    props.setStartListening(true);
    e.preventDefault();
    e.target.style.display = "none";
  }
  function handlePlayThrough() {
    props.sendTrackLoaded();
  }
  function handleTimeChange(e) {
    // let percentage = (e.target.currentTime / props.currentSong[4][0]) * 100;
    // progressBar.current.style.width = percentage.toString() + "%";
    let percentage = (e.target.currentTime / props.currentSong[4][0]) * 100;
    progressBar.current.style.height = percentage.toString() + "%";
  }

  const handleVolumeChange = (e) => {
    setVolume(e.target.value);
    audioPlayer.current.volume = volume / 100;
  };

  return (
    <div id="player">
      <div id="progressContainer" max={1}>
        <div ref={progressBar} id="progressbar"></div>
        <img src={currentWaveForm} id="waveForm"></img>
      </div>
      <audio
        ref={audioPlayer}
        src={currentTrack}
        onEnded={props.sendTrackFinished}
        onCanPlayThrough={handlePlayThrough}
        onTimeUpdate={handleTimeChange}
      ></audio>
      <input
        id="volume"
        type="range"
        onChange={handleVolumeChange}
        value={volume}
        max="100"
        onClick={handleVolumeChange}
      />
      <input
        id="skipButton"
        type="button"
        value={props.votes == 0 ? "Skip" : "Skip " + props.votes + "%"}
        onClick={props.sendVote}
      />
      <button
        className="btn btn-danger"
        onClick={(e) => {
          handlePlay(e);
        }}
      >
        Connect
      </button>
    </div>
  );
};

export default Player;
