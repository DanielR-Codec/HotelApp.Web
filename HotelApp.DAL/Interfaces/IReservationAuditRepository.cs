using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Interfaces
{
    public interface IReservationAuditRepository : IBaseRepository<ReservationAudit>
    {
        Task<IEnumerable<ReservationAudit>> GetByReservationAsync(int reservationId);

        /// <summary>Records within [from, toExclusive). If action is null, all actions are returned.</summary>
        Task<IEnumerable<ReservationAudit>> GetByRangeAsync(DateTime from, DateTime toExclusive, string action = null);
    }
}
