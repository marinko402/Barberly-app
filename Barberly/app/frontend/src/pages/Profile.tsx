import { type FC } from "react";
import ProfileSidebar from "../components/Profile/ProfileSidebar";
import ProfileContent from "../components/Profile/ProfileContent";
import { useLocation } from "react-router";

const Profile: FC = () => {
  const location = useLocation();
  const hash = location.hash;

  return (
    <div className="w-full h-dvh p-4 md:p-5 bg-barber-shop bg-no-repeat bg-cover bg-center overflow-hidden flex flex-col">
      <div className="absolute inset-0 bg-black/70 backdrop-blur-[2px] z-0" />

      <div className="relative z-10 pt-16 md:pt-18 flex flex-col md:flex-row justify-start gap-6 md:gap-10 w-full flex-1 h-0 overflow-hidden">
        <ProfileSidebar />
        <ProfileContent section={hash.length === 0 ? "#info" : hash} />
      </div>
    </div>
  );
};

export default Profile;
