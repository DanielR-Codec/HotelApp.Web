using System.ComponentModel.DataAnnotations;
using HotelApp.DAL.Models.dbo;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelApp.Web.Models.Rooms
{
    public class RoomListItemViewModel
    {
        public int RoomId { get; set; }
        public string Number { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public string BlockName { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public decimal BaseRate { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class RoomIndexViewModel
    {
        public List<RoomListItemViewModel> Items { get; set; } = new();
        public int? HotelId { get; set; }
        public int? BlockId { get; set; }
        public int? FloorId { get; set; }
        public bool IncludeInactive { get; set; }
        public IEnumerable<SelectListItem> HotelOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> BlockOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> FloorOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        /// <summary>CU-012: statuses that can be set by hand (Occupied is only set by the check-in).</summary>
        public IEnumerable<string> ManualStatuses { get; set; } = Enumerable.Empty<string>();
    }

    public class RoomFormViewModel
    {
        public int RoomId { get; set; }

        public bool IsNew => RoomId == 0;

        [Required(ErrorMessage = "El número de habitación es obligatorio.")]
        [StringLength(10, ErrorMessage = "El número no puede pasar de {1} caracteres.")]
        [Display(Name = "Número de habitación")]
        public string Number { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione el piso.")]
        [Display(Name = "Hotel / Bloque / Piso")]
        public int FloorId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione el tipo de habitación.")]
        [Display(Name = "Tipo de habitación")]
        public int RoomTypeId { get; set; }

        [Display(Name = "Activa")]
        public bool IsActive { get; set; } = true;

        /// <summary>Only shown when editing; it is changed from the list (CU-012) or by check-in / check-out.</summary>
        public string? CurrentStatus { get; set; }

        public IEnumerable<SelectListItem> FloorOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> RoomTypeOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        public static RoomFormViewModel FromEntity(Room room) => new()
        {
            RoomId = room.RoomId,
            Number = room.Number,
            FloorId = room.FloorId,
            RoomTypeId = room.RoomTypeId,
            IsActive = room.IsActive,
            CurrentStatus = room.CurrentStatus
        };

        public Room ToEntity() => new()
        {
            RoomId = RoomId,
            Number = Number,
            FloorId = FloorId,
            RoomTypeId = RoomTypeId,
            IsActive = IsActive
        };
    }
}
