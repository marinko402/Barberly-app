import { redirect, type LoaderFunctionArgs } from "react-router-dom";
import apiClient from "../services/client";

export const ProtectedLoader = async ({ request }: LoaderFunctionArgs) => {
  try {
    await apiClient.get("api/Auth/Me");
    return null;
  } catch {
    const url = new URL(request.url);
    throw redirect(`/login?from=${url.pathname}`);
  }
};
