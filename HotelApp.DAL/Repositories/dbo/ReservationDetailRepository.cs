using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelApp.DAL.Base;
using HotelApp.DAL.Context;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelApp.DAL.Repositories.dbo
{
    public class ReservationDetailRepository : BaseRepository<ReservationDetail>, IReservationDetailRepository
    {
        public ReservationDetailRepository(HotelContext context, ILogger<ReservationDetailRepository> logger) : base(context, logger) { }

        public Task<IEnumerable<ReservationDetail>> GetByReservationAsync(int reservationId)
        {
            if (reservationId <= 0)
                return Task.FromResult(Enumerable.Empty<ReservationDetail>());

            return ExecuteAsync<IEnumerable<ReservationDetail>>(async () =>
                await _dbSet.Where(d => d.ReservationId == reservationId).ToListAsync(),
                nameof(GetByReservationAsync));
        }

        public Task<IEnumerable<ReservationDetail>> GetByReservationsAsync(IEnumerable<int> reservationIds)
        {
            var list = (reservationIds ?? Enumerable.Empty<int>()).Where(id => id > 0).Distinct().ToList();
            if (list.Count == 0)
                return Task.FromResult(Enumerable.Empty<ReservationDetail>());

            return ExecuteAsync<IEnumerable<ReservationDetail>>(async () =>
                await _dbSet.Where(d => list.Contains(d.ReservationId)).ToListAsync(),
                nameof(GetByReservationsAsync));
        }
    }
}
