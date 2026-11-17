using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using HotelApp.DAL.Constants;

namespace HotelApp.DAL.Models.dbo
{
    public class Reservation : IValidatableObject
    {
        [Key]
        public int ReservationId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El código de reserva es obligatorio.")]
        [StringLength(30, ErrorMessage = "El código de reserva no puede pasar de {1} caracteres.")]
        public string ReservationCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La reserva debe tener un huésped titular.")]
        public int GuestId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La reserva debe indicar el usuario que la registró.")]
        public int UserId { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El estado de la reserva es obligatorio.")]
        [StringLength(30, ErrorMessage = "El estado de la reserva no puede pasar de {1} caracteres.")]
        public string Status { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public DateTime CreatedAt { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CheckInDate == default)
                yield return new ValidationResult("La fecha de llegada es obligatoria.", new[] { nameof(CheckInDate) });

            if (CheckOutDate == default)
                yield return new ValidationResult("La fecha de salida es obligatoria.", new[] { nameof(CheckOutDate) });

            // RN-003: there are no 0-night reservations
            if (CheckInDate != default && CheckOutDate != default && CheckOutDate.Date <= CheckInDate.Date)
                yield return new ValidationResult("La fecha de salida debe ser al menos un día después de la llegada.", new[] { nameof(CheckOutDate) });

            if (!ReservationStatus.All.Contains(Status))
                yield return new ValidationResult(
                    $"El estado '{Status}' no es válido. Estados permitidos: {string.Join(", ", ReservationStatus.All)}.", new[] { nameof(Status) });

            if (Total < 0)
                yield return new ValidationResult("El total de la reserva no puede ser negativo.", new[] { nameof(Total) });

            if (CreatedAt == default)
                yield return new ValidationResult("La fecha de registro es obligatoria.", new[] { nameof(CreatedAt) });
        }
    }
}
