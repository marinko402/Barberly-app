import type { FC } from "react";
import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuth } from "../../../context/auth/useAuth";
import { toast } from "react-toastify";
import {
  getBarberDailySchedule,
  createTimeslot,
  deleteTimeslot,
  updateTimeslot,
  cancelBooking,
} from "../../../services/TimeslotService";
import TimeslotForm from "./TimeslotForm";
import TimeslotList from "./TimeslotList";

const Timeslot: FC = () => {
  const queryClient = useQueryClient();
  const { id: barberId, user } = useAuth();

  const [selectedDate, setSelectedDate] = useState<string>(
    new Date().toISOString().split("T")[0],
  );
  const [editingSlot, setEditingSlot] = useState<any | null>(null);
  const [customError, setCustomError] = useState<string | null>(null);

  const { data: slots = [], isLoading } = useQuery<any[]>({
    queryKey: ["barberSchedule", barberId, selectedDate],
    queryFn: () => getBarberDailySchedule(barberId || "", selectedDate),
    enabled: !!barberId,
  });

  const handleMutationError = (err: any) => {
    const errorData = err.response?.data;
    if (errorData && typeof errorData === "object") {
      if (errorData.errors) {
        const firstErrorKey = Object.keys(errorData.errors)[0];
        setCustomError(errorData.errors[firstErrorKey][0]);
      } else if (errorData.title) {
        setCustomError(errorData.title);
      } else {
        setCustomError("An error occurred.");
      }
    } else {
      setCustomError(errorData || "An error occurred.");
    }
  };

  const createMutation = useMutation({
    mutationFn: createTimeslot,
    onSuccess: () => {
      setCustomError(null);
      queryClient.invalidateQueries({
        queryKey: ["barberSchedule", barberId, selectedDate],
      });
      toast.success("Timeslot successfully created!");
    },
    onError: handleMutationError,
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) =>
      updateTimeslot(id, data),
    onSuccess: () => {
      setCustomError(null);
      setEditingSlot(null);
      queryClient.invalidateQueries({
        queryKey: ["barberSchedule", barberId, selectedDate],
      });
      toast.success("Timeslot successfully updated!");
    },
    onError: handleMutationError,
  });

  const deleteMutation = useMutation({
    mutationFn: deleteTimeslot,
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["barberSchedule", barberId, selectedDate],
      });
      toast.success("Timeslot deleted.");
    },
    onError: (err: any) => {
      toast.error(err.response?.data || "Unable to delete the timeslot.");
    },
  });

  const cancelBookingMutation = useMutation({
    mutationFn: cancelBooking,
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["barberSchedule", barberId, selectedDate],
      });
      toast.success("Booking cancelled. Timeslot is now available!");
    },
    onError: (err: any) => {
      toast.error(err.response?.data || "Unable to cancel the booking.");
    },
  });

  const handleFormSubmit = (formData: {
    date: string;
    startTime: string;
    duration: number;
  }) => {
    if (!barberId) return;
    setCustomError(null);

    const payload = {
      barberId,
      date: formData.date,
      salonId: user?.salonId || barberId,
      startTime: `${formData.startTime}:00`,
      duration: formData.duration,
      isBooked: false,
    };

    if (editingSlot) {
      updateMutation.mutate({ id: editingSlot.timeslotId, data: payload });
    } else {
      createMutation.mutate(payload);
    }
  };

  const isMutating =
    createMutation.isPending ||
    updateMutation.isPending ||
    deleteMutation.isPending ||
    cancelBookingMutation.isPending;

  return (
    <div className="w-full text-white py-2 px-1 sm:px-4 transition-colors duration-300">
      <div className="flex flex-col gap-6">
        <TimeslotForm
          selectedDate={selectedDate}
          setSelectedDate={setSelectedDate}
          editingSlot={editingSlot}
          onCancelEdit={() => setEditingSlot(null)}
          onSubmit={handleFormSubmit}
          isPending={createMutation.isPending || updateMutation.isPending}
          customError={customError}
        />

        <TimeslotList
          slots={slots}
          isLoading={isLoading}
          selectedDate={selectedDate}
          editingId={editingSlot?.timeslotId || null}
          onEdit={(slot) => setEditingSlot(slot)}
          onDelete={(id) => deleteMutation.mutate(id)}
          onCancelBooking={(id) => cancelBookingMutation.mutate(id)}
          isMutating={isMutating}
        />
      </div>
    </div>
  );
};

export default Timeslot;
