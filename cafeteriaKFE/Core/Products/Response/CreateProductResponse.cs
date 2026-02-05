using System.ComponentModel.DataAnnotations;

namespace cafeteriaKFE.Core.Products.Response
{
    public class CreateProductResponse
    {
        [Required(ErrorMessage = "El tipo de producto es requerido.")]
        [Display(Name = "Tipo de producto")]
        public int ProductTypeId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(120, ErrorMessage = "El nombre no puede exceder los 120 caracteres.")]
        [Display(Name = "Nombre del producto")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "El código de barras es requerido.")]
        [Range(1, int.MaxValue, ErrorMessage = "El código de barras debe ser mayor a 0.")]
        [Display(Name = "Código de barras")]
        public int BarCode { get; set; }

        [Required(ErrorMessage = "El precio base es requerido.")]
        [Range(0, 999999, ErrorMessage = "El precio base no puede ser negativo.")]
        [Display(Name = "Precio base")]
        public decimal BasePrice { get; set; }
    }
}
