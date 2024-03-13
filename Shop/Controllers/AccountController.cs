using Application;
using Application.Accounts;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Shop.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController
        (
            UserManager<User> userManager,
            SignInManager<User> signInManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "AdminProduct");
            }
            return View();
        }

        private async Task<User?> FindUser(string input)
        {
            return await _userManager.FindByEmailAsync(input) ?? await _userManager.FindByNameAsync(input);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await FindUser(model.EmailOrUsername);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return View(model);
            }
            if (!user.EmailConfirmed)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User haven't confirmed email!"));
                return View(model);
            }
            if (await _userManager.IsLockedOutAsync(user))
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User is locked out!"));
                return View(model);
            }
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"User {user.UserName} logged in as {string.Join(", ", roles)}"));
                return RedirectToAction("Index", "AdminProduct");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User password not correct!"));
            return View(model);
        }

        [Authorize]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}