using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewsWebsite.Models;

namespace NewsWebsite.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = ["Admin", "Editor", "Author"];
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminUser = new ApplicationUser
            {
                UserName = "admin@newswebsite.com",
                Email = "admin@newswebsite.com",
                FullName = "Site Administrator"
            };

            string adminPassword = "Admin@123";
            var user = await userManager.FindByEmailAsync(adminUser.Email);
            if (user == null)
            {
                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var helloWorld = "Hello World";
            var firstArticle = await db.Articles.Where(a => a.Title == helloWorld)
                .FirstOrDefaultAsync();
            if (firstArticle == null)
            {
                db.Articles.Add(new Article
                {
                    AuthorId = user!.Id,
                    CategoryId = db.Categories.First().Id,
                    Title = helloWorld,
                    Content = "Hello World!",
                    PublishDate = DateTime.Now
                });
                await db.SaveChangesAsync();
            }
        }
    }
}