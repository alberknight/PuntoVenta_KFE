using System.ComponentModel.DataAnnotations;

namespace cafeteriaKFE.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Display(Name = "¿Recordarme?")]
        public bool RememberMe { get; set; }
    }
}
