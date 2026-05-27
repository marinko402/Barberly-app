import type { FC } from "react";
import { useState, useEffect, useRef } from "react";
import { FiMapPin, FiUsers, FiCalendar } from "react-icons/fi";
import { useNavigate } from "react-router";
import { useQuery } from "@tanstack/react-query";
import {
  getSalonsCount,
  getTotalBookingsCount,
  getTopSalons,
} from "../../services/SalonService";

export const HomeFindBarber: FC = () => {
  const [currentIndex, setCurrentIndex] = useState<number>(0);
  const navigate = useNavigate();

  const sliderRef = useRef<HTMLDivElement>(null);
  const scrollContainerRef = useRef<HTMLDivElement>(null);

  const { data: salonsCount, isLoading: loadingSalons } = useQuery<number>({
    queryKey: ["salonsCount"],
    queryFn: getSalonsCount,
  });

  const { data: bookingsCount, isLoading: loadingBookings } = useQuery<number>({
    queryKey: ["bookingsCount"],
    queryFn: getTotalBookingsCount,
  });

  const { data: topSalons = [], isLoading: loadingTopSalons } = useQuery<any[]>(
    {
      queryKey: ["topSalons"],
      queryFn: getTopSalons,
    },
  );

  const scrollToContainerIndex = (index: number) => {
    const container = scrollContainerRef.current;
    if (!container) return;

    const cards = container.querySelectorAll(".barber-card");
    const targetCard = cards[index] as HTMLElement;

    if (targetCard) {
      const containerWidth = container.offsetWidth;
      const cardWidth = targetCard.offsetWidth;
      const targetScrollLeft =
        targetCard.offsetLeft - containerWidth / 2 + cardWidth / 2;

      container.scrollTo({
        left: targetScrollLeft,
        behavior: "smooth",
      });
    }
  };

  const handleDotClick = (index: number) => {
    setCurrentIndex(index);
    scrollToContainerIndex(index);
  };

  const handleScroll = () => {
    const container = scrollContainerRef.current;
    if (!container) return;

    const containerCenter = container.scrollLeft + container.offsetWidth / 2;
    const cards = container.querySelectorAll(".barber-card");

    let closestIndex = 0;
    let minDistance = Infinity;

    cards.forEach((card, index) => {
      const element = card as HTMLElement;
      const cardCenter = element.offsetLeft + element.offsetWidth / 2;
      const distance = Math.abs(containerCenter - cardCenter);

      if (distance < minDistance) {
        minDistance = distance;
        closestIndex = index;
      }
    });

    if (closestIndex !== currentIndex) {
      setCurrentIndex(closestIndex);
    }
  };

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (
        document.activeElement?.closest(".slider-container") &&
        topSalons.length > 0
      ) {
        let nextIndex = currentIndex;
        if (e.key === "ArrowLeft") {
          nextIndex =
            currentIndex === 0 ? topSalons.length - 1 : currentIndex - 1;
        } else if (e.key === "ArrowRight") {
          nextIndex =
            currentIndex === topSalons.length - 1 ? 0 : currentIndex + 1;
        }

        if (nextIndex !== currentIndex) {
          setCurrentIndex(nextIndex);
          scrollToContainerIndex(nextIndex);
        }
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [currentIndex, topSalons.length]);

  const hasInitialScrolled = useRef(false);

  useEffect(() => {
    if (topSalons.length > 0 && !hasInitialScrolled.current) {
      const timer = setTimeout(() => {
        scrollToContainerIndex(0);
        hasInitialScrolled.current = true;
      }, 150);
      return () => clearTimeout(timer);
    }
  }, [topSalons.length]);

  return (
    <section className="w-full py-20 sm:py-28 px-4 sm:px-16 text-slate-900 dark:text-white overflow-hidden transition-colors duration-300">
      <div className="max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-12 gap-16 items-center">
        <div className="grid lg:col-span-5 space-y-8 w-full">
          <div className="space-y-3 text-center lg:text-left">
            <h2 className="text-3xl sm:text-4xl font-extrabold tracking-tight">
              Find Master Barbers{" "}
              <span className="bg-linear-to-r from-blue-600 to-red-500 dark:from-blue-400 dark:via-white dark:to-red-400 bg-clip-text text-transparent">
                Near You
              </span>
            </h2>
            <p className="text-slate-500 dark:text-gray-400 text-sm sm:text-base font-light max-w-md mx-auto lg:mx-0">
              Discover local top-rated shops, compare pricing, and schedule your
              next fresh cut instantly.
            </p>
          </div>

          <div className="w-full bg-slate-50 dark:bg-white/5 border border-slate-900/5 dark:border-white/10 rounded-2xl p-6 shadow-xl backdrop-blur-md space-y-6">
            <div className="grid grid-cols-2 gap-4 border-b border-slate-200 dark:border-white/5 pb-4">
              <div className="space-y-1 text-center sm:text-left">
                {loadingSalons ? (
                  <div className="h-9 w-16 bg-slate-200 dark:bg-white/10 rounded-lg animate-pulse mx-auto sm:mx-0" />
                ) : (
                  <span className="text-3xl font-black text-slate-900 dark:text-white tracking-tight">
                    {salonsCount ?? 0}+
                  </span>
                )}
                <p className="text-xs text-slate-400 dark:text-gray-500 uppercase tracking-wider font-bold">
                  Premium Salons
                </p>
              </div>

              <div className="space-y-1 text-center sm:text-left border-l border-slate-200 dark:border-white/5 pl-4">
                {loadingBookings ? (
                  <div className="h-9 w-24 bg-slate-200 dark:bg-white/10 rounded-lg animate-pulse mx-auto sm:mx-0" />
                ) : (
                  <span className="text-3xl font-black text-slate-900 dark:text-white tracking-tight">
                    {bookingsCount?.toLocaleString() ?? 0}+
                  </span>
                )}
                <p className="text-xs text-slate-400 dark:text-gray-500 uppercase tracking-wider font-bold">
                  Total Bookings
                </p>
              </div>
            </div>

            <div className="relative w-full rounded-xl p-[1.5px] overflow-hidden group/btn">
              <div className="absolute inset-0 rounded-xl bg-[linear-gradient(67deg,rgba(255,255,255,0.8)_0%,rgba(59,130,246,0.8)_25%,rgba(255,255,255,0.8)_50%,rgba(239,68,68,0.8)_75%,rgba(255,255,255,0.8)_100%)] bg-size-[200%_100%] animate-[barber_4s_linear_infinite] opacity-0 group-hover/btn:opacity-100 transition-opacity duration-300" />

              <button
                onClick={() => navigate("/barbers")}
                className="relative z-10 w-full py-3.5 rounded-xl font-bold text-sm tracking-wide shadow-md transition-all duration-300 cursor-pointer text-center active:scale-[0.98] bg-slate-900 text-white hover:bg-slate-800 group-hover/btn:bg-slate-900/90 dark:bg-linear-to-r dark:from-blue-600 dark:to-blue-500 dark:text-white sm:dark:bg-[#0f1115] sm:dark:from-transparent sm:dark:to-transparent sm:dark:border sm:dark:border-white/10 dark:hover:text-white sm:dark:group-hover/btn:bg-[#0f1115]/90"
              >
                Browse All Available Shops
              </button>
            </div>
          </div>
        </div>

        <div
          ref={sliderRef}
          tabIndex={0}
          className="slider-container lg:col-span-7 w-full flex flex-col items-center justify-center overflow-hidden relative min-h-110 px-2 focus:outline-hidden group"
        >
          {loadingTopSalons ? (
            <div className="flex gap-6 animate-pulse">
              <div className="w-65 sm:w-75 h-95 bg-slate-200 dark:bg-white/5 rounded-3xl" />
            </div>
          ) : topSalons.length === 0 ? (
            <p className="text-sm text-slate-400">
              No active salons available.
            </p>
          ) : (
            <>
              <div
                ref={scrollContainerRef}
                onScroll={handleScroll}
                className="w-full flex items-center overflow-x-auto scroll-smooth snap-x snap-mandatory h-95 no-scrollbar px-[calc(50%-130px)] sm:px-[calc(50%-150px)] gap-6"
                style={{
                  scrollbarWidth: "none",
                  WebkitOverflowScrolling: "touch",
                }}
              >
                {topSalons.map((salon, index) => {
                  const isCenter = index === currentIndex;

                  return (
                    <div
                      key={salon.salonId}
                      onClick={() => handleDotClick(index)}
                      className={`barber-card shrink-0 snap-center w-65 sm:w-75 rounded-3xl bg-slate-50 dark:bg-white/5 border border-slate-900/10 dark:border-white/10 p-3 shadow-2xl backdrop-blur-md transition-all duration-500 ease-in-out cursor-pointer select-none ${
                        isCenter
                          ? "scale-100 opacity-100 z-20"
                          : "scale-85 opacity-40 blur-[1px] z-10"
                      }`}
                    >
                      <div className="relative aspect-4/5 w-full rounded-2xl overflow-hidden group/img">
                        <div className="absolute inset-0 bg-linear-to-t from-slate-950/90 via-slate-950/20 to-transparent z-10" />
                        <img
                          src="https://images.unsplash.com/photo-1585747860715-2ba37e788b70?auto=format&fit=crop&w=600&q=80"
                          alt={salon.name}
                          className="w-full h-full object-cover pointer-events-none"
                        />
                        <div className="absolute bottom-4 left-4 right-4 z-20 text-white">
                          <h3 className="font-bold text-lg tracking-tight truncate">
                            {salon.name}
                          </h3>
                          <p className="text-xs text-gray-300 flex items-center gap-1 mt-0.5 truncate">
                            <FiMapPin className="text-red-400 h-3 w-3 shrink-0" />
                            {salon.address}, {salon.city}
                          </p>

                          <div className="flex flex-wrap items-center gap-2 mt-3">
                            <div className="flex items-center gap-1 bg-black/40 backdrop-blur-xs px-2.5 py-1 rounded-lg text-[11px] font-semibold border border-white/10">
                              <FiUsers className="text-blue-400 h-3.5 w-3.5" />
                              <span>{salon.staffCount} Staff</span>
                            </div>

                            <div className="flex items-center gap-1 bg-black/40 backdrop-blur-xs px-2.5 py-1 rounded-lg text-[11px] font-semibold border border-white/10">
                              <FiCalendar className="text-emerald-400 h-3.5 w-3.5" />
                              <span>{salon.totalBookings} Bookings</span>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>

              <div className="absolute bottom-2 left-0 right-0 flex items-center justify-center gap-2 z-40">
                {topSalons.map((_, index) => (
                  <button
                    key={index}
                    onClick={() => handleDotClick(index)}
                    className={`h-2 rounded-full transition-all duration-300 cursor-pointer ${
                      index === currentIndex
                        ? "w-5 bg-blue-600 dark:bg-blue-400 opacity-100"
                        : "w-2 bg-slate-400 dark:bg-gray-600 opacity-40"
                    }`}
                  />
                ))}
              </div>
            </>
          )}

          <div className="absolute top-2 right-4 text-[10px] uppercase font-bold tracking-wider text-slate-400 dark:text-gray-500 opacity-0 group-hover:opacity-100 transition-opacity hidden sm:block">
            Use ◄ / ► keys to navigate
          </div>
        </div>
      </div>

      <style>{`
        .no-scrollbar::-webkit-scrollbar {
          display: none;
        }
      `}</style>
    </section>
  );
};
