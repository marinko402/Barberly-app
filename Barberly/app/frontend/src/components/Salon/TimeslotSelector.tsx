import { type FC } from "react";
import { FiCalendar } from "react-icons/fi";
import type { Timeslot } from "../../models/Timeslot";

interface TimeSlotSelectorProps {
  selectedDate: string;
  onDateChange: (date: string) => void;
  slots: Timeslot[];
  isSlotsLoading: boolean;
  onSelectSlot: (slot: Timeslot) => void;
}

export const TimeslotSelector: FC<TimeSlotSelectorProps> = ({
  selectedDate,
  onDateChange,
  slots,
  isSlotsLoading,
  onSelectSlot,
}) => {
  const availableSlots = slots.filter((s) => !s.isBooked);

  return (
    <div className="space-y-6">
      <div className="space-y-1.5">
        <label className="text-xs font-bold uppercase tracking-wider text-gray-400">
          Select Date
        </label>
        <div className="relative">
          <FiCalendar className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400" />
          <input
            type="date"
            value={selectedDate}
            min={new Date().toISOString().split("T")[0]}
            onChange={(e) => onDateChange(e.target.value)}
            className="w-full bg-black/20 border border-white/10 rounded-xl py-3 pl-11 pr-4 text-sm font-semibold focus:outline-hidden focus:border-blue-500 transition-colors"
          />
        </div>
      </div>

      {isSlotsLoading ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 gap-3 animate-pulse">
          {[1, 2, 3, 4].map((n) => (
            <div key={n} className="h-14 bg-white/5 rounded-xl" />
          ))}
        </div>
      ) : availableSlots.length === 0 ? (
        <div className="text-center py-12 border border-dashed border-white/10 rounded-2xl bg-black/10">
          <p className="text-sm text-gray-500">
            All slots are taken or none are created for this day.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 gap-3 max-h-96 overflow-y-auto pr-1 no-scrollbar">
          {availableSlots.map((slot) => {
            const timeDisplay = slot.startTime.substring(0, 5);
            return (
              <button
                key={slot.timeslotId}
                onClick={() => onSelectSlot(slot)}
                className="group flex flex-col items-center justify-center py-3 px-4 bg-white/5 border border-white/10 rounded-2xl hover:border-blue-500/50 hover:bg-blue-600/5 transition-all text-center cursor-pointer active:scale-[0.98]"
              >
                <span className="font-bold text-base text-white group-hover:text-blue-400 transition-colors">
                  {timeDisplay}
                </span>
                <span className="text-[10px] text-gray-500 font-medium mt-0.5">
                  {slot.duration} min
                </span>
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
};
