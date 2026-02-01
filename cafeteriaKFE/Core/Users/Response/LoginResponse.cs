using System.ComponentModel.DataAnnotations;

namespace cafeteriaKFE.Models.Users
{
    public class LoginResponse
    {
        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;

        [Display(Name = "¿Recordarme?")]
        public bool RememberMe { get; set; }
    }
}
