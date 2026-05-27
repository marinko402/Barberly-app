import { type FC } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Store, MapPin, Shield, Edit2, Check, X, Trash2 } from "lucide-react";
import type { Salon } from "../../../models/Salon";

const salonSchema = z.object({
  name: z.string().min(2, "Salon name must be at least 2 characters"),
  address: z.string().min(1, "Address is required"),
  city: z.string().min(1, "City is required"),
});

type SalonFormData = z.infer<typeof salonSchema>;

interface SalonFormProps {
  mySalon?: Salon;
  isOwner: boolean;
  isEditing: boolean;
  setIsEditing: (val: boolean) => void;
  isPending: boolean;
  onSubmit: (data: SalonFormData) => void;
  onDelete: (id: string) => void;
  isPendingDelete: boolean;
}

export const SalonForm: FC<SalonFormProps> = ({
  mySalon,
  isOwner,
  isEditing,
  setIsEditing,
  isPending,
  onSubmit,
  onDelete,
  isPendingDelete,
}) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<SalonFormData>({
    resolver: zodResolver(salonSchema),
    values: mySalon
      ? {
          name: mySalon.name,
          address: mySalon.address || "",
          city: mySalon.city || "",
        }
      : undefined,
  });

  const salonFields = [
    {
      id: "name",
      label: "Salon Name",
      type: "text",
      icon: Store,
      placeholder: "e.g., The Gentleman's Club",
    },
    {
      id: "address",
      label: "Address",
      type: "text",
      icon: MapPin,
      placeholder: "e.g., Knez Mihailova 21",
    },
    {
      id: "city",
      label: "City",
      type: "text",
      icon: MapPin,
      placeholder: "e.g., Belgrade",
    },
  ];

  const handleCancel = () => {
    reset();
    setIsEditing(false);
  };

  const showEditableStyles = !mySalon || isEditing;

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="w-full flex flex-col justify-between h-full"
    >
      <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-6 w-full">
        {!mySalon && (
          <div className="col-span-1 md:col-span-2 mb-2">
            <h2 className="text-lg font-bold tracking-wide text-white">
              Register New Salon
            </h2>
            <p className="text-xs text-neutral-400 mt-0.5">
              You are currently not assigned to any salon. Please create one to
              set up your workspace.
            </p>
          </div>
        )}

        {salonFields.map((field) => {
          const Icon = field.icon;
          const hasError = !!errors[field.id as keyof SalonFormData];

          return (
            <div key={field.id} className="flex flex-col gap-1.5">
              <label className="text-xs font-bold text-neutral-400 uppercase tracking-wider">
                {field.label}
              </label>
              <div className="relative flex items-center">
                <Icon className="absolute left-4 text-neutral-500 w-4 h-4" />
                <input
                  {...register(field.id as keyof SalonFormData)}
                  type={field.type}
                  disabled={mySalon && !isEditing}
                  placeholder={field.placeholder}
                  className={`w-full pl-12 pr-4 py-3 bg-black/20 border rounded-xl text-white text-sm font-medium outline-none transition-all duration-300 ${
                    hasError
                      ? "border-red-500/50 bg-red-500/5 focus:border-red-500"
                      : showEditableStyles
                        ? "border-blue-500/40 bg-black/40 focus:border-blue-500 shadow-[0_0_15px_rgba(59,130,246,0.05)]"
                        : "border-white/5 cursor-not-allowed opacity-60"
                  }`}
                />
              </div>
              {hasError && (
                <p className="text-red-500 text-xs font-medium mt-0.5 pl-1">
                  {errors[field.id as keyof SalonFormData]?.message}
                </p>
              )}
            </div>
          );
        })}

        {mySalon && (
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-bold text-neutral-400 uppercase tracking-wider">
              Owner
            </label>
            <div className="relative flex items-center">
              <Shield className="absolute left-4 text-neutral-500 w-4 h-4" />
              <input
                type="text"
                disabled
                value={
                  mySalon.owner
                    ? `${mySalon.owner.firstName} ${mySalon.owner.lastName} (@${mySalon.owner.userName})`
                    : "No Owner Assigned"
                }
                className="w-full pl-12 pr-4 py-3 bg-black/20 border border-white/5 rounded-xl text-white text-sm font-medium opacity-60 cursor-not-allowed"
              />
            </div>
          </div>
        )}
      </div>

      <div className="flex justify-end items-center gap-4 pt-6 border-t border-white/5 w-full mt-10">
        {!mySalon ? (
          <button
            type="submit"
            disabled={isPending}
            className="flex items-center gap-2 px-6 py-2.5 rounded-xl bg-linear-to-r from-blue-400 via-white/80 to-red-400 text-custom-gray text-sm font-semibold hover:scale-105 shadow-lg active:scale-95 transition-all duration-200 cursor-pointer disabled:opacity-50"
          >
            {isPending ? "Creating..." : "Launch Salon"}
          </button>
        ) : (
          isOwner &&
          (!isEditing ? (
            <div className="flex items-center gap-4 w-full justify-end">
              <button
                type="button"
                onClick={() => setIsEditing(true)}
                className="flex items-center gap-2 px-6 py-2.5 rounded-xl bg-white/5 hover:bg-white/10 text-white border border-white/10 text-sm font-medium active:scale-95 transition-all duration-200 cursor-pointer"
              >
                <Edit2 className="w-4 h-4 text-blue-400" /> Edit Salon
              </button>
              <button
                type="button"
                disabled={isPendingDelete || !mySalon}
                className="flex items-center gap-2 px-6 py-2.5 rounded-xl bg-red-500/20 hover:bg-red-500/30 text-white border border-white/10 text-sm font-medium active:scale-95 transition-all duration-200 cursor-pointer disabled:opacity-50 disabled:curser-not-allowed"
                onClick={() => mySalon && onDelete(mySalon.salonId)}
              >
                <Trash2 className="w-4 h-4 text-red-400" />
                Delete Salon
              </button>
            </div>
          ) : (
            <>
              <button
                type="button"
                onClick={handleCancel}
                className="flex items-center gap-2 px-5 py-2.5 rounded-xl border border-white/10 text-white text-sm font-medium hover:bg-white/5 active:scale-95 transition-all duration-200 cursor-pointer"
              >
                <X className="w-4 h-4" /> Cancel
              </button>
              <button
                type="submit"
                disabled={isPending}
                className="flex items-center gap-2 px-6 py-2.5 rounded-xl bg-linear-to-r from-blue-400 via-white/80 to-red-400 text-custom-gray text-sm font-semibold hover:scale-105 shadow-lg active:scale-95 transition-all duration-200 cursor-pointer disabled:opacity-50"
              >
                {isPending ? (
                  "Saving..."
                ) : (
                  <>
                    <Check className="w-4 h-4" /> Save Changes
                  </>
                )}
              </button>
            </>
          ))
        )}
      </div>
    </form>
  );
};
