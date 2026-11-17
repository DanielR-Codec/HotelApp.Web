using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApp.DAL.Models.dbo
{
    public class ReservationDetail : IValidatableObject
    {
        [Key]
        public int ReservationDetailId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El detalle debe pertenecer a una reserva.")]
        public int ReservationId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El detalle debe indicar la habitación.")]
        public int RoomId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UnitPrice < 0)
                yield return new ValidationResult("El precio por noche no puede ser negativo.", new[] { nameof(UnitPrice) });
        }
    }
}
