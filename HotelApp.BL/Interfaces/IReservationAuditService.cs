using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.BL.Dtos;

namespace HotelApp.BL.Interfaces
{
    /// <summary>
    /// RNF-007: read-only access to the reservation audit log. Only the Administrator can review it.
    /// Audit records are written exclusively by IReservationService, so they can never be edited or deleted.
    /// </summary>
    public interface IReservationAuditService
    {
        /// <summary>Full history of one reservation, ordered by date.</summary>
        Task<IEnumerable<ReservationAuditDto>> GetByReservationAsync(int reservationId, int requestingUserId);

        /// <summary>Records between two dates (inclusive). If action is null, every action is returned (see AuditAction).</summary>
        Task<IEnumerable<ReservationAuditDto>> GetByRangeAsync(DateTime from, DateTime to, string? action, int requestingUserId);
    }
}
