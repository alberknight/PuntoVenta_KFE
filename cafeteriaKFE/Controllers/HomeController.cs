using cafeteriaKFE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using cafeteriaKFE.Services;

namespace cafeteriaKFE.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly IHomeService _dashboard;
        /// <summary>
        /// Punto de entrada después del login.
        /// Redirige según rol.
        /// </summary>
        public HomeController(IHomeService dashboard)
        {
            _dashboard = dashboard;
        }
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
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var vm = await _dashboard.GetDashboardAsync();
            return View(vm); // Views/Inicio/Dashboard.cshtml
        }

    }
}
