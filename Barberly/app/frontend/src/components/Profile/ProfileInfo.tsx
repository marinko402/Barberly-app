import { useState, type FC } from "react";
import { useAuth } from "../../context/auth/useAuth";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "react-toastify";
import { updateUser } from "../../services/AuthService";
import { useMutation } from "@tanstack/react-query";
import { User, Phone, Mail, Calendar, Edit2, Check, X } from "lucide-react";

const formSchema = z.object({
  email: z.string().email("Invalid email"),
  username: z.string().min(3, "Username must be at least 3 characters"),
  name: z.string().min(1, "Name is required"),
  surname: z.string().min(1, "Surname is required"),
  birthDate: z.string().min(1, "Birth date is required"),
  phoneNumber: z
    .string()
    .regex(/^\+[1-9]\d{6,14}$/, "Invalid phone number (format: +381...)"),
});

type FormData = z.infer<typeof formSchema>;

const ProfileInfo: FC = () => {
  const [isEditing, setIsEditing] = useState(false);
  const { user, id, updateUserContext } = useAuth();

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<FormData>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      email: user?.email || "",
      username: user?.userName || "",
      name: user?.firstName || "",
      surname: user?.lastName || "",
      birthDate: user?.dateOfBirth || "",
      phoneNumber: user?.phoneNumber || "",
    },
  });

  const onSubmit = async (data: FormData) => {
    updateUserDataMutation.mutate(data);
  };

  const updateUserDataMutation = useMutation({
    mutationFn: (data: FormData) =>
      updateUser({
        id: id,
        userName: data.username,
        email: data.email,
        firstName: data.name,
        lastName: data.surname,
        phoneNumber: data.phoneNumber,
        dateOfBirth: data.birthDate,
        password: "placeholder",
      }),
    onSuccess: (updatedUser) => {
      localStorage.setItem("user", JSON.stringify(updatedUser));
      updateUserContext(updatedUser);

      toast.success("Profile updated successfully!");
      setIsEditing(false);
    },
    onError: (err: any) => {
      const serverMessage =
        err.response?.data?.message || "Error while updating profile!";
      toast.error(serverMessage);
      console.error(err);
    },
  });

  const cancelEdit = () => {
    reset();
    setIsEditing(false);
  };

  const fields = [
    { id: "name", label: "First Name", type: "text", icon: User },
    { id: "surname", label: "Last Name", type: "text", icon: User },
    {
      id: "username",
      label: "Username",
      type: "text",
      icon: null,
      isUsername: true,
    },
    { id: "email", label: "Email Address", type: "text", icon: Mail },
    { id: "phoneNumber", label: "Phone Number", type: "text", icon: Phone },
    { id: "birthDate", label: "Birth Date", type: "date", icon: Calendar },
  ];

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="w-full h-full flex flex-col justify-between"
    >
      <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-6 w-full">
        {fields.map((field) => {
          const Icon = field.icon;
          const hasError = !!errors[field.id as keyof FormData];

          return (
            <div key={field.id} className="flex flex-col gap-1.5">
              <label className="text-xs font-bold text-neutral-400 uppercase tracking-wider">
                {field.label}
              </label>

              <div className="relative flex items-center">
                {field.isUsername ? (
                  <span className="absolute left-4 text-neutral-500 font-bold text-sm">
                    @
                  </span>
                ) : (
                  Icon && (
                    <Icon className="absolute left-4 text-neutral-500 w-4 h-4" />
                  )
                )}

                <input
                  {...register(field.id as keyof FormData)}
                  type={field.type}
                  disabled={!isEditing}
                  className={`w-full pr-4 py-3 bg-black/20 border rounded-xl text-white text-sm font-medium outline-none transition-all duration-300 ${
                    field.isUsername ? "pl-10" : "pl-12"
                  } ${
                    hasError
                      ? "border-red-500/50 bg-red-500/5 focus:border-red-500"
                      : isEditing
                        ? "border-blue-500/40 bg-black/40 focus:border-blue-500 shadow-[0_0_15px_rgba(59,130,246,0.05)]"
                        : "border-white/5 cursor-not-allowed opacity-60"
                  } ${field.type === "date" ? "scheme-dark" : ""}`}
                />
              </div>

              {hasError && (
                <p className="text-red-500 text-xs font-medium mt-0.5 pl-1">
                  {errors[field.id as keyof FormData]?.message}
                </p>
              )}
            </div>
          );
        })}
      </div>

      <div className="flex justify-end items-center gap-4 pt-6 border-t border-white/5 w-full mt-10">
        {!isEditing ? (
          <button
            type="button"
            onClick={() => setIsEditing(true)}
            className="flex items-center gap-2 px-6 py-2.5 rounded-xl bg-white/5 hover:bg-white/10 text-white border border-white/10 text-sm font-medium active:scale-95 transition-all duration-200 cursor-pointer"
          >
            <Edit2 className="w-4 h-4 text-blue-400" /> Edit Profile
          </button>
        ) : (
          <>
            <button
              type="button"
              onClick={cancelEdit}
              className="flex items-center gap-2 px-5 py-2.5 rounded-xl border border-white/10 text-white text-sm font-medium hover:bg-white/5 active:scale-95 transition-all duration-200 cursor-pointer"
            >
              <X className="w-4 h-4" /> Cancel
            </button>
            <button
              type="submit"
              disabled={updateUserDataMutation.isPending}
              className="flex items-center gap-2 px-6 py-2.5 rounded-xl bg-linear-to-r from-blue-400 via-white/80 to-red-400 text-custom-gray text-sm font-semibold hover:scale-105 shadow-lg active:scale-95 transition-all duration-200 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {updateUserDataMutation.isPending ? (
                "Saving..."
              ) : (
                <>
                  <Check className="w-4 h-4" /> Save Changes
                </>
              )}
            </button>
          </>
        )}
      </div>
    </form>
  );
};

export default ProfileInfo;
