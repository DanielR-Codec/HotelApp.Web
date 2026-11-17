namespace HotelApp.BL.Dtos
{
    /// <summary>One row of the "Search Availability" result (CU-004, step 5: the room with its price).</summary>
    public class AvailableRoomDto
    {
        public int RoomId { get; set; }
        public string Number { get; set; } = string.Empty;
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string BlockName { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public int RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public decimal RatePerNight { get; set; }
        public int Nights { get; set; }
        public decimal EstimatedTotal { get; set; }
    }
}
