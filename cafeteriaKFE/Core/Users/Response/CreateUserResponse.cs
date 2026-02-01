using System.ComponentModel.DataAnnotations;

namespace cafeteriaKFE.Models.Users
{
    public class CreateUserResponse
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(80, ErrorMessage = "El nombre no puede exceder los 80 caracteres.")]
        [Display(Name = "Nombre(s)")]
        public string name { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es requerido.")]
        [StringLength(80, ErrorMessage = "El apellido no puede exceder los 80 caracteres.")]
        [Display(Name = "Apellidos")]
        public string lastname { get; set; } = null!;

        [Phone(ErrorMessage = "Formato de número de teléfono inválido.")]
        [Display(Name = "Número de Teléfono")]
        public string? phoneNumber { get; set; }

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido.")]
        [Display(Name = "Correo Electrónico")]
        public string email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string password { get; set; } = null!;
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la contraseña de confirmación no coinciden.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
