import type { FC } from "react";
import barberCut from "../../assets/images/barber-cut.png";
import barberTools from "../../assets/images/barber-tools.png";

const AboutUs: FC = () => {
  return (
    <section
      id="about-us"
      className="relative dark:bg-custom-gray text-slate-900py-12 sm:py-20 px-4 sm:px-16 overflow-hidden transition-colors duration-300"
    >
      <div className="max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-2 gap-10 sm:gap-12 items-center">
        <div className="space-y-4 sm:space-y-6 z-10 text-center lg:text-left">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-slate-500/5 dark:bg-white/5 border border-slate-900/10 dark:border-white/10 text-xs tracking-wider uppercase text-blue-600 dark:text-blue-400">
            <span className="w-2 h-2 rounded-full bg-red-500 animate-pulse" />
            Who We Are
          </div>

          <h2 className="text-3xl sm:text-5xl font-bold tracking-tight">
            About{" "}
            <span className="bg-linear-to-r from-blue-600 to-red-500 dark:from-blue-400 dark:to-red-400 bg-clip-text text-transparent">
              Barberly
            </span>
          </h2>

          <p className="text-slate-600 dark:text-gray-400 text-base sm:text-lg leading-relaxed font-light">
            We believe in the power of a great cut and the artistry that every
            barber brings to their craft.{" "}
            <strong className="font-medium">
              Barberly
            </strong>{" "}
            is more than just a platform – it is a dedicated ecosystem bridging
            clients who value master barbers with top-tier professionals.
          </p>

          <p className="text-slate-600 dark:text-gray-400 text-base sm:text-lg leading-relaxed font-light">
            We carefully curate and gather the best local shops, making finding
            and booking your next style simple, seamless, and certain. For
            barbers, we provide the tools to showcase your unique skill, manage
            your business stress-free, and easily connect with new clients.
          </p>

          <div className="pt-4 border-t border-slate-900/5 dark:border-white/5 flex flex-row items-center justify-around lg:justify-start lg:gap-12 gap-4">
            <div className="text-center lg:text-left">
              <p className="text-2xl sm:text-3xl font-bold">
                100%
              </p>
              <p className="text-[10px] sm:text-xs uppercase tracking-wider text-slate-400 dark:text-gray-500 mt-1">
                Verified Barbers
              </p>
            </div>
            <div className="h-8 w-px bg-slate-900/10 dark:bg-white/10" />
            <div className="text-center lg:text-left">
              <p className="text-2xl sm:text-3xl font-bold text-slate-900 dark:text-white">
                Easy
              </p>
              <p className="text-[10px] sm:text-xs uppercase tracking-wider text-slate-400 dark:text-gray-500 mt-1">
                Booking
              </p>
            </div>
            <div className="h-8 w-px bg-slate-900/10 dark:bg-white/10 hidden sm:block" />
            <div className="hidden sm:block">
              <p className="text-2xl sm:text-3xl font-bold text-blue-600 dark:text-blue-400 font-mono">
                #LookGoodFeelFresh
              </p>
            </div>
          </div>
          <div className="sm:hidden pt-2 text-center">
            <p className="text-xl font-bold text-blue-600 dark:text-blue-400 font-mono">
              #LookGoodFeelFresh
            </p>
          </div>
        </div>

        <div className="relative grid grid-cols-1 sm:grid-cols-2 gap-4 z-10 mt-6 lg:mt-0 mb-5">
          <div className="w-full">
            <div className="aspect-4/5 rounded-3xl overflow-hidden border border-slate-900/10 dark:border-white/10 bg-slate-500/5 dark:bg-white/5 backdrop-blur-md p-2 shadow-2xl transition-transform duration-500 hover:scale-[1.02]">
              <img
                src={barberCut}
                alt="Master cut"
                className="w-full h-full object-cover rounded-2xl"
              />
            </div>
          </div>

          <div className="w-full sm:pt-8">
            <div className="aspect-4/5 rounded-3xl overflow-hidden border border-slate-900/10 dark:border-white/10 bg-slate-500/5 dark:bg-white/5 backdrop-blur-md p-2 shadow-2xl transition-transform duration-500 hover:scale-[1.02]">
              <img
                src={barberTools}
                alt="Barber tools"
                className="w-full h-full object-cover rounded-2xl"
              />
            </div>
          </div>

          <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-16 sm:w-20 h-16 sm:h-20 bg-slate-50 dark:bg-[#0f1115] border-2 border-slate-900/10 dark:border-white/10 rounded-full flex items-center justify-center p-1 shadow-xl transition-colors duration-300 max-sm:hidden">
            <div className="w-full h-full rounded-full bg-linear-to-tr from-blue-500 via-white to-red-500 opacity-20 absolute animate-pulse" />
            <svg
              className="w-6 sm:w-8 h-6 sm:h-8 text-slate-900 dark:text-white z-10"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={1.5}
                d="M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138 3.42 3.42 0 00.806 1.946 3.42 3.42 0 010 4.438 3.42 3.42 0 00-.806 1.946 3.42 3.42 0 01-3.138 3.138 3.42 3.42 0 00-1.946.806 3.42 3.42 0 01-4.438 0 3.42 3.42 0 00-1.946-.806 3.42 3.42 0 01-3.138-3.138 3.42 3.42 0 00-.806-1.946 3.42 3.42 0 010-4.438 3.42 3.42 0 00.806-1.946 3.42 3.42 0 013.138-3.138z"
              />
            </svg>
          </div>
        </div>
      </div>
    </section>
  );
};

export default AboutUs;
