using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using GoVip_FileUploader.Data;
using GoVip_FileUploader.ViewModels;
using Mapster;
using GoVip_FileUploader.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace GoVip_FileUploader.Controllers
{
    public class ItemController : BaseApiController
    {
        public static IHostingEnvironment _environment;
        public ItemController(
            ApplicationDBContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IHostingEnvironment environment
            )
            : base(context, roleManager, userManager, configuration)
        { ;
            _environment = environment;
        }

        // GET: api/item
        [HttpGet]
        public IActionResult List()
        {
            var results = DbContext.Items.ToArray();

            return new JsonResult(results.Adapt<ItemViewModel[]>(), JsonSettings);
        }

        // GET: api/item/{id}
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var result = DbContext.Items.Where(r => r.Id == id).FirstOrDefault();
            if (result == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Item with ID {0} not found!", id)
                });
            }
            return new JsonResult(result.Adapt<ItemViewModel>(), JsonSettings);
        }

        // POST: api/item
        //adds new item from the database
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromForm] ItemViewModel model)
        {
            try
            {
                if(model.File == null) return BadRequest("File should not be empty!");

                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", model.File.FileName);

                using (Stream stream = new FileStream(path, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                //return 500 if payload is invalid
                if (model == null) return new StatusCodeResult(500);
                // validate user input
                if (model.UploadedBy == null) return BadRequest("UploadedBy is required!");
                // check if item already exist in the database
                var existingItem = DbContext.Items.Where(i => i.Name == model.Name).FirstOrDefault();

                if (existingItem != null) return BadRequest("Item already exist!");

                //handle insert (w/o object mapping)
                var item = model.Adapt<Item>();

                //properties taken from the request
                item.Name = model.Name;
                item.Description = model.Description;
                item.Filepath = path;

                // properties set from server-side
                item.DateTimeCreated = DateTime.Now;
                item.DateTimeUpdated = item.DateTimeCreated;

                //set user id 
                var currentUser = DbContext.Users.Where(u => u.Id == model.UploadedBy).FirstOrDefault();
                if (currentUser == null)
                {
                    return NotFound(new
                    {
                        Error = String.Format("User ID {0} not found!", model.Id)
                    });
                }
                item.UploadedBy = currentUser.Id;
                item.UpdatedBy = currentUser.Id;

                // add the new item
                DbContext.Items.Add(item);
                // persist changes to database
                DbContext.SaveChanges();

                // return newly created item to client
                //return new JsonResult(item.Adapt<ItemViewModel>(), JsonSettings);
                return StatusCode(StatusCodes.Status201Created);
            }
            catch
            {
                return BadRequest();
            }

        }

        // PATCH: api/item
        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> Put([FromForm] ItemViewModel model)
        {
            if (model == null) return new StatusCodeResult(500);
            // validate user input
            if (model.UpdatedBy == null) return BadRequest("UpdatedBy is required!");

            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", model.File.FileName);

            using (Stream stream = new FileStream(path, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            var item = DbContext.Items.Where(r => r.Id == model.Id).FirstOrDefault();

            if (item == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Item with ID {0} not found!", model.Id)
                });
            }

            item.Name = model.Name;
            item.Description = model.Description;
            item.Filepath = path;
            item.UpdatedBy = DbContext.Users.Where(u => u.Id == model.UpdatedBy).FirstOrDefault().Id;
            item.DateTimeCreated = DateTime.Now;
            item.DateTimeUpdated = item.DateTimeCreated;
            //persist changes to database
            DbContext.SaveChanges();

            return new JsonResult(item.Adapt<ItemViewModel>(), JsonSettings);
        }

        // DELETE: api/item/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var result = DbContext.Items.Where(r => r.Id == id).FirstOrDefault();

            if (result == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Item with ID {0} not found!", id)
                });
            }

            DbContext.Items.Remove(result);
            // persist changes to database
            DbContext.SaveChanges();

            return new OkResult();
        }

    }
}
