using System.ComponentModel.DataAnnotations;

namespace HotelApp.DAL.Models.dbo
{
    /// <summary>A hotel (property). It is made of blocks, which have floors, which have rooms.</summary>
    public class Hotel
    {
        [Key]
        public int HotelId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre del hotel es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del hotel no puede pasar de {1} caracteres.")]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "La dirección del hotel es obligatoria.")]
        [StringLength(200, ErrorMessage = "La dirección no puede pasar de {1} caracteres.")]
        public string Address { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "La ciudad del hotel es obligatoria.")]
        [StringLength(80, ErrorMessage = "La ciudad no puede pasar de {1} caracteres.")]
        public string City { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El teléfono del hotel es obligatorio.")]
        [StringLength(30, ErrorMessage = "El teléfono no puede pasar de {1} caracteres.")]
        [RegularExpression(@"^[0-9+()\-\s]{7,20}$", ErrorMessage = "El teléfono solo puede tener números, espacios y los símbolos + ( ) - (entre 7 y 20 caracteres).")]
        public string Phone { get; set; }

        [StringLength(150, ErrorMessage = "El correo no puede pasar de {1} caracteres.")]
        [EmailAddress(ErrorMessage = "El correo del hotel no tiene un formato válido.")]
        public string Email { get; set; }

        /// <summary>Soft delete: false = hotel deactivated.</summary>
        public bool IsActive { get; set; } = true;
    }
}
