import { useEffect, useState, componentDidMount, useRef } from "react";
import Search from "./Search";
import Spotify from "./Spotify";
import PlaylistControls from "./PlaylistControls";

const Controlls = (props) => {
  const selection = {
    Search: "Search",
    Spotify: "Spotify",
    PlaylistControls: "PlaylistControls",
  };
  const [selected, setSelected] = useState(selection.Search);

  // const handleSelect = (currentValue) => {
  //   setSelected(currentValue.target.value);
  // };
  function handleSelect(currentValue) {
    switch (currentValue.target.value) {
      case "Spotify":
        setSelected(selection.Spotify);
        break;
      case "Search":
        setSelected(selection.Search);
        break;
      case "PlaylistControls":
        setSelected(selection.PlaylistControls);
      default:
        break;
    }
  }

  return (
    <div id="search">
      <select value={selected} className="form-control" onChange={handleSelect}>
        <option value="Search">Search</option>
        <option value="Spotify">Add Spotify Playlist</option>
        <option value="PlaylistControls">Playlist Manager</option>
      </select>
      {selected === selection.Spotify ? (
        <Spotify
          sendAddSpotifyPlaylist={props.sendAddSpotifyPlaylist}
        ></Spotify>
      ) : (
        <></>
      )}
      {selected === selection.PlaylistControls ? (
        <PlaylistControls
          voteStartPlaylist={props.voteStartPlaylist}
        ></PlaylistControls>
      ) : (
        <></>
      )}
      {selected === selection.Search ? (
        <Search
          sendSearchSong={props.sendSearchSong}
          searchResults={props.searchResults}
          sendAddToQueue={props.sendAddToQueue}
          disableSearch={props.disableSearch}
        ></Search>
      ) : (
        <></>
      )}
    </div>
  );
};

export default Controlls;
