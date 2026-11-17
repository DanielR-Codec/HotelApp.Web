using System.ComponentModel.DataAnnotations;
using HotelApp.DAL.Models.dbo;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelApp.Web.Models.Floors
{
    public class FloorListItemViewModel
    {
        public int FloorId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string BlockName { get; set; } = string.Empty;
        public int Number { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class FloorIndexViewModel
    {
        public List<FloorListItemViewModel> Items { get; set; } = new();
        public int? HotelId { get; set; }
        public int? BlockId { get; set; }
        public bool IncludeInactive { get; set; }
        public IEnumerable<SelectListItem> HotelOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> BlockOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }

    public class FloorFormViewModel
    {
        public int FloorId { get; set; }

        public bool IsNew => FloorId == 0;

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione el bloque.")]
        [Display(Name = "Hotel / Bloque")]
        public int BlockId { get; set; }

        [Required(ErrorMessage = "El número de piso es obligatorio.")]
        [Range(0, 200, ErrorMessage = "El número de piso debe estar entre {1} y {2}.")]
        [Display(Name = "Número de piso")]
        public int? Number { get; set; }

        [StringLength(250, ErrorMessage = "La descripción no puede pasar de {1} caracteres.")]
        [Display(Name = "Descripción (opcional)")]
        public string? Description { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        public IEnumerable<SelectListItem> BlockOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        public static FloorFormViewModel FromEntity(Floor floor) => new()
        {
            FloorId = floor.FloorId,
            BlockId = floor.BlockId,
            Number = floor.Number,
            Description = floor.Description,
            IsActive = floor.IsActive
        };

        public Floor ToEntity() => new()
        {
            FloorId = FloorId,
            BlockId = BlockId,
            Number = Number ?? 0,
            Description = Description!,
            IsActive = IsActive
        };
    }
}
