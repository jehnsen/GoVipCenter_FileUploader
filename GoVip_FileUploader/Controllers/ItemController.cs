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

namespace GoVip_FileUploader.Controllers
{
    public class ItemController : BaseApiController
    {
        public ItemController(
            ApplicationDBContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration
            )
            : base(context, roleManager, userManager, configuration) { }

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
        public IActionResult Post([FromBody] ItemViewModel model)
        {
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
            item.ImgUrl = model.ImgUrl;

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
            return new JsonResult(item.Adapt<ItemViewModel>(), JsonSettings);
        }

        // PATCH: api/item
        [HttpPatch]
        [Authorize]
        public IActionResult Put([FromBody] ItemViewModel model)
        {
            if (model == null) return new StatusCodeResult(500);
            // validate user input
            if (model.UpdatedBy == null) return BadRequest("UpdatedBy is required!");

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
            item.ImgUrl = model.ImgUrl;
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
