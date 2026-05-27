import {
  forwardRef,
  type ElementType,
  type InputHTMLAttributes,
  useId,
  useState,
} from "react";
import { LuEye, LuEyeClosed } from "react-icons/lu";

type TextFieldProps = {
  label: string;
  error?: string;
  icon: ElementType;
  type?: string;
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
      <label htmlFor={id} className="w-full block text-left text-white/80 mb-4">
        <span className="block text-white/70 font-semibold text-xs uppercase tracking-wider mb-1.5 pl-1">
          {label}
        </span>
        <div className="relative group/input">
          <input
            ref={ref}
            id={id}
            type={inputType}
            {...props}
            className={`w-full pl-10 ${isPasswordField ? "pr-10" : "pr-4"} py-2.5  bg-[#161616]/40 text-white rounded-xl border border-white/10  placeholder-white/25 text-sm font-medium transition-all duration-200 focus:outline-hidden focus:border-blue-500 focus:bg-black/40  hover:border-white/20  ${error ? "border-red-500/50 focus:border-red-500" : ""}`}
          />
          {Icon && (
            <Icon className="absolute left-3.5 top-1/2 -translate-y-1/2 h-5 w-5 text-white/50 group-focus-within/input:text-blue-500 transition-colors pointer-events-none" />
          )}
          {isPasswordField && (
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute right-3 top-1/2 -translate-y-1/2 p-1 text-white/50 hover:text-white transition-colors cursor-pointer z-30 focus:outline-hidden"
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
          <span className="block text-red-400 text-xs font-medium mt-1.5 pl-1 animate-pulse">
            {error}
          </span>
        )}
      </label>
    );
  },
);

TextField.displayName = "TextField";

export default TextField;
