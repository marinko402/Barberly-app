import type { Barber } from "./Barber";

export type SalonDto = {
  name: string;
  address?: string;
  city?: string;
  owner: Barber | null;
  barbers: Barber[];
};
