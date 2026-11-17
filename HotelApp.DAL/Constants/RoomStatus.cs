namespace HotelApp.DAL.Constants
{
    /// <summary>Possible states of a room (column Room.CurrentStatus). The values are what gets stored in the database.</summary>
    public static class RoomStatus
    {
        public const string Available = "Disponible";
        public const string Occupied = "Ocupada";
        public const string Cleaning = "En Limpieza";
        public const string Maintenance = "Mantenimiento";

        public static readonly string[] All = { Available, Occupied, Cleaning, Maintenance };
    }
}
