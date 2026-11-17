using System.ComponentModel.DataAnnotations;
using HotelApp.DAL.Models.dbo;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelApp.Web.Models.Blocks
{
    public class BlockListItemViewModel
    {
        public int BlockId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class BlockIndexViewModel
    {
        public List<BlockListItemViewModel> Items { get; set; } = new();
        public int? HotelId { get; set; }
        public bool IncludeInactive { get; set; }
        public IEnumerable<SelectListItem> HotelOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }

    public class BlockFormViewModel
    {
        public int BlockId { get; set; }

        public bool IsNew => BlockId == 0;

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione el hotel.")]
        [Display(Name = "Hotel")]
        public int HotelId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(60, ErrorMessage = "El nombre no puede pasar de {1} caracteres.")]
        [Display(Name = "Nombre del bloque")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "La descripción no puede pasar de {1} caracteres.")]
        [Display(Name = "Descripción (opcional)")]
        public string? Description { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        public IEnumerable<SelectListItem> HotelOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        public static BlockFormViewModel FromEntity(Block block) => new()
        {
            BlockId = block.BlockId,
            HotelId = block.HotelId,
            Name = block.Name,
            Description = block.Description,
            IsActive = block.IsActive
        };

        public Block ToEntity() => new()
        {
            BlockId = BlockId,
            HotelId = HotelId,
            Name = Name,
            Description = Description!,
            IsActive = IsActive
        };
    }
}
