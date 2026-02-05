using cafeteriaKFE.Core.Pos.Request;
using cafeteriaKFE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cafeteriaKFE.Controllers
{
    public class PosController : Controller
    {
        private readonly IPosService _pos;

        public PosController(IPosService pos)
        {
            _pos = pos;
        }
        /// <summary>
        /// Pantalla principal del Punto de Venta (Cajero)
        /// </summary>
        [Authorize(Roles = "Customer,Admin")]
        public IActionResult Venta()
        {
            // Aquí después cargarás:
            // - configuraciones del POS
            // - datos iniciales (si aplica)
            return View();
        }

        /// <summary>
        /// Listado / búsqueda completa de productos
        /// (acceso desde el POS)
        /// </summary>
        [Authorize(Roles = "Customer,Admin")]
        public IActionResult Productos()
        {
            // Placeholder: más adelante aquí cargarás
            // - lista de productos
            // - filtros
            return View();
        }

        /// <summary>
        /// Historial de ventas del día (opcional)
        /// </summary>
        [Authorize(Roles = "Admin")]
        public IActionResult Historial()
        {
            // Placeholder: ventas, tickets, filtros por fecha
            return View();
        }

        // AJAX: buscar por barcode o nombre
        // GET /Pos/Search?query=...
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var results = await _pos.SearchProductsAsync(query, take: 10);
            return Ok(results);
        }

        // AJAX: opciones obligatorias (Sizes y MilkTypes)
        // GET /Pos/Options
        [HttpGet]
        public async Task<IActionResult> Options()
        {
            var data = await _pos.GetOptionsAsync();
            return Ok(data);
        }

        // AJAX: checkout
        // POST /Pos/Checkout
        // Nota: si quieres CSRF aquí, luego agregas antiforgery token en fetch.
        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var result = await _pos.CheckoutAsync(request);
            return Ok(result);
        }
    }
}
