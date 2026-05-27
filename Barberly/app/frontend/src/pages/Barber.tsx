import { useMemo, useState, type FC } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router";
import {
  Search,
  MapPin,
  Users,
  Store,
  Loader2,
  ArrowRight,
  AlertTriangle,
} from "lucide-react";
import { getAllSalons } from "../services/SalonService";
import type { Salon } from "../models/Salon";

const Barber: FC = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const navigate = useNavigate();

  const {
    data: salons,
    isLoading,
    isError,
  } = useQuery<Salon[]>({
    queryKey: ["salons"],
    queryFn: getAllSalons,
  });

  const filteredSalons = useMemo(() => {
    if (!salons) return [];
    const searchLower = searchTerm.toLowerCase();

    return salons.filter(
      (salon) =>
        salon.name.toLowerCase().includes(searchLower) ||
        (salon.city?.toLowerCase().includes(searchLower) ?? false) ||
        (salon.address?.toLowerCase().includes(searchLower) ?? false),
    );
  }, [salons, searchTerm]);

  return (
    <div className="w-full min-h-dvh p-4 md:p-5 pt-20 md:pt-24 flex flex-col gap-10 text-neutral-800 dark:text-white pb-16">
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-6 p-6 bg-white dark:bg-black/30 border border-neutral-200 dark:border-white/10 rounded-2xl backdrop-blur-xl shadow-xl dark:shadow-2xl">
        <div>
          <h1 className="text-3xl font-bold tracking-wide text-neutral-900 dark:text-white">
            Available{" "}
            <span className="text-red-500 dark:text-red-400">Salons</span>
          </h1>
          <p className="text-sm text-neutral-500 dark:text-neutral-400 mt-1.5 max-w-lg">
            Explore and find your perfect workspace. Browse our verified partner
            shops and view their details.
          </p>
        </div>

        <div className="relative flex items-center w-full md:w-96">
          <Search className="absolute left-4 text-neutral-400 dark:text-neutral-500 w-5 h-5" />
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search by name, city or address..."
            className="w-full pl-12 pr-4 py-3.5 bg-neutral-100 dark:bg-black/40 border border-neutral-300 dark:border-white/10 rounded-xl text-neutral-900 dark:text-white text-sm font-medium outline-none transition-all duration-300 focus:border-red-500 dark:focus:border-red-400/60 focus:bg-white dark:focus:bg-black/60 shadow-sm"
          />
        </div>
      </div>

      {isLoading && (
        <div className="w-full h-full flex flex-col items-center justify-center py-24 gap-4 p-8 bg-white dark:bg-black/30 border border-neutral-200 dark:border-white/10 rounded-2xl shadow-md dark:shadow-2xl">
          <Loader2 className="w-10 h-10 animate-spin text-red-500 dark:text-red-400" />
          <p className="text-base font-semibold text-neutral-600 dark:text-neutral-300">
            Loading available salons...
          </p>
        </div>
      )}

      {isError && (
        <div className="text-center py-20 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-500/20 rounded-2xl p-8 flex flex-col items-center gap-3">
          <AlertTriangle className="w-10 h-10 text-red-500" />
          <p className="text-lg text-red-600 dark:text-red-400 font-bold">
            Failed to load salons.
          </p>
          <p className="text-sm text-red-500/80 dark:text-red-300/80">
            Please check your internet connection and try again later.
          </p>
        </div>
      )}

      {!isLoading && !isError && filteredSalons?.length === 0 && (
        <div className="text-center py-24 bg-white dark:bg-black/30 border border-neutral-200 dark:border-white/10 rounded-2xl p-8 flex flex-col items-center gap-3 shadow-md dark:shadow-2xl">
          <Store className="w-10 h-10 text-neutral-400 dark:text-neutral-600" />
          <p className="text-lg text-neutral-700 dark:text-neutral-300 font-semibold">
            No salons found.
          </p>
          <p className="text-sm text-neutral-400 dark:text-neutral-500">
            Try adjusting your search criteria.
          </p>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
        {filteredSalons?.map((salon) => (
          <div
            key={salon.salonId}
            className="group flex flex-col justify-between p-7 bg-white dark:bg-black/30 border border-neutral-200 dark:border-white/10 rounded-3xl transition-all duration-500 hover:border-neutral-300 dark:hover:border-white/20 hover:bg-neutral-50/50 dark:hover:bg-black/40 hover:-translate-y-1.5 shadow-xl hover:shadow-2xl dark:shadow-2xl dark:hover:shadow-red-950/20"
          >
            <div>
              <div className="flex items-center justify-between gap-4 mb-6">
                <div className="p-4 rounded-2xl bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-500/20 text-red-500 dark:text-red-400 shadow-sm">
                  <Store className="w-6 h-6" />
                </div>

                <div className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-neutral-100 dark:bg-white/5 border border-neutral-200 dark:border-white/5 text-neutral-600 dark:text-neutral-300 text-xs font-semibold uppercase tracking-wider">
                  <Users className="w-4 h-4 text-blue-500 dark:text-blue-400" />
                  <span>{salon.barbers?.length || 0} Staff</span>
                </div>
              </div>

              <h3 className="text-xl font-extrabold text-neutral-900 dark:text-white group-hover:text-red-500 dark:group-hover:text-red-400 transition-colors duration-300 line-clamp-1 leading-tight">
                {salon.name}
              </h3>

              <div className="flex flex-col gap-2 mt-4 text-neutral-600 dark:text-neutral-400 text-sm font-medium">
                <div className="flex items-center gap-2.5">
                  <MapPin className="w-5 h-5 text-neutral-400 dark:text-neutral-600 shrink-0" />
                  <span className="line-clamp-1">
                    {salon.address || "Address not listed"}
                  </span>
                </div>
                <div className="flex items-center gap-2.5 pl-7.5 text-xs text-neutral-400 dark:text-neutral-500">
                  <span>{salon.city || "Unknown City"}</span>
                </div>
              </div>
            </div>

            <div className="mt-8 pt-5 border-t border-neutral-100 dark:border-white/5">
              <div className="relative w-full rounded-xl p-[1.5px] overflow-hidden group">
                <div className="absolute inset-0 rounded-xl bg-[linear-gradient(67deg,rgba(255,255,255,0.8)_0%,rgba(59,130,246,0.8)_25%,rgba(255,255,255,0.8)_50%,rgba(239,68,68,0.8)_75%,rgba(255,255,255,0.8)_100%)] bg-size-[200%_100%] animate-[barber_4s_linear_infinite]" />

                <button
                  onClick={() =>
                    navigate(`/salon/${salon.name}`, {
                      state: { salonId: salon.salonId },
                    })
                  }
                  className="relative z-10 w-full flex items-center justify-center gap-2.5 px-5 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all duration-300 cursor-pointer backdrop-blur-md active:scale-[0.98] bg-white/90 text-neutral-900 hover:bg-white/70 dark:bg-black/70 dark:text-white dark:hover:bg-black/40"
                >
                  View Salon <ArrowRight className="w-4 h-4 shrink-0" />
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Barber;
