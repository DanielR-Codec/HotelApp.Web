using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Interfaces
{
    public interface IRoomRepository : IBaseRepository<Room>
    {
        /// <summary>
        /// Rooms free for [checkIn, checkOut) with enough capacity for the given number of guests.
        /// Excludes rooms taken out of service (or whose floor, block or hotel is inactive), rooms under Maintenance (RN-011)
        /// and rooms that already have an active reservation on those dates (RN-001). Optionally only one hotel.
        /// </summary>
        Task<IEnumerable<Room>> SearchAvailableAsync(DateTime checkIn, DateTime checkOut, int guestCount, int? hotelId = null);

        /// <summary>Rooms ordered by block, floor and number; optionally filtered by hotel, block and/or floor.</summary>
        Task<IEnumerable<Room>> GetFilteredAsync(int? hotelId, int? blockId, int? floorId, bool includeInactive);

        Task<IEnumerable<Room>> GetByIdsAsync(IEnumerable<int> ids);

        /// <summary>true if the room has no active reservations overlapping [checkIn, checkOut). One reservation can be ignored (when editing it).</summary>
        Task<bool> IsAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null);

        /// <summary>true if another room of the same hotel (any block or floor) already uses that number.</summary>
        Task<bool> NumberExistsInHotelAsync(int hotelId, string number, int? excludeRoomId = null);
        Task<bool> AnyRoomsOfTypeAsync(int roomTypeId);

        /// <summary>true if the room has active reservations that end after the given date.</summary>
        Task<bool> HasUpcomingReservationsAsync(int roomId, DateTime fromDate);
    }
}
