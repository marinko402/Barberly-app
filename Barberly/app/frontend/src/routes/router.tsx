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
      },
      {
        path: "register",
        element: <Register />,
        loader: LoginLoader,
      },
      {
        path: "barbers",
        element: <Barber />,
      },
      {
        path: "profile",
        element: <Profile />,
        loader: ProtectedLoader,
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
