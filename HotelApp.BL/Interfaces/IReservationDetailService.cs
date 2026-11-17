using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Interfaces
{
    /// <summary>
    /// Read-only. Details (the rooms of a reservation) are created and modified
    /// only through IReservationService, so they never skip the business rules.
    /// </summary>
    public interface IReservationDetailService
    {
        Task<IEnumerable<ReservationDetail>> GetAllAsync();
        Task<ReservationDetail?> GetByIdAsync(int id);
        Task<IEnumerable<ReservationDetail>> GetByReservationAsync(int reservationId);
    }
}
