import { useState, useEffect } from "react";
import "../Css/playlist.css";
const Playlist = (props) => {
  const [displayList, setDisplayList] = useState([]);
  useEffect(() => {
    setDisplayList(() => {
      if (props.currentQue.length === 0) return [];
      else return props.currentQue;
    });
  }, [props.currentQue]);

  const VoteNextSong = function (id) {
    props.voteNextSong(id);
  };

  return (
    <div id="playlist">
      <table className="table table-dark">
        <thead>
          <tr>
            <th scopt="col"></th>
            <th scope="col">Interpret</th>
            <th scope="col">Title</th>
            <th scope="col">Duration</th>
            <th scope="col">Vote</th>
          </tr>
        </thead>
        <tbody>
          {Array.isArray(displayList)
            ? displayList.map((song) => (
                <tr key={song[0]}>
                  <td
                    className={
                      props.currentSong[0] == song[0]
                        ? "orange playlistItem"
                        : "playlistItem"
                    }
                    onClick={() => VoteNextSong(song[0])}
                  >
                    <img
                      src={"jukebox/api/downloadmusic/DownloadCover/" + song[0]}
                    />
                  </td>
                  <td
                    className={
                      props.currentSong[0] == song[0]
                        ? "orange playlistItem"
                        : "playlistItem"
                    }
                    onClick={() => VoteNextSong(song[0])}
                  >
                    {song[1]}
                  </td>
                  <td
                    className={
                      props.currentSong[0] == song[0]
                        ? "orange playlistItem"
                        : "playlistItem"
                    }
                    onClick={() => VoteNextSong(song[0])}
                  >
                    {song[2]}
                  </td>
                  <td
                    className={props.currentSong[0] == song[0] ? "orange" : ""}
                  >
                    {song[3]}
                  </td>
                  <td
                    className={props.currentSong[0] == song[0] ? "orange" : ""}
                  >
                    {song[4] != 0 ? song[4] : ""}
                  </td>
                </tr>
              ))
            : "empty"}
        </tbody>
      </table>
    </div>
  );
};

export default Playlist;
