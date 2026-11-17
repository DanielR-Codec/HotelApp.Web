namespace HotelApp.DAL.Constants
{
    /// <summary>Actions recorded in ReservationAudit (RNF-007). The values are what gets stored in the database.</summary>
    public static class AuditAction
    {
        public const string Created = "Creada";
        public const string Confirmed = "Confirmada";
        public const string Modified = "Modificada";
        public const string Cancelled = "Cancelada";
        public const string CheckIn = "Check-in";
        public const string CheckOut = "Check-out";
        public const string NoShow = "No Show";
    }
}
