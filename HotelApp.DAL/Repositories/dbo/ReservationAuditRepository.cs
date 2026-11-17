using System;
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
    public class ReservationAuditRepository : BaseRepository<ReservationAudit>, IReservationAuditRepository
    {
        public ReservationAuditRepository(HotelContext context, ILogger<ReservationAuditRepository> logger) : base(context, logger) { }

        public Task<IEnumerable<ReservationAudit>> GetByReservationAsync(int reservationId)
        {
            if (reservationId <= 0)
                return Task.FromResult(Enumerable.Empty<ReservationAudit>());

            return ExecuteAsync<IEnumerable<ReservationAudit>>(async () =>
                await _dbSet.Where(a => a.ReservationId == reservationId)
                            .OrderBy(a => a.Timestamp)
                            .ToListAsync(),
                nameof(GetByReservationAsync));
        }

        public Task<IEnumerable<ReservationAudit>> GetByRangeAsync(DateTime from, DateTime toExclusive, string action = null)
        {
            if (toExclusive <= from)
                throw new ArgumentException("El final del rango debe ser posterior al inicio.", nameof(toExclusive));

            return ExecuteAsync<IEnumerable<ReservationAudit>>(async () =>
            {
                var query = _dbSet.Where(a => a.Timestamp >= from && a.Timestamp < toExclusive);

                if (!string.IsNullOrWhiteSpace(action))
                    query = query.Where(a => a.Action == action);

                return await query.OrderBy(a => a.Timestamp).ToListAsync();
            }, nameof(GetByRangeAsync));
        }
    }
}
