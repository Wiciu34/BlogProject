using BlogProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BlogProject.Data;

public static class Seed
{
    public static void  SeedData(IApplicationBuilder applicationBuilder)
    {
        using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope()) 
        {
            var context = serviceScope.ServiceProvider.GetService<BlogDbContext>();

            context.Database.EnsureCreated();

            if (context.Articles.Any())
            {
                return;
            }
            context.Articles.AddRange(
               new Article
               {
                   Title = "test",
                   Content = "test"
               },
               new Article
               {
                   Title = "test",
                   Content = "test"
               },
               new Article
               {
                   Title = "Test",
                   Content = "test"
               }
            );
            context.SaveChanges();
        }
    }

    public static async Task SeedUserAndRoleAsync(IApplicationBuilder applicationBuilder) 
    {
        using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
        {
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await roleManager.RoleExistsAsync(UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            string adminUserEmail = "admin@example.com";

            var adminUser = await userManager.FindByEmailAsync(adminUserEmail);
            if (adminUser == null)
            {
                var newAdminUser = new AppUser()
                {
                    UserName = "Admin",
                    Email = adminUserEmail,
                    EmailConfirmed = true,
                  
                };
                await userManager.CreateAsync(newAdminUser, "Admin@123");
                await userManager.AddToRoleAsync(newAdminUser, UserRoles.Admin);
            }

        }
    }
}
