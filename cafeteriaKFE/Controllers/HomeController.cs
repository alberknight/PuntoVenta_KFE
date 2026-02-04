using cafeteriaKFE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cafeteriaKFE.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        /// <summary>
        /// Punto de entrada después del login.
        /// Redirige según rol.
        /// </summary>
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction(nameof(Dashboard));

            // Cajero / Empleado
            return RedirectToAction("Venta", "Pos");
        }

        /// <summary>
        /// Dashboard principal (solo Admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        public IActionResult Dashboard()
        {
            // Más adelante aquí cargarás:
            // - ventas del día
            // - KPIs
            // - gráficas
            // - alertas

            return View();
        }
    }
}
