import React, { useState, useRef, useEffect } from "react";
import "../../Css/playlistControls.css";
const PlaylistControls = (props) => {
  const userNameRef = useRef();
  const dateRef = useRef();
  const [date, setDate] = useState("desc");
  const [requester, setRequester] = useState("");
  const searchString = useRef();
  const [playlists, setPlaylists] = useState([]);
  const [requesters, setRequesters] = useState([]);
  const [selectChanged, setSelectChanged] = useState(false);

  useEffect(() => {
    RequestPlaylist();
  }, []);
  useEffect(() => {
    RequestRequesters();
  }, []);
  useEffect(() => {
    RequestPlaylist();
  }, [selectChanged]);
  //useEffect(() => {}, [setPlaylists]);
  const RequestRequesters = () => {
    fetch("jukebox/api/DownloadMusic/GetRequesters")
      .then(async (response) => {
        const data = await response.json();
        setRequesters(data);
        if (!response.ok) {
        }
      })
      .catch((error) => {});
  };
  const RequestPlaylist = () => {
    const requestOptions = {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        uploader: requester,
        date: date,
        searchString: searchString.current.value,
      }),
    };

    fetch("jukebox/api/DownloadMusic/GetPlaylist", requestOptions)
      .then(async (response) => {
        const data = await response.json();
        setPlaylists(data);
        if (!response.ok) {
        }
      })
      .catch((error) => {});
  };

  const handleDate = (e) => {
    //e.target.defaultValue = e.target.value;
    setDate(e.target.value);
    selectChanged ? setSelectChanged(false) : setSelectChanged(true);
    //RequestPlaylist();
  };
  const handleRequester = (e) => {
    setRequester(e.target.value);
    selectChanged ? setSelectChanged(false) : setSelectChanged(true);
    //RequestPlaylist();
  };
  const handleSearchInput = (e) => {
    if (e.key === "Enter" || e.key === 13) {
      selectChanged ? setSelectChanged(false) : setSelectChanged(true);
    }
  };
  const voteStartPlaylist = (playlistId) => {
    props.voteStartPlaylist(playlistId);
  };

  return (
    <>
      <div className="form-row controltop">
        <div className="form-group col">
          <label className="text-light">Date</label>
          <select
            defaultValue={date}
            className="form-control"
            onChange={(e) => handleDate(e)}
          >
            <option value="desc">Descending</option>
            <option value="asc">Ascending</option>
          </select>
        </div>
        <div className="form-group col">
          <label className="text-light">Requester</label>
          <select className="form-control" onChange={(e) => handleRequester(e)}>
            <option value="">All</option>
            {requesters.map((x, index) => (
              <option key={index} value={x}>
                {x}
              </option>
            ))}
          </select>
        </div>
        <div className="form-group col">
          <label className="text-light">Playlist Name</label>
          <input
            ref={searchString}
            className="form-control"
            onKeyUp={(e) => handleSearchInput(e)}
            type="text"
          ></input>
        </div>
      </div>
      <div className="table-wrapper">
        <table className="table table-striped table-dark">
          <thead>
            <tr>
              <th>Name</th>
              <th>Requester</th>
              <th>Date</th>
            </tr>
          </thead>
          <tbody>
            {playlists.map((x) => (
              <tr key={x[0]} onClick={() => voteStartPlaylist(x[0])}>
                <th scope="row">{x[1]}</th>
                <td scope="row">{x[2]}</td>
                <td scope="row">{x[3]}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
};

export default PlaylistControls;
