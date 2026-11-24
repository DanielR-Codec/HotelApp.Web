using System.ComponentModel.DataAnnotations;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.API.Dtos
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }

    public class UserInfoDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public static UserInfoDto FromEntity(User user) => new()
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAtUtc { get; set; }
        public UserInfoDto User { get; set; } = new();
    }
}
