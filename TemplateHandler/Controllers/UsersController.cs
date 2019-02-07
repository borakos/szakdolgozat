using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TemplateHandler.Models;
using TemplateHandler.Connection;

namespace TemplateHandler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private UserContext context;

        public UsersController() {
            context = ConnectionContext.Instace.createUserContext();
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
        public void create([FromBody] UserModel user) {
            context.createUser(user);
        }

        [HttpPut, Route("edit/{id}"), Authorize(Roles = "admin")]
        public void edit([FromBody] UserModel user,int id) {
            context.updateUserByUser(user,id);
        }

        [HttpDelete, Route("delete/{id}"), Authorize(Roles = "admin")]
        public void delete(int id) {
            context.deleteUser(id);
        }
    }
}