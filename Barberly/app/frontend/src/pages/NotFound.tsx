import { type FC } from "react";
import { Link } from "react-router";
import { Home, Compass } from "lucide-react";

const NotFound: FC = () => {
  return (
    <div className="w-full min-h-dvh relative flex flex-col justify-center items-center p-4 bg-barber-shop bg-no-repeat bg-cover bg-center overflow-x-hidden">
      <div className="absolute inset-0 bg-black/75 backdrop-blur-xs z-0" />

      <div className="relative z-10 flex flex-col items-center max-w-md w-full text-center px-4">
        <div className="relative mb-6 animate-pulse">
          <h1 className="text-8xl sm:text-9xl font-extrabold tracking-widest text-white/10 select-none">
            404
          </h1>
          <div className="absolute inset-0 flex items-center justify-center">
            <Compass className="w-16 h-16 sm:w-20 sm:h-20 text-blue-400/80 animate-[float_4s_ease-in-out_infinite]" />
          </div>
        </div>

        <div className="w-full p-6 sm:p-8 bg-black/30 border border-white/10 backdrop-blur-xl rounded-2xl shadow-2xl flex flex-col items-center gap-3">
          <h2
            className="text-xl sm:text-2xl font-bold text-white tracking-wide"
            role="alert"
          >
            Page Not Found
          </h2>

          <p className="text-xs sm:text-sm text-neutral-400 font-light max-w-sm leading-relaxed">
            The chair is empty, and the mirror is blank. The page you are
            looking for doesn't exist or has been moved.
          </p>
        </div>

        <div className="relative rounded-xl p-px overflow-hidden group/btn shadow-lg mt-8 w-48">
          <div className="absolute inset-0 rounded-2xl bg-[linear-gradient(67deg,rgba(255,255,255,0.8)_0%,rgba(59,130,246,0.8)_25%,rgba(255,255,255,0.8)_50%,rgba(239,68,68,0.8)_75%,rgba(255,255,255,0.8)_100%)] bg-size-[200%_100%] animate-[barber_4s_linear_infinite]" />

          <Link
            to="/"
            className="relative z-10 py-3 w-full rounded-2xl bg-black/40 border border-white/10 hover:bg-black/60 text-xs sm:text-sm text-white transition-all font-semibold tracking-wider backdrop-blur-md flex items-center justify-center gap-2 group"
          >
            <Home className="w-4 h-4 text-blue-400 transition-transform group-hover:scale-110" />
            Go Home
          </Link>
        </div>
      </div>
    </div>
  );
};

export default NotFound;
