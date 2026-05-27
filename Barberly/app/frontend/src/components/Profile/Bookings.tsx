import type { FC } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  FiClock,
  FiUser,
  FiMail,
  FiCheckCircle,
  FiXCircle,
  FiCalendar,
} from "react-icons/fi";
import { getBarberDailySchedule } from "../../services/TimeslotService";
import { useAuth } from "../../context/auth/useAuth";
import { Phone } from "lucide-react";

export const Bookings: FC = () => {
  const { id } = useAuth();
  const todayStr = new Date().toISOString().split("T")[0];

  const { data: slots = [], isLoading } = useQuery({
    queryKey: ["barberSchedule", id, todayStr],
    queryFn: () => getBarberDailySchedule(id, todayStr),
  });

  const formattedDate = new Date().toLocaleDateString("sr-RS", {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  return (
    <div className="w-full py-12 px-4 sm:px-16 text-white transition-colors duration-300">
      <div className=" mx-auto space-y-8">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 border-b border-white/10 pb-6">
          <div className="space-y-1">
            <h1 className="text-3xl font-extrabold tracking-tight text-white">
              Today's Schedule
            </h1>
            <p className="text-sm font-medium text-blue-400 flex items-center gap-2 capitalize">
              <FiCalendar className="shrink-0" />
              {formattedDate}
            </p>
          </div>

          <div className="bg-white/5 border border-white/10 rounded-xl px-4 py-2.5 w-fit">
            <span className="text-xs font-bold text-gray-500 uppercase tracking-wider block">
              Total Slots
            </span>
            <span className="text-xl font-black text-white">
              {slots.length}
            </span>
          </div>
        </div>

        {isLoading ? (
          <div className="space-y-4 animate-pulse">
            {[1, 2, 3].map((n) => (
              <div key={n} className="h-24 w-full bg-white/5 rounded-2xl" />
            ))}
          </div>
        ) : slots.length === 0 ? (
          <div className="text-center py-16 bg-white/5 border border-dashed border-white/10 rounded-2xl">
            <p className="text-gray-400 font-medium">
              No timeslots generated for today.
            </p>
          </div>
        ) : (
          <div className="relative border-l-2 border-white/10 ml-4 sm:ml-6 space-y-6 py-2">
            {slots.map((slot: any) => {
              const [hours, minutes] = slot.startTime.split(":");
              const timeDisplay = `${hours}:${minutes}`;

              return (
                <div
                  key={slot.timeslotId}
                  className="relative pl-6 sm:pl-8 group"
                >
                  <div
                    className={`absolute -left-2.25 top-6 h-4 w-4 rounded-full border-2 bg-slate-900 transition-all duration-300 ${
                      slot.isBooked
                        ? "border-red-500 bg-red-500 shadow-[0_0_8px_rgba(239,68,68,0.5)]"
                        : "border-blue-500 bg-slate-950"
                    }`}
                  />

                  <div
                    className={`w-full rounded-2xl border p-5 backdrop-blur-md transition-all duration-300 flex flex-col sm:flex-row sm:items-center justify-between gap-4 shadow-sm ${
                      slot.isBooked
                        ? "bg-red-500/5 border-red-500/10"
                        : "bg-white/5 border-white/10"
                    }`}
                  >
                    <div className="flex items-center gap-4">
                      <div
                        className={`p-3 rounded-xl shrink-0 ${
                          slot.isBooked
                            ? "bg-red-500/10 text-red-400"
                            : "bg-blue-500/10 text-blue-400"
                        }`}
                      >
                        <FiClock className="h-6 w-6" />
                      </div>

                      <div className="space-y-1">
                        <span className="text-xl font-bold tracking-tight text-white">
                          {timeDisplay}
                        </span>
                        <p className="text-xs text-gray-500 font-medium">
                          Duration: {slot.duration} min
                        </p>
                      </div>
                    </div>

                    <div className="flex-1 sm:max-w-md bg-black/20 rounded-xl p-3 border border-white/5">
                      {slot.isBooked ? (
                        <div className="space-y-1.5">
                          <div className="flex items-center gap-2 text-sm font-semibold text-white">
                            <FiUser className="text-gray-400 h-3.5 w-3.5" />
                            <span className="truncate">
                              {slot.customerName}
                            </span>
                          </div>
                          <div className="flex items-center gap-2 text-xs text-gray-400">
                            <FiMail className="text-gray-500 h-3.5 w-3.5" />
                            <span className="truncate">
                              {slot.customerEmail}
                            </span>
                          </div>
                          <div className="flex items-center gap-2 text-xs text-gray-400">
                            <Phone className="text-gray-500 h-3.5 w-3.5" />
                            <span className="truncate">
                              {slot.customerPhoneNumber}
                            </span>
                          </div>
                        </div>
                      ) : (
                        <div className="flex items-center gap-2 text-xs font-semibold text-gray-500 uppercase tracking-wider py-1.5 px-1">
                          Available for booking
                        </div>
                      )}
                    </div>

                    <div className="flex items-center sm:justify-end">
                      {slot.isBooked ? (
                        <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold bg-red-500/10 text-red-500 border border-red-500/20">
                          <FiXCircle className="h-3.5 w-3.5" /> Booked
                        </span>
                      ) : (
                        <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold bg-blue-400/10 text-blue-400 border border-blue-400/20">
                          <FiCheckCircle className="h-3.5 w-3.5" /> Open
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};

export default Bookings;
