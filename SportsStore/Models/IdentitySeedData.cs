using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace SportsStore.Models
{
    public static class IdentitySeedData
    {
        private const string adminUser = "Admin";
        private const string adminPassword = "Secret123$";
        public static async void EnsurePopulated(IApplicationBuilder app)
        {
            UserManager<AppUser> userManager = app.ApplicationServices
                .GetRequiredService<UserManager<AppUser>>();
            RoleManager<IdentityRole> roleManager = app.ApplicationServices
                .GetRequiredService<RoleManager<IdentityRole>>();

            // Create roles
            await roleManager.CreateAsync(new IdentityRole { Name = "SuperAdmin" });
            await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
            await roleManager.CreateAsync(new IdentityRole { Name = "User" });
            //Task.WaitAll(superAdminTask, adminTask, userTask);

            AppUser user = await userManager.FindByNameAsync(adminUser);
            if (user == null)
            {
                user = new AppUser("Admin");
                await userManager.CreateAsync(user, adminPassword);
            }
            await userManager.AddToRoleAsync(user, "SuperAdmin");
        }
    }
}