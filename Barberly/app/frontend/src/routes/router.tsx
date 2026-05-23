import { createBrowserRouter } from "react-router-dom";
import Layout from "./Layout";
import Home from "../pages/Home";
import NotFound from "../pages/NotFound";

export const router = createBrowserRouter([
  {
    element: <Layout />,
    children: [
      {
        index: true,
        element: <Home />,
      },
      // {
      //   path: "login",
      //   element: <Login />,
      // },
      // {
      //   path: "register",
      //   element: <Register />,
      // },
      // {
      //   path: "profile",
      //   element: <ProfilePage />,
      //   loader: ProtectedLoader,
      // },
      {
        path: "*",
        element: <NotFound />,
      },
    ],
  },
]);
