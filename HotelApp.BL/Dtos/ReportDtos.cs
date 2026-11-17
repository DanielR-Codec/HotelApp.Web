using System;
using System.Collections.Generic;

namespace HotelApp.BL.Dtos
{
    public class ReservationSummaryDto
    {
        public int ReservationId { get; set; }
        public string ReservationCode { get; set; } = string.Empty;
        public string Guest { get; set; } = string.Empty;
        public string Rooms { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    /// <summary>RF-015: list of reservations within a date range.</summary>
    public class ReservationsReportDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<ReservationSummaryDto> Reservations { get; set; } = new();
        public int TotalReservations { get; set; }
        public Dictionary<string, int> ReservationsByStatus { get; set; } = new();

        /// <summary>Sum of the totals of the Confirmed, In Progress and Completed reservations in the list.</summary>
        public decimal EstimatedRevenue { get; set; }

        /// <summary>Penalties charged for late cancellations (RN-008) within the range.</summary>
        public decimal PenaltyRevenue { get; set; }
    }

    /// <summary>CU-014: occupancy and revenue for a date range.</summary>
    public class OccupancyReportDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableNights { get; set; }
        public int OccupiedNights { get; set; }
        public decimal OccupancyPercentage { get; set; }

        /// <summary>Revenue from the nights in the range for Confirmed, In Progress and Completed reservations.</summary>
        public decimal LodgingRevenue { get; set; }
        public decimal PenaltyRevenue { get; set; }
    }
}
