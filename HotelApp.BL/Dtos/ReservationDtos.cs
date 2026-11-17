using System;
using System.Collections.Generic;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.BL.Dtos
{
    /// <summary>Data sent by the screen to create a reservation (CU-005).</summary>
    public class CreateReservationRequest
    {
        public int GuestId { get; set; }

        /// <summary>User handling the reservation (the logged-in receptionist or administrator).</summary>
        public int UserId { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int GuestCount { get; set; }
        public List<int> RoomIds { get; set; } = new();

        /// <summary>RN-004: true = guarantee payment received, the reservation starts as "Confirmed"; false = starts as "Pending".</summary>
        public bool GuaranteePaymentReceived { get; set; }

        /// <summary>RN-012: 0 to 100. A receptionist cannot go above 50.</summary>
        public decimal DiscountPercentage { get; set; }
    }

    public class ReservationDetailDto
    {
        public int ReservationDetailId { get; set; }
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public decimal Subtotal { get; set; }
    }

    /// <summary>Reservation with its guest and rooms, ready to show on screen or build the email.</summary>
    public class FullReservationDto
    {
        public Reservation Reservation { get; set; } = null!;
        public Guest Guest { get; set; } = null!;
        public int Nights { get; set; }
        public List<ReservationDetailDto> Rooms { get; set; } = new();
    }

    public class CancellationResult
    {
        public int ReservationId { get; set; }
        public string ReservationCode { get; set; } = string.Empty;

        /// <summary>RN-008: 100% of the first night if cancelled on the arrival day (or later).</summary>
        public decimal PenaltyApplied { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CheckOutResult
    {
        public string ReservationCode { get; set; } = string.Empty;

        /// <summary>RN-007: true if the guest left after 12:00 on the check-out day.</summary>
        public bool LateCheckOut { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
