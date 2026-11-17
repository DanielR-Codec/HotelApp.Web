using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Interfaces
{
    public interface IReservationDetailRepository : IBaseRepository<ReservationDetail>
    {
        Task<IEnumerable<ReservationDetail>> GetByReservationAsync(int reservationId);
        Task<IEnumerable<ReservationDetail>> GetByReservationsAsync(IEnumerable<int> reservationIds);
    }
}
