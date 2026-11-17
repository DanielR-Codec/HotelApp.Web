using System.ComponentModel.DataAnnotations;

namespace HotelApp.DAL.Models.dbo
{
    /// <summary>A floor of a block. It contains rooms.</summary>
    public class Floor
    {
        [Key]
        public int FloorId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el bloque al que pertenece el piso.")]
        public int BlockId { get; set; }

        [Range(0, 200, ErrorMessage = "El número de piso debe estar entre {1} y {2}.")]
        public int Number { get; set; }

        [StringLength(250, ErrorMessage = "La descripción no puede pasar de {1} caracteres.")]
        public string Description { get; set; }

        /// <summary>Soft delete: false = floor deactivated.</summary>
        public bool IsActive { get; set; } = true;
    }
}
