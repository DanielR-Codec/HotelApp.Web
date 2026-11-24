using System.ComponentModel.DataAnnotations;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.API.Dtos
{
    public class FloorDto
    {
        public int FloorId { get; set; }
        public int BlockId { get; set; }
        public int Number { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public static FloorDto FromEntity(Floor floor) => new()
        {
            FloorId = floor.FloorId,
            BlockId = floor.BlockId,
            Number = floor.Number,
            Description = floor.Description,
            IsActive = floor.IsActive
        };
    }

    /// <summary>Body of POST /api/floors. A new floor is always created active.</summary>
    public class FloorCreateRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el bloque al que pertenece el piso.")]
        public int BlockId { get; set; }

        [Range(0, 200, ErrorMessage = "El número de piso debe estar entre {1} y {2}.")]
        public int Number { get; set; }

        [StringLength(250, ErrorMessage = "La descripción no puede pasar de {1} caracteres.")]
        public string? Description { get; set; }

        public Floor ToEntity(int floorId = 0, bool isActive = true) => new()
        {
            FloorId = floorId,
            BlockId = BlockId,
            Number = Number,
            Description = Description!,
            IsActive = isActive
        };
    }

    /// <summary>Body of PUT /api/floors/{id}. A floor that already has rooms cannot move to another block.</summary>
    public class FloorUpdateRequest : FloorCreateRequest
    {
        public bool IsActive { get; set; } = true;
    }
}
