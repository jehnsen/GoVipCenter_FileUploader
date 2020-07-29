using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TestMakerApp.ViewModels;
using TestMakerApp.Data;
using Mapster;
using TestMakerApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace TestMakerApp.Controllers
{
    public class QuizController : BaseApiController
    {
        public QuizController(
            ApplicationDBContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration
            ) : base(context, roleManager, userManager, configuration){ }

        // GET api/quiz/latest
        [HttpGet("Latest/{num}")]
        public IActionResult Latest(int num = 10)
        {
            // add a first sample quiz
            var quizzes = DbContext.Quizzes.OrderByDescending(q => q.CreatedDate)
                .Take(num)
                .ToArray();

            return new JsonResult(quizzes.Adapt<QuizViewModel[]>(), JsonSettings);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var quiz = DbContext.Quizzes.Where(q => q.Id == id).FirstOrDefault();

            //handle request asking for non-existng quizes
            if(quiz == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Quiz ID {0} not found", id)
                });
            }

            return new JsonResult(quiz.Adapt<QuizViewModel>(), JsonSettings);

        }

        // GET api/quiz/ByTitle
        [HttpGet("ByTitle/{num:int?}")]
        public  IActionResult ByTitle(int num = 10)
        {
            var quizzes = DbContext.Quizzes
                .OrderBy(q => q.Title)
                .Take(10)
                .ToArray();

            return new JsonResult(quizzes.Adapt<QuizViewModel[]>(), JsonSettings);

        }

        [HttpGet("Random/{num:int?}")]
        public IActionResult Random(int num = 10)
        {
            var quizzes = DbContext.Quizzes
                .OrderBy(q => Guid.NewGuid())
                .Take(num)
                .ToArray();

            return new JsonResult(quizzes.Adapt<QuizViewModel[]>(), JsonSettings);
        }

        //adds new quiz from the database
        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody]QuizViewModel model) 
        {
            //return 500 if payload is invalid
            if (model == null) return new StatusCodeResult(500);

            //handle insert (w/o object mapping)
            var quiz = model.Adapt<Quiz>();

            //properties taken from the request
            quiz.Title = model.Title;
            quiz.Description = model.Description;
            quiz.Text = model.Text;
            quiz.Notes = model.Notes;

            // properties set from server-side
            quiz.CreatedDate = DateTime.Now;
            quiz.LastModifiedDate = quiz.CreatedDate;

            //set temporary author using Admin user
            quiz.UserId = DbContext.Users.Where(u => u.UserName == "Admin").FirstOrDefault().Id;

            // add the new quiz
            DbContext.Quizzes.Add(quiz);
            // persist changes to database
            DbContext.SaveChanges();

            // return newly created quiz to client
            return new JsonResult(quiz.Adapt<QuizViewModel>(), JsonSettings);
        }

        // update quiz w/ the given id
        [HttpPut]
        [Authorize]
        public IActionResult Put([FromBody]QuizViewModel model)
        {
            // return http code 500 if payload is invalid
            if (model == null) return new StatusCodeResult(500);

            var quiz = DbContext.Quizzes.Where(q => q.Id == model.Id).FirstOrDefault();

            if (quiz == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Quiz ID {0} not found!", model.Id)
                });
            }

            //properties taken from the request
            quiz.Title = model.Title;
            quiz.Description = model.Description;
            quiz.Text = model.Text;
            quiz.Notes = model.Notes;

            // properties set from server-side
            quiz.LastModifiedDate = DateTime.Now;

            // persist changes to database
            DbContext.SaveChanges();

            return new JsonResult(quiz.Adapt<QuizViewModel>(),
                new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                });
        }

        //deletes the quiz from the database w/ a given id
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var quiz = DbContext.Quizzes.Where(q => q.Id == id).FirstOrDefault();

            if (quiz == null)
            {
                return NotFound(new
                {
                    Error = String.Format("Quiz with ID {0} not found!", id)
                });
            }

            DbContext.Quizzes.Remove(quiz);

            DbContext.SaveChanges();

            return new OkResult();
        }
    }
}

