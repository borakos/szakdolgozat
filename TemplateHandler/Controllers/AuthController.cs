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
using TemplateHandler.Connection;

namespace TemplateHandler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost, Route("login")]
        public IActionResult login([FromBody]LoginModel user) {
            try {
                if (user == null) {
                    return BadRequest("Invalid user parameters.");
                }
                UserContext context = ConnectionContext.Instace.createUserContext();
                string error = null;
                if (context.validateUser(user.userName, user.password, out error)) {
                    UserModel loggedInUser = context.getUserByUserName(user.userName, out error);
                    if (loggedInUser != null) {
                        SymmetricSecurityKey secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Startup.Configuration["Authentication:Key:SymmetricSecurityKey"]));
                        SigningCredentials signInCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                        List<Claim> claims = new List<Claim> {
                            new Claim("userName", loggedInUser.userName),
                            new Claim("role", loggedInUser.role.ToString()),
                            new Claim("nativeName", loggedInUser.nativeName),
                            new Claim("email", loggedInUser.email),
                            new Claim("id", loggedInUser.id.ToString())
                        };
                        JwtSecurityToken tokenOptions = new JwtSecurityToken(
                            issuer: Startup.Configuration["Server:Host"],
                            audience: Startup.Configuration["Server:Host"],
                            claims: claims,
                            expires: DateTime.Now.AddHours(1),
                            signingCredentials: signInCredentials
                        );
                        string tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
                        return Ok(new { token = tokenString });
                    } else if (error == null) {
                        return StatusCode(500, "[AuthController/login] Cannot find user.");
                    } else {
                        return StatusCode(500, "[AuthController/login]" + error);
                    }
                } else if (error == null) {
                    return StatusCode(401, "Invalid username or password");
                } else {
                    return StatusCode(500, "[AuthController/login]" + error);
                }
            }catch (Exception ex){
                return StatusCode(500, "[AuthController/login]" + ex.Message);
            }
        }
    }
}