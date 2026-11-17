using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelApp.DAL.Base;
using HotelApp.DAL.Constants;
using HotelApp.DAL.Context;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelApp.DAL.Repositories.dbo
{
    public class RoomRepository : BaseRepository<Room>, IRoomRepository
    {
        public RoomRepository(HotelContext context, ILogger<RoomRepository> logger) : base(context, logger) { }

        public Task<IEnumerable<Room>> SearchAvailableAsync(DateTime checkIn, DateTime checkOut, int guestCount, int? hotelId = null)
        {
            ValidateRange(checkIn, checkOut);
            if (guestCount < 1)
                throw new ArgumentOutOfRangeException(nameof(guestCount), "La cantidad de personas debe ser al menos 1.");

            return ExecuteAsync<IEnumerable<Room>>(async () =>
            {
                var activeStatuses = ReservationStatus.Active.ToList();
                var maintenance = RoomStatus.Maintenance;

                // Rooms already taken by an active reservation overlapping the requested dates
                var occupied = from d in _context.ReservationDetails
                               join r in _context.Reservations on d.ReservationId equals r.ReservationId
                               where activeStatuses.Contains(r.Status)
                                  && r.CheckInDate < checkOut && r.CheckOutDate > checkIn
                               select d.RoomId;

                var query = from room in _context.Rooms
                            join t in _context.RoomTypes on room.RoomTypeId equals t.RoomTypeId
                            join f in _context.Floors on room.FloorId equals f.FloorId
                            join b in _context.Blocks on f.BlockId equals b.BlockId
                            join h in _context.Hotels on b.HotelId equals h.HotelId
                            where room.IsActive && f.IsActive && b.IsActive && h.IsActive
                               && (hotelId == null || h.HotelId == hotelId)
                               && room.CurrentStatus != maintenance
                               && t.MaxCapacity >= guestCount
                               && !occupied.Contains(room.RoomId)
                            orderby h.Name, b.Name, f.Number, room.Number
                            select room;

                return await query.ToListAsync();
            }, nameof(SearchAvailableAsync));
        }

        public Task<IEnumerable<Room>> GetFilteredAsync(int? hotelId, int? blockId, int? floorId, bool includeInactive)
        {
            return ExecuteAsync<IEnumerable<Room>>(async () =>
            {
                var query = from room in _context.Rooms
                            join f in _context.Floors on room.FloorId equals f.FloorId
                            join b in _context.Blocks on f.BlockId equals b.BlockId
                            where (hotelId == null || b.HotelId == hotelId)
                               && (blockId == null || f.BlockId == blockId)
                               && (floorId == null || room.FloorId == floorId)
                               && (includeInactive || room.IsActive)
                            orderby b.Name, f.Number, room.Number
                            select room;

                return await query.ToListAsync();
            }, nameof(GetFilteredAsync));
        }

        public Task<IEnumerable<Room>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var list = (ids ?? Enumerable.Empty<int>()).Where(id => id > 0).Distinct().ToList();
            if (list.Count == 0)
                return Task.FromResult(Enumerable.Empty<Room>());

            return ExecuteAsync<IEnumerable<Room>>(async () =>
                await _dbSet.Where(r => list.Contains(r.RoomId)).ToListAsync(),
                nameof(GetByIdsAsync));
        }

        public async Task<bool> IsAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
        {
            ValidateRange(checkIn, checkOut);
            if (roomId <= 0)
                return false;

            var isOccupied = await ExecuteAsync(async () =>
            {
                var activeStatuses = ReservationStatus.Active.ToList();

                return await (from d in _context.ReservationDetails
                              join r in _context.Reservations on d.ReservationId equals r.ReservationId
                              where d.RoomId == roomId
                                 && activeStatuses.Contains(r.Status)
                                 && r.CheckInDate < checkOut && r.CheckOutDate > checkIn
                                 && (excludeReservationId == null || r.ReservationId != excludeReservationId)
                              select d.ReservationDetailId).AnyAsync();
            }, nameof(IsAvailableAsync));

            return !isOccupied;
        }

        public Task<bool> NumberExistsInHotelAsync(int hotelId, string number, int? excludeRoomId = null)
        {
            if (hotelId <= 0 || string.IsNullOrWhiteSpace(number))
                return Task.FromResult(false);

            var clean = number.Trim().ToLower();
            return ExecuteAsync(async () =>
                await (from room in _context.Rooms
                       join f in _context.Floors on room.FloorId equals f.FloorId
                       join b in _context.Blocks on f.BlockId equals b.BlockId
                       where b.HotelId == hotelId
                          && room.Number.ToLower() == clean
                          && (excludeRoomId == null || room.RoomId != excludeRoomId)
                       select room.RoomId).AnyAsync(),
                nameof(NumberExistsInHotelAsync));
        }

        public Task<bool> AnyRoomsOfTypeAsync(int roomTypeId)
        {
            if (roomTypeId <= 0)
                return Task.FromResult(false);

            return ExecuteAsync(async () =>
                await _dbSet.AnyAsync(r => r.RoomTypeId == roomTypeId),
                nameof(AnyRoomsOfTypeAsync));
        }

        public Task<bool> HasUpcomingReservationsAsync(int roomId, DateTime fromDate)
        {
            if (roomId <= 0)
                return Task.FromResult(false);

            return ExecuteAsync(async () =>
            {
                var activeStatuses = ReservationStatus.Active.ToList();

                return await (from d in _context.ReservationDetails
                              join r in _context.Reservations on d.ReservationId equals r.ReservationId
                              where d.RoomId == roomId
                                 && activeStatuses.Contains(r.Status)
                                 && r.CheckOutDate > fromDate
                              select d.ReservationDetailId).AnyAsync();
            }, nameof(HasUpcomingReservationsAsync));
        }

        private static void ValidateRange(DateTime checkIn, DateTime checkOut)
        {
            if (checkOut <= checkIn)
                throw new ArgumentException("La fecha de salida debe ser posterior a la de llegada.", nameof(checkOut));
        }
    }
}
