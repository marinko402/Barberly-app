import { useState, type FC } from "react";
import { useAuth } from "../../../context/auth/useAuth";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "react-toastify";
import { Users, Trash2, AlertTriangle } from "lucide-react";
import {
  getAllSalons,
  createSalon,
  updateSalon,
  addBarberToSalonByUsername,
  removeBarberFromSalon,
  deleteSalon,
} from "../../../services/SalonService";
import { SalonForm } from "./SalonForm";
import { AddBarberPanel } from "./AddBarberPanel";
import type { Barber } from "../../../models/Barber";
import { getUserData } from "../../../services/AuthService";

const MySalon: FC = () => {
  const [isEditing, setIsEditing] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [barberToDelete, setBarberToDelete] = useState<Barber | null>(null);
  const { id: currentUserId, user, updateUserContext } = useAuth();
  const queryClient = useQueryClient();

  const { data: salons, isLoading } = useQuery({
    queryKey: ["salons"],
    queryFn: getAllSalons,
  });

  const mySalon = salons?.find(
    (s) =>
      s.owner?.id === currentUserId ||
      s.barbers.some((b) => b.id === currentUserId),
  );
  const isOwner = mySalon?.owner?.id === currentUserId;

  const createSalonMutation = useMutation({
    mutationFn: (data: any) => {
      const ownerObject: Barber = {
        id: currentUserId,
        firstName: user?.firstName || "",
        lastName: user?.lastName || "",
        userName: user?.userName || "",
        salonId: null,
      };
      return createSalon({
        ...data,
        owner: ownerObject,
        barbers: [ownerObject],
      });
    },
    onSuccess: async () => {
      toast.success("Salon registered successfully!");
      queryClient.invalidateQueries({ queryKey: ["salons"] });

      if (currentUserId) {
        try {
          const userData = await getUserData(currentUserId);

          const updatedStorageData = {
            id: userData.id,
            userName: userData.userName,
            email: userData.email,
            firstName: userData.firstName,
            lastName: userData.lastName,
            phoneNumber: userData.phoneNumber,
            birthDate: userData.birthDate,
            salonId: userData.salonId,
            password: "placeholder",
          };

          updateUserContext(updatedStorageData);
        } catch (error) {
          console.error(
            "Failed to refresh user data after salon creation:",
            error,
          );
        }
      }
    },
    onError: () => toast.error("Error while creating salon!"),
  });

  const updateSalonMutation = useMutation({
    mutationFn: (data: any) =>
      updateSalon(mySalon!.salonId, {
        ...data,
        owner: mySalon!.owner || null,
        barbers: mySalon!.barbers || [],
      }),
    onSuccess: () => {
      toast.success("Salon updated successfully!");
      setIsEditing(false);
      queryClient.invalidateQueries({ queryKey: ["salons"] });
    },
    onError: () => toast.error("Error while updating salon!"),
  });

  const addBarberMutation = useMutation({
    mutationFn: (data: { username: string }) =>
      addBarberToSalonByUsername(data.username, mySalon!.salonId),
    onSuccess: () => {
      toast.success("Barber successfully added to salon!");
      queryClient.invalidateQueries({ queryKey: ["salons"] });
    },
    onError: (err: any) => {
      const msg =
        err.response?.data?.message ||
        err.response?.data ||
        "Barber not found!";
      toast.error(msg);
    },
  });

  const removeBarberMutation = useMutation({
    mutationFn: (barberId: string) =>
      removeBarberFromSalon(barberId, mySalon!.salonId, currentUserId!),
    onSuccess: () => {
      toast.success("Barber successfully removed from salon.");
      setIsModalOpen(false);
      setBarberToDelete(null);
      queryClient.invalidateQueries({ queryKey: ["salons"] });
    },
    onError: (err: any) => {
      const msg = err.response?.data || "Error removing barber.";
      toast.error(msg);
    },
  });

  const deleteSalonMutation = useMutation({
    mutationFn: (id: string) => deleteSalon(id),
    onSuccess: async () => {
      toast.success("Salon successfully deleted!");
      queryClient.invalidateQueries({ queryKey: ["salons"] });

      if (currentUserId) {
        try {
          const userData = await getUserData(currentUserId);
          const updatedStorageData = {
            id: userData.id,
            userName: userData.userName,
            email: userData.email,
            firstName: userData.firstName,
            lastName: userData.lastName,
            phoneNumber: userData.phoneNumber,
            birthDate: userData.birthDate,
            salonId: userData.salonId,
            password: "placeholder",
          };
          updateUserContext(updatedStorageData);
        } catch (error) {
          console.error(
            "Failed to refresh user data after salon deletion:",
            error,
          );
        }
      }
    },
    onError: (err: any) => {
      toast.error("Error while deleting salon!");
      console.log("Delete salon error: ", err);
    },
  });

  const openDeleteModal = (barber: Barber) => {
    setBarberToDelete(barber);
    setIsModalOpen(true);
  };

  if (isLoading) {
    return (
      <div className="text-white text-center p-10 font-medium">
        Loading salon data...
      </div>
    );
  }

  return (
    <div className="w-full flex flex-col gap-8 text-white">
      {!mySalon ? (
        <SalonForm
          isOwner={true}
          isEditing={true}
          setIsEditing={setIsEditing}
          isPending={createSalonMutation.isPending}
          onSubmit={createSalonMutation.mutate}
          onDelete={deleteSalonMutation.mutate}
          isPendingDelete={deleteSalonMutation.isPending}
        />
      ) : (
        <div
          className={`grid grid-cols-1 ${isOwner ? "lg:grid-cols-3" : "lg:grid-cols-1"} gap-8 w-full h-auto`}
        >
          <div
            className={`${isOwner ? "lg:col-span-2" : "lg:col-span-3"} flex flex-col justify-between h-auto`}
          >
            <SalonForm
              mySalon={mySalon}
              isOwner={isOwner}
              isEditing={isEditing}
              setIsEditing={setIsEditing}
              isPending={updateSalonMutation.isPending}
              onSubmit={updateSalonMutation.mutate}
              onDelete={deleteSalonMutation.mutate}
              isPendingDelete={deleteSalonMutation.isPending}
            />

            <div className="mt-8 pt-6 border-t border-white/5 w-full">
              <div className="flex items-center gap-2 mb-4 text-neutral-400">
                <Users className="w-4 h-4" />
                <h3 className="text-xs font-bold uppercase tracking-wider">
                  Active Team ({mySalon.barbers?.length || 0})
                </h3>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {mySalon.barbers?.map((barber) => (
                  <div
                    key={barber.id}
                    className="p-3 bg-white/5 border border-white/5 rounded-xl flex items-center justify-between transition-all duration-300 hover:border-white/10"
                  >
                    <div>
                      <p className="text-sm font-medium text-white">
                        {barber.firstName} {barber.lastName}
                      </p>
                      <p className="text-xs text-neutral-500">
                        @{barber.userName}
                      </p>
                    </div>

                    <div className="flex items-center gap-3">
                      <span className="text-[10px] uppercase font-bold tracking-wider px-2 py-1 rounded-md bg-blue-500/10 text-blue-400 border border-blue-500/10">
                        Staff
                      </span>

                      {isOwner && barber.id !== currentUserId && (
                        <button
                          onClick={() => openDeleteModal(barber)}
                          disabled={removeBarberMutation.isPending}
                          className="cursor-pointer p-1.5 rounded-lg bg-red-500/10 text-red-400 border border-red-500/10 hover:bg-red-500/20 transition-colors disabled:opacity-50"
                          title="Remove from salon"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {isOwner && (
            <AddBarberPanel
              isOwner={isOwner}
              isPending={addBarberMutation.isPending}
              onAddBarber={addBarberMutation.mutate}
            />
          )}
        </div>
      )}

      <div
        className={`modal modal-bottom sm:modal-middle ${isModalOpen ? "modal-open" : ""}`}
      >
        <div className="modal-box bg-neutral-900 border border-white/10 text-white shadow-2xl">
          <div className="flex items-center gap-3 text-red-400 mb-4">
            <AlertTriangle className="w-6 h-6" />
            <h3 className="font-bold text-lg">Remove Team Member</h3>
          </div>

          <p className="text-sm text-neutral-300 py-2">
            Are you sure you want to remove{" "}
            <span className="font-semibold text-white">
              {barberToDelete?.firstName} {barberToDelete?.lastName}
            </span>{" "}
            (@{barberToDelete?.userName}) from the salon? They will lose access
            to the shop dashboard.
          </p>

          <div className="modal-action gap-2">
            <button
              onClick={() => {
                setIsModalOpen(false);
                setBarberToDelete(null);
              }}
              className="btn btn-ghost text-neutral-400 hover:text-white"
            >
              Cancel
            </button>

            <button
              onClick={() =>
                barberToDelete && removeBarberMutation.mutate(barberToDelete.id)
              }
              disabled={removeBarberMutation.isPending}
              className="btn bg-red-600 hover:bg-red-700 text-white border-none min-w-25"
            >
              {removeBarberMutation.isPending ? (
                <span className="loading loading-spinner loading-xs"></span>
              ) : (
                "Remove"
              )}
            </button>
          </div>
        </div>

        <div
          className="modal-backdrop bg-black/60"
          onClick={() => {
            setIsModalOpen(false);
            setBarberToDelete(null);
          }}
        ></div>
      </div>
    </div>
  );
};

export default MySalon;
