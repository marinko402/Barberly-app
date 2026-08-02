import type { LoginUser, User } from "../models/User";
import axios from "axios";
import { toast } from "react-toastify";
import apiClient from "./client";

export const register = async (user: User) => {
  try {
    const data = await apiClient.post("api/Auth/Register", {
      userName: user.userName,
      email: user.email,
      password: user.password,
      firstName: user.firstName,
      lastName: user.lastName,
      phoneNumber: user.phoneNumber,
      birthDate: user.dateOfBirth,
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

export const login = async (user: LoginUser) => {
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

export const logoutApi = async () => {
  try {
    await apiClient.post("api/Auth/Logout");
  } catch (e) {
    console.log("Logout error: ", e);
  }
};

export const getUserData = async (id: string) => {
  try {
    const res = await apiClient.get(`api/Auth/GetUserData/${id}`);
    return res.data;
  } catch (e) {
    console.log("Error getting user data: ", e);
    throw e;
  }
};

export const getCurrentUser = async () => {
  try {
    const res = await apiClient.get("api/Auth/Me");
    return res.data;
  } catch (e) {
    console.log("Error getting  current user data: ", e);
    throw e;
  }
}

export const updateUser = async (user: User) => {
  try {
    const response = await apiClient.put(`api/Auth/UpdateUser`, {
      id: user.id,
      userName: user.userName,
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      phoneNumber: user.phoneNumber,
      birthDate: user.dateOfBirth,
    });
    return response.data;
  } catch (e) {
    throw e;
  }
};

export const verifyPassword = async (
  userId: string,
  passwordToCheck: string,
) => {
  try {
    const response = await apiClient.post("api/Auth/CheckPassword", {
      userId,
      password: passwordToCheck,
    });
    return response.data;
  } catch (e) {
    throw e;
  }
};

export const changePassword = async (
  userId: string,
  oldPassword: string,
  newPassword: string,
) => {
  try {
    const response = await apiClient.post("api/Auth/ChangePassword", {
      userId,
      currentPassword: oldPassword,
      newPassword: newPassword,
    });
    return response.data;
  } catch (e) {
    throw e;
  }
};
