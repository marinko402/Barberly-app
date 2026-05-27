import { type FC } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { UserPlus } from "lucide-react";

const addBarberSchema = z.object({
  username: z.string().min(1, "Username is required"),
});

type AddBarberFormData = z.infer<typeof addBarberSchema>;

interface AddBarberPanelProps {
  isOwner: boolean;
  isPending: boolean;
  onAddBarber: (data: AddBarberFormData) => void;
}

export const AddBarberPanel: FC<AddBarberPanelProps> = ({
  isOwner,
  isPending,
  onAddBarber,
}) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<AddBarberFormData>({
    resolver: zodResolver(addBarberSchema),
  });

  const onSubmitForm = (data: AddBarberFormData) => {
    onAddBarber(data);
    reset();
  };

  return (
    <div className="flex flex-col justify-between h-fit p-6 bg-white/5 border border-white/10 rounded-2xl backdrop-blur-md">
      <form onSubmit={handleSubmit(onSubmitForm)} className="space-y-4 w-full">
        <div className="flex flex-col gap-1">
          <div className="flex items-center gap-2 text-white">
            <UserPlus className="w-4 h-4 text-red-400" />
            <h3 className="text-sm font-bold uppercase tracking-wider">
              Add Team Member
            </h3>
          </div>
          <p className="text-xs text-neutral-400">
            {isOwner
              ? "Recruit new staff by entering their system username below."
              : "Only the salon owner can recruit or add members to this staff."}
          </p>
        </div>

        <hr className="border-white/5" />

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-neutral-400 uppercase tracking-wider">
            Barber Username
          </label>
          <div className="relative flex items-center">
            <span className="absolute left-4 text-neutral-500 font-bold text-sm">
              @
            </span>
            <input
              {...register("username")}
              type="text"
              disabled={!isOwner || isPending}
              placeholder="e.g., john.barber"
              className={`w-full pl-10 pr-4 py-3 bg-black/20 border rounded-xl text-white text-sm font-medium outline-none transition-all duration-300 ${
                !!errors.username
                  ? "border-red-500/50 bg-red-500/5 focus:border-red-500"
                  : isOwner
                    ? "border-white/10 focus:border-red-400/60 focus:bg-black/40 shadow-[0_0_15px_rgba(239,68,68,0.02)]"
                    : "border-white/5 cursor-not-allowed opacity-50"
              }`}
            />
          </div>
          {errors.username && (
            <p className="text-red-500 text-xs font-medium mt-0.5 pl-1">
              {errors.username.message}
            </p>
          )}
        </div>

        <button
          type="submit"
          disabled={!isOwner || isPending}
          className="w-full flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl border border-white/10 text-white text-xs font-bold uppercase tracking-wider hover:bg-white/5 active:scale-95 transition-all duration-200 cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed"
        >
          {isPending ? "Adding..." : "Add Barber"}
        </button>
      </form>
    </div>
  );
};
