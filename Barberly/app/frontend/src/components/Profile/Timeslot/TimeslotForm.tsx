import type { FC } from "react";
import { useState, useEffect } from "react";
import {
  FiCalendar,
  FiClock,
  FiPlus,
  FiEdit2,
  FiX,
  FiCheck,
  FiAlertCircle,
} from "react-icons/fi";

interface TimeslotFormProps {
  selectedDate: string;
  setSelectedDate: (date: string) => void;
  editingSlot: any | null;
  onCancelEdit: () => void;
  onSubmit: (data: {
    date: string;
    startTime: string;
    duration: number;
  }) => void;
  isPending: boolean;
  customError: string | null;
}

const TimeslotForm: FC<TimeslotFormProps> = ({
  selectedDate,
  setSelectedDate,
  editingSlot,
  onCancelEdit,
  onSubmit,
  isPending,
  customError,
}) => {
  const [startTime, setStartTime] = useState<string>("09:00");
  const [duration, setDuration] = useState<number>(30);

  useEffect(() => {
    if (editingSlot) {
      const [hours, minutes] = editingSlot.startTime.split(":");
      setStartTime(`${hours}:${minutes}`);
      setDuration(editingSlot.duration);
    } else {
      setStartTime("09:00");
      setDuration(30);
    }
  }, [editingSlot]);

  const handleFormSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit({ date: selectedDate, startTime, duration });
  };

  return (
    <div className="w-full bg-white/5 border border-white/10 rounded-2xl p-4 sm:p-5 shadow-xl backdrop-blur-md">
      <form onSubmit={handleFormSubmit} className="flex flex-col gap-4">
        <div className="flex items-center justify-between border-b border-white/5 pb-2">
          <div className="flex items-center gap-2">
            <div
              className={`p-1.5 rounded-lg ${editingSlot ? "bg-amber-500/20 text-amber-400" : "bg-blue-500/20 text-blue-400"}`}
            >
              {editingSlot ? (
                <FiEdit2 className="w-4 h-4" />
              ) : (
                <FiPlus className="w-4 h-4" />
              )}
            </div>
            <h3 className="text-sm font-bold tracking-wide uppercase">
              {editingSlot ? "Modify Selected Slot" : "Quick Slot Generator"}
            </h3>
          </div>

          {editingSlot && (
            <button
              type="button"
              onClick={onCancelEdit}
              className="flex items-center gap-1 text-[11px] font-semibold text-gray-400 hover:text-white bg-white/5 px-2 py-1 rounded-lg transition-colors cursor-pointer"
            >
              <FiX className="w-3 h-3" /> Cancel Edit
            </button>
          )}
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div className="relative flex items-center">
            <FiCalendar className="absolute left-3.5 text-gray-400 pointer-events-none z-10 w-4 h-4" />
            <input
              type="date"
              value={selectedDate}
              onChange={(e) => setSelectedDate(e.target.value)}
              className="w-full bg-black/30 border border-white/10 rounded-xl py-2.5 pl-10 pr-3 text-xs font-semibold text-white focus:outline-hidden focus:border-blue-500 transition-colors [&::-webkit-calendar-picker-indicator]:opacity-30 [&::-webkit-calendar-picker-indicator]:invert"
              required
            />
          </div>

          <div className="relative flex items-center">
            <FiClock className="absolute left-3.5 text-gray-400 pointer-events-none z-10 w-4 h-4" />
            <input
              type="time"
              value={startTime}
              onChange={(e) => setStartTime(e.target.value)}
              className="w-full bg-black/30 border border-white/10 rounded-xl py-2.5 pl-10 pr-3 text-xs font-semibold text-white focus:outline-hidden focus:border-blue-500 transition-colors [&::-webkit-calendar-picker-indicator]:opacity-0"
              required
            />
          </div>

          <div className="relative">
            <select
              value={duration}
              onChange={(e) => setDuration(Number(e.target.value))}
              className="w-full bg-black/30 border border-white/10 rounded-xl py-2.5 px-3.5 text-xs font-semibold focus:outline-hidden focus:border-blue-500 transition-colors h-10 text-white appearance-none"
            >
              <option value={15} className="bg-[#12141c]">
                15 min
              </option>
              <option value={30} className="bg-[#12141c]">
                30 min
              </option>
              <option value={45} className="bg-[#12141c]">
                45 min
              </option>
              <option value={60} className="bg-[#12141c]">
                60 min
              </option>
              <option value={90} className="bg-[#12141c]">
                90 min
              </option>
            </select>
            <div className="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none text-gray-400 text-[10px]">
              ▼
            </div>
          </div>
        </div>

        {customError && (
          <div className="flex items-center gap-2 text-[11px] font-semibold bg-red-500/10 text-red-400 border border-red-500/20 p-2.5 rounded-xl animate-fade-in">
            <FiAlertCircle className="shrink-0 h-3.5 w-3.5" />
            <span>{customError}</span>
          </div>
        )}

        <button
          type="submit"
          disabled={isPending}
          className={`w-full text-white font-bold text-xs uppercase tracking-widest py-3 rounded-xl shadow-md transition-all active:scale-[0.99] disabled:opacity-50 flex items-center justify-center gap-2 cursor-pointer ${
            editingSlot
              ? "bg-amber-600 hover:bg-amber-500 shadow-amber-600/10"
              : "bg-blue-600 hover:bg-blue-500 shadow-blue-600/10"
          }`}
        >
          {isPending ? (
            "Processing..."
          ) : editingSlot ? (
            <>
              <FiCheck /> Save Changes
            </>
          ) : (
            <>
              <FiPlus /> Create Timeslot
            </>
          )}
        </button>
      </form>
    </div>
  );
};

export default TimeslotForm;
