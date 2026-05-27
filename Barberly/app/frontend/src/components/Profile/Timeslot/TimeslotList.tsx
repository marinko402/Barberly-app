import type { FC } from "react";
import { FiActivity } from "react-icons/fi";
import TimeslotItem from "./TimeslotItem";

interface TimeslotListProps {
  slots: any[];
  isLoading: boolean;
  selectedDate: string;
  editingId: string | null;
  onEdit: (slot: any) => void;
  onDelete: (id: string) => void;
  onCancelBooking: (id: string) => void;
  isMutating: boolean;
}

const TimeslotList: FC<TimeslotListProps> = ({
  slots,
  isLoading,
  selectedDate,
  editingId,
  onEdit,
  onDelete,
  onCancelBooking,
  isMutating,
}) => {
  const formattedDate = new Date(selectedDate).toLocaleDateString("en-US", {
    day: "numeric",
    month: "short",
    year: "numeric",
  });

  return (
    <div className="space-y-4 w-full">
      <div className="flex items-center justify-between border-b border-white/10 pb-3">
        <div>
          <h2 className="text-lg font-extrabold tracking-tight text-white">
            Active Schedule
          </h2>
          <p className="text-[11px] text-gray-400 font-light mt-0.5">
            Slots for {formattedDate}
          </p>
        </div>
        <div className="bg-white/5 border border-white/10 rounded-xl px-2.5 py-1 text-xs font-bold flex items-center gap-1.5 text-white shadow-xs">
          <FiActivity className="text-blue-500 animate-pulse" /> {slots.length}{" "}
          Slots
        </div>
      </div>

      {isLoading ? (
        <div className="space-y-2.5 animate-pulse">
          {[1, 2, 3].map((n) => (
            <div key={n} className="h-14 w-full bg-white/5 rounded-xl" />
          ))}
        </div>
      ) : slots.length === 0 ? (
        <div className="text-center py-12 bg-white/5 border border-dashed border-white/10 rounded-2xl">
          <p className="text-xs text-gray-400">
            No timeslots generated for this date yet. Use the quick builder
            above!
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-2.5 max-h-96 overflow-y-auto no-scrollbar pr-0.5">
          {slots.map((slot) => (
            <TimeslotItem
              key={slot.timeslotId}
              slot={slot}
              isEditing={slot.timeslotId === editingId}
              onEdit={onEdit}
              onDelete={onDelete}
              onCancelBooking={onCancelBooking}
              isMutating={isMutating}
            />
          ))}
        </div>
      )}
    </div>
  );
};

export default TimeslotList;
