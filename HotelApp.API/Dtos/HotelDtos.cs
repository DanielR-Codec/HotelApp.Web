using System.ComponentModel.DataAnnotations;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.API.Dtos
{
    public class HotelDto
    {
        public int HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; }

        public static HotelDto FromEntity(Hotel hotel) => new()
        {
            HotelId = hotel.HotelId,
            Name = hotel.Name,
            Address = hotel.Address,
            City = hotel.City,
            Phone = hotel.Phone,
            Email = hotel.Email,
            IsActive = hotel.IsActive
        };
    }

    public class HotelCreateRequest
    {
        [Required(ErrorMessage = "El nombre del hotel es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede pasar de {1} caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(200, ErrorMessage = "La dirección no puede pasar de {1} caracteres.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        [StringLength(80, ErrorMessage = "La ciudad no puede pasar de {1} caracteres.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^[0-9+()\-\s]{7,20}$", ErrorMessage = "El teléfono solo puede tener números, espacios y los símbolos + ( ) - (entre 7 y 20 caracteres).")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "El correo no puede pasar de {1} caracteres.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string? Email { get; set; }

        public Hotel ToEntity(int hotelId = 0, bool isActive = true) => new()
        {
            HotelId = hotelId,
            Name = Name,
            Address = Address,
            City = City,
            Phone = Phone,
            Email = Email!,
            IsActive = isActive
        };
    }

    public class HotelUpdateRequest : HotelCreateRequest
    {
        public bool IsActive { get; set; } = true;
    }
}
