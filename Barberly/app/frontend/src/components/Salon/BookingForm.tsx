import { type FC, type FormEvent } from "react";
import { FiClock, FiCheckCircle } from "react-icons/fi";
import { CiUser, CiMail, CiPhone } from "react-icons/ci";
import type { Timeslot } from "../../models/Timeslot";
import TextField from "../TextField";

interface BookingFormProps {
  selectedSlot: Timeslot;
  selectedDate: string;
  firstName: string;
  setFirstName: (val: string) => void;
  lastName: string;
  setLastName: (val: string) => void;
  email: string;
  setEmail: (val: string) => void;
  phone: string;
  setPhone: (val: string) => void;
  onSubmit: (e: FormEvent) => void;
  isPending: boolean;
}

export const BookingForm: FC<BookingFormProps> = ({
  selectedSlot,
  selectedDate,
  firstName,
  setFirstName,
  lastName,
  setLastName,
  email,
  setEmail,
  phone,
  setPhone,
  onSubmit,
  isPending,
}) => {
  return (
    <form onSubmit={onSubmit} className="space-y-4">
      <div className="bg-blue-600/5 border border-blue-500/20 rounded-2xl p-4 flex items-center justify-between text-sm">
        <div className="flex items-center gap-3">
          <FiClock className="text-blue-400 h-5 w-5" />
          <div>
            <span className="font-bold text-white block">
              {selectedSlot.startTime.substring(0, 5)} ({selectedSlot.duration}{" "}
              mins)
            </span>
            <span className="text-xs text-gray-400">
              {new Date(selectedDate).toLocaleDateString("en-US", {
                day: "numeric",
                month: "long",
                year: "numeric",
              })}
            </span>
          </div>
        </div>
        <span className="text-xs font-bold uppercase text-blue-400 tracking-wider">
          Selected Slot
        </span>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-4 gap-y-1">
        <TextField
          label="First Name"
          required
          value={firstName}
          onChange={(e) => setFirstName(e.target.value)}
          placeholder="Name"
          icon={CiUser}
        />
        <TextField
          label="Last Name"
          required
          value={lastName}
          onChange={(e) => setLastName(e.target.value)}
          placeholder="Surname"
          icon={CiUser}
        />
      </div>

      <TextField
        label="Email Address"
        type="email"
        required
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        placeholder="email@example.com"
        icon={CiMail}
      />

      <TextField
        label="Phone Number"
        type="tel"
        required
        value={phone}
        onChange={(e) => setPhone(e.target.value)}
        placeholder="+381 6X XXX XXXX"
        icon={CiPhone}
      />

      <button
        type="submit"
        disabled={isPending}
        className="w-full bg-blue-600 hover:bg-blue-500 text-white font-bold text-sm tracking-wide py-3.5 rounded-xl shadow-md cursor-pointer transition-all active:scale-[0.99] disabled:opacity-50 flex items-center justify-center gap-2 mt-2"
      >
        {isPending ? (
          "Processing..."
        ) : (
          <>
            <FiCheckCircle /> Confirm Appointment
          </>
        )}
      </button>
    </form>
  );
};
