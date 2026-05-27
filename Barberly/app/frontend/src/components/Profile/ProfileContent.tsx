import { type FC, type ReactNode } from "react";
import { AlertCircle, ArrowRight } from "lucide-react";
import { toast } from "react-toastify";
import { useAuth } from "../../context/auth/useAuth";

import ProfileInfo from "./ProfileInfo";
import ChangePassword from "./ChangePassword/ChangePassword";
import Bookings from "./Bookings";
import MySalon from "./MySalon/MySalon";
import Timeslot from "./Timeslot/Timeslot";

const sections: Record<string, { component: ReactNode; roles: string[] }> = {
  "#info": { component: <ProfileInfo />, roles: ["Barber", "Admin"] },
  "#security": { component: <ChangePassword />, roles: ["Barber", "Admin"] },
  "#salon": { component: <MySalon />, roles: ["Barber"] },
  "#timeslots": { component: <Timeslot />, roles: ["Barber"] },
  "#bookings": { component: <Bookings />, roles: ["Barber"] },
};

interface ProfileContentProps {
  section: string;
}

const ProfileContent: FC<ProfileContentProps> = ({ section }) => {
  const { user, role } = useAuth();
  const config = sections[section];

  const hasSalon = role === "Barber" ? !!user?.salonId : true;

  if (!config) {
    toast.error("Invalid section");
    return (
      <div className="text-red-500 font-semibold p-4">Invalid section</div>
    );
  }

  if (!config.roles.includes(role!)) {
    return null;
  }

  const isRestrictedSection =
    section === "#timeslots" || section === "#bookings";

  return (
    <div className="w-full md:flex-1 flex flex-col gap-4 h-full overflow-hidden">
      <div className="flex flex-col gap-1 px-1 shrink-0">
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

      <div className="rounded-2xl flex flex-col justify-start items-start p-5 sm:p-8 w-full flex-1 h-0 bg-black/30 border border-white/10 backdrop-blur-xl shadow-2xl overflow-y-auto">
        {role === "Barber" && isRestrictedSection && !hasSalon ? (
          <div className="w-full flex flex-col items-center justify-center text-center py-16 px-4 max-w-md mx-auto h-full space-y-5 animate-fade-in">
            <div className="p-4 rounded-2xl bg-red-500/10 border border-red-500/20 text-red-400 shadow-xl backdrop-blur-md">
              <AlertCircle className="w-8 h-8" />
            </div>

            <div className="space-y-2">
              <h3 className="text-lg font-bold text-white tracking-wide">
                Feature Temporarily Locked
              </h3>
              <p className="text-sm text-neutral-400 leading-relaxed">
                You cannot manage your{" "}
                {section === "#timeslots" ? "timeslots" : "bookings"} until you
                are actively registered under or owning a salon workspace.
              </p>
            </div>

            <a
              href="#salon"
              className="flex items-center gap-2 px-5 py-3 rounded-xl bg-blue-600 hover:bg-blue-500 text-white font-bold text-xs uppercase tracking-widest transition-all duration-300 shadow-md active:scale-95"
            >
              Set up My Salon <ArrowRight className="w-4 h-4" />
            </a>
          </div>
        ) : (
          config.component
        )}
      </div>
    </div>
  );
};

export default ProfileContent;
