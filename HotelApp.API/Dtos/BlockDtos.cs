using System.ComponentModel.DataAnnotations;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.API.Dtos
{
    public class BlockDto
    {
        public int BlockId { get; set; }
        public int HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public static BlockDto FromEntity(Block block) => new()
        {
            BlockId = block.BlockId,
            HotelId = block.HotelId,
            Name = block.Name,
            Description = block.Description,
            IsActive = block.IsActive
        };
    }

    /// <summary>Body of POST /api/blocks. A new block is always created active.</summary>
    public class BlockCreateRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el hotel al que pertenece el bloque.")]
        public int HotelId { get; set; }

        [Required(ErrorMessage = "El nombre del bloque es obligatorio.")]
        [StringLength(60, ErrorMessage = "El nombre no puede pasar de {1} caracteres.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "La descripción no puede pasar de {1} caracteres.")]
        public string? Description { get; set; }

        public Block ToEntity(int blockId = 0, bool isActive = true) => new()
        {
            BlockId = blockId,
            HotelId = HotelId,
            Name = Name,
            Description = Description!,
            IsActive = isActive
        };
    }

    /// <summary>Body of PUT /api/blocks/{id}. A block that already has floors cannot move to another hotel.</summary>
    public class BlockUpdateRequest : BlockCreateRequest
    {
        public bool IsActive { get; set; } = true;
    }
}
