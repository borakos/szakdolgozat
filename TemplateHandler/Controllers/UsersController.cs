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
using System.IO;
using System.Net.Http.Headers;

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
        public IActionResult index() {
            try {
                string error = null;
                UserModel[] list = context.getAllUsers(out error);
                if (list != null) {
                    return Ok(list);
                } else {
                    return StatusCode(500, "[UserController/index] " + error);
                }
            }catch(Exception ex) {
                return StatusCode(500, "[UserController/index] " + ex.Message);
            }
        }

        [HttpGet, Route("filter"), Authorize(Roles = "admin")]
        public IActionResult filter(string filter=null) {
            try {
                string error = null;
                if (filter == null) {
                    UserModel[] list = context.getAllUsers(out error);
                    if (list != null) {
                        return Ok(list);
                    } else {
                        return StatusCode(500, "[UserController/filter] " + error);
                    }
                } else {
                    UserModel[] list = context.getFilteredUsers(filter, out error);
                    if (list != null) {
                        return Ok(list);
                    } else {
                        return StatusCode(500, "[UserController/filter] " + error);
                    }
                }
            } catch (Exception ex) {
                return StatusCode(500, "[UserController/filter] " + ex.Message);
            }
        }

        [HttpGet, Route("details/{id}"), Authorize(Roles = "admin")]
        public IActionResult details(int id) {
            try { 
                string error = null;
                UserModel user = context.getUserById(id, out error);
                if (user != null) {
                    return Ok(user);
                } else if (error == null) {
                    return Ok(user);
                } else {
                    return StatusCode(500, "[UserController/details] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[UserController/details] " + ex.Message);
            }
        }

        [HttpGet, Route("teszt/{userName}"),Authorize]
        public IActionResult teszt(string userName) {
            try { 
                string error = null;
                UserModel user = context.getUserByUserName(userName, out error);
                if (user != null) {
                    return Ok(false);
                } else if (error == null) {
                    return Ok(true);
                } else {
                    return StatusCode(500, "[UserController/teszt] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[UserController/teszt] " + ex.Message);
            }
        }

        [HttpPost, Route("create"), Authorize(Roles = "admin")]
        public IActionResult create([FromBody] UserModel user) {
            try {
                string error = null;
                bool answer = context.createUser(user, out error);
                if (answer) {
                    return Ok(true);
                } else {
                    return StatusCode(500, "[UserController/create] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[UserController/create] " + ex.Message);
            }
        }

        [HttpPut, Route("edit/{id}/{cid}"), Authorize]
        public IActionResult edit([FromBody] UserModel user,int id, int cid) {
            try {
                string error = null;
                bool answer = context.updateUser(user, id, cid, out error);
                if (answer) {
                    return Ok(true);
                } else {
                    return StatusCode(500, "[UserController/edit] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[UserController/edit] " + ex.Message);
            }
        }

        [HttpDelete, Route("delete/{id}"), Authorize(Roles = "admin")]
        public IActionResult delete(int id) {
            try {
                string error = null;
                bool answer = context.deleteUser(id, out error);
                if (answer) {
                    return Ok(true);
                } else {
                    return StatusCode(500, "[UserController/delete] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[UserController/delete] " + ex.Message);
            }
        }
    }
}