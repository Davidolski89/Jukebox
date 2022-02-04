import "../Css/popPlaylist.css";
const PopPlaylist = (props) => {
  const handleClick = function (e) {
    if (e.target.value === "Yes") {
      props.sendPlaylistVote("true");
    } else {
      props.sendPlaylistVote("false");
    }
  };

  return (
    <div id={props.popPlaylistEnabled ? "popPlaylist" : "popPlaylistHidden"}>
      <div className="popMessage">{props.voteMessage}</div>
      <div className="popDecision">
        <input
          className="btn btn-primary yesDecision"
          type="button"
          value="Yes"
          onClick={(e) => handleClick(e)}
        ></input>
        <input
          className="btn btn-danger noDecision"
          type="button"
          value="No"
          onClick={(e) => handleClick(e)}
        ></input>
      </div>
    </div>
  );
};

export default PopPlaylist;
