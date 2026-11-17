using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelApp.DAL.Models.dbo
{
    public class Guest : IValidatableObject
    {
        [Key]
        public int GuestId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El documento de identidad es obligatorio.")]
        [StringLength(30, ErrorMessage = "El documento de identidad no puede pasar de {1} caracteres.")]
        public string IdentityDocument { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Los nombres del huésped son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden pasar de {1} caracteres.")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Los apellidos del huésped son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden pasar de {1} caracteres.")]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El teléfono del huésped es obligatorio.")]
        [StringLength(30, ErrorMessage = "El teléfono no puede pasar de {1} caracteres.")]
        [RegularExpression(@"^[0-9+()\-\s]{7,20}$", ErrorMessage = "El teléfono solo puede tener números, espacios y los símbolos + ( ) - (entre 7 y 20 caracteres).")]
        public string Phone { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El correo del huésped es obligatorio.")]
        [StringLength(150, ErrorMessage = "El correo no puede pasar de {1} caracteres.")]
        [EmailAddress(ErrorMessage = "El correo del huésped no tiene un formato válido.")]
        public string Email { get; set; }

        public DateTime BirthDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BirthDate == default || BirthDate.Date > DateTime.Today || BirthDate.Year < 1900)
                yield return new ValidationResult("La fecha de nacimiento no es válida.", new[] { nameof(BirthDate) });
        }
    }
}
