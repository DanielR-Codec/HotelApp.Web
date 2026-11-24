using System.ComponentModel.DataAnnotations;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.API.Dtos
{
    public class RoomTypeDto
    {
        public int RoomTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public decimal BaseRate { get; set; }

        public static RoomTypeDto FromEntity(RoomType roomType) => new()
        {
            RoomTypeId = roomType.RoomTypeId,
            Name = roomType.Name,
            MaxCapacity = roomType.MaxCapacity,
            BaseRate = roomType.BaseRate
        };
    }

    /// <summary>Body of POST /api/roomtypes and PUT /api/roomtypes/{id} (RN-014: Administrator only).</summary>
    public class RoomTypeRequest
    {
        [Required(ErrorMessage = "El nombre del tipo de habitación es obligatorio.")]
        [StringLength(60, ErrorMessage = "El nombre no puede pasar de {1} caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Range(1, 20, ErrorMessage = "La capacidad máxima debe estar entre {1} y {2} personas.")]
        public int MaxCapacity { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999999999", ErrorMessage = "La tarifa base debe ser mayor que cero.")]
        public decimal BaseRate { get; set; }

        public RoomType ToEntity(int roomTypeId = 0) => new()
        {
            RoomTypeId = roomTypeId,
            Name = Name,
            MaxCapacity = MaxCapacity,
            BaseRate = BaseRate
        };
    }
}
