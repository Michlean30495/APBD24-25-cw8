using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
    Task<List<ClientTripDTO>> GetClientTrips(int idClient);
    Task<int> AddClient(ClientDTO client);
    Task<bool> RegisterClientToTrip(int idClient, int idTrip);
    Task<bool> RemoveClientFromTrip(int idClient, int idTrip);
}