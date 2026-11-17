using HotelApp.BL.Interfaces;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Services
{
    public class ReservationDetailService : IReservationDetailService
    {
        private readonly IReservationDetailRepository _repository;

        public ReservationDetailService(IReservationDetailRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ReservationDetail>> GetAllAsync() => await _repository.GetAllAsync();
        public async Task<ReservationDetail?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task<IEnumerable<ReservationDetail>> GetByReservationAsync(int reservationId) => await _repository.GetByReservationAsync(reservationId);
    }
}
