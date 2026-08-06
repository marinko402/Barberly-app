import { useEffect, type FC } from "react";
import HomeStart from "../components/HomeScreen/HomeStart";
import { useLocation } from "react-router";
import { HomeFindBarber } from "../components/HomeScreen/HomeFindBarber";
import AboutUs from "../components/HomeScreen/AboutUs";

const Home: FC = () => {
  const location = useLocation();

  useEffect(() => {
    if (location.hash) {
      const element = document.getElementById(location.hash.substring(1));
      if (element) {
        element.scrollIntoView({ behavior: "smooth", block: "start" });
      }
    }
  }, [location.hash]);

  return (
    <>
      <HomeStart />
      <HomeFindBarber />
      <AboutUs />
    </>
  );
};

export default Home;
