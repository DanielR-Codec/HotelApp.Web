using System.ComponentModel.DataAnnotations;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.API.Dtos
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string Number { get; set; } = string.Empty;
        public int FloorId { get; set; }
        public int RoomTypeId { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public static RoomDto FromEntity(Room room) => new()
        {
            RoomId = room.RoomId,
            Number = room.Number,
            FloorId = room.FloorId,
            RoomTypeId = room.RoomTypeId,
            CurrentStatus = room.CurrentStatus,
            IsActive = room.IsActive
        };
    }

    /// <summary>Body of POST /api/rooms. A new room is always created active and "Disponible".</summary>
    public class RoomCreateRequest
    {
        [Required(ErrorMessage = "El número de habitación es obligatorio.")]
        [StringLength(10, ErrorMessage = "El número no puede pasar de {1} caracteres.")]
        public string Number { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el piso de la habitación.")]
        public int FloorId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el tipo de habitación.")]
        public int RoomTypeId { get; set; }

        public Room ToEntity(int roomId = 0, bool isActive = true) => new()
        {
            RoomId = roomId,
            Number = Number,
            FloorId = FloorId,
            RoomTypeId = RoomTypeId,
            IsActive = isActive
        };
    }

    /// <summary>Body of PUT /api/rooms/{id}. The status is not changed here: use PATCH /api/rooms/{id}/status.</summary>
    public class RoomUpdateRequest : RoomCreateRequest
    {
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Body of PATCH /api/rooms/{id}/status (CU-012): "Disponible", "En Limpieza" or "Mantenimiento".</summary>
    public class RoomStatusRequest
    {
        [Required(ErrorMessage = "El estado es obligatorio.")]
        public string Status { get; set; } = string.Empty;
    }
}
