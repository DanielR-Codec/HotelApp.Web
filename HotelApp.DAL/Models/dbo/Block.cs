using System.ComponentModel.DataAnnotations;

namespace HotelApp.DAL.Models.dbo
{
    /// <summary>A building, tower or wing of a hotel (for example "Torre A"). It contains floors.</summary>
    public class Block
    {
        [Key]
        public int BlockId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el hotel al que pertenece el bloque.")]
        public int HotelId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre del bloque es obligatorio.")]
        [StringLength(60, ErrorMessage = "El nombre del bloque no puede pasar de {1} caracteres.")]
        public string Name { get; set; }

        [StringLength(250, ErrorMessage = "La descripción no puede pasar de {1} caracteres.")]
        public string Description { get; set; }

        /// <summary>Soft delete: false = block deactivated.</summary>
        public bool IsActive { get; set; } = true;
    }
}
