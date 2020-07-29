using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestMakerApp.Data;
using TestMakerApp.Data.Models;
using TestMakerApp.ViewModels;
using Mapster;

namespace TestMakerApp.Controllers
{
    public class UserController : BaseApiController
    {
        public UserController(
            ApplicationDBContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration
            ) : base(context, roleManager, userManager, configuration) { }

        [HttpPost()]
        public async Task<IActionResult> Add([FromBody]UserViewModel model)
        {
            if (model == null) return new StatusCodeResult(500);

            //check if username/email already exist
            ApplicationUser user = await UserManager.FindByNameAsync(model.UserName);

            if (user != null) return BadRequest("Username already exist!");

            user = await UserManager.FindByEmailAsync(model.Email);

            if (user != null) return BadRequest("Email already exist!");

            var now = DateTime.Now;

            //create new item w/ the client sent json data
            user = new ApplicationUser()
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName,
                Email = model.Email,
                DisplayName = model.DisplayName,
                CreatedDate = now,
                LastModifiedDate = now
            };

            //add the user to database
            await UserManager.CreateAsync(user, model.Password);

            //assign role
            await UserManager.AddToRoleAsync(user, "RegisteredUser");

            //remove lockout & email confirmation
            user.EmailConfirmed = true;
            user.LockoutEnabled = false;

            //persist changes to database
            DbContext.SaveChanges();

            //return the newly created user
            return Json(user.Adapt<UserViewModel>(), JsonSettings);

        }

    }
}
