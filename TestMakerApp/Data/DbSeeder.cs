using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestMakerApp.Data;
using TestMakerApp.Data.Models;

namespace TestMakerApp.Data
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

            //create quizzes if none, together w/ Q&A
           if (!dbContext.Quizzes.Any()) CreateQuizzes(dbContext);
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
                Email = "admin@microsoft.com",
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

            #if DEBUG
            var user_Stark = new ApplicationUser()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = "Stark",
                    Email = "stark@microsoft.com",
                    CreatedDate = createdDate,
                    LastModifiedDate = lastModifiedDate
                };
            // insert the admin to database & assign administrator & registered user roles 
            if (await userManager.FindByNameAsync(user_Stark.UserName) == null)
            {
                await userManager.CreateAsync(user_Stark, "Pass4Stark");
                await userManager.AddToRoleAsync(user_Stark, role_RegisteredUser);
                await userManager.AddToRoleAsync(user_Stark, role_Administrator);
                //remove lockout and email confirmation
                user_Stark.EmailConfirmed = true;
                user_Stark.LockoutEnabled = false;
            }

            var user_Rogers = new ApplicationUser()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = "Rogers",
                    Email = "rogers@microsoft.com",
                    CreatedDate = createdDate,
                    LastModifiedDate = lastModifiedDate
                };
            // insert the admin to database & assign administrator & registered user roles 
            if (await userManager.FindByNameAsync(user_Rogers.UserName) == null)
            {
                await userManager.CreateAsync(user_Rogers, "Pass4Rogers");
                await userManager.AddToRoleAsync(user_Rogers, role_RegisteredUser);
                await userManager.AddToRoleAsync(user_Rogers, role_Administrator);
                //remove lockout and email confirmation
                user_Rogers.EmailConfirmed = true;
                user_Rogers.LockoutEnabled = false;
            }

            var user_Banner = new ApplicationUser()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = "Banner",
                    Email = "banner@microsoft.com",
                    CreatedDate = createdDate,
                    LastModifiedDate = lastModifiedDate
                };
            // insert the admin to database & assign administrator & registered user roles 
            if (await userManager.FindByNameAsync(user_Banner.UserName) == null)
            {
                await userManager.CreateAsync(user_Banner, "Pass4Banner");
                await userManager.AddToRoleAsync(user_Banner, role_RegisteredUser);
                await userManager.AddToRoleAsync(user_Banner, role_Administrator);
                //remove lockout and email confirmation
                user_Banner.EmailConfirmed = true;
                user_Banner.LockoutEnabled = false;
            }

            ////insert users to db
            //dbContext.Users.AddRange(user_Stark, user_Rogers, user_Banner);
            # endif 

            await dbContext.SaveChangesAsync();
        }

        private static void CreateQuizzes(ApplicationDBContext dbContext)
        {
            //local variables
            DateTime createdDate = new DateTime(2016, 03, 01, 12, 30, 00);
            DateTime lastModifiedDate = DateTime.Now;

            //retrieve admin user as a default author
            var authorId = dbContext.Users
                .Where(u => u.UserName == "Admin")
                .FirstOrDefault()
                .Id;

#if DEBUG

            //create sample quizzes including questions, anwers, results
            var num = 47;

            for (int i = 1; i <= num; i++)
            {
                CreateSampleQuiz(
                    dbContext,
                    i,
                    authorId,
                    num - i,
                    3,
                    3,
                    3,
                    createdDate.AddDays(-num));
            }

#endif

            // create 3 more quizzes
            EntityEntry<Quiz> e1 = dbContext.Quizzes.Add(new Quiz()
            {
                UserId = authorId,
                Title = "Which side are you, the avengers or justice league?",
                Description = "Superhero personality test.",
                Text = String.Format("Choose wisely this test will prove if your will is strong enough"),
                ViewCount = 2345,
                CreatedDate = createdDate,
                LastModifiedDate = lastModifiedDate
            });

            EntityEntry<Quiz> e2 = dbContext.Quizzes.Add(new Quiz()
            {
                UserId = authorId,
                Title = "Quantum Physics and Nuclear Science",
                Description = "Scientific test",
                Text = String.Format("This test will prove if your knowledge in science."),
                ViewCount = 1289,
                CreatedDate = createdDate,
                LastModifiedDate = lastModifiedDate
            });

            EntityEntry<Quiz> e3 = dbContext.Quizzes.Add(new Quiz()
            {
                UserId = authorId,
                Title = "Which Anime character are you?",
                Description = "Anime relatest test.",
                Text = String.Format("Choose wisely this test your character."),
                ViewCount = 3468,
                CreatedDate = createdDate,
                LastModifiedDate = lastModifiedDate
            });

            //persist the changes to the database
            dbContext.SaveChanges();
        }


        #region Utility Methods

        private static void CreateSampleQuiz(
            ApplicationDBContext dbContext,
            int num,
            string authorId,
            int viewCount,
            int numberOfQuestions,
            int numberOfAnswerPerQuestion,
            int numberOfResults,
            DateTime createdDate)
        {
            var quiz = new Quiz()
            {
                UserId = authorId,
                Title = String.Format("Quiz {0} Title", num),
                Description = String.Format("This is a sample description for quiz no. {0}.", num),
                Text = String.Format("This is generated by the SeedeDB class."),
                ViewCount = 3468,
                CreatedDate = createdDate,
                LastModifiedDate = createdDate
            };

            dbContext.Quizzes.Add(quiz);
            dbContext.SaveChanges();

            for (int i = 1; i < numberOfQuestions; i++)
            {
                var question = new Question()
                {
                    QuizId = quiz.Id,
                    Text = String.Format("This is a generated question by the SeederDB class."),
                    CreatedDate = createdDate,
                    LastModifiedDate = createdDate
                };
                dbContext.Questions.Add(question);
                dbContext.SaveChanges();

                for (int x = 1; x < numberOfAnswerPerQuestion; x++)
                {
                    var numAns = dbContext.Answers.Add(new Answer()
                    {
                        QuestionId = question.Id,
                        Text = String.Format("This is a sample answer generated by the SeederDB class."),
                        Value = x,
                        CreatedDate = createdDate,
                        LastModifiedDate = createdDate
                    });
                   
                }
            }

            for (int i = 0; i < numberOfResults; i++)
            {
                dbContext.Results.Add(new Result()
                {
                    QuizId = quiz.Id,
                    Text = String.Format("This is a sample result generated by the SeedeDB class."),
                    MinValue =0,
                    MaxValue =0,
                    CreatedDate = createdDate,
                    LastModifiedDate = createdDate
                });
                
            }

            dbContext.SaveChanges();

        }

        #endregion
    }
}
