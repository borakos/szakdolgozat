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

namespace TemplateHandler.Controllers{
    [Route("api/templatefiles")]
    public class TemplateFileController : Controller{
        private TemplateFileContext context;

        public TemplateFileController() {
            context = ConnectionContext.Instace.createTemplateFileContext();
        }

        [HttpGet, Route("index/{id}"), Authorize]
        public IEnumerable<GrouppedTemplatesModel> index(int id) {
            return context.getGroupedTemplate(id);
        }

        [HttpGet, Route("details/{id}"), Authorize]
        public GroupDataModel details(int id) {
            GroupDataModel data = new GroupDataModel();
            data.templates = context.getTemplatesInGroup(id);
            data.group = context.getGroup(id);
            return data;
        }

        [HttpPost, Route("create"), Authorize]
        public void create([FromBody] GrouppedTemplatesModel group) {
            context.createGroup(group);
        }

        [HttpPut, Route("edit/{method}"), Authorize]
        public void edit([FromBody] GroupDataModel groupData, string method) {
            context.editGroup(groupData.group);
            if (method == "all") {
                context.createTemplate(groupData.templates[1]);
            }
        }

        [HttpDelete, Route("deletegroup/{id}"), Authorize]
        public void delete(int id) {
            context.deleteGroup(id);
        }

        [HttpGet, Route("detailstemplates/{id}"), Authorize(Roles = "admin")]
        public IEnumerable<TemplateFileModel> details2(int id) {
            return context.getTemplatesInGroup(id);
        }

        [HttpGet, Route("group/{id}"), Authorize(Roles = "admin")]
        public GrouppedTemplatesModel getGroup(int id) {
            return context.getGroup(id);
        }

        [HttpGet, Route("teszt/{groupName}/{oid}"), Authorize]
        public Boolean teszt(string groupName, int oid) {
            return context.tesztGroupName(groupName, oid);
        }

        [HttpDelete, Route("removetemplate/{id}/{gid}/{set}"), Authorize]
        public void removeTemplate(int id, int gid, bool set) {
            context.removeTemplate(id);
            if (set) {
                context.setMaxVersionDefault(gid);
            }
        }

        [HttpPost, Route("upload"), Authorize, DisableRequestSizeLimit]
        public IActionResult upload() {
            try {
                IFormFile file = Request.Form.Files[0];
                String pathToSave = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("Resources", "Templates"));
                if (file.Length > 0) {
                    String fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    String fullPath = Path.Combine(pathToSave, fileName);
                    FileStream stream = new FileStream(fullPath, FileMode.Create);
                    file.CopyTo(stream);
                    return Ok();
                } else {
                    return BadRequest();
                }
            } catch {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}