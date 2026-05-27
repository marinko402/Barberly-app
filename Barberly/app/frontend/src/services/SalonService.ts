import type { Barber } from "../models/Barber";
import type { Salon } from "../models/Salon";
import type { SalonDto } from "../models/SalonDto";
import apiClient from "./client";

export const getAllSalons = async () => {
  const res = await apiClient.get<Salon[]>("Salon/GetAllSalons");
  return res.data;
};

export const getSalonById = async (id: string) => {
  const res = await apiClient.get<Salon>(`Salon/GetSalonById/${id}`);
  return res.data;
};

export const createSalon = async (dto: SalonDto) => {
  const res = await apiClient.post("Salon/CreateSalon", dto);
  return res.data;
};

export const updateSalon = async (id: string, dto: SalonDto) => {
  const res = await apiClient.put(`Salon/UpdateSalon/${id}`, dto);
  return res.data;
};

export const addBarberToSalon = async (barberId: string, salonId: string) => {
  const res = await apiClient.put<string>("Salon/AddBarberToSalon", null, {
    params: {
      barberId,
      salonId,
    },
  });
  return res.data;
};

export const addBarberToSalonByUsername = async (
  username: string,
  salonId: string,
) => {
  const userRes = await apiClient.get<Barber>(
    `api/Auth/GetByUsername/${username}`,
  );
  const targetBarberId = userRes.data.id;

  return await addBarberToSalon(targetBarberId, salonId);
};

export const removeBarberFromSalon = async (
  barberId: string,
  salonId: string,
  ownerId: string,
) => {
  const res = await apiClient.put<string>("Salon/RemoveBarberFromSalon", null, {
    params: {
      barberId,
      salonId,
      ownerId,
    },
  });
  return res.data;
};

export const getSalonsCount = async () => {
  const res = await apiClient.get<number>("Salon/GetSalonsCount");
  return res.data;
};

export const getTotalBookingsCount = async () => {
  const res = await apiClient.get<number>("Booking/GetTotalBookingsCount");
  return res.data;
};

export const getTopSalons = async () => {
  const res = await apiClient.get<Salon[]>("Salon/GetTopSalons");
  return res.data;
};

export const deleteSalon = async (id: string) => {
  const res = await apiClient.delete(`Salon/DeleteSalon/${id}`);
  return res.data;
};