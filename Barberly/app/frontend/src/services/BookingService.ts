import type { Booking, BookingDto } from "../models/Booking";
import apiClient from "./client";

export const getAllBookings = async () => {
  const res = await apiClient.get<Booking[]>("Booking/GetAllBookings");
  return res.data;
};

export const createBooking = async (dto: BookingDto) => {
  const res = await apiClient.post<Booking>("Booking/CreateBooking", dto);
  return res.data;
};

export const deleteBooking = async (id: string) => {
  await apiClient.delete(`Booking/DeleteBooking/${id}`);
};

export const getTotalBookingsCount = async () => {
  const res = await apiClient.get<number>("Booking/GetTotalBookingsCount");
  return res.data;
};
