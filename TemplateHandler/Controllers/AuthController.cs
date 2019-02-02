using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TemplateHandler.Models;

namespace TemplateHandler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost, Route("login")]
        public IActionResult login([FromBody]LoginModel user) {
            if (user == null) {
                //Debug.WriteLine("\n\nDidn't get user\n\n");
                return BadRequest("Invalid user parameters.");
            }
            //Debug.WriteLine("\n\nusername:"+user.userName+", password:"+user.password+"\n\n");
            ConnectionContext context = HttpContext.RequestServices.GetService(typeof(ConnectionContext)) as ConnectionContext;
            if (context.validateUser(user.userName, user.password)) {
                UserModel loggedInUser = context.getUserByUserName(user.userName);
                SymmetricSecurityKey secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Startup.Configuration["Authentication:Key:SymmetricSecurityKey"]));
                SigningCredentials signInCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                List<Claim> claims = new List<Claim> {
                    new Claim("userName", user.userName),
                    new Claim("role", loggedInUser.role.ToString())
                };
                JwtSecurityToken tokenOptions = new JwtSecurityToken(
                    issuer: Startup.Configuration["Server:Host"],
                    audience: Startup.Configuration["Server:Host"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(5),
                    signingCredentials: signInCredentials
                );
                string tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
                return Ok(new { token = tokenString });
            } else {
                return Unauthorized();
            }
        }
    }
}