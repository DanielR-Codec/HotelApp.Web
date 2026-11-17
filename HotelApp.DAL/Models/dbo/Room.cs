using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HotelApp.DAL.Constants;

namespace HotelApp.DAL.Models.dbo
{
    public class Room : IValidatableObject
    {
        [Key]
        public int RoomId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El número de la habitación es obligatorio.")]
        [StringLength(10, ErrorMessage = "El número de la habitación no puede pasar de {1} caracteres.")]
        public string Number { get; set; }

        /// <summary>Floor where the room is (Floor → Block → Hotel).</summary>
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el piso de la habitación.")]
        public int FloorId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el tipo de habitación.")]
        public int RoomTypeId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El estado de la habitación es obligatorio.")]
        [StringLength(30, ErrorMessage = "El estado de la habitación no puede pasar de {1} caracteres.")]
        public string CurrentStatus { get; set; }

        /// <summary>Soft delete (RF-001): false = room taken out of service.</summary>
        public bool IsActive { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!RoomStatus.All.Contains(CurrentStatus))
                yield return new ValidationResult(
                    $"El estado '{CurrentStatus}' no es válido. Estados permitidos: {string.Join(", ", RoomStatus.All)}.", new[] { nameof(CurrentStatus) });
        }
    }
}
