using System.ComponentModel.DataAnnotations;

namespace cafeteriaKFE.Core.Users.Response
{
    public class UpdateUserResponse
    {
        [Required(ErrorMessage = "El ID de usuario es requerido.")]
        [Display(Name = "ID de Usuario")]
        public long userId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(80, ErrorMessage = "El nombre no puede exceder los 80 caracteres.")]
        [Display(Name = "Nombre(s)")]
        public string name { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es requerido.")]
        [StringLength(80, ErrorMessage = "El apellido no puede exceder los 80 caracteres.")]
        [Display(Name = "Apellidos")]
        public string lastName { get; set; } = null!;

        [Phone(ErrorMessage = "Formato de número de teléfono inválido.")]
        [Display(Name = "Número de Teléfono")]
        public string? phoneNumber { get; set; }

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido.")]
        [Display(Name = "Correo Electrónico")]
        public string email { get; set; } = null!;
    }
}