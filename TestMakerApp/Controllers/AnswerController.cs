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
    public class AnswerController : BaseApiController
    {
        public AnswerController(
            ApplicationDBContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration
            ) 
            : base(context, roleManager, userManager, configuration) { }

        [HttpGet("All/{questionId}")]
        public IActionResult All(int questionId)
        {
            var answers = DbContext.Answers.Where(a => a.QuestionId == questionId).ToArray();

            return new JsonResult(answers.Adapt<AnswerViewModel[]>(), JsonSettings);
        }

        // retrieves answer w/ the given id
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var answer = DbContext.Answers.Where(a => a.Id == id).FirstOrDefault();

            if (answer == null)
            {
                return NotFound(new
                {
                    Error = String.Format("No asnwer found with the given Id {0}", id)
                });
            }

            return new JsonResult(answer.Adapt<AnswerViewModel>(),JsonSettings);
        }

        // add new answer to the database
        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody]AnswerViewModel model)
        {
            if (model == null) return new StatusCodeResult(500);

            // map the viewmodel to the model
            var answer = model.Adapt<Answer>();

            answer.QuestionId = model.QuestionId;
            answer.Text = model.Text;
            answer.Notes = model.Notes;

            answer.CreatedDate = DateTime.Now;
            answer.LastModifiedDate = answer.CreatedDate;

            DbContext.Answers.Add(answer);
            // persist changes to database
            DbContext.SaveChanges();

            return new JsonResult(answer.Adapt<AnswerViewModel>(),JsonSettings);
        }

        [HttpPut]
        [Authorize]
        public IActionResult Put([FromBody]AnswerViewModel model) 
        {
            if (model == null) return new StatusCodeResult(500);

            var answer = DbContext.Answers.Where(a => a.Id == model.Id).FirstOrDefault();

            if (answer == null)
            {
                return NotFound(new
                {
                    Error = String.Format("No answer found with the ID {0}", model.Id)
                });
            }

            answer.QuestionId = model.QuestionId;
            answer.Text = model.Text;
            answer.Notes = model.Notes;
            answer.Value = model.Value;
            answer.LastModifiedDate = DateTime.Now;
            // persist changes to database
            DbContext.SaveChanges();

            return new JsonResult(answer.Adapt<AnswerViewModel>(), JsonSettings);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var answer = DbContext.Answers.Where(a => a.Id == id).FirstOrDefault();

            if (answer == null)
            {
                return NotFound(new
                {
                    Error = String.Format("answer with ID {0} not found!", id)
                });
            }

            DbContext.Answers.Remove(answer);

            DbContext.SaveChanges();

            return new OkResult();
        }

    }
}
