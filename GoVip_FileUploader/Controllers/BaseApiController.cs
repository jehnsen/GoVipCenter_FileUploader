using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoVip_FileUploader.Data;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using GoVip_FileUploader.Data.Models;

namespace GoVip_FileUploader.Controllers
{
    [Route("api/[controller]")]
    public class BaseApiController : Controller
    {
        protected ApplicationDBContext DbContext { get; private set; }
        protected RoleManager<IdentityRole> RoleManager { get; private set; }
        protected UserManager<ApplicationUser> UserManager { get; private set; }
        protected IConfiguration Configuration { get; private set; }

        public BaseApiController(
            ApplicationDBContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration
            )
        {
            //instantiate ApplicationDBContext thru DI
            DbContext = context;
            RoleManager = roleManager;
            UserManager = userManager;
            Configuration = configuration;
        }

        //instantiate a single JsonSerializerSettings
        public JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented
        };
      
    }
}
