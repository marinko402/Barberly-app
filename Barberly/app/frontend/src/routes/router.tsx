import { createBrowserRouter } from "react-router-dom";
import Layout from "./Layout";
import Home from "../pages/Home";
import NotFound from "../pages/NotFound";
import Login from "../pages/Login";
import Register from "../pages/Register";
import Profile from "../pages/Profile";
import { ProtectedLoader } from "./ProtectedLoader";
import Barber from "../pages/Barber";

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
      },
      {
        path: "register",
        element: <Register />,
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
        path: "*",
        element: <NotFound />,
      },
    ],
  },
]);
