using Application;
using Application.Accounts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Shop.Controllers
{
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
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
    }
}
