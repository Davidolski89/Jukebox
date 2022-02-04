import { useEffect, useState } from "react";
import "../Css/jukeConsole.css";

const JukeConsole = (props) => {
  const [console, setConsole] = useState([]);
  useEffect(() => {
    setConsole(() => {
      const arraylol = props.consoleQue.slice();
      return arraylol;
    });
  }, [props.consoleQue]);

  return (
    <div id="jukeConsole">
      <span>Console: </span>
      <ul>
        {console.map((message) => (
          <li key={message[0]}>{message[1]}</li>
        ))}
      </ul>
    </div>
  );
};

export default JukeConsole;
