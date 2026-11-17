using HotelApp.DAL.Models.dbo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.BL.Dtos;

namespace HotelApp.BL.Interfaces
{
    /// <summary>
    /// Reservation lifecycle: Pending → Confirmed → In Progress → Completed,
    /// or Cancelled / No Show. Reservations are never edited or deleted directly:
    /// every change goes through one of these methods, which apply the business rules and leave an audit trail.
    /// </summary>
    public interface IReservationService
    {
        Task<IEnumerable<Reservation>> GetAllAsync();
        Task<Reservation?> GetByIdAsync(int id);
        Task<FullReservationDto?> GetByCodeAsync(string code);
        Task<IEnumerable<Reservation>> GetByGuestAsync(int guestId);

        /// <summary>CU-008, step 1: reservations arriving today (or on the given date).</summary>
        Task<IEnumerable<Reservation>> GetArrivalsForDateAsync(DateTime? date = null);

        /// <summary>CU-005 / RF-005 / RF-006 / RF-013. Everything happens in one transaction (RNF-006).</summary>
        Task<Reservation> CreateAsync(CreateReservationRequest request);

        /// <summary>RN-004: moves a Pending reservation to Confirmed when the guarantee payment is received.</summary>
        Task ConfirmAsync(int reservationId, int userId);

        /// <summary>CU-006 / RN-015: moves the dates and validates availability again.</summary>
        Task UpdateDatesAsync(int reservationId, DateTime newCheckIn, DateTime newCheckOut, int userId);

        /// <summary>CU-007 / RF-007 / RF-014 / RN-008 / RNF-007.</summary>
        Task<CancellationResult> CancelAsync(int reservationId, int userId);

        /// <summary>CU-008 / RF-008 / RN-006.</summary>
        Task CheckInAsync(int reservationId, int userId);

        /// <summary>CU-009 / RF-009 / RN-007 / RN-010.</summary>
        Task<CheckOutResult> CheckOutAsync(int reservationId, int userId);

        /// <summary>
        /// RN-009: cancels reservations that never checked in. Returns how many were marked as No Show.
        /// Must be called periodically (for example, a BackgroundService in the Web layer every few minutes).
        /// </summary>
        Task<int> ProcessNoShowsAsync();
    }
}
