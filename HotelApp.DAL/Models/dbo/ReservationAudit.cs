using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApp.DAL.Models.dbo
{
    /// <summary>
    /// Log of what happens to a reservation: who did it, what and when (RNF-007).
    /// UserId is null when the system did it (for example, an automatic No-Show).
    /// </summary>
    public class ReservationAudit : IValidatableObject
    {
        [Key]
        public int ReservationAuditId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El registro de auditoría debe pertenecer a una reserva.")]
        public int ReservationId { get; set; }

        public int? UserId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "La acción auditada es obligatoria.")]
        [StringLength(30, ErrorMessage = "La acción auditada no puede pasar de {1} caracteres.")]
        public string Action { get; set; }

        public DateTime Timestamp { get; set; }

        /// <summary>Amount tied to the action (for example, the penalty for a late cancellation, RN-008).</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }

        [StringLength(500, ErrorMessage = "El detalle de la auditoría no puede pasar de {1} caracteres.")]
        public string Details { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UserId is <= 0)
                yield return new ValidationResult("El usuario de la auditoría no es válido.", new[] { nameof(UserId) });

            if (Timestamp == default)
                yield return new ValidationResult("La fecha y hora de la auditoría es obligatoria.", new[] { nameof(Timestamp) });

            if (Amount < 0)
                yield return new ValidationResult("El monto de la auditoría no puede ser negativo.", new[] { nameof(Amount) });
        }
    }
}
