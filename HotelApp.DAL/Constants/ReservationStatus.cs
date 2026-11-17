namespace HotelApp.DAL.Constants
{
    /// <summary>Possible states of a reservation (column Reservation.Status). The values are what gets stored in the database.</summary>
    public static class ReservationStatus
    {
        public const string Pending = "Pendiente";       // created, no guarantee payment yet (RN-004)
        public const string Confirmed = "Confirmada";    // guarantee payment received (RN-004)
        public const string InProgress = "En Curso";     // guest already checked in (CU-008)
        public const string Completed = "Completada";    // guest already checked out (CU-009)
        public const string Cancelled = "Cancelada";     // CU-007
        public const string NoShow = "No Show";          // RN-009

        /// <summary>States that block the room for other reservations (RN-001).</summary>
        public static readonly string[] Active = { Pending, Confirmed, InProgress };

        public static readonly string[] All = { Pending, Confirmed, InProgress, Completed, Cancelled, NoShow };
    }
}
