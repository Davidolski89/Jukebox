import React, { useState, useRef, useEffect } from "react";
import "../../Css/spotify.css";
const Spotify = (props) => {
  const playListLink = useRef();
  const playListName = useRef();
  const playlistRequester = useRef();
  const [remark, setRemark] = useState("");

  const handleTransmission = () => {
    let link = playListLink.current.value;
    let name = playListName.current.value;
    let requester = playlistRequester.current.value;
    if (playListLink.current.value.length > 9) {
      if (
        playListName.current.value.length > 2 ||
        playListName.current.value.length == 0
      ) {
        props.sendAddSpotifyPlaylist(link, name, requester);
      } else {
        setRemark("Playlist Name is too short");
      }
    } else {
      setRemark("Link is too short");
      return;
    }
  };
  const handleLinkInput = (target) => {
    if (target.target.value.length < 10) {
      setRemark("Link is too short");
    } else {
      setRemark("");
    }
  };

  const handleNameInput = (target) => {
    if (target.target.value.length < 3 && target.target.value != 0) {
      setRemark("Playlist Name is too short");
    } else {
      setRemark("");
    }
  };

  return (
    <>
      <div className="input-group mb-1">
        <span className="input-group-text">Link</span>
        <input
          onInput={handleLinkInput}
          ref={playListLink}
          type="text"
          className="form-control spotiInput"
        ></input>
      </div>
      <div className="input-group mb-1">
        <span className="input-group-text">Playlist Name</span>
        <input
          onInput={handleNameInput}
          ref={playListName}
          type="text"
          className="form-control"
          placeholder="Leave empty for default"
        ></input>
      </div>
      <div className="input-group mb-1">
        <span className="input-group-text">Requester</span>
        <input
          ref={playlistRequester}
          type="text"
          className="form-control"
          placeholder="Leave empty for Anonymous"
        ></input>
      </div>
      <div className="input-group mb-1">
        <input
          type="button"
          className="btn btn-primary"
          value="Add Playlist"
          onClick={handleTransmission}
        />
      </div>
      <span style={{ color: "red" }}>{remark}</span>
    </>
  );
};

export default Spotify;
