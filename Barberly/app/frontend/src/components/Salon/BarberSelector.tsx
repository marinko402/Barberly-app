import { type FC } from "react";
import { FiUser } from "react-icons/fi";
import type { Barber } from "../../models/Barber";

interface BarberSelectorProps {
  barbers?: Barber[];
  selectedBarber: Barber | null;
  onSelectBarber: (barber: Barber) => void;
}

export const BarberSelector: FC<BarberSelectorProps> = ({
  barbers = [],
  selectedBarber,
  onSelectBarber,
}) => {
  return (
    <div className="bg-white/5 border border-white/10 rounded-3xl p-6 shadow-xl backdrop-blur-md space-y-4">
      <div>
        <h2 className="text-xl font-extrabold tracking-tight">
          Choose Your Barber
        </h2>
        <p className="text-xs text-gray-500 font-light mt-0.5">
          Select a team member to view availability.
        </p>
      </div>

      <div className="space-y-3">
        {barbers.length > 0 ? (
          barbers.map((barber) => (
            <button
              key={barber.id}
              onClick={() => onSelectBarber(barber)}
              className={`w-full flex items-center justify-between p-4 rounded-2xl border text-left transition-all duration-200 cursor-pointer ${
                selectedBarber?.id === barber.id
                  ? "bg-blue-600/10 border-blue-500 shadow-md"
                  : "bg-black/20 border-white/10 hover:border-white/20"
              }`}
            >
              <div className="flex items-center gap-4">
                <div
                  className={`p-3 rounded-xl ${
                    selectedBarber?.id === barber.id
                      ? "bg-blue-500 text-white"
                      : "bg-white/5 text-gray-400"
                  }`}
                >
                  <FiUser className="h-5 w-5" />
                </div>
                <div>
                  <span className="font-bold text-base block text-white">
                    {barber.firstName} {barber.lastName}
                  </span>
                  <span className="text-xs text-gray-500">
                    @{barber.userName}
                  </span>
                </div>
              </div>
              {selectedBarber?.id === barber.id && (
                <span className="text-xs font-bold text-blue-400 bg-blue-500/10 border border-blue-500/20 px-2 py-1 rounded-md">
                  Selected
                </span>
              )}
            </button>
          ))
        ) : (
          <p className="text-sm text-gray-500 text-center py-4">
            No barbers found for this salon.
          </p>
        )}
      </div>
    </div>
  );
};
