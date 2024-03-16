using Application;
using Application.Accounts;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Shop.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private const string ImageFolder = "AccountAvatars";


        public AccountController
        (
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
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

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new UpdateAccountViewModel
            {
                UserName = user!.UserName!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
        {
            if (currentPassword.IsNullOrEmpty() || newPassword.IsNullOrEmpty() || confirmNewPassword.IsNullOrEmpty())
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Invalid change password information!"));
                return RedirectToAction("Profile");
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Profile");
            }
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "Change password successfully!"));
                return RedirectToAction("Profile");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Change password failed!"));
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateAccountViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Profile");
            }
            if (!user.UserName!.Equals(model.UserName))
            {
                var checkUser = await _userManager.FindByNameAsync(model.UserName);
                if (checkUser != null)
                {
                    TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "New username has been used!"));
                    return RedirectToAction("Profile");
                }
                user.UserName = model.UserName;
            }
            user.PhoneNumber = model.PhoneNumber;
            if (model.Avatar != null)
            {
                DeleteImage(user.AvatarUrl);
                user.AvatarUrl = await SaveImage(model.Avatar);
            }
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User update failed!"));
                return RedirectToAction("Profile");
            }
            await _signInManager.RefreshSignInAsync(user);
            if (!user.Email!.Equals(model.Email))
            {
                var checkUser = await _userManager.FindByEmailAsync(model.Email);
                if (checkUser != null)
                {
                    TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Updated user, but new email has been used!"));
                    return RedirectToAction("Profile");
                }
                var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
                var sendEmailResult = await SendChangeConfirmEmail(user, model.Email, token);

                if (!sendEmailResult)
                {
                    TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Change email failed!"));
                    return RedirectToAction("Profile");
                }
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"User '{user.UserName}' updated successfully"));
            return RedirectToAction("Profile");
        }

        private async Task<bool> SendChangeConfirmEmail(User user, string newEmail, string token)
        {
            try
            {
                var callbackUrl = Url.Action("ConfirmChangeEmail", "Account", new { userId = user.Id, newEmail = newEmail, token = token }, protocol: HttpContext.Request.Scheme);
                var subject = "Confirm your new email";
                var message = $"Please confirm your new email by <a href='{callbackUrl}'>clicking here</a>.";
                await _emailSender.SendEmailAsync(newEmail, subject, message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmChangeEmail(string userId, string newEmail, string token)
        {
            if (userId.IsNullOrEmpty() || newEmail.IsNullOrEmpty() || token.IsNullOrEmpty())
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Invalid confirm information!"));
                return RedirectToAction("Login");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Login");
            }
            if (user.Email!.Equals(newEmail))
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "New email is the same as old email!"));
                return RedirectToAction("Login");
            }
            var checkUser = await _userManager.FindByEmailAsync(newEmail);
            if (checkUser != null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "This email has been used!"));
                return RedirectToAction("Login");
            }
            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "Confirm email change successfully!"));
                return RedirectToAction("Login");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Confirm email change failed!"));
            return RedirectToAction("Login");
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, ShopConstants.UploadFolder);
            string productImageDir = Path.Combine(uploadFolder, ImageFolder);
            if (!Directory.Exists(productImageDir))
            {
                Directory.CreateDirectory(productImageDir);
            }
            string fileName = $"{Guid.NewGuid()}-{image.FileName}";
            string fileUrl = $"/{ShopConstants.UploadFolder}/{ImageFolder}/{fileName}";
            using var stream = new FileStream(Path.Combine(productImageDir, fileName), FileMode.Create);
            await image.CopyToAsync(stream);
            return fileUrl;
        }

        private void DeleteImage(string image)
        {
            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, ShopConstants.UploadFolder);
            var productImageDir = Path.Combine(uploadFolder, ImageFolder);
            if (image.IsNullOrEmpty()) return;
            var fileName = image.Split('/').Last();
            var filePath = Path.Combine(productImageDir, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}