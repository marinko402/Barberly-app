import type { FC } from "react";
import { FiClock, FiX, FiEdit2, FiTrash2 } from "react-icons/fi";

interface TimeslotItemProps {
  slot: any;
  isEditing: boolean;
  onEdit: (slot: any) => void;
  onDelete: (id: string) => void;
  onCancelBooking: (id: string) => void;
  isMutating: boolean;
}

const TimeslotItem: FC<TimeslotItemProps> = ({
  slot,
  isEditing,
  onEdit,
  onDelete,
  onCancelBooking,
  isMutating,
}) => {
  const [hours, minutes] = slot.startTime.split(":");
  const timeDisplay = `${hours}:${minutes}`;

  return (
    <div
      className={`flex items-center justify-between p-3 rounded-xl border backdrop-blur-md transition-all duration-200 ${
        slot.isBooked
          ? "bg-red-500/5 border-red-500/10 opacity-80"
          : isEditing
            ? "bg-amber-500/10 border-amber-500/40 shadow-lg shadow-amber-500/5"
            : "bg-white/5 border-white/10 hover:border-white/20"
      }`}
    >
      <div className="flex items-center gap-3">
        <div
          className={`p-2 rounded-lg ${slot.isBooked ? "bg-red-500/10 text-red-400" : "bg-blue-500/10 text-blue-400"}`}
        >
          <FiClock className="h-4 w-4" />
        </div>
        <div>
          <span className="font-bold tracking-tight text-sm block text-white">
            {timeDisplay}
          </span>
          <span className="text-[10px] text-gray-400">
            {slot.duration} mins
          </span>
        </div>
      </div>

      <div className="flex items-center gap-2">
        {slot.isBooked ? (
          <div className="flex items-center gap-2">
            <div className="text-right max-w-30 sm:max-w-50">
              <span className="text-[10px] font-bold text-red-400 block uppercase tracking-wider">
                Booked
              </span>
              <span className="text-[11px] text-gray-300 truncate block font-medium">
                {slot.customerName}
              </span>
            </div>

            <button
              onClick={() => onCancelBooking(slot.timeslotId)}
              className="p-2 text-gray-400 hover:text-red-400 rounded-lg hover:bg-red-500/10 transition-colors cursor-pointer"
              title="Cancel booking & free up slot"
            >
              <FiX className="h-3.5 w-3.5" />
            </button>
          </div>
        ) : (
          <div className="flex items-center gap-1">
            <span className="text-[9px] font-extrabold text-emerald-400 bg-emerald-500/10 border border-emerald-500/20 px-1.5 py-0.5 rounded-md uppercase tracking-wider mr-1 hidden xs:inline-block">
              Open
            </span>

            <button
              onClick={() => onEdit(slot)}
              disabled={isMutating}
              className={`p-2 rounded-lg transition-colors cursor-pointer ${
                isEditing
                  ? "text-amber-400 bg-amber-500/10"
                  : "text-gray-400 hover:text-amber-400 hover:bg-amber-500/5"
              }`}
              title="Edit timeslot"
            >
              <FiEdit2 className="h-3.5 w-3.5" />
            </button>

            <button
              onClick={() => onDelete(slot.timeslotId)}
              disabled={isMutating}
              className="p-2 text-gray-400 hover:text-red-400 rounded-lg hover:bg-red-500/5 transition-colors cursor-pointer"
              title="Delete timeslot"
            >
              <FiTrash2 className="h-3.5 w-3.5" />
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default TimeslotItem;
