import {
  forwardRef,
  type ElementType,
  type FC,
  type InputHTMLAttributes,
  useId,
  useState,
} from "react";
import barberChair from "../assets/images/barberChair.png";
import barberlyLogo from "../assets/images/barberlyLogo3.png";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router-dom";
import { useMutation } from "@tanstack/react-query";
import { toast } from "react-toastify";
import { CgProfile } from "react-icons/cg";
import { CiLock, CiMail, CiUser, CiPhone, CiCalendar } from "react-icons/ci";
import { LuEye, LuEyeClosed } from "react-icons/lu";
import { useAuth } from "../context/auth/useAuth";
import { motion } from "framer-motion";

const formSchema = z
  .object({
    userName: z
      .string()
      .min(3, { message: "Username must be at least 3 characters." }),
    email: z.string().email({ message: "Invalid email address." }),
    password: z
      .string()
      .min(8, { message: "Password must be at least 8 characters." })
      .regex(/[a-z]/, {
        message: "Must include at least one lowercase letter.",
      })
      .regex(/[A-Z]/, {
        message: "Must include at least one uppercase letter.",
      })
      .regex(/[0-9]/, { message: "Must include at least one number." }),
    confirmPassword: z
      .string()
      .min(1, { message: "Confirm password is required." }),
    firstName: z.string().min(1, { message: "First name is required." }),
    lastName: z.string().min(1, { message: "Last name is required." }),
    phoneNumber: z.string().min(6, { message: "Invalid phone number." }),
    birthDate: z.string().min(1, { message: "Birth date is required." }),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords do not match.",
    path: ["confirmPassword"],
  });

type FormInputs = z.infer<typeof formSchema>;

type TextFieldProps = {
  label: string;
  error?: string;
  icon: ElementType;
  isPasswordField?: boolean;
} & InputHTMLAttributes<HTMLInputElement>;

const TextField = forwardRef<HTMLInputElement, TextFieldProps>(
  (
    {
      label,
      error,
      icon: Icon,
      type = "text",
      isPasswordField = false,
      ...props
    },
    ref,
  ) => {
    const id = useId();
    const [showPassword, setShowPassword] = useState(false);
    const inputType = isPasswordField
      ? showPassword
        ? "text"
        : "password"
      : type;

    return (
      <label htmlFor={id} className="w-full block text-left text-white/80">
        <span className="block text-white/70 font-semibold text-xs uppercase tracking-wider mb-1 pl-1">
          {label}
        </span>
        <div className="relative group/input">
          <input
            ref={ref}
            id={id}
            type={inputType}
            {...props}
            className={`
              w-full pl-10 ${isPasswordField ? "pr-10" : "pr-4"} py-2.5
              bg-[#161616]/40 text-white rounded-xl border border-white/10
              placeholder-white/25 text-sm font-medium transition-all duration-200
              focus:outline-hidden focus:border-blue-500 focus:bg-black/40
              hover:border-white/20
              ${error ? "border-red-500/50 focus:border-red-500" : ""}
            `}
          />
          {Icon && (
            <Icon className="absolute left-3.5 top-1/2 -translate-y-1/2 h-5 w-5 text-white/40 group-focus-within/input:text-blue-500 transition-colors pointer-events-none" />
          )}
          {isPasswordField && (
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute right-3 top-1/2 -translate-y-1/2 p-1 text-white/40 hover:text-white transition-colors cursor-pointer z-30 focus:outline-hidden"
            >
              {showPassword ? (
                <LuEye className="h-4 w-4" />
              ) : (
                <LuEyeClosed className="h-4 w-4" />
              )}
            </button>
          )}
        </div>
        {error && (
          <span className="block text-red-400 text-[11px] font-medium mt-1 pl-1">
            {error}
          </span>
        )}
      </label>
    );
  },
);

