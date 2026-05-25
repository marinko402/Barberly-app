import type { LoginUser, User } from "../../models/User";
import { createContext } from "react";

type AuthContextType = {
  user: User | null;
  token: string | null;
  registerUser: (user: User) => Promise<void>;
  loginUser: (user: LoginUser) => Promise<void>;
  logout: (expired?: boolean) => void;
  isLoggedIn: () => boolean;
  role: string;
  id: string;
  updateUserContext: (updatedUser: User) => void;
};

export const AuthContext = createContext<AuthContextType>(
  {} as AuthContextType,
);
