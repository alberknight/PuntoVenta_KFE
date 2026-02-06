using cafeteriaKFE.Core.Users.Request; // Added for request models
using cafeteriaKFE.Core.Users.Response; // Added for response models
using cafeteriaKFE.Models;
using cafeteriaKFE.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace cafeteriaKFE.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: User
        public async Task<IActionResult> All([FromQuery] GetUsersRequest? request = null)
        {
            var users = await _userService.GetAllUsers();
            return View(users);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(long id) // Keep long id
        {
            if (id == 0) // Check for default long value instead of null
            {
                return NotFound();
            }

            var user = await _userService.GetUserById(id); // Returns GetUsersDetailsRequest
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View(new CreateUserResponse());
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserResponse request)
        {
            
                var result = await _userService.CreateUser(request); // Get IdentityResult
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(All)); // Changed from Index to All
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            
            return View(request);
        }

        // GET: User/Update/5
        public async Task<IActionResult> Update(long id) // Keep long id
        {
            if (id == 0) // Check for default long value instead of null
            {
                return NotFound();
            }

            var user = await _userService.GetUserById(id); // Returns GetUsersDetailsRequest
            if (user == null)
            {
                return NotFound();
            }

            // Map GetUsersDetailsRequest to UpdateUserResponse
            var updateUserResponse = new UpdateUserResponse
            {
                userId = user.userId, // Using userId from GetUsersDetailsRequest
                email = user.email,
                name = user.name,
                lastName = user.lastname,
                phoneNumber = user.phoneNumber
            };
            return View(updateUserResponse);
        }

        // POST: User/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            long id, // Keep long id
            UpdateUserResponse request)
        {
            if (id != request.userId) // Compare long with long
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _userService.UpdateUser(request); // Get IdentityResult
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(All)); // Changed from Index to All
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(request);
        }

        // GET: User/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _userService.DeleteUser(id.ToString());

            if (!result.Succeeded)
            {
                TempData["Error"] = "No se pudo eliminar el usuario.";
                return RedirectToAction(nameof(All));
            }

            TempData["Success"] = "Usuario eliminado correctamente.";
            return RedirectToAction(nameof(All));
        }


        private bool UserExists(string id)
        {
            return _userService.UserExists(id);
        }
    }
}
