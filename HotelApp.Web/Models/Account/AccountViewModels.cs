using System.ComponentModel.DataAnnotations;

namespace HotelApp.Web.Models.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [Display(Name = "Correo")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Clave")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    /// <summary>Creates the first Administrator of a brand-new installation (CU-013 bootstrap).</summary>
    public class SetupViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede pasar de {1} caracteres.")]
        [Display(Name = "Nombre completo")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150, ErrorMessage = "El correo no puede pasar de {1} caracteres.")]
        [Display(Name = "Correo")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La clave debe tener entre {2} y {1} caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Clave")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme la clave.")]
        [Compare(nameof(Password), ErrorMessage = "Las claves no coinciden.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar clave")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
