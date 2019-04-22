using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportsStore.Models;

namespace SportsStore.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminController : Controller
    {
        private IProductRepository repository;
        private UserManager<AppUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        public AdminController(IProductRepository repository, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.repository = repository;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        public ViewResult Index() => View(repository.Products);

        public ViewResult Edit(int productId) =>
            View(repository.Products
                .FirstOrDefault(p => p.ProductID == productId));

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                repository.SaveProduct(product);
                TempData["message"] = $"{product.Name} has been saved";
                /*
                 * Note on how data is passed between controllers and views
                 *      ViewBag - avalible only for one http request
                 *      SessionData - persisted untill explicitly deleted
                 *      TempData - same as session data, but deleted as it is read
                 */
                return RedirectToAction("Index");
            }
            else
            {
                // there is something wrong with the data values
                return View(product);
            }
        }
        public ViewResult Create() => View("Edit", new Product());

        [HttpPost]
        public IActionResult Delete(int productId)
        {
            try
            {
                Product deletedProduct = repository.DeleteProduct(productId);
                if (deletedProduct != null)
                {
                    TempData["message"] = $"{deletedProduct.Name} was deleted";
                }
            }
            catch (InvalidOperationException e)
            {
                TempData["message"] = e.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SeedDatabase()
        {
            SeedData.EnsurePopulated(HttpContext.RequestServices);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "SuperAdmin")]
        public ActionResult Users()
        {
            ViewBag.userManager = userManager;
            return View(userManager.Users);
        }

        [Authorize(Roles = "SuperAdmin")]
        public ViewResult CreateUser() => View();

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(string userName, string password, string confirmPassword, string roles)
        {
            if (ModelState.IsValid) // Add ViewModel instead of string parametes
            {
                if (password != confirmPassword)
                {
                    ModelState.AddModelError("", "Passwords don't match");
                    return View();
                }

                AppUser user = new AppUser
                {
                    UserName = userName
                };

                foreach (var validator in userManager.PasswordValidators)
                {
                    var validationResult = await validator.ValidateAsync(userManager, user, password);
                    if (!validationResult.Succeeded)
                    {
                        foreach (IdentityError error in validationResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View();
                    }
                }

                IdentityResult result
                    = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    // Assign roles here (cant do it before creating because in that case security stamp is not assigned)
                    if (roles != null)
                    {
                        var rolesList = roles.Replace(" ", string.Empty).Split(",").ToList();
                        var validRolesList = roleManager.Roles.Select(r => r.ToString()).ToList();
                        if (!rolesList.All(r => validRolesList.Contains(r)))
                        {
                            ModelState.AddModelError("", "User is created, but roles are not assigned (invalid roles in roles list)");
                            return View();
                        }
                        IdentityResult addRolesResult = await userManager.AddToRolesAsync(user, rolesList);

                        if (!addRolesResult.Succeeded)
                        {
                            foreach (IdentityError error in addRolesResult.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                            return View();
                        }
                    }
                    return RedirectToAction("Users");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> EditUser(string id)
        {
            ViewBag.userManager = userManager;
            AppUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> EditUser(string id, string userName, string money, string roles)
        {
            ViewBag.userManager = userManager;
            AppUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Change username
                if (!string.IsNullOrEmpty(userName))
                {
                    user.UserName = userName;
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username");
                }
                // Change amount of money
                // Parse the hell out of it:
                if (decimal.TryParse(money.Replace(",", "."),
                    NumberStyles.Currency,
                    CultureInfo.InvariantCulture,
                    out decimal newMoney) 
                    && newMoney > 0) // TODO: DataAnnotations dont work here, figure out why
                {
                    user.Money = newMoney;
                }
                else
                {
                    ModelState.AddModelError("", "Invalid money amount entered");
                }

                // Update user
                if (ModelState.IsValid)
                {
                    var updateUsernameResult = await userManager.UpdateAsync(user);
                    if (!updateUsernameResult.Succeeded)
                    {
                        ModelState.AddModelError("", "Failed to update user");
                    }
                }

                // Update roles separately
                var rolesList = roles?.Replace(" ", string.Empty)?.Split(",")?.ToList() ?? new List<string>();
                var validRolesList = roleManager.Roles.Select(r => r.ToString()).ToList();
                if (!rolesList.All(r => validRolesList.Contains(r)))
                {
                    ModelState.AddModelError("", "Failed to assign roles - one or more roles do not exist");
                    return View(user);
                }
                var currentRoles = await userManager.GetRolesAsync(user);
                await userManager.RemoveFromRolesAsync(user, currentRoles);
                var updateRolesResult = await userManager.AddToRolesAsync(user, rolesList);
                if (!updateRolesResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to assign roles");
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return ModelState.IsValid
                ? RedirectToAction("Users") as IActionResult
                : View(user);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Users");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return View("Users", userManager.Users);
        }
    }
}