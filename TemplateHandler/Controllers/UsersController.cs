using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TemplateHandler.Models;

namespace TemplateHandler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private ConnectionContext context;

        public UsersController() {
            context = HttpContext.RequestServices.GetService(typeof(ConnectionContext)) as ConnectionContext;
        }

        [HttpGet, Route("index"), Authorize(Roles ="admin")]
        public IEnumerable<UserModel> index() {
            return context.getAllUsers();
        }

        [HttpGet, Route("details/{id}"), Authorize(Roles = "admin")]
        public UserModel details(int id) {
            return context.getUserById(id);
        }

        [HttpPost, Route("create"), Authorize(Roles = "admin")]
        public int create([FromBody] UserModel user) {
            return 1;
        }

        [HttpPut, Route("edit/{id}"), Authorize(Roles = "admin")]
        public int edit(int id) {
            return 1;
        }

        [HttpDelete, Route("delete/{id}"), Authorize(Roles = "admin")]
        public int delete(int id) {
            return 1;
        }
    }
}