import { useState, type FC } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Lock, ArrowRight } from "lucide-react";
import { LuEye, LuEyeClosed } from "react-icons/lu";

const verifySchema = z.object({
  oldPassword: z.string().min(1, "Current password is required"),
});

type VerifyData = z.infer<typeof verifySchema>;

interface VerifyStepProps {
  onVerify: (data: VerifyData) => void;
  isPending: boolean;
}

export const VerifyStep: FC<VerifyStepProps> = ({ onVerify, isPending }) => {
  const [showPassword, setShowPassword] = useState<boolean>(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<VerifyData>({
    resolver: zodResolver(verifySchema),
  });

  return (
    <form
      onSubmit={handleSubmit(onVerify)}
      className="w-full h-full flex flex-col justify-between"
    >
      <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-6 w-full">
        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-neutral-400 uppercase tracking-wider">
            Current Password
          </label>
          <div className="relative flex items-center group/input">
            <Lock className="absolute left-4 text-neutral-500 w-4 h-4 group-focus-within/input:text-blue-500 transition-colors pointer-events-none" />
            <input
              {...register("oldPassword")}
              type={showPassword ? "text" : "password"}
              placeholder="Enter your current password to verify identity"
              className={`w-full pr-10 py-3 bg-black/20 border rounded-xl text-white text-sm font-medium outline-hidden transition-all duration-300 pl-12 ${
                !!errors.oldPassword
                  ? "border-red-500/50 bg-red-500/5 focus:border-red-500"
                  : "border-white/5 bg-black/40 focus:border-blue-500 shadow-[0_0_15px_rgba(59,130,246,0.05)]"
              }`}
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute right-3 p-1 text-white/50 hover:text-white transition-colors cursor-pointer z-30 focus:outline-hidden"
            >
              {showPassword ? (
                <LuEye className="h-4 w-4" />
              ) : (
                <LuEyeClosed className="h-4 w-4" />
              )}
            </button>
          </div>
          {errors.oldPassword && (
            <p className="text-red-500 text-xs font-medium mt-0.5 pl-1">
              {errors.oldPassword.message}
            </p>
          )}
        </div>
      </div>

      <div className="flex justify-end items-center gap-4 pt-6 border-t border-white/5 w-full mt-10">
        <button
          type="submit"
          disabled={isPending}
          className="flex items-center gap-2 px-6 py-2.5 rounded-xl bg-white/5 hover:bg-white/10 text-white border border-white/10 text-sm font-medium active:scale-95 transition-all duration-200 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isPending ? (
            "Verifying..."
          ) : (
            <>
              Verify Password <ArrowRight className="w-4 h-4 text-blue-400" />
            </>
          )}
        </button>
      </div>
    </form>
  );
};
