using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using cafeteriaKFE.Models;
using cafeteriaKFE.Services;

namespace cafeteriaKFE.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public IList<Product> Products { get;set; }

        public async Task OnGetAsync()
        {
            Products = await _productService.GetAllProducts();
        }
    }
}