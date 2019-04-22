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
            //Task.WaitAll(superAdminTask, adminTask, userTask);

            AppUser admin = await userManager.FindByNameAsync(adminUser);
            if (admin == null)
            {
                admin = new AppUser(adminUser)
                {
                    Money = 999999
                };
                await userManager.CreateAsync(admin, adminPassword);
            }
            await userManager.AddToRoleAsync(admin, "SuperAdmin");

            AppUser user = await userManager.FindByNameAsync("User");
            if (user == null)
            {
                user = new AppUser("User")
                {
                    Money = 300
                };
                await userManager.CreateAsync(user, adminPassword);
            }
        }
    }
}