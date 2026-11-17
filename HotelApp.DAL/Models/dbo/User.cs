using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HotelApp.DAL.Constants;

namespace HotelApp.DAL.Models.dbo
{
    public class User : IValidatableObject
    {
        [Key]
        public int UserId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre del usuario es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del usuario no puede pasar de {1} caracteres.")]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El correo del usuario es obligatorio.")]
        [StringLength(150, ErrorMessage = "El correo del usuario no puede pasar de {1} caracteres.")]
        [EmailAddress(ErrorMessage = "El correo del usuario no tiene un formato válido.")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "La clave del usuario es obligatoria.")]
        [StringLength(256, ErrorMessage = "El hash de la clave no puede pasar de {1} caracteres.")]
        public string PasswordHash { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El rol del usuario es obligatorio.")]
        [StringLength(30, ErrorMessage = "El rol no puede pasar de {1} caracteres.")]
        public string Role { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!UserRole.All.Contains(Role))
                yield return new ValidationResult(
                    $"El rol '{Role}' no es válido. Roles permitidos: {string.Join(", ", UserRole.All)}.", new[] { nameof(Role) });
        }
    }
}
