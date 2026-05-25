import type { FC, ReactNode } from "react";
import ProfileInfo from "./ProfileInfo";
import ChangePassword from "./ChangePassword";
import Bookings from "./Bookings";
import { toast } from "react-toastify";
import { useAuth } from "../../context/auth/useAuth";
import Salon from "./Salon";

const sections: Record<string, { component: ReactNode; roles: string[] }> = {
  "#info": { component: <ProfileInfo />, roles: ["Barber", "Admin"] },
  "#security": { component: <ChangePassword />, roles: ["Barber", "Admin"] },
  "#bookings": { component: <Bookings />, roles: ["Barber"] },
  "#salon": { component: <Salon />, roles: ["Barber"] },
};

const ProfileContent: FC<{ section: string }> = ({ section }) => {
  const role = localStorage.getItem("role");
  const { user } = useAuth();

  const config = sections[section];

  if (!config) {
    toast.error("Invalid section");
    return <div className="text-error">Invalid section</div>;
  }

  if (!config.roles.includes(role!)) {
    return null;
  }

  return (
    <div className="w-full md:flex-1 flex flex-col gap-4 h-full">
      <div className="flex flex-col gap-1 px-1">
        <div className="flex flex-wrap items-center gap-2 sm:gap-3">
          <h1 className="text-2xl sm:text-3xl font-bold text-white tracking-wide">
            Welcome back, {user?.firstName}
          </h1>
          <span className="px-2.5 py-0.5 text-[10px] sm:text-xs font-semibold rounded-full bg-blue-500/10 text-blue-400 border border-blue-500/20 backdrop-blur-md uppercase tracking-wider">
            {role}
          </span>
        </div>
        <p className="text-xs sm:text-sm text-neutral-400">
          Here's what's happening with your shop today.
        </p>
      </div>

      <div className="rounded-2xl flex flex-col justify-start items-start p-5 sm:p-8 overflow-y-auto w-full flex-1 bg-black/30 border border-white/10 backdrop-blur-xl shadow-2xl min-h-87.5">
        {config.component}
      </div>
    </div>
  );
};

export default ProfileContent;
