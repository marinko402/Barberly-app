import { useState, type FC } from "react";
import { useAuth } from "../../../context/auth/useAuth";
import { useMutation } from "@tanstack/react-query";
import { toast } from "react-toastify";
import axios from "axios";

import { verifyPassword, changePassword } from "../../../services/AuthService";
import { VerifyStep } from "./VerifyStep";
import { UpdateStep } from "./UpdateStep";

const ChangePassword: FC = () => {
  const [step, setStep] = useState<1 | 2>(1);
  const [verifiedOldPassword, setVerifiedOldPassword] = useState("");
  const { id } = useAuth();

  const verifyMutation = useMutation({
    mutationFn: (data: { oldPassword: string }) =>
      verifyPassword(id, data.oldPassword),
    onSuccess: (_, variables) => {
      toast.success("Identity verified! Enter your new password.");
      setVerifiedOldPassword(variables.oldPassword);
      setStep(2);
    },
    onError: (err: any) => {
      const serverMessage =
        err.response?.data?.message || "Incorrect current password!";
      toast.error(serverMessage);
    },
  });

  const changeMutation = useMutation({
    mutationFn: (data: { newPassword: string }) =>
      changePassword(id, verifiedOldPassword, data.newPassword),
    onSuccess: () => {
      toast.success("Password changed successfully!");
      handleResetFlow();
    },
    onError: (err: any) => {
      if (axios.isAxiosError(err) && err.response) {
        if (Array.isArray(err.response.data)) {
          toast.error(
            err.response.data[0]?.description || "Password change failed.",
          );
        } else {
          toast.error(err.response.data?.message || "Password change failed.");
        }
      } else {
        toast.error("An error occurred while changing the password.");
      }
    },
  });

  const handleResetFlow = () => {
    setVerifiedOldPassword("");
    setStep(1);
  };

  return (
    <div className="w-full h-full flex flex-col justify-between">
      {step === 1 ? (
        <VerifyStep
          onVerify={verifyMutation.mutate}
          isPending={verifyMutation.isPending}
        />
      ) : (
        <UpdateStep
          onSubmit={changeMutation.mutate}
          onCancel={handleResetFlow}
          isPending={changeMutation.isPending}
          verifiedOldPassword={verifiedOldPassword}
        />
      )}
    </div>
  );
};

export default ChangePassword;
