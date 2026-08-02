import { useState, type FC } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Lock, Check, X } from "lucide-react";
import { LuEye, LuEyeClosed } from "react-icons/lu";
import { toast } from "react-toastify";

const changeSchema = z
  .object({
    newPassword: z
      .string()
      .min(8, { message: "Password must be at least 8 characters." })
      .regex(/[a-z]/, {
        message: "Password must include at least one lowercase letter.",
      })
      .regex(/[A-Z]/, {
        message: "Password must include at least one uppercase letter.",
      })
      .regex(/[0-9]/, { message: "Password must include at least one number." })
      .regex(/[@$!%*?&#]/, {
        message: "Password must include at least one special character.",
      }),
    confirmPassword: z.string().min(1, "Please confirm your password"),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

type ChangeData = z.infer<typeof changeSchema>;

interface UpdateStepProps {
  onSubmit: (data: ChangeData) => void;
  onCancel: () => void;
  isPending: boolean;
  verifiedOldPassword: string;
}

export const UpdateStep: FC<UpdateStepProps> = ({
  onSubmit,
  onCancel,
  isPending,
  verifiedOldPassword,
}) => {
  const [showNewPassword, setShowNewPassword] = useState<boolean>(false);
  const [showConfirmPassword, setShowConfirmPassword] =
    useState<boolean>(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ChangeData>({
    resolver: zodResolver(changeSchema),
  });

  const handleFormSubmit = (data: ChangeData) => {
    if (data.newPassword === verifiedOldPassword) {
      toast.error("New password cannot be the same as the old password!");
      return;
    }
    onSubmit(data);
  };

  return (
    <form
      onSubmit={handleSubmit(handleFormSubmit)}
      className="w-full h-full flex flex-col justify-between"
    >
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-x-8 gap-y-6 w-full">
        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-neutral-400 uppercase tracking-wider">
            New Password
          </label>
          <div className="relative flex items-center group/input">
            <Lock className="absolute left-4 text-neutral-500 w-4 h-4 group-focus-within/input:text-blue-500 transition-colors pointer-events-none" />
            <input
              {...register("newPassword")}
              type={showNewPassword ? "text" : "password"}
              placeholder="e.g., ••••••••••••"
              className={`w-full pr-10 py-3 bg-black/20 border rounded-xl text-white text-sm font-medium outline-hidden transition-all duration-300 pl-12 ${
                !!errors.newPassword
                  ? "border-red-500/50 bg-red-500/5 focus:border-red-500"
                  : "border-blue-500/40 bg-black/40 focus:border-blue-500 shadow-[0_0_15px_rgba(59,130,246,0.05)]"
              }`}
            />
            <button
              type="button"
              onClick={() => setShowNewPassword(!showNewPassword)}
              className="absolute right-3 p-1 text-white/50 hover:text-white transition-colors cursor-pointer z-30 focus:outline-hidden"
            >
              {showNewPassword ? (
                <LuEye className="h-4 w-4" />
              ) : (
                <LuEyeClosed className="h-4 w-4" />
              )}
            </button>
          </div>
          {errors.newPassword && (
            <p className="text-red-500 text-xs font-medium mt-0.5 pl-1 max-w-md">
              {errors.newPassword.message}
            </p>
          )}
        </div>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-neutral-400 uppercase tracking-wider">
            Confirm New Password
          </label>
          <div className="relative flex items-center group/input">
            <Lock className="absolute left-4 text-neutral-500 w-4 h-4 group-focus-within/input:text-blue-500 transition-colors pointer-events-none" />
            <input
              {...register("confirmPassword")}
              type={showConfirmPassword ? "text" : "password"}
              placeholder="Repeat your new password"
              className={`w-full pr-10 py-3 bg-black/20 border rounded-xl text-white text-sm font-medium outline-hidden transition-all duration-300 pl-12 ${
                !!errors.confirmPassword
                  ? "border-red-500/50 bg-red-500/5 focus:border-red-500"
                  : "border-blue-500/40 bg-black/40 focus:border-blue-500 shadow-[0_0_15px_rgba(59,130,246,0.05)]"
              }`}
            />
            <button
              type="button"
              onClick={() => setShowConfirmPassword(!showConfirmPassword)}
              className="absolute right-3 p-1 text-white/50 hover:text-white transition-colors cursor-pointer z-30 focus:outline-hidden"
            >
              {showConfirmPassword ? (
                <LuEye className="h-4 w-4" />
              ) : (
                <LuEyeClosed className="h-4 w-4" />
              )}
            </button>
          </div>
          {errors.confirmPassword && (
            <p className="text-red-500 text-xs font-medium mt-0.5 pl-1">
              {errors.confirmPassword.message}
            </p>
          )}
        </div>
      </div>

      <div className="flex justify-end items-center gap-4 pt-6 border-t border-white/5 w-full mt-10">
        <button
          type="button"
          onClick={onCancel}
          disabled={isPending}
          className="flex items-center gap-2 px-5 py-2.5 rounded-xl border border-white/10 text-white text-sm font-medium hover:bg-white/5 active:scale-95 transition-all duration-200 cursor-pointer disabled:opacity-50"
        >
          <X className="w-4 h-4" /> Cancel
        </button>
        <button
          type="submit"
          disabled={isPending}
          className="flex items-center gap-2 px-6 py-2.5 rounded-xl bg-linear-to-r from-blue-400 via-white/80 to-red-400 text-custom-gray text-sm font-semibold hover:scale-105 shadow-lg active:scale-95 transition-all duration-200 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isPending ? (
            "Saving..."
          ) : (
            <>
              <Check className="w-4 h-4" /> Update Password
            </>
          )}
        </button>
      </div>
    </form>
  );
};
