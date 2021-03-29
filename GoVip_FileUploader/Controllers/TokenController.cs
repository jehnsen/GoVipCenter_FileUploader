using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using GoVip_FileUploader.ViewModels;
using GoVip_FileUploader.Data;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GoVip_FileUploader.Data.Models;

namespace GoVip_FileUploader.Controllers
{
    public class TokenController : BaseApiController
    {
        public TokenController(
            ApplicationDBContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration
            )
            : base(context, roleManager, userManager, configuration) { }

        [HttpPost("Auth")]
        public async Task<IActionResult> Jwt([FromBody]TokenRequestViewModel model)
        {
            if (model == null) return new StatusCodeResult(500);

            switch (model.grant_type)
            {
                case "password":
                    return await GetToken(model);
                case "refresh_token":
                    return await RefreshToken(model);
                default:
                   //return http 401 - unauthorized
                   return new UnauthorizedResult();
            }

        }

        private async Task<IActionResult> GetToken(TokenRequestViewModel model)
        {
            try
            {
                //check if there's a user 
                var user = await UserManager.FindByNameAsync(model.username);
                //fallback to support email address
                if (user == null && model.username.Contains("@"))
                    user = await UserManager.FindByEmailAsync(model.username);

                if (user == null)
                {
                    // user not exist or password mismatch
                    return NotFound(new
                    {
                        Error = String.Format("username or password not found")
                    });
                }
                if (!await UserManager.CheckPasswordAsync(user, model.password))
                {
                    return new UnauthorizedResult();
                }

                // username & password matches, create & return the jwt token
                var rt = CreateRefreshToken(model.client_id, user.Id);

                //return new JsonResult(new { logInfo = rt, userInfo = user, modelInfo = model });

                //add the new refresh token to the database
                DbContext.Tokens.Add(rt);
                DbContext.SaveChanges();

                //build and return the access token
                var t = CreateAccessToken(user.Id, rt.Value);
                return Json(t);

            }
            catch (Exception ex)
            {
                //return new UnauthorizedResult();
                return new JsonResult(new { exception = ex});
            }

        }

        private async Task<IActionResult> RefreshToken(TokenRequestViewModel model)
        {
            try
            {
                //check if recieved refresh token exist for the given client id
                var rt = DbContext.Tokens.FirstOrDefault(t => 
                    t.ClientId == model.client_id 
                    && t.Value == model.refresh_token);
     
                if (rt == null)
                {
                    //refresh token not found or invalid
                    return new UnauthorizedResult();
                }

                //check if there's a user with the refresh token's userId
                var user = await UserManager.FindByIdAsync(rt.UserId);

                //if user not found
                if (user == null)
                {
                    return new UnauthorizedResult();
                }

                //genearate a new refresh token
                var rtNew = CreateRefreshToken(rt.ClientId, rt.UserId);

                //invalidate the old refresh token (delete)
                DbContext.Tokens.Remove(rt);

                //add the new refresh token
                DbContext.Tokens.Add(rtNew);

                //persist changes to database
                DbContext.SaveChanges();

                //create a new access token
                var response = CreateAccessToken(rtNew.UserId, rtNew.Value);

                //send to client
                return Json(response);
            }
            catch (Exception ex)   
            {
                //return new UnauthorizedResult();
                return new JsonResult(new { exception = ex });
            }

        }

        private Token CreateRefreshToken(string clientId, string userId)
        {
            return new Token()
            {
                ClientId = clientId,
                UserId = userId,
                Type = 0,
                Value = Guid.NewGuid().ToString("N"),
                CreatedDate = DateTime.UtcNow
            };
        }

        private TokenResponseViewModel CreateAccessToken(string userId, string refreshToken)
        {
            DateTime now = DateTime.UtcNow;

            //add the registered claims for JWT (RFC7519)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString())
            };

            var tokenExpirationMins = Configuration.GetValue<int>("Auth:Jwt:TokenExpirationInMinutes");
            var issuerSigningkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Auth:Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: Configuration["Auth:Jwt:Issuer"],
                audience: Configuration["Auth:Jwt:Audience"],
                claims: claims,
                notBefore: now,
                expires: now.Add(TimeSpan.FromMinutes(tokenExpirationMins)),
                signingCredentials: new SigningCredentials(issuerSigningkey, SecurityAlgorithms.HmacSha256)
                );

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new TokenResponseViewModel()
            {
                token = encodedToken,
                expiration = tokenExpirationMins,
                refresh_token = refreshToken
            };
        }

        public IActionResult FacebookSignIn()
        {
            //appId: 543658282996904
            //appSecret: 829e6b89b5ec984a68e0e7c286a65799
            //displayName: Eugune

            return new JsonResult(new { facebook = "" });
        }
    }
}
