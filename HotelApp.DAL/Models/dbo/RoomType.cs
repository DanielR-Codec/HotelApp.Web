using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApp.DAL.Models.dbo
{
    public class RoomType : IValidatableObject
    {
        [Key]
        public int RoomTypeId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre del tipo de habitación es obligatorio.")]
        [StringLength(60, ErrorMessage = "El nombre del tipo de habitación no puede pasar de {1} caracteres.")]
        public string Name { get; set; }

        [Range(1, 20, ErrorMessage = "La capacidad máxima debe estar entre {1} y {2} personas.")]
        public int MaxCapacity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseRate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BaseRate <= 0)
                yield return new ValidationResult("La tarifa base debe ser mayor que cero.", new[] { nameof(BaseRate) });
        }
    }
}
