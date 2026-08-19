import { redirect, type LoaderFunctionArgs } from "react-router";
import apiClient from "../services/client";

export const LoginLoader = async ({ request }: LoaderFunctionArgs) => {
  try {
    await apiClient.get("api/Auth/Me");
  } catch {
    return null;
  }

  const url = new URL(request.url);
  return redirect(`/profile?from=${url.pathname}`);
};
