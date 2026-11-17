using System.ComponentModel.DataAnnotations;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.Web.Models.Hotels
{
    public class HotelIndexViewModel
    {
        public List<Hotel> Items { get; set; } = new();
        public bool IncludeInactive { get; set; }
    }

    public class HotelFormViewModel
    {
        public int HotelId { get; set; }

        public bool IsNew => HotelId == 0;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede pasar de {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(200, ErrorMessage = "La dirección no puede pasar de {1} caracteres.")]
        [Display(Name = "Dirección")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        [StringLength(80, ErrorMessage = "La ciudad no puede pasar de {1} caracteres.")]
        [Display(Name = "Ciudad")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^[0-9+()\-\s]{7,20}$", ErrorMessage = "Solo números, espacios y + ( ) - (entre 7 y 20 caracteres).")]
        [Display(Name = "Teléfono")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150, ErrorMessage = "El correo no puede pasar de {1} caracteres.")]
        [Display(Name = "Correo (opcional)")]
        public string? Email { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        public static HotelFormViewModel FromEntity(Hotel hotel) => new()
        {
            HotelId = hotel.HotelId,
            Name = hotel.Name,
            Address = hotel.Address,
            City = hotel.City,
            Phone = hotel.Phone,
            Email = hotel.Email,
            IsActive = hotel.IsActive
        };

        public Hotel ToEntity() => new()
        {
            HotelId = HotelId,
            Name = Name,
            Address = Address,
            City = City,
            Phone = Phone,
            Email = Email!,
            IsActive = IsActive
        };
    }
}
