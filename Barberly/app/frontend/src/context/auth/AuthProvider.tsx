import { useNavigate } from "react-router";
import type { LoginUser, User } from "../../models/User";
import {
  useCallback,
  useEffect,
  useState,
  type FC,
  type ReactNode,
} from "react";
import { toast } from "react-toastify";
import { AuthContext } from "./AuthContext";
import {
  getUserData,
  login,
  logoutApi,
  register,
} from "../../services/AuthService";
import apiClient from "../../services/client";

type Props = { children: ReactNode };

export const AuthProvider: FC<Props> = ({ children }) => {
  const navigate = useNavigate();
  const [user, setUser] = useState<User | null>(null);
  const [role, setRole] = useState<string>("");
  const [id, setId] = useState<string>("");

  const logout = useCallback(
    async (expired = false) => {
      await logoutApi();

      setUser(null);
      setRole("");
      setId("");

      if (expired) toast.info("Session expired. Please login again.");
      else toast.success("You're logged out");

      navigate("/");
    },
    [navigate],
  );

  useEffect(() => {
    const interceptor = apiClient.interceptors.response.use(
      (response) => response,
      (error) => {
        const requestUrl = error.config?.url || "";
        const isMeRoute = requestUrl.includes("Auth/Me");

        if (error.response && error.response.status === 401 && !isMeRoute) {
          logout(true);
        }
        return Promise.reject(error);
      },
    );

    return () => {
      apiClient.interceptors.response.eject(interceptor);
    };
  }, [logout]);

  useEffect(() => {
    const initializeAuth = async () => {
      try {
        const res = await apiClient.get("api/Auth/Me");
        const userData = res.data;

        if (userData) {
          const mappedUser: User = {
            id: userData.id,
            userName: userData.userName,
            email: userData.email,
            firstName: userData.firstName,
            lastName: userData.lastName,
            phoneNumber: userData.phoneNumber,
            dateOfBirth: userData.birthDate,
            salonId: userData.salonId,
            password: "placeholder",
          };

          setUser(mappedUser);
          setId(userData.id);
          setRole(userData.role || "Barber");
        }
      } catch {
        setUser(null);
      }
    };

    initializeAuth();
  }, []);

  const registerUser = async (user: User) => {
    try {
      await register(user);
    } catch (e) {
      console.log(e);
      throw e;
    }
  };

  const loginUser = async (credentials: LoginUser) => {
    try {
      const res = await login(credentials);
      const data = res?.data;

      const userId = data?.userId;
      const userRole = data?.roles?.[0] ?? "";

      setId(userId);
      setRole(userRole);

      const userData = await getUserData(userId);

      const storageData: User = {
        id: userData.id,
        userName: userData.userName,
        email: userData.email,
        firstName: userData.firstName,
        lastName: userData.lastName,
        phoneNumber: userData.phoneNumber,
        dateOfBirth: userData.birthDate,
        salonId: userData.salonId,
        password: "placeholder",
      };

      setUser(storageData);

      toast.success("Login success!");
      navigate("/profile");
    } catch (e) {
      console.log("LOGIN ERROR: " + e);
      throw e;
    }
  };

  const isLoggedIn = (): boolean => {
    return !!user;
  };

  const updateUserContext = useCallback(
    (updatedUser: {
      id: string;
      email: string;
      password: string;
      firstName: string;
      lastName: string;
      userName: string;
      birthDate: string;
      phoneNumber: string;
      salonId: string;
    }) => {
      const uu: User = {
        id: updatedUser.id,
        userName: updatedUser.userName,
        email: updatedUser.email,
        firstName: updatedUser.firstName,
        lastName: updatedUser.lastName,
        phoneNumber: updatedUser.phoneNumber,
        dateOfBirth: updatedUser.birthDate,
        salonId: updatedUser.salonId,
        password: "placeholder",
      };
      setUser(uu);
    },
    [],
  );

  return (
    <AuthContext.Provider
      value={{
        registerUser,
        loginUser,
        user,
        logout: () => logout(false),
        isLoggedIn,
        role,
        id,
        updateUserContext,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
