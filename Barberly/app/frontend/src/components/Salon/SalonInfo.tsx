import { type FC } from "react";
import { FiMapPin } from "react-icons/fi";

interface SalonInfoProps {
  name?: string;
  address?: string;
  city?: string;
}

export const SalonInfo: FC<SalonInfoProps> = ({ name, address, city }) => {
  return (
    <div className="bg-white/5 border border-white/10 rounded-3xl p-6 shadow-xl backdrop-blur-md space-y-4">
      <div>
        <span className="text-xs font-bold uppercase tracking-widest text-blue-500">
          Welcome to
        </span>
        <h1 className="text-3xl font-black tracking-tight text-white mt-1">
          {name || "Barber Shop"}
        </h1>
      </div>
      <div className="space-y-3 text-sm font-medium text-gray-400">
        <div className="flex items-center gap-3">
          <FiMapPin className="text-blue-400 shrink-0 h-5 w-5" />
          <span>
            {address}, {city}
          </span>
        </div>
      </div>
    </div>
  );
};
