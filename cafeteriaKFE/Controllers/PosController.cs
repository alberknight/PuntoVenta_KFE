using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cafeteriaKFE.Controllers
{
    public class PosController : Controller
    {
        /// <summary>
        /// Pantalla principal del Punto de Venta (Cajero)
        /// </summary>
        [Authorize(Roles = "Cashier,Admin")]
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
        [Authorize(Roles = "Cashier,Admin")]
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

        // -------------------------------------------------
        // FUTURO (no implementar todavía)
        // -------------------------------------------------

        // [HttpGet]
        // public IActionResult Search(string query)
        // {
        //     // Buscar producto por código o nombre
        //     // Retornar JSON
        // }

        // [HttpPost]
        // public IActionResult Checkout(CheckoutRequest request)
        // {
        //     // Guardar Order + OrderDetail + Payment
        //     // Transacción
        // }
    }
}
