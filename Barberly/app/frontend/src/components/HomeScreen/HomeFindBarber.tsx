import type { FC } from "react";
import { useState, useEffect, useRef } from "react";
import { FiMapPin, FiScissors, FiSearch, FiStar, FiX } from "react-icons/fi";

export const HomeFindBarber: FC = () => {
  const [currentIndex, setCurrentIndex] = useState<number>(1);
  const [selectedService, setSelectedService] = useState<string>("");
  const [isLocationFocused, setIsLocationFocused] = useState<boolean>(false);

  const sliderRef = useRef<HTMLDivElement>(null);
  const scrollContainerRef = useRef<HTMLDivElement>(null);

  const services = ["Fade", "Beard Trim", "Hair and beard", "Hair styling"];

  const featuredShops = [
    {
      id: 1,
      name: "Royal Cuts",
      rating: "4.9",
      reviews: "110",
      location: "New York, NY",
      image:
        "https://images.unsplash.com/photo-1585747860715-2ba37e788b70?auto=format&fit=crop&w=600&q=80",
    },
    {
      id: 2,
      name: "The Vintage Post",
      rating: "4.8",
      reviews: "95",
      location: "Brooklyn, NY",
      image:
        "https://images.unsplash.com/photo-1605497746444-ac9dbd324ce8?auto=format&fit=crop&w=600&q=80",
    },
    {
      id: 3,
      name: "Masterstroke Barber",
      rating: "5.0",
      reviews: "142",
      location: "Manhattan, NY",
      image:
        "https://images.unsplash.com/photo-1599351431202-1e0f0137899a?auto=format&fit=crop&w=600&q=80",
    },
  ];

  const handleFilterClick = (filter: string) => {
    if (selectedService === filter) {
      setSelectedService("");
    } else {
      setSelectedService(filter);
    }
  };

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
      if (document.activeElement?.closest(".slider-container")) {
        let nextIndex = currentIndex;
        if (e.key === "ArrowLeft") {
          nextIndex =
            currentIndex === 0 ? featuredShops.length - 1 : currentIndex - 1;
        } else if (e.key === "ArrowRight") {
          nextIndex =
            currentIndex === featuredShops.length - 1 ? 0 : currentIndex + 1;
        }

        if (nextIndex !== currentIndex) {
          setCurrentIndex(nextIndex);
          scrollToContainerIndex(nextIndex);
        }
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [currentIndex, featuredShops.length]);

  useEffect(() => {
    setTimeout(() => scrollToContainerIndex(1), 100);
  }, []);

  return (
    <section className="w-full py-20 sm:py-28 px-4 sm:px-16 text-slate-900 dark:text-white overflow-hidden transition-colors duration-300">
      <div className="max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-12 gap-16 items-center">
      
        <div className="grid lg:col-span-5 space-y-8 w-full">
          <div className="space-y-3 text-center lg:text-left">
            <h2 className="text-3xl sm:text-4xl font-extrabold tracking-tight">
              Find Master Barbers{" "}
              <span className="bg-linear-to-r from-blue-600 to-red-500 dark:from-blue-400 dark:to-red-400 bg-clip-text text-transparent">
                Near You
              </span>
            </h2>
            <p className="text-slate-500 dark:text-gray-400 text-sm sm:text-base font-light max-w-md mx-auto lg:mx-0">
              Discover local top-rated shops, compare pricing, and schedule your
              next fresh cut instantly.
            </p>
          </div>

          <div className="w-full bg-slate-50 dark:bg-white/5 border border-slate-900/5 dark:border-white/10 rounded-2xl p-3 sm:p-4 shadow-xl backdrop-blur-md">
            <div className="flex flex-col sm:flex-row bg-white dark:bg-[#0f1115]/80 border border-slate-900/10 dark:border-white/5 rounded-xl overflow-hidden p-1 gap-1 relative">
              <div className="flex items-center px-3 py-2 flex-1 group">
                <FiMapPin
                  className={`mr-2.5 h-4 w-4 shrink-0 transition-colors duration-200 ${isLocationFocused ? "text-blue-500" : "text-slate-400 dark:text-gray-500"}`}
                />
                <input
                  type="text"
                  placeholder="Location..."
                  onFocus={() => setIsLocationFocused(true)}
                  onBlur={() => setIsLocationFocused(false)}
                  className="bg-transparent text-sm font-medium focus:outline-hidden text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-gray-500 w-full"
                />
              </div>

              <div className="hidden sm:block w-px bg-slate-200 dark:bg-white/10 my-2" />

              <div className="flex items-center px-3 py-2 flex-1 border-t sm:border-t-0 border-slate-100 dark:border-white/5 relative select-none">
                <FiScissors
                  className={`mr-2.5 h-4 w-4 shrink-0 transition-colors duration-200 ${selectedService ? "text-red-500" : "text-slate-400 dark:text-gray-500"}`}
                />
                <div className="flex-1 text-sm font-medium text-slate-900 dark:text-white truncate pr-6">
                  {selectedService ? (
                    <span className="font-semibold text-blue-600 dark:text-blue-400">
                      {selectedService}
                    </span>
                  ) : (
                    <span className="text-slate-400 dark:text-gray-500 font-light">
                      Any service (All)
                    </span>
                  )}
                </div>
                {selectedService && (
                  <button
                    onClick={() => setSelectedService("")}
                    className="absolute right-3 p-1 rounded-md hover:bg-slate-100 dark:hover:bg-white/10 text-slate-400 hover:text-red-500 transition-colors cursor-pointer"
                  >
                    <FiX className="h-3.5 w-3.5" />
                  </button>
                )}
              </div>

              <button className="relative group rounded-lg overflow-hidden p-0.5 focus:outline-hidden cursor-pointer shrink-0 mt-2 sm:mt-0">
                <span className="absolute inset-0 bg-linear-to-r from-blue-600 via-red-500 to-blue-600 dark:from-blue-500 dark:via-red-500 dark:to-blue-500 bg-size-[200%_auto] transition-all duration-500 opacity-0 group-hover:opacity-100 group-hover:animate-pulse" />
                <div className="relative flex items-center justify-center gap-2 bg-slate-900 text-white dark:bg-linear-to-r dark:from-blue-600 dark:to-blue-500 sm:dark:from-transparent sm:dark:to-transparent sm:dark:bg-[#0f1115] px-5 py-2.5 sm:py-3 rounded-[6px] font-semibold text-sm transition-colors duration-300 group-hover:bg-transparent group-hover:text-white">
                  <FiSearch className="h-4 w-4" />
                  <span className="sm:hidden lg:inline">Search</span>
                </div>
              </button>
            </div>
          </div>

          <div className="flex flex-wrap gap-2 justify-center lg:justify-start">
            {services.map((filter, index) => (
              <button
                key={index}
                onClick={() => handleFilterClick(filter)}
                className={`px-4 py-1.5 text-xs font-medium rounded-full border transition-all duration-200 cursor-pointer ${
                  selectedService === filter
                    ? "bg-blue-600 text-white border-blue-600 shadow-md scale-105"
                    : "bg-slate-100 hover:bg-slate-200 dark:bg-white/5 dark:hover:bg-white/10 text-slate-600 dark:text-gray-300 hover:text-slate-900 hover:dark:text-white border-slate-900/5 dark:border-white/5"
                }`}
              >
                {filter}
              </button>
            ))}
          </div>
        </div>

        <div
          ref={sliderRef}
          tabIndex={0}
          className="slider-container lg:col-span-7 w-full flex flex-col items-center justify-center overflow-hidden relative min-h-110 px-2 focus:outline-hidden group"
        >
          <div
            ref={scrollContainerRef}
            onScroll={handleScroll}
            className="w-full flex items-center overflow-x-auto scroll-smooth snap-x snap-mandatory h-95 no-scrollbar px-[calc(50%-130px)] sm:px-[calc(50%-150px)] gap-6"
            style={{
              scrollbarWidth: "none",
              WebkitOverflowScrolling: "touch",
            }}
          >
            {featuredShops.map((shop, index) => {
              const isCenter = index === currentIndex;

              return (
                <div
                  key={shop.id}
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
                      src={shop.image}
                      alt={shop.name}
                      className="w-full h-full object-cover pointer-events-none"
                    />
                    <div className="absolute bottom-4 left-4 right-4 z-20 text-white">
                      <h3 className="font-bold text-lg tracking-tight">
                        {shop.name}
                      </h3>
                      <p className="text-xs text-gray-300 flex items-center gap-1 mt-0.5">
                        <FiMapPin className="text-red-400 h-3 w-3" />
                        {shop.location}
                      </p>

                      <div className="flex items-center gap-1.5 mt-2.5 bg-black/40 backdrop-blur-xs px-2.5 py-1 rounded-lg w-fit text-xs font-semibold border border-white/10">
                        <FiStar className="fill-amber-400 text-amber-400 h-3.5 w-3.5" />
                        <span>{shop.rating}</span>
                        <span className="text-gray-300 font-light">
                          ({shop.reviews})
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>

          <div className="absolute bottom-2 left-0 right-0 flex items-center justify-center gap-2 z-40">
            {featuredShops.map((_, index) => (
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
