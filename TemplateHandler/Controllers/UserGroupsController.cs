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
    [Route("api/usergroups")]
    [ApiController]
    public class UserGroupsController : ControllerBase
    {
        private UserGroupContext context;

        public UserGroupsController() {
            context = ConnectionContext.Instace.createUserGroupContext();
        }

        [HttpGet, Route("index"), Authorize(Roles ="admin")]
        public IActionResult index() {
            try {
                string error = null;
                UserGroupModel[] list = context.getAllUserGroups(out error);
                if (list != null) {
                    return Ok(list);
                } else {
                    return StatusCode(500, "[UserGroupController/index] " + error);
                }
            }catch(Exception ex) {
                return StatusCode(500, "[UserGroupController/index] " + ex.Message);
            }
        }

        [HttpGet, Route("details/{id}"), Authorize(Roles = "admin")]
        public IActionResult details(int id) {
            try {
                string error = null;
                UserGroupModel user = context.getUserGroupById(id, out error);
                if (user != null) {
                    return Ok(user);
                } else if (error == null) {
                    return StatusCode(500, "[UserGroupController/details] Cannot find user group.");
                } else {
                    return StatusCode(500, "[UserGroupController/details] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[UserGroupController/details] " + ex.Message);
            }
        }

        [HttpGet, Route("members/{id}"), Authorize(Roles = "admin")]
        public IActionResult members(int id) {
            try {
                string error = null;
                MemberModel[] list = context.getUserGroupMember(id, out error);
                if (list != null) {
                    return Ok(list);
                } else {
                    return StatusCode(500, "[UserGroupController/members] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[UserGroupController/members] " + ex.Message);
            }
        }

        [HttpGet, Route("teszt/{groupName}"),Authorize(Roles = "admin")]
        public IActionResult teszt(string groupName) {
            try {
                string error = null;
                UserGroupModel group = context.getUserGroupByGroupName(groupName, out error);
                if (group != null) {
                    return Ok(false);
                } else if (error == null) {
                    return Ok(true);
                } else {
                    return StatusCode(500, "[UserGroupController/teszt] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[UserGroupController/teszt] " + ex.Message);
            }
        }

        [HttpPost, Route("create"), Authorize(Roles = "admin")]
        public IActionResult create([FromBody] UserGroupModel group) {
            try {
                string error = null;
                group.realGroup = true;
                bool answer = context.createGroup(group,out error);
                if (answer) {
                    return Ok(true);
                } else {
                    return StatusCode(500, "[UserGroupController/create] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[UserGroupController/create] " + ex.Message);
            }
        }

        [HttpPut, Route("edit/{method}"), Authorize(Roles = "admin")]
        public IActionResult edit(string method, int groupId, string description, string groupName, int userId, int rights) {
            try {
                string error = null;
                bool answer = context.editGroup(groupId, description, groupName, out error);
                if (answer) {
                    if (method == "all") {
                        answer = context.createMember(groupId, userId, rights, out error);
                        if (!answer) {
                            return StatusCode(500, "[UserGroupController/edit] " + error);
                        }
                    }
                    return Ok(true);
                } else {
                    return StatusCode(500, "[UserGroupController/edit] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[UserGroupController/edit] " + ex.Message);
            }
        }

        [HttpDelete, Route("delete/{id}"), Authorize(Roles = "admin")]
        public IActionResult delete(int id) {
            try {
                string error = null;
                bool answer = context.deleteUserGroup(id, out error);
                if (answer) {
                    return Ok(true);
                } else {
                    return StatusCode(500, "[UserGroupController/delete] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[UserGroupController/delete] " + ex.Message);
            }
        }

        [HttpDelete, Route("removeuser/{id}"), Authorize(Roles = "admin")]
        public IActionResult removeUser(int id) {
            try {
                string error = null;
                bool answer = context.removeUser(id, out error);
                if (answer) {
                    return Ok(true);
                } else {
                    return StatusCode(500, "[UserGroupController/removeUser] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[UserGroupController/removeUser] " + ex.Message);
            }
        }
    }
}