const Register: FC = () => {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormInputs>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      userName: "",
      email: "",
      password: "",
      confirmPassword: "",
      firstName: "",
      lastName: "",
      phoneNumber: "",
      birthDate: "",
    },
  });

  const navigate = useNavigate();
  const { registerUser } = useAuth();

  const registerMutation = useMutation({
    mutationFn: async (data: FormInputs) => {
      registerUser({
        id: "",
        email: data.email,
        password: data.password,
        firstName: data.firstName,
        lastName: data.lastName,
        userName: data.userName,
        dateOfBirth: data.birthDate,
        phoneNumber: data.phoneNumber,
      });
    },
    onSuccess: () => {
      toast.success("Registration successful!");
      navigate("/login");
    },
    onError: (err) => {
      toast.error("Registration failed!");
      console.error(err);
    },
  });

  function onSubmit(data: FormInputs) {
    registerMutation.mutate(data);
  }

  return (
    <div className="w-dvw h-dvh bg-barber-shop bg-no-repeat bg-cover bg-center flex justify-center items-center p-4 overflow-auto">
      <div className="absolute inset-0 bg-black/70 backdrop-blur-[2px] z-0" />

      <div className="relative w-full max-w-4xl bg-white/5 border border-white/10 p-6 sm:p-8 backdrop-blur-2xl rounded-3xl flex flex-col md:flex-row gap-10 lg:gap-16 items-center z-10 shadow-2xl overflow-hidden">
        <motion.div
          initial={{ opacity: 0, x: 60, scale: 0.96, rotateY: 5 }}
          animate={{ opacity: 1, x: 0, scale: 1, rotateY: 0 }}
          transition={{ duration: 0.6, ease: [0.16, 1, 0.3, 1], delay: 0.05 }}
          className="w-full flex flex-col space-y-6 text-left order-2 md:order-1"
        >
          <div className="text-end">
            <h1 className="font-playfair text-4xl sm:text-5xl font-black tracking-tight mb-2 bg-linear-to-r from-blue-400 to-red-400 bg-clip-text text-transparent">
              Barberly
            </h1>
            <h3 className="text-xl font-bold text-white tracking-wide">
              Grow Your Barber Shop
            </h3>
            <p className="text-white/50 text-xs font-light mt-0.5">
              Showcase your skill, set up your shop, and manage bookings
              stress-free.
            </p>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="w-full space-y-4">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-4 gap-y-3.5">
              <TextField
                label="First Name"
                error={errors.firstName?.message}
                icon={CiUser}
                placeholder="First name"
                {...register("firstName")}
              />
              <TextField
                label="Last Name"
                error={errors.lastName?.message}
                icon={CiUser}
                placeholder="Last name"
                {...register("lastName")}
              />
              <TextField
                label="Username"
                error={errors.userName?.message}
                icon={CgProfile}
                placeholder="Username"
                {...register("userName")}
              />
              <TextField
                label="Email"
                type="email"
                error={errors.email?.message}
                icon={CiMail}
                placeholder="Email address"
                {...register("email")}
              />
              <TextField
                label="Phone Number"
                type="tel"
                error={errors.phoneNumber?.message}
                icon={CiPhone}
                placeholder="Phone number"
                {...register("phoneNumber")}
              />
              <TextField
                label="Birth Date"
                type="date"
                error={errors.birthDate?.message}
                icon={CiCalendar}
                {...register("birthDate")}
              />
              <TextField
                label="Password"
                error={errors.password?.message}
                icon={CiLock}
                placeholder="Password"
                isPasswordField={true}
                {...register("password")}
              />
              <TextField
                label="Confirm Password"
                error={errors.confirmPassword?.message}
                icon={CiLock}
                placeholder="Confirm password"
                isPasswordField={true}
                {...register("confirmPassword")}
              />
            </div>

            <div className="pt-2">
              <div className="relative w-full rounded-xl p-0.75 overflow-hidden group/btn shadow-lg">
                <div className="absolute inset-0 rounded-2xl bg-[linear-gradient(67deg,rgba(255,255,255,0.8)_0%,rgba(59,130,246,0.8)_25%,rgba(255,255,255,0.8)_50%,rgba(239,68,68,0.8)_75%,rgba(255,255,255,0.8)_100%)] bg-size-[200%_100%] animate-[barber_4s_linear_infinite]" />
                <button
                  type="submit"
                  disabled={registerMutation.isPending}
                  className="relative z-10 w-full py-1 rounded-2xl bg-black/25 border border-white/20 hover:bg-black/35 text-2xl text-white transition-all cursor-pointer font-semibold tracking-wide backdrop-blur-md hover:scale-[1.01] active:scale-[0.99] disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Register
                </button>
              </div>
            </div>
          </form>

          <div className="text-end  text-sm text-white/60 pt-2 border-t border-white/5">
            Already have an account?{" "}
            <Link
              to="/login"
              className="text-blue-400 font-medium hover:text-blue-300 transition-colors inline-flex items-center gap-1"
            >
              Login here
            </Link>
          </div>
        </motion.div>

        <motion.div
          layoutId="shared-auth-image"
          transition={{
            type: "spring",
            stiffness: 120,
            damping: 19,
            mass: 1.2,
          }}
          className="relative w-full md:w-72 shrink-0 hidden md:block group order-1 md:order-2 self-stretch overflow-hidden rounded-2xl"
        >
          <motion.img
            initial={{ scale: 1.05 }}
            animate={{ scale: 1 }}
            transition={{ duration: 0.7 }}
            src={barberChair}
            alt="barber chair"
            className="w-full h-full min-h-125 object-cover rounded-2xl border border-white/10 shadow-lg"
          />
          <div className="absolute inset-0 bg-black/40 rounded-2xl z-10 transition-opacity group-hover:opacity-30 duration-300" />

          <motion.img
            initial={{ opacity: 0, scale: 0.8, y: 10 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            transition={{ delay: 0.15, duration: 0.5 }}
            src={barberlyLogo}
            alt="barberly logo"
            className="w-48 absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 z-20 pointer-events-none filter drop-shadow-[0_10px_15px_rgba(0,0,0,0.7)]"
          />
        </motion.div>
      </div>

      <button
        className="cursor-pointer px-4 py-2 bg-white/5 hover:bg-white/10 border border-white/10 text-white text-sm font-medium rounded-xl absolute right-6 bottom-6 hover:scale-105 active:scale-95 transition-all shadow-md backdrop-blur-md z-20"
        onClick={() => navigate("/")}
      >
        Go home
      </button>
    </div>
  );
};

export default Register;
