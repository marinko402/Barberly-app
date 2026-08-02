import { redirect, type LoaderFunctionArgs } from "react-router-dom";
import apiClient from "../services/client";

export const LoginLoader = async ({ request }: LoaderFunctionArgs) => {
  try {
    await apiClient.get("api/Auth/Me");
    
    const url = new URL(request.url);
    const from = url.searchParams.get("from") || "/profile";
    throw redirect(from);
  } catch {
    return null;
  }
};