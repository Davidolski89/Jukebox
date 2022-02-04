import "../Css/search.css";

const LoadingScreen = () => {
  return (
    <div id="animationContainer">
      <div id="inbetweenContainer">
        <div className="lds-dual-ring"></div>
      </div>
    </div>
  );
};

export default LoadingScreen;
