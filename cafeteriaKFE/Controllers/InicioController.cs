using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cafeteriaKFE.Models;
using System.Threading.Tasks;

namespace cafeteriaKFE.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InicioController : Controller
    {
        private readonly UserManager<User> _userManager;

        public InicioController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Admin/ or /Admin/Index
        public async Task<IActionResult> Dashboard()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }
    }
}
