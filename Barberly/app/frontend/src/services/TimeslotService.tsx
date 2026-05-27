import type {
  CreateTimeslotDto,
  Timeslot,
  UpdateTimeslotDto,
} from "../models/Timeslot";
import apiClient from "./client";

export const getBarberDailySchedule = async (
  barberId: string,
  dateString: string,
) => {
  const res = await apiClient.get("Timeslot/GetBarberDailySchedule", {
    params: {
      barberId: barberId,
      date: dateString,
    },
  });
  return res.data;
};

export const createTimeslot = async (dto: CreateTimeslotDto) => {
  const res = await apiClient.post("Timeslot/CreateTimeslot", dto);
  return res.data;
};

export const deleteTimeslot = async (id: string) => {
  await apiClient.delete(`Timeslot/DeleteTimeslot/${id}`);
};

export const updateTimeslot = async (
  id: string,
  timeslotDto: UpdateTimeslotDto,
) => {
  const res = await apiClient.put(`Timeslot/UpdateTimeslot/${id}`, timeslotDto);
  return res.data;
};

export const getAllFreeTimeslots = async () => {
  const res = await apiClient.get<Timeslot[]>("Timeslot/GetAllFreeTimeslots");
  return res.data;
};

export const getAllTimeslots = async () => {
  const res = await apiClient.get<Timeslot[]>("Timeslot/GetAllTimeslots");
  return res.data;
};

export const cancelBooking = async (id: string) => {
  const res = await apiClient.put(`Timeslot/CancelBooking/${id}`);
  return res.data;
};
