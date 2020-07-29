using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TestMakerApp.Data;
using TestMakerApp.ViewModels;
using Mapster;
using TestMakerApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace TestMakerApp.Controllers
{
    public class ResultController : BaseApiController   
    {
        public ResultController(
            ApplicationDBContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration
            ) 
            : base(context, roleManager, userManager, configuration) { }

        // GET: api/quiestion/all
        [HttpGet("All/{quizId}")]
        public IActionResult All(int quizId)
        {
            var results = DbContext.Results.Where(r => r.QuizId == quizId).ToArray();

            if(results == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Quiz with ID {0} not found!", quizId)
                });
            }

            return new JsonResult(results.Adapt<ResultViewModel[]>(), JsonSettings);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var result = DbContext.Results.Where(r => r.Id == id).FirstOrDefault();
            if (result == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Result with ID {0} not found!", id)
                });
            }
            return new JsonResult(result.Adapt<ResultViewModel>(), JsonSettings);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody]ResultViewModel model)
        {
            if (model == null) return new StatusCodeResult(500);

            var result = model.Adapt<Result>();

            result.CreatedDate = DateTime.Now;
            result.LastModifiedDate = result.CreatedDate;

            // add new result object
            DbContext.Results.Add(result);
            // persist changes to database
            DbContext.SaveChanges();

            return new JsonResult(result.Adapt<ResultViewModel>(), JsonSettings);
        }

        [HttpPut]
        [Authorize]
        public IActionResult Put([FromBody]ResultViewModel model)
        {
            if (model == null) return new StatusCodeResult(500);

            var result = DbContext.Results.Where(r => r.Id == model.Id).FirstOrDefault();

            if (result == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Result with ID {0} not found!", model.Id)
                });
            }

            result.QuizId = model.QuizId;
            result.Text = model.Text;
            result.Notes = model.Notes;
            result.MinValue = model.MinValue;
            result.MaxValue = model.MaxValue;
            result.CreatedDate = DateTime.Now;
            result.LastModifiedDate = result.CreatedDate;
            // persist changes to database
            DbContext.SaveChanges();

            return new JsonResult(result.Adapt<ResultViewModel>(), JsonSettings);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var result = DbContext.Results.Where(r => r.Id == id).FirstOrDefault();

            if (result == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Result with ID {0} not found!", id)
                });
            }

            DbContext.Results.Remove(result);
            // persist changes to database
            DbContext.SaveChanges();

            return new OkResult();
        }
        
    }
}
