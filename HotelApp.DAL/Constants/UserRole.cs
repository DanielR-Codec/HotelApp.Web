namespace HotelApp.DAL.Constants
{
    /// <summary>System roles (column User.Role). The values are what gets stored in the database.</summary>
    public static class UserRole
    {
        public const string Administrator = "Administrador";
        public const string Receptionist = "Recepcionista";

        public static readonly string[] All = { Administrator, Receptionist };
    }
}
