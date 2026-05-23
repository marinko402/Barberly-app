import type { FC } from "react";
import logo from "../../assets/images/barberly logo 2.png";

const HomeStart: FC = () => {
  return (
    <div className="h-screen justify-center items-center flex flex-col overflow-hidden bg-barber-shop bg-no-repeat bg-cover bg-center text-white">
      <div className="absolute inset-0 bg-black/60 backdrop-blur-[1px] z-0" />

      <div className="relative z-10 flex flex-col md:flex-row flex-1 justify-center lg:justify-between items-center gap-5 w-full">
        <div className="flex flex-col justify-center items-center text-center w-full">
          <h1 className="font-black tracking-wide leading-tight">
            FIND AND BOOK <br /> THE PERFECT CUT
          </h1>
          <h5 className="text-gray-300 mt-2 font-light tracking-wider">
            Search. Book. Look Good.
          </h5>

          <div className="relative inline-block rounded-2xl p-0.75 overflow-hidden mt-4 group">
            <div className="absolute inset-0 rounded-2xl bg-[linear-gradient(67deg,rgba(255,255,255,0.8)_0%,rgba(59,130,246,0.8)_25%,rgba(255,255,255,0.8)_50%,rgba(239,68,68,0.8)_75%,rgba(255,255,255,0.8)_100%)] bg-size-[200%_100%] animate-[barber_4s_linear_infinite]" />
            <button className="relative z-10 w-80 h-15 text-2xl max-sm:text-[1rem] max-sm:w-45 max-sm:h-10 rounded-2xl bg-white/10 border border-white/20 hover:bg-white/20 text-white font-semibold tracking-wide backdrop-blur-md hover:cursor-pointer hover:scale-[1.02] active:scale-[0.98]">
              Find Barber
            </button>
          </div>
        </div>

        <div className="flex justify-center items-center max-lg:order-first">
          <img
            src={logo}
            alt="Barberly Logo"
            className="w-150 max-md:w-50 max-md:h-40 h-auto hover:scale-105"
          />
        </div>

        <div className="flex flex-col justify-center items-center text-center w-full">
          <h1 className="font-black tracking-wide leading-tight">
            GROW YOUR <br /> BARBER SHOP
          </h1>
          <h5 className="text-gray-300 mt-2 font-light tracking-wider">
            Showcase your skill. Manage bookings.
          </h5>

          <div className="relative inline-block rounded-2xl p-0.75 overflow-hidden mt-4 group">
            <div className="absolute inset-0 rounded-2xl bg-[linear-gradient(67deg,rgba(255,255,255,0.8)_0%,rgba(59,130,246,0.8)_25%,rgba(255,255,255,0.8)_50%,rgba(239,68,68,0.8)_75%,rgba(255,255,255,0.8)_100%)] bg-size-[200%_100%] animate-[barber_4s_linear_infinite]" />
            <button className="relative z-10 w-80 h-15 text-2xl max-sm:text-[1rem] max-sm:w-45 max-sm:h-10 rounded-2xl bg-white/10 border border-white/20 hover:bg-white/20 text-white font-semibold tracking-wide backdrop-blur-md hover:cursor-pointer hover:scale-[1.02] active:scale-[0.98]">
              Join as Barber
            </button>
          </div>
        </div>
      </div>

      <div className="relative z-10 w-full max-w-2xl mb-5 flex items-center justify-center">
        <p className="text-center max-md:mx-5 text-gray-300 text-xs sm:text-sm md:text-base lg:text-lg leading-relaxed font-light px-4 bg-black/20 py-3 lg:py-4 rounded-2xl backdrop-blur-sm border border-white/5">
          <span className="font-semibold text-white">Barberly</span> connects
          clients with top barber shops and simplifies booking. Barbers can list
          their shops, manage schedules, and find new clients. Clients can
          search, compare, and book the perfect cut.
        </p>
      </div>
    </div>
  );
};

export default HomeStart;
