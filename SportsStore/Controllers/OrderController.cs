using Microsoft.AspNetCore.Mvc;
using SportsStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace SportsStore.Controllers
{
    public class OrderController : Controller
    {
        private IOrderRepository repository;
        private Cart cart;
        private UserManager<AppUser> userManager;

        public OrderController(IOrderRepository repository, Cart cart, UserManager<AppUser> userManager)
        {
            this.repository = repository;
            this.cart = cart;
            this.userManager = userManager;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        public ViewResult List() =>
            View(repository.Orders.Where(o => !o.Shipped));

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult MarkShipped(int orderID)
        {
            Order order = repository.Orders
                .FirstOrDefault(o => o.OrderID == orderID);
            if (order != null)
            {
                order.Shipped = true;
                repository.SaveOrder(order);
            }
            return RedirectToAction(nameof(List));
        }

        [Authorize]
        public ViewResult Checkout() => View(new Order());

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout(Order order)
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            var cost = cart.ComputeTotalValue();

            if (cart.Lines.Count() == 0)
            {
                ModelState.AddModelError("", "Your cart is empty!");
            }

            if (user.Money < cost)
            {
                ModelState.AddModelError("", "Insufficient funds");
            }

            if (ModelState.IsValid)
            {
                order.Lines = cart.Lines.ToArray();
                // TODO: See if it is possible to use transactions here.
                // Probably create method in repository that takes user and performs transaction
                user.Money -= cost;
                repository.SaveOrder(order);
                return RedirectToAction(nameof(Completed));
            }
            else
            {
                return View(order);
            }
        }

        [Authorize]
        public ViewResult Completed()
        {
            cart.Clear();
            return View();
        }
    }


}
