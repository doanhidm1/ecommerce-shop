using Application;
using Application.Roles;
using Domain.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;

namespace Shop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, IUnitOfWork unitOfWork)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles ?? new List<IdentityRole>());
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role not exist!"));
                return RedirectToAction("Index");
            }
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!) ?? new List<User>();
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                foreach (var user in usersInRole)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                }
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Deleted role {role.Name} successfully"));
                return RedirectToAction("Index");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Delete role failed!"));
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            bool isRoleExisted = await _roleManager.RoleExistsAsync(model.RoleName);
            if (isRoleExisted)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role name existed!"));
                return View(model);
            }
            var role = new IdentityRole
            {
                Name = model.RoleName
            };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Created role {role.Name} successfully"));
                return RedirectToAction("Index");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Create role failed!"));
            return View(model);
        }

        public async Task<IActionResult> Update(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role not exist!"));
                return RedirectToAction("Index");
            }
            var model = new UpdateRoleViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name!
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateRoleViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role not exist!"));
                return RedirectToAction("Index");
            }
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!) ?? new List<User>();
            if (!role.Name!.Equals(model.RoleName))
            {
                bool isRoleExisted = await _roleManager.RoleExistsAsync(model.RoleName);
                if (isRoleExisted)
                {
                    TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role name existed!"));
                    return RedirectToAction("Update", new { id = role.Id });
                }
                role.Name = model.RoleName;
            }
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                foreach (var user in usersInRole)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                }
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Role '{role.Name}' updated successfully"));
                return RedirectToAction("Index");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role update failed!"));
            return RedirectToAction("Update", new { id = role.Id });
        }

        public async Task<IActionResult> ManageUser(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role not exist!"));
                return RedirectToAction("Index");
            }
            var usersInRole = (await _userManager.GetUsersInRoleAsync(role.Name!)).Select(u => u.UserName!).ToList();
            var model = new ManageUserViewModel
            {
                Id = role.Id,
                Name = role.Name!,
                CurrentUser = usersInRole,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUser(ManageUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Invalid model!"));
                return RedirectToAction("Index");
            }

            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role not exist!"));
                return RedirectToAction("Index");
            }

            var allUsers = await _userManager.Users.ToListAsync();
            var UsersInRole = await _userManager.Users.Where(u => model.CurrentUser!.Contains(u.UserName!)).ToListAsync();
            var UsersNotInRole = allUsers.Except(UsersInRole).ToList();

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var user in UsersInRole)
                {
                    if (!await _userManager.IsInRoleAsync(user, role.Name!))
                    {
                        var result = await _userManager.AddToRoleAsync(user, role.Name!);
                        if (!result.Succeeded) throw new Exception("Add user to role failed!");
                        await _userManager.UpdateSecurityStampAsync(user);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
                foreach (var user in UsersNotInRole)
                {
                    if (await _userManager.IsInRoleAsync(user, role.Name!))
                    {
                        var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);
                        if (!result.Succeeded) throw new Exception("Remove user from role failed!");
                        await _userManager.UpdateSecurityStampAsync(user);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "Update user(s) for role successfully"));
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Update user(s) for role failed!"));
                return RedirectToAction("Index");
            }
        }


    }
}
