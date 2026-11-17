using System.ComponentModel.DataAnnotations;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.Web.Models.RoomTypes
{
    public class RoomTypeFormViewModel
    {
        public int RoomTypeId { get; set; }

        public bool IsNew => RoomTypeId == 0;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(60, ErrorMessage = "El nombre no puede pasar de {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La capacidad máxima es obligatoria.")]
        [Range(1, 20, ErrorMessage = "La capacidad máxima debe estar entre {1} y {2} personas.")]
        [Display(Name = "Capacidad máxima (personas)")]
        public int? MaxCapacity { get; set; }

        [Required(ErrorMessage = "La tarifa base es obligatoria.")]
        [Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "La tarifa base debe ser mayor que cero.",
            ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
        [Display(Name = "Tarifa base por noche (RD$)")]
        public decimal? BaseRate { get; set; }

        public static RoomTypeFormViewModel FromEntity(RoomType roomType) => new()
        {
            RoomTypeId = roomType.RoomTypeId,
            Name = roomType.Name,
            MaxCapacity = roomType.MaxCapacity,
            BaseRate = roomType.BaseRate
        };

        public RoomType ToEntity() => new()
        {
            RoomTypeId = RoomTypeId,
            Name = Name,
            MaxCapacity = MaxCapacity ?? 0,
            BaseRate = BaseRate ?? 0
        };
    }
}
