using Microsoft.AspNetCore.Mvc;
using CollectionKnowledgeProject.Data;
using CollectionKnowledgeProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CollectionKnowledgeProject.Controllers
{
    public class UsersController : Controller
    {
        public readonly ApplicationDbContext db;
        public readonly UserManager<ApplicationUser> _userManager;
        public readonly RoleManager<IdentityRole> _roleManager;
        public readonly SignInManager<ApplicationUser> _signInManager;
        private IWebHostEnvironment _env;
        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env, SignInManager<ApplicationUser> signInManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
            _signInManager = signInManager;
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            var roles = _roleManager.Roles.ToList();

            ViewBag.users = users;
            ViewBag.roles = roles;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            ViewBag.user = user;
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(ApplicationUser newUser, IFormFile ProfilePhoto)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if(newUser.PhoneNumber != null)
            {
                user.PhoneNumber = newUser.PhoneNumber;
            }

            if(newUser.PhoneNumber != null)
            {
                user.FirstName = newUser.FirstName;
            }

            if (newUser.LastName != null)
            {
                user.LastName = newUser.LastName;
            }

            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                var storagePath = Path.Combine(
                    _env.WebRootPath, 
                    "images",
                    ProfilePhoto.FileName
                );

                var databaseFileName = "/images/" + ProfilePhoto.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await ProfilePhoto.CopyToAsync(fileStream);
                }
                user.ProfilePhotoPath = databaseFileName;
            }

            await _userManager.UpdateAsync(user);

            return RedirectToAction("Profile", "Users");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(string userId, string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            await _userManager.AddToRoleAsync(user, role.Name);

            return RedirectToAction("Index");
        }

    }

}
