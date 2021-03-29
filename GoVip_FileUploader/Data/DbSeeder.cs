using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoVip_FileUploader.Data;
using GoVip_FileUploader.Data.Models;

namespace GoVip_FileUploader.Data
{
    public class DbSeeder
    {
        public static void Seed(
            ApplicationDBContext dbContext,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager
            )
        {
            // create default users if empty
            if (!dbContext.Users.Any())
            {
                CreateUsers(dbContext, roleManager, userManager)
                    .GetAwaiter()
                    .GetResult();
            }

        }

        private static async Task CreateUsers(
            ApplicationDBContext dbContext,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager
            )
        {
            //local variables
            DateTime createdDate = new DateTime(2016, 03, 01, 12, 30, 00);
            DateTime lastModifiedDate = DateTime.Now;

            string role_Administrator = "Administrator";
            string role_RegisteredUser = "RegisteredUser";

            //create roles if doesn't exist yet
            if (!await roleManager.RoleExistsAsync(role_Administrator))
            {
                await roleManager.CreateAsync(new IdentityRole(role_Administrator));
            }
            if (!await roleManager.RoleExistsAsync(role_RegisteredUser))
            {
                await roleManager.CreateAsync(new IdentityRole(role_RegisteredUser));
            }

            //create admin account if does'nt exist yet
            var user_Admin = new ApplicationUser()
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = "Admin",
                Email = "admin@govipcenter.com",
                CreatedDate = createdDate,
                LastModifiedDate = lastModifiedDate
            };

            // insert the admin to database & assign administrator & registered user roles 
            if (await userManager.FindByNameAsync(user_Admin.UserName) == null)
            {
                await userManager.CreateAsync(user_Admin, "Pass4Admin");
                await userManager.AddToRoleAsync(user_Admin, role_RegisteredUser);
                await userManager.AddToRoleAsync(user_Admin, role_Administrator);
                //remove lockout and email confirmation
                user_Admin.EmailConfirmed = true;
                user_Admin.LockoutEnabled = false;
            }

            await dbContext.SaveChangesAsync();
        }

        private static void CreateItem(ApplicationDBContext dbContext)
        {
            //local variables
            DateTime createdDate = new DateTime(2016, 03, 01, 12, 30, 00);
            DateTime lastModifiedDate = DateTime.Now;

            //retrieve admin user as a default user
            var userId = dbContext.Users
                .Where(u => u.UserName == "Admin")
                .FirstOrDefault()
                .Id;
            //persist the changes to the database
            dbContext.SaveChanges();
        }

    }
}
