import "../../Css/search.css";
import LoadingScreen from "./../LoadingScreen";
import { useEffect, useRef } from "react";

const Search = (props) => {
  const searchInput = useRef();

  useEffect(() => {}, [props.searchResults]);

  const handleEnter = (e) => {
    if (e.key === "Enter" || e.key === 13) {
      props.sendSearchSong(e.target.value);
    }
  };

  return (
    <>
      <div>
        <div className="input-group">
          <input
            placeholder="Search for a song"
            className="form-control border border-dark search"
            disabled={props.disableSearch}
            ref={searchInput}
            type="text"
            onKeyUp={handleEnter}
          />
          <button
            className="btn btn-dark"
            disabled={props.disableSearch}
            onClick={() => props.sendSearchSong(searchInput.current.value)}
          >
            search
          </button>
        </div>
      </div>

      {props.disableSearch ? (
        <LoadingScreen></LoadingScreen>
      ) : (
        <ul className="list-group">
          {props.searchResults.map((result, index) => (
            <li
              className="list-group-item searchResult"
              key={index}
              onClick={() => props.sendAddToQueue(result[3], result[4])}
            >
              {result[0] + " - " + result[2]}
              {/* {result[0] + " - " + result[1] + " " + result[2]} */}
            </li>
          ))}
        </ul>
      )}
    </>
  );
};

export default Search;
