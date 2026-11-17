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
    public class ReservationRepository : BaseRepository<Reservation>, IReservationRepository
    {
        public ReservationRepository(HotelContext context, ILogger<ReservationRepository> logger) : base(context, logger) { }

        public Task<Reservation> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Task.FromResult<Reservation>(null);

            var clean = code.Trim();
            return ExecuteAsync(async () =>
                await _dbSet.FirstOrDefaultAsync(r => r.ReservationCode == clean),
                nameof(GetByCodeAsync));
        }

        public Task<bool> CodeExistsAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Task.FromResult(false);

            var clean = code.Trim();
            return ExecuteAsync(async () =>
                await _dbSet.AnyAsync(r => r.ReservationCode == clean),
                nameof(CodeExistsAsync));
        }

        public Task<IEnumerable<Reservation>> GetByGuestAsync(int guestId)
        {
            if (guestId <= 0)
                return Task.FromResult(Enumerable.Empty<Reservation>());

            return ExecuteAsync<IEnumerable<Reservation>>(async () =>
                await _dbSet
                    .Where(r => r.GuestId == guestId)
                    .OrderByDescending(r => r.CheckInDate)
                    .ToListAsync(),
                nameof(GetByGuestAsync));
        }

        public Task<IEnumerable<Reservation>> GetByDateRangeAsync(DateTime from, DateTime toExclusive)
        {
            if (toExclusive <= from)
                throw new ArgumentException("El final del rango debe ser posterior al inicio.", nameof(toExclusive));

            return ExecuteAsync<IEnumerable<Reservation>>(async () =>
                await _dbSet
                    .Where(r => r.CheckInDate < toExclusive && r.CheckOutDate > from)
                    .OrderBy(r => r.CheckInDate)
                    .ToListAsync(),
                nameof(GetByDateRangeAsync));
        }

        public Task<IEnumerable<Reservation>> GetArrivalsForDateAsync(DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            return ExecuteAsync<IEnumerable<Reservation>>(async () =>
                await _dbSet
                    .Where(r => r.CheckInDate >= start && r.CheckInDate < end
                             && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed))
                    .OrderBy(r => r.ReservationCode)
                    .ToListAsync(),
                nameof(GetArrivalsForDateAsync));
        }

        public Task<IEnumerable<Reservation>> GetWithoutCheckInUntilAsync(DateTime date)
        {
            var limit = date.Date.AddDays(1);

            return ExecuteAsync<IEnumerable<Reservation>>(async () =>
                await _dbSet
                    .Where(r => r.CheckInDate < limit
                             && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed))
                    .ToListAsync(),
                nameof(GetWithoutCheckInUntilAsync));
        }

        public Task<Reservation> GetDuplicateReservationAsync(int guestId, DateTime checkIn, DateTime checkOut, IEnumerable<int> roomIds)
        {
            var ids = (roomIds ?? Enumerable.Empty<int>()).Where(id => id > 0).Distinct().ToList();
            if (guestId <= 0 || ids.Count == 0 || checkOut <= checkIn)
                return Task.FromResult<Reservation>(null);

            return ExecuteAsync(async () =>
            {
                var activeStatuses = ReservationStatus.Active.ToList();

                return await (from r in _context.Reservations
                              join d in _context.ReservationDetails on r.ReservationId equals d.ReservationId
                              where r.GuestId == guestId
                                 && activeStatuses.Contains(r.Status)
                                 && r.CheckInDate < checkOut && r.CheckOutDate > checkIn
                                 && ids.Contains(d.RoomId)
                              select r).FirstOrDefaultAsync();
            }, nameof(GetDuplicateReservationAsync));
        }
    }
}
