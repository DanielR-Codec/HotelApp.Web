using System;

namespace HotelApp.BL.Dtos
{
    /// <summary>One row of a reservation's history (RNF-007): who did what and when.</summary>
    public class ReservationAuditDto
    {
        public int ReservationAuditId { get; set; }
        public int ReservationId { get; set; }
        public string ReservationCode { get; set; } = string.Empty;

        /// <summary>Name of the user, or "Sistema" when the action was automatic (for example, a No-Show).</summary>
        public string UserName { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public decimal? Amount { get; set; }
        public string? Details { get; set; }
    }
}
