import type { LoginUser, User } from "../models/User";
import axios from "axios";
import { toast } from "react-toastify";
import apiClient from "./client";

const register = async (user: User) => {
  try {
    const data = await apiClient.post("api/Auth/Register", {
      userName: user.userName,
      email: user.email,
      password: user.password,
      firstName: user.firstName,
      lastName: user.lastName,
      phoneNumber: user.phoneNumber,
      dateOfBirth: user.dateOfBirth,
    });
    return data;
  } catch (e) {
    if (axios.isAxiosError(e) && e.response) {
      const message = e.response.data?.message || "Register failed";
      toast.error(message);
      console.log("Register ERROR:", message);
    } else {
      toast.error("Register failed! Please try again later.");
      console.log(e);
    }
    throw e;
  }
};

const login = async (user: LoginUser) => {
  try {
    const data = await apiClient.post("api/Auth/Login", {
      userName: user.username,
      password: user.password,
    });
    return data;
  } catch (e) {
    if (axios.isAxiosError(e) && e.response) {
      const message = e.response.data?.message || "Login failed";
      toast.error(message);
      console.log("LOGIN ERROR:", message);
    } else {
      toast.error("Login failed! Please try again later.");
      console.log(e);
    }
    throw e;
  }
};

const getUserData = async (id: string) => {
  try {
    const res = await apiClient.get(`api/Auth/GetUserData/${id}`);
    return res.data;
  } catch (e) {
    console.log("Error getting user data: ", e);
    throw e;
  }
};

export const updateUser = async (user: User) => {
  try {
    const response = await apiClient.put(`api/Auth/UpdateUser`, {
      id: user.id,
      userName: user.userName,
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      phoneNumber: user.phoneNumber,
      dateOfBirth: user.dateOfBirth,
    });
    return response;
  } catch (e) {
    if (axios.isAxiosError(e) && e.response) {
      const message = e.response.data?.message || `Update failed`;
      toast.error(message);
      console.log(e);
    } else {
      toast.error("An unexpected error occurred during update.");
      console.log(e);
      throw e;
    }
  }
};


export const forgotPassword = async (email: string) => {
  const response = await apiClient.post("api/Auth/ForgotPassword", { email });
  return response.data;
};

export const resetPassword = async (email: string, token: string, newPassword: string) => {
  const response = await apiClient.post("api/Auth/ResetPassword", { email, token, newPassword });
  return response.data;
};

export const verifyPassword = async (userId: string, oldPassword: string) => {
  const response = await apiClient.post("api/Auth/VerifyPassword", { userId, oldPassword });
  return response.data;
};

export const changePassword = async (userId: string, oldPassword: string, newPassword: string) => {
  const response = await apiClient.post("api/Auth/ChangePassword", { userId, oldPassword, newPassword });
  return response.data;
};

export { register, login, getUserData };