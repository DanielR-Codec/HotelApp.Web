using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Interfaces
{
    public interface IReservationRepository : IBaseRepository<Reservation>
    {
        Task<Reservation> GetByCodeAsync(string code);
        Task<bool> CodeExistsAsync(string code);
        Task<IEnumerable<Reservation>> GetByGuestAsync(int guestId);

        /// <summary>Reservations overlapping the range [from, toExclusive) (RF-015).</summary>
        Task<IEnumerable<Reservation>> GetByDateRangeAsync(DateTime from, DateTime toExclusive);

        /// <summary>Pending/Confirmed reservations arriving on the given date (CU-008, step 1).</summary>
        Task<IEnumerable<Reservation>> GetArrivalsForDateAsync(DateTime date);

        /// <summary>Pending/Confirmed reservations whose arrival is up to the given date (inclusive) and that did not check in (RN-009).</summary>
        Task<IEnumerable<Reservation>> GetWithoutCheckInUntilAsync(DateTime date);

        /// <summary>
        /// Finds an active reservation of the same guest, on overlapping dates, that already includes any of the requested rooms (RN-013).
        /// </summary>
        Task<Reservation> GetDuplicateReservationAsync(int guestId, DateTime checkIn, DateTime checkOut, IEnumerable<int> roomIds);
    }
}
