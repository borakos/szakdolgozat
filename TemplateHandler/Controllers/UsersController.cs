using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TemplateHandler.Models;
using TemplateHandler.Connection;
using System.Diagnostics;

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

        [HttpGet, Route("teszt/{userName}"),Authorize]
        public Boolean teszt(string userName) {
            UserModel user = context.getUserByUserName(userName);
            if (user == null) {
                return true;
            }
            return false;
        }

        [HttpPost, Route("create"), Authorize(Roles = "admin")]
        public void create([FromBody] UserModel user) {
            context.createUser(user);
        }

        [HttpPut, Route("edit/{id}/{cid}"), Authorize]
        public void edit([FromBody] UserModel user,int id, int cid) {
            context.updateUser(user,id,cid);
        }

        [HttpDelete, Route("delete/{id}"), Authorize(Roles = "admin")]
        public void delete(int id) {
            context.deleteUser(id);
        }
    }
}