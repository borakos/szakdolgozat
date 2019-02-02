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
        [HttpGet,Authorize(Roles ="admin")]
        public IEnumerable<string> Get() {
            return new string[] { "Teszt1", "Teszt2" };
        }

        [HttpPost, Route("thisuser"), Authorize(Roles = "admin")]
        public string GetThis([FromBody] string id) {
            return id;
        }
    }
}