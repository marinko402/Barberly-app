import { type FC, type ReactNode } from "react";
import { useLocation, useNavigate } from "react-router";
import { useAuth } from "../../context/auth/useAuth";
import barberlyLogo from "../../assets/images/BarberlyLogo3.png";
import {
  CalendarDays,
  ClipboardClock,
  LogOut,
  Store,
  UserKey,
  UserPen,
  Lock,
} from "lucide-react";

type ProfileTab = {
  key: string;
  label: string;
  roles: string[];
  icon: ReactNode;
};

const profileTabs: ProfileTab[] = [
  {
    key: "#info",
    label: "Profile info",
    roles: ["Barber", "Admin"],
    icon: <UserPen className="h-5 w-5" />,
  },
  {
    key: "#security",
    label: "Change password",
    roles: ["Barber", "Admin"],
    icon: <UserKey className="h-5 w-5" />,
  },
  {
    key: "#salon",
    label: "My salon",
    roles: ["Barber"],
    icon: <Store className="h-5 w-5" />,
  },
  {
    key: "#timeslots",
    label: "Timeslots",
    roles: ["Barber"],
    icon: <CalendarDays className="h-5 w-5" />,
  },
  {
    key: "#bookings",
    label: "Bookings",
    roles: ["Barber"],
    icon: <ClipboardClock className="h-5 w-5" />,
  },
];

const ProfileSidebar: FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { logout, user, role } = useAuth();

  const hash = location.hash;

  const hasSalon = role === "Barber" ? !!user?.salonId : true;

  const visibleTabs = profileTabs.filter((tab) => tab.roles.includes(role!));

  return (
    <>
      <div className="flex md:hidden flex-col w-full bg-black/40 rounded-2xl border border-white/10 p-4 gap-4 text-white shadow-xl backdrop-blur-xl">
        <div className="flex items-center justify-between border-b border-white/5 pb-3">
          <div className="flex items-center gap-3">
            <img
              src={barberlyLogo}
              alt="logo"
              className="w-16 filter drop-shadow-md"
            />
            <div>
              <p className="text-sm font-semibold">
                {user?.firstName} {user?.lastName}
              </p>
              <p className="text-xs bg-linear-to-r from-blue-400 via-white to-red-400 bg-clip-text text-transparent">
                @{user?.userName || "username"}
              </p>
            </div>
          </div>
          <button
            onClick={() => logout(false)}
            className="p-2.5 rounded-xl bg-red-500/10 text-red-400 border border-red-500/20 active:scale-95 transition-all"
          >
            <LogOut className="h-4 w-4" />
          </button>
        </div>

        <nav className="w-full overflow-x-auto no-scrollbar">
          <ul className="flex gap-2 list-none p-0 m-0 pb-1">
            {visibleTabs.map((t) => {
              const isActive = (hash.length === 0 ? "#info" : hash) === t.key;
              const isLocked =
                !hasSalon && (t.key === "#timeslots" || t.key === "#bookings");

              return (
                <li
                  key={t.key}
                  onClick={() =>
                    navigate(
                      { hash: t.key },
                      { replace: true, state: location.state },
                    )
                  }
                  className={`px-4 py-2.5 rounded-xl cursor-pointer transition-all text-xs font-medium flex items-center gap-2 whitespace-nowrap ${
                    isActive
                      ? "bg-white/10 border border-white/20 text-white shadow-md"
                      : "text-white/60 bg-white/5 border border-transparent"
                  } ${isLocked ? "opacity-40" : ""}`}
                >
                  <span
                    className={isActive ? "text-blue-400" : "text-white/40"}
                  >
                    {isLocked ? (
                      <Lock className="h-3.5 w-3.5 text-red-400" />
                    ) : (
                      t.icon
                    )}
                  </span>
                  <span>{t.label}</span>
                </li>
              );
            })}
          </ul>
        </nav>
      </div>

      <aside className="hidden md:flex w-64 h-full bg-black/30 rounded-2xl backdrop-blur-xl border border-white/10 p-2 flex-col justify-between text-white shadow-2xl shrink-0 overflow-auto">
        <div className="flex flex-col w-full">
          <div className="p-6 flex flex-col items-center border-b border-white/5">
            <img
              src={barberlyLogo}
              alt="barberly logo"
              className="w-28 pointer-events-none filter drop-shadow-[0_8px_12px_rgba(0,0,0,0.5)]"
            />
            <p className="mt-3 text-sm font-semibold tracking-wide text-white">
              {user?.firstName} {user?.lastName}
            </p>
            <p className="mt-1 text-sm font-semibold tracking-wide bg-linear-to-r from-blue-400 via-white to-red-400 bg-clip-text text-transparent">
              @{user?.userName || "username"}
            </p>
          </div>

          <nav className="px-3 py-6 text-nowrap">
            <ul className="space-y-2 list-none p-0 m-0">
              {visibleTabs.map((t) => {
                const isActive = (hash.length === 0 ? "#info" : hash) === t.key;
                const isLocked =
                  !hasSalon &&
                  (t.key === "#timeslots" || t.key === "#bookings");

                return (
                  <li
                    key={t.key}
                    onClick={() =>
                      navigate(
                        { hash: t.key },
                        { replace: true, state: location.state },
                      )
                    }
                    className={`px-4 py-3 rounded-xl cursor-pointer transition-all duration-200 text-sm font-medium flex items-center justify-between group ${
                      isActive
                        ? "bg-white/5 border border-white/10 text-white shadow-lg"
                        : "text-white/60 hover:text-white hover:bg-white/5"
                    } ${isLocked ? "opacity-35 hover:bg-transparent" : ""}`}
                  >
                    <div className="flex items-center gap-3.5">
                      <span
                        className={`transition-colors duration-200 ${isActive ? "text-blue-400" : "text-white/40 group-hover:text-white/80"}`}
                      >
                        {t.icon}
                      </span>
                      <span
                        className={
                          isActive
                            ? "bg-linear-to-r from-blue-400 via-white to-red-300 bg-clip-text text-transparent font-bold"
                            : ""
                        }
                      >
                        {t.label}
                      </span>
                    </div>

                    {isLocked && (
                      <span className="flex items-center gap-1 text-[10px] text-red-400 font-extrabold uppercase tracking-widest bg-red-500/5 px-2 py-0.5 rounded-md border border-red-500/10">
                        <Lock className="h-2.5 w-2.5" /> Lock
                      </span>
                    )}
                  </li>
                );
              })}
            </ul>
          </nav>
        </div>

        <div className="p-3 border-t border-white/5">
          <button
            className="w-full px-4 py-3 rounded-xl cursor-pointer transition-all duration-200 text-sm font-medium flex items-center gap-3.5 text-white/50 hover:text-red-400 hover:bg-red-500/10 border border-transparent hover:border-red-500/20"
            onClick={() => logout(false)}
          >
            <LogOut className="h-5 w-5" />
            Logout
          </button>
        </div>
      </aside>
    </>
  );
};

export default ProfileSidebar;
