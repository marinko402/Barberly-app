import { createBrowserRouter } from "react-router-dom";
import Layout from "./Layout";
import Home from "../pages/Home";
import NotFound from "../pages/NotFound";
import Login from "../pages/Login";
import Register from "../pages/Register";
import Profile from "../pages/Profile";
import { ProtectedLoader } from "./ProtectedLoader";
import Barber from "../pages/Barber";
import { LoginLoader } from "./LoginLoader";
import Salon from "../pages/Salon";
import { Loader } from "lucide-react";

const pageLoader = (
  <div className="w-dvw h-dvh flex justify-center items-center text-2xl gap-5">
    <p>Loading</p> <Loader className="animate-spin" />
  </div>
);

export const router = createBrowserRouter([
  {
    element: <Layout />,
    children: [
      {
        index: true,
        element: <Home />,
      },
      {
        path: "login",
        element: <Login />,
        loader: LoginLoader,
        hydrateFallbackElement: pageLoader,
      },
      {
        path: "register",
        element: <Register />,
        loader: LoginLoader,
        hydrateFallbackElement: pageLoader,
      },
      {
        path: "barbers",
        element: <Barber />,
      },
      {
        path: "profile",
        element: <Profile />,
        loader: ProtectedLoader,
        hydrateFallbackElement: pageLoader,
      },
      {
        path: "salon/:name",
        element: <Salon />,
      },
      {
        path: "*",
        element: <NotFound />,
      },
    ],
  },
]);
