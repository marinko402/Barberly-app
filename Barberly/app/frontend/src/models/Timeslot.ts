import type { Barber } from "./Barber";
import type { Salon } from "./Salon";

export type Timeslot = {
  timeslotId: string;
  date: string;
  startTime: string;
  duration: number;
  isBooked: boolean;
  salon?: Salon;
  barber?: Barber;
};

export interface CreateTimeslotDto {
  barberId: string;
  salonId: string | null;
  date: string;
  startTime: string;
  duration: number;
  isBooked?: boolean;
}

export interface UpdateTimeslotDto {
  date: string;
  startTime: string;
  duration: number;
  salonId: string;
  barberId: string;
  isBooked: boolean;
}
