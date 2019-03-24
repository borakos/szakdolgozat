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
        public bool create([FromBody] GrouppedTemplatesModel group) {
            return context.createGroup(group);
        }

        [HttpPut, Route("edit/{method}"), Authorize, DisableRequestSizeLimit]
        public IActionResult edit(string method, int groupId, string description, string groupName, int defaultVersion, string templateName, TemplateFileModel.Type templateType) {
            context.editGroup(groupId, description, groupName, defaultVersion);
            if (method == "all") {
                int ownerId = context.getOwnerOfGroup(groupId);
                if (ownerId != -1) {
                    int version = context.getNextVersionOfGroup(groupId);
                    if (version != -1) {
                        string path = uploadTemplate(Request.Form.Files[0],ownerId, groupId, templateName, version);
                        if (path != null) {
                            context.createTemplate(groupId, ownerId, version, templateName, templateType, path);
                        } else {
                            return StatusCode(500, "Cannot save template file");
                        }
                    } else {
                        return StatusCode(500, "Version of group cannot be set");
                    }
                } else {
                    return StatusCode(500, "Owner of given group not exist");
                }
            }
            return Ok();
        }

        private string uploadTemplate(IFormFile file, int ownerId, int groupId, String name, int version) {
            String fullPath = null;
            try {
                String pathToSave = Path.Combine(Directory.GetCurrentDirectory(), "Resources\\Templates\\" + ownerId + "\\" + groupId);
                if (!Directory.Exists(pathToSave)) {
                    Directory.CreateDirectory(pathToSave);
                }
                if (file.Length > 0) {
                    String fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    String[] fileNameExtension = fileName.Split(".");
                    fileNameExtension[0] = ownerId + "_" + groupId + "_" + version + "_" + name;
                    fullPath = Path.Combine(pathToSave, String.Join(".", fileNameExtension));
                    FileStream stream = new FileStream(fullPath, FileMode.Create);
                    file.CopyTo(stream);
                    stream.Close();
                    return fullPath;
                } else {
                    return fullPath;
                }
            } catch {
                return fullPath;
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

        [HttpDelete, Route("removetemplate/{id}"), Authorize]
        public IActionResult removeTemplate(int id) {
            int groupId = context.removeTemplate(id);
            if (groupId != -1) {
                if (context.setMaxVersionDefault(groupId)) {
                    return Ok();
                } else {
                    return StatusCode(500, "Group or templates not exist");
                }
            } else {
                return StatusCode(500, "Group not exist");
            }
        }
    }
}