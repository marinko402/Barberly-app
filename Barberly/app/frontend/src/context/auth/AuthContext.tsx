import type { LoginUser, User } from "../../models/User";
import { createContext } from "react";

type AuthContextType = {
  user: User | null;
  registerUser: (user: User) => Promise<void>;
  loginUser: (user: LoginUser) => Promise<void>;
  logout: (expired?: boolean) => void;
  isLoggedIn: () => boolean;
  role: string;
  id: string;
  updateUserContext: (updatedUser: {
    id: string;
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    userName: string;
    birthDate: string;
    phoneNumber: string;
    salonId: string;
  }) => void;
};

export const AuthContext = createContext<AuthContextType>(
  {} as AuthContextType,
);
