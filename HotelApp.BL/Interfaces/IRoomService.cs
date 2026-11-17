using HotelApp.DAL.Models.dbo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.BL.Dtos;

namespace HotelApp.BL.Interfaces
{
    public interface IRoomService
    {
        /// <summary>Rooms ordered by block, floor and number. By default rooms taken out of service are not listed.</summary>
        Task<IEnumerable<Room>> GetAllAsync(int? hotelId = null, int? blockId = null, int? floorId = null, bool includeInactive = false);
        Task<Room?> GetByIdAsync(int id);

        /// <summary>CU-004 / RF-004: free rooms by dates and number of guests, with their price. Optionally only one hotel.</summary>
        Task<IEnumerable<AvailableRoomDto>> SearchAvailabilityAsync(DateTime checkInDate, DateTime checkOutDate, int guestCount, int? hotelId = null);

        // CU-011 / RF-001: managing rooms is the Administrator's job
        Task AddAsync(Room room, int requestingUserId);

        /// <summary>Changes number, floor, type and whether it is active. The status is changed with ChangeStatusAsync.</summary>
        Task UpdateAsync(Room room, int requestingUserId);

        /// <summary>Soft delete (RF-001): the room becomes inactive, it is not removed from the database.</summary>
        Task DeleteAsync(int roomId, int requestingUserId);

        /// <summary>CU-012 / RF-010: manual status change (Available, Cleaning or Maintenance). Any active user can do it.</summary>
        Task ChangeStatusAsync(int roomId, string newStatus, int userId);
    }
}
