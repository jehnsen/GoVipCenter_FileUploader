using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TestMakerApp.Data;
using TestMakerApp.Data.Models;
using TestMakerApp.ViewModels;

namespace TestMakerApp.Controllers
{
    public class QuestionController : BaseApiController
    {
        public QuestionController(
            ApplicationDBContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration
            ) : base(context, roleManager, userManager, configuration) { }

        [HttpGet("All/{quizId}")]
        public IActionResult All(int quizId)
        {
            var questions = DbContext.Questions.Where(q => q.QuizId == quizId).ToArray();

            return new JsonResult(questions.Adapt<QuestionViewModel[]>(), JsonSettings);

        }

        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody]QuestionViewModel model)
        {
            if (model == null) return new StatusCodeResult(500);

            var question = model.Adapt<Question>();

            question.Text = model.Text;
            question.Notes = model.Notes;
            question.QuizId = model.QuizId;
            question.CreatedDate = DateTime.Now;
            question.LastModifiedDate = question.CreatedDate;
            //persist changes to database
            DbContext.Questions.Add(question);
            DbContext.SaveChanges();

            return new JsonResult(question.Adapt<QuestionViewModel>(), JsonSettings);

        }

        [HttpPut]
        [Authorize]
        public IActionResult Put([FromBody]QuestionViewModel model)
        {
            if (model == null) return new StatusCodeResult(500);

            var question = DbContext.Questions.Where(q => q.Id == model.Id).FirstOrDefault();

            if (question == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Question with the ID {0} not found!", model.Id)
                });
            }

            question.QuizId = model.QuizId;
            question.Text = model.Text;
            question.Notes = model.Notes;
            question.LastModifiedDate = DateTime.Now;
            //persist changes to database
            DbContext.SaveChanges();

            return new JsonResult(question.Adapt<QuestionViewModel>(),JsonSettings);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var question = DbContext.Questions.Where(q => q.Id == id).FirstOrDefault();
            if(question == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Question with ID {0} not found!", id)
                });
            }

            DbContext.Remove(question);
            DbContext.SaveChanges();

            return new OkResult();
        }
    }
}
