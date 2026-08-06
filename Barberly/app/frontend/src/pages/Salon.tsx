import { type FC, useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useLocation } from "react-router";
import { FiCalendar, FiArrowLeft } from "react-icons/fi";
import { toast } from "react-toastify";
import { getSalonById } from "../services/SalonService";
import { createBooking } from "../services/BookingService";
import { getBarberDailySchedule } from "../services/TimeslotService";
import type { Timeslot } from "../models/Timeslot";
import type { Barber } from "../models/Barber";
import { SalonInfo } from "../components/Salon/SalonInfo";
import { BarberSelector } from "../components/Salon/BarberSelector";
import { BookingForm } from "../components/Salon/BookingForm";
import { TimeslotSelector } from "../components/Salon/TimeslotSelector";

const Salon: FC = () => {
  const queryClient = useQueryClient();
  const location = useLocation();
  const salonId = location.state?.salonId;
  const [selectedBarber, setSelectedBarber] = useState<Barber | null>(null);
  const [selectedSlot, setSelectedSlot] = useState<Timeslot | null>(null);
  const [selectedDate, setSelectedDate] = useState<string>(
    new Date().toISOString().split("T")[0],
  );
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  
  const { data: salon, isLoading: isSalonLoading } = useQuery({
    queryKey: ["salonDetails", salonId],
    queryFn: () => getSalonById(salonId),
    enabled: !!salonId,
  });

  const { data: slots = [], isLoading: isSlotsLoading } = useQuery<Timeslot[]>({
    queryKey: ["barberSlots", selectedBarber?.id, selectedDate],
    queryFn: () => getBarberDailySchedule(selectedBarber!.id, selectedDate),
    enabled: !!selectedBarber,
  });

  const bookingMutation = useMutation({
    mutationFn: createBooking,
    onSuccess: () => {
      toast.success("Appointment successfully booked!");
      setSelectedSlot(null);
      setFirstName("");
      setLastName("");
      setEmail("");
      setPhone("");

      queryClient.invalidateQueries({
        queryKey: ["barberSlots", selectedBarber?.id, selectedDate],
      });
    },
    onError: (err: any) => {
      toast.error(
        err.response?.data || "Something went wrong. Please try again.",
      );
    },
  });

  const handleBookingSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedSlot) return;

    bookingMutation.mutate({
      timeslotId: selectedSlot.timeslotId,
      customerFirstName: firstName,
      customerLastName: lastName,
      customerEmail: email,
      customerPhoneNumber: phone || undefined,
    });
  };

  if (isSalonLoading) {
    return (
      <div className="w-dvw h-dvw mt-20 flex justify-center items-center text-white py-12 px-6 text-center animate-pulse text-lg font-medium">
        Loading salon experience...
      </div>
    );
  }

  return (
    <div className="w-dvw h-dvh pt-25 text-white py-28 px-5 sm:px-8 bg-barber-shop bg-no-repeat bg-cover bg-center overflow-auto">
      <div className="absolute inset-0 bg-black/60 backdrop-blur-[1px] z-0" />

      <div className="relative z-10 mx-auto grid grid-cols-1 lg:grid-cols-12 gap-8 items-start max-w-7xl overflow-auto">
        <div className="lg:col-span-5 space-y-6">
          <SalonInfo
            name={salon?.name}
            address={salon?.address}
            city={salon?.city}
          />

          <BarberSelector
            barbers={salon?.barbers}
            selectedBarber={selectedBarber}
            onSelectBarber={(barber) => {
              setSelectedBarber(barber);
              setSelectedSlot(null);
            }}
          />
        </div>

        <div className="lg:col-span-7 w-full">
          {!selectedBarber ? (
            <div className="text-center py-24 bg-white/5 border border-dashed border-white/10 rounded-3xl backdrop-blur-md">
              <FiCalendar className="mx-auto h-12 w-12 text-gray-600 mb-3" />
              <p className="text-base font-semibold text-gray-400">
                Please select a barber from the left side
              </p>
              <p className="text-xs text-gray-600 mt-1">
                to view available timeslots and book an appointment.
              </p>
            </div>
          ) : (
            <div className="bg-white/5 border border-white/10 rounded-3xl p-6 shadow-xl backdrop-blur-md space-y-6">
              <div className="flex items-center justify-between border-b border-white/10 pb-4">
                <div>
                  <h2 className="text-xl font-extrabold tracking-tight">
                    {selectedSlot
                      ? "Complete Booking"
                      : `${selectedBarber.firstName}'s Schedule`}
                  </h2>
                  <p className="text-xs text-gray-500 font-light mt-0.5">
                    {selectedSlot
                      ? `Filling details for ${selectedSlot.startTime.substring(0, 5)}`
                      : "Pick a suitable time to book your service."}
                  </p>
                </div>

                {selectedSlot && (
                  <button
                    type="button"
                    onClick={() => setSelectedSlot(null)}
                    className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold text-gray-400 hover:text-white bg-white/5 hover:bg-white/10 border border-white/10 rounded-xl transition-colors cursor-pointer"
                  >
                    <FiArrowLeft className="h-3.5 w-3.5" /> Back to Slots
                  </button>
                )}
              </div>

              {!selectedSlot ? (
                <TimeslotSelector
                  selectedDate={selectedDate}
                  onDateChange={setSelectedDate}
                  slots={slots}
                  isSlotsLoading={isSlotsLoading}
                  onSelectSlot={setSelectedSlot}
                />
              ) : (
                <BookingForm
                  selectedSlot={selectedSlot}
                  selectedDate={selectedDate}
                  firstName={firstName}
                  setFirstName={setFirstName}
                  lastName={lastName}
                  setLastName={setLastName}
                  email={email}
                  setEmail={setEmail}
                  phone={phone}
                  setPhone={setPhone}
                  onSubmit={handleBookingSubmit}
                  isPending={bookingMutation.isPending}
                />
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Salon;
