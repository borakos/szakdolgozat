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
        public IEnumerable<UserGroupModel> index() {
            return context.getAllUserGroups();
        }

        [HttpGet, Route("details/{id}"), Authorize(Roles = "admin")]
        public UserGroupModel details(int id) {
            return context.getUserGroupById(id);
        }

        [HttpGet, Route("members/{id}"), Authorize(Roles = "admin")]
        public IEnumerable<MemberModel> members(int id) {
            return context.getUserGroupMember(id);
        }

        [HttpGet, Route("teszt/{groupName}"),Authorize(Roles = "admin")]
        public Boolean teszt(string groupName) {
            UserGroupModel group = context.getUserGroupByGroupName(groupName);
            if (group == null) {
                return true;
            }
            return false;
        }

        [HttpPost, Route("create"), Authorize(Roles = "admin")]
        public Boolean create([FromBody] UserGroupModel group) {
            group.realGroup = true;
            return context.createGroup(group);
        }

        [HttpPut, Route("edit/{method}"), Authorize(Roles = "admin")]
        public IActionResult edit(string method, int groupId, string description, string groupName, int userId, int rights) {
            context.editGroup(groupId, description, groupName);
            if (method == "all") {
                context.createMember(groupId, userId, rights);
            }
            return Ok();
        }

        [HttpDelete, Route("delete/{id}"), Authorize(Roles = "admin")]
        public Boolean delete(int id) {
            return context.deleteUserGroup(id);
        }

        [HttpDelete, Route("removeuser/{id}"), Authorize(Roles = "admin")]
        public Boolean removeUser(int id) {
            return context.removeUser(id);
        }
    }
}