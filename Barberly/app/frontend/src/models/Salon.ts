import type { Barber } from "./Barber";

export type Salon = {
  salonId: string;
  name: string;
  address?: string;
  city?: string;
  owner?: Barber;
  barbers: Barber[];
};
