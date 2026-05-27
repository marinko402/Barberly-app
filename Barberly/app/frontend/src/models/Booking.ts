import type { Timeslot } from "./Timeslot";

export interface Booking {
  bookingId: string;
  timeslot?: Timeslot;
  customerFirstName: string;
  customerLastName: string;
  customerEmail: string;
  customerPhoneNumber?: string;
}

export interface BookingDto {
  timeslotId: string;
  customerFirstName: string;
  customerLastName: string;
  customerEmail: string;
  customerPhoneNumber?: string;
}
