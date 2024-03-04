using Application.Accounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Shop.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController
        (
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult Login()
        {
            _signInManager.SignOutAsync();
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return Redirect("/adminproduct/index");
                }
                ModelState.AddModelError(string.Empty, "password is not correct!");
                return View(model);
            }
            ModelState.AddModelError(string.Empty, "email is not correct");
            return View(model);
        }
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return Redirect("/account/login");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Roles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var role = new IdentityRole
            {
                Name = model.RoleName
            };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return Redirect("/Account/Roles");
            }
            ModelState.AddModelError(string.Empty, GetErrorMessage(result));
            return View(model);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }
        public async Task<IActionResult> CreateUser()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = roles;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email,
            };
            var userResult = await _userManager.CreateAsync(user, model.Password);
            if (userResult.Succeeded)
            {
                var role = await _roleManager.FindByIdAsync(model.RoleId);
                var roleResult = await _userManager.AddToRoleAsync(user, role.Name);
                if (roleResult.Succeeded)
                {
                    return Redirect("/Account/users");
                }
                else
                {
                    await _userManager.DeleteAsync(user);
                    ModelState.AddModelError(string.Empty, GetErrorMessage(roleResult));
                    return View(model);
                }
            }

            ModelState.AddModelError(string.Empty, GetErrorMessage(userResult));
            return View(model);
        }

        private string GetErrorMessage(IdentityResult result)
        {
            if (result.Errors.Any())
            {
                var errorMessage = string.Join(" ", result.Errors.Select(x => x.Description).ToList());
                return errorMessage;
            }
            return string.Empty;
        }
    }
}