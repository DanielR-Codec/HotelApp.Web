using System;
using System.Threading.Tasks;
using HotelApp.BL.Dtos;

namespace HotelApp.BL.Interfaces
{
    /// <summary>
    /// Reports (CU-014 / RF-015). Only the Administrator can request them.
    /// The BL returns the data; showing it on screen or exporting it to PDF is the Web layer's job.
    /// </summary>
    public interface IReportService
    {
        /// <summary>RF-015: reservations touching the date range, with totals per status.</summary>
        Task<ReservationsReportDto> ReservationsByRangeAsync(DateTime from, DateTime to, int requestingUserId);

        /// <summary>CU-014: occupancy percentage and revenue for the range.</summary>
        Task<OccupancyReportDto> OccupancyAsync(DateTime from, DateTime to, int requestingUserId);
    }
}
