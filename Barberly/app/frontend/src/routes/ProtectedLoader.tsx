import { redirect, type LoaderFunctionArgs } from "react-router-dom";

export const ProtectedLoader = ({ request }: LoaderFunctionArgs) => {
  if (!localStorage.getItem("token")) {
    const url = new URL(request.url);
    throw redirect(`/login?from=${url.pathname}`);
  }
  return null;
};
