using System;

namespace HotelApp.BL.Constants
{
    /// <summary>Business rule values (section 8 of the analysis and design document), in one place.</summary>
    public static class HotelRules
    {
        /// <summary>RN-006: earliest time to check in.</summary>
        public static readonly TimeSpan CheckInTime = new(15, 0, 0);

        /// <summary>RN-007: time by which the room must be handed back.</summary>
        public static readonly TimeSpan CheckOutTime = new(12, 0, 0);

        /// <summary>RN-009: after this time on the arrival day, a reservation without check-in is cancelled as No Show.</summary>
        public static readonly TimeSpan NoShowCutoffTime = new(23, 59, 0);

        /// <summary>RN-012: maximum discount (%) that a non-administrator can apply.</summary>
        public const decimal MaxReceptionistDiscount = 50m;

        /// <summary>RN-002: minimum age of the primary guest.</summary>
        public const int MinPrimaryGuestAge = 18;

        public const int MinPasswordLength = 8;
    }
}
