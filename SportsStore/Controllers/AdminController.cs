﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        private UserManager<IdentityUser> userManager;
        public AdminController(IProductRepository repository, UserManager<IdentityUser> userManager)
        {
            this.repository = repository;
            this.userManager = userManager;
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
        public async Task<IActionResult> EditUser(string id)
        {
            ViewBag.userManager = userManager;
            IdentityUser user = await userManager.FindByIdAsync(id);
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
        public async Task<IActionResult> EditUser(string id, string userName, string roles)
        {
            ViewBag.userManager = userManager;
            IdentityUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Update username
                if(!string.IsNullOrEmpty(userName))
                {
                    user.UserName = userName;
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username");
                }
                var updateUsernameResult = await userManager.UpdateAsync(user);
                if (!updateUsernameResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to update username");
                }

                // Update roles
                var rolesList = roles.Replace(" ", string.Empty).Split(",").ToList();
                var currentRoles = await userManager.GetRolesAsync(user);
                await userManager.RemoveFromRolesAsync(user, currentRoles);
                var updateRolesResult = await userManager.AddToRolesAsync(user, rolesList);
                if(!updateRolesResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to assign roles");
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return View(user);
        }
    }
}