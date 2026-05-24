import { useNavigate } from "react-router-dom";
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
import { getUserData, login, register } from "../../services/AuthService";
import { jwtDecode } from "jwt-decode";
import type { JwtPayload } from "../../models/jwt.ts";
import axios from "axios";

type Props = { children: ReactNode };

export const AuthProvider: FC<Props> = ({ children }) => {
  const navigate = useNavigate();
  const [token, setToken] = useState<string | null>(() =>
    localStorage.getItem("token"),
  );
  const [user, setUser] = useState<User | null>(() => {
    const stored = localStorage.getItem("user");
    return stored ? JSON.parse(stored) : null;
  });
  const [isReady, setIsReady] = useState<boolean>(true);
  const [role, setRole] = useState<string>(
    () => localStorage.getItem("role") ?? "",
  );
  const [id, setId] = useState<string>(() => {
    try {
      const storedToken = localStorage.getItem("token");
      if (!storedToken) return "";
      const decoded = jwtDecode<JwtPayload>(storedToken);
      return decoded.id;
    } catch {
      localStorage.removeItem("token");
      return "";
    }
  });

  const logout = useCallback(
    (expired = false) => {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      localStorage.removeItem("role");
      localStorage.removeItem("tokenExpiresAt");

      setUser(null);
      setToken("");
      setRole("");
      setId("");

      if (expired) toast.info("Session expired. Please login again.");
      else toast.success("You're logged out");

      navigate("/");
    },
    [navigate],
  );

  useEffect(() => {
    const expiresAt = localStorage.getItem("tokenExpiresAt");
    if (!expiresAt) return;

    const timeout = Number(expiresAt) - Date.now();

    if (timeout <= 0) {
      setTimeout(() => logout(true), 0);
      return;
    }

    const timer = setTimeout(() => {
      logout(true);
    }, timeout);

    return () => clearTimeout(timer);
  }, [logout]);

  useEffect(() => {
    const interceptor = axios.interceptors.request.use(
      (config) => {
        const expiresAt = localStorage.getItem("tokenExpiresAt");

        if (expiresAt && Date.now() > Number(expiresAt)) {
          logout(true);
          return Promise.reject(new Error("Token expired"));
        }

        const token = localStorage.getItem("token");
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }

        return config;
      },
      (error) => Promise.reject(error),
    );

    return () => {
      axios.interceptors.request.eject(interceptor);
    };
  }, [logout]);

  const registerUser = async (user: User) => {
    try {
      await register(user);
      // await loginUser({ username: user.userName, password: user.password });
    } catch (e) {
      console.log(e);
      throw e;
    }
  };

  const loginUser = async (user: LoginUser) => {
    try {
      await login(user)
        .then(async (res) => {
          const data = res?.data;
          const token = data?.token;
          const decoded = jwtDecode<JwtPayload>(token);

          console.log("DECODED TOKEN", decoded);

          const expiresAt = decoded.exp * 1000;
          localStorage.setItem("tokenExpiresAt", expiresAt.toString());

          const id = decoded.id;
          setId(id);

          localStorage.setItem("token", data.token);
          setToken(data.token);

          localStorage.setItem("role", decoded.role);
          setRole(decoded.role);

          axios.defaults.headers.common["Authorization"] =
            `Bearer ${data.token}`;

          let userData: User = {} as User;
          userData = await getUserData(id);

          console.log("USER DATA", userData);

          localStorage.setItem("user", JSON.stringify(userData));
          setUser(userData);

          setIsReady(true);

          toast.success("Login success!");
          navigate("/profile");
        })
        .catch((e) => {
          console.log("LOGIN ERROR: " + e);
        });
    } catch (e) {
      console.log(e);
      throw e;
    }
  };

  const isLoggedIn = (): boolean => {
    return !!localStorage.getItem("token");
  };

  return (
    <AuthContext.Provider
      value={{
        registerUser,
        loginUser,
        user,
        token,
        logout,
        isLoggedIn,
        role,
        id,
      }}
    >
      {isReady ? children : null}
    </AuthContext.Provider>
  );
};
