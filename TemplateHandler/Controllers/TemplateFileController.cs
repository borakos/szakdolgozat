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
        public IActionResult index(int id) {
            try {
                string error = null;
                GrouppedTemplatesModel[] list = context.getGroupedTemplate(id, out error);
                if (list != null) {
                    return Ok(list);
                } else {
                    return StatusCode(500, "[TemplateFileController/index] " + error);
                }
            }catch(Exception ex) {
                return StatusCode(500, "[TemplateFileController/index] " + ex.Message);
            }
        }

        [HttpGet, Route("indexexecution/{id}"), Authorize]
        public IActionResult indexExecution(int id) {
            try {
                string error = null;
                GrouppedTemplatesModel[] list = context.getGroupedTemplateExecution(id, out error);
                if (list != null) {
                    return Ok(list);
                } else {
                    return StatusCode(500, "[TemplateFileController/indexExecution] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[TemplateFileController/indexExecution] " + ex.Message);
            }
        }

        [HttpGet, Route("details/{id}"), Authorize]
        public IActionResult details(int id) {
            try {
                string error = null;
                GroupDataModel data = new GroupDataModel();
                data.templates = context.getTemplatesInGroup(id, out error);
                if (data.templates == null) {
                    return StatusCode(500, "[TemplateFileController/details] " + error);
                }
                data.group = context.getGroup(id, out error);
                if (data.group == null) {
                    return StatusCode(500, "[TemplateFileController/details] " + error);
                }
                return Ok(data);
            } catch (Exception ex) {
                return StatusCode(500, "[TemplateFileController/details] " + ex.Message);
            }
        }

        [HttpPost, Route("create"), Authorize]
        public IActionResult create([FromBody] GrouppedTemplatesModel group) {
            try {
                string error = null;
                if (context.createGroup(group, out error)) {
                    return Ok(true);
                } else {
                    return StatusCode(500, "[TemplateFileController/create] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[TemplateFileController/create] " + ex.Message);
            }
        }

        [HttpGet, Route("usergroups/{id}"), Authorize]
        public IActionResult usergroups(int id) {
            try {
                string error = null;
                UserGroupModel[] list = context.getUserGroups(id, out error);
                if (list != null) {
                    return Ok(list);
                } else {
                    return StatusCode(500, "[TemplateFileController/userGroups] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[TemplateFileController/userGroups] " + ex.Message);
            }
        }

        [HttpPut, Route("edit/{method}"), Authorize, DisableRequestSizeLimit]
        public IActionResult edit(string method, int groupId, string description, string groupName, int owner, int defaultVersion, string templateName, TemplateFileModel.Type templateType) {
            try {
                string error = null;
                if (context.editGroup(groupId, description, groupName, owner, defaultVersion, out error)) {
                    if (method == "all") {
                        int ownerId = context.getOwnerOfGroup(groupId, out error);
                        if (ownerId != -1) {
                            int version = context.getNextVersionOfGroup(groupId, out error);
                            if (version != -1) {
                                string path = uploadTemplate(Request.Form.Files[0], ownerId, groupId, templateName, version, out error);
                                if (path != null) {
                                    if (context.createTemplate(groupId, ownerId, version, templateName, templateType, path, out error)) {
                                        return Ok(true);
                                    } else {
                                        return StatusCode(500, "[TemplateFileController/edit] " + error);
                                    }
                                } else if (error == null) {
                                    return StatusCode(500, "[TemplateFileController/edit] Cannot save template file");
                                } else {
                                    return StatusCode(500, "[TemplateFileController/edit] " + error);
                                }
                            } else if (error == null) {
                                return StatusCode(500, "[TemplateFileController/edit] Version of group cannot be set");
                            } else {
                                return StatusCode(500, "[TemplateFileController/edit] " + error);
                            }
                        } else if (error == null) {
                            return StatusCode(500, "[TemplateFileController/edit] Owner of given group not exist");
                        } else {
                            return StatusCode(500, "[TemplateFileController/edit] " + error);
                        }
                    } else {
                        return Ok(true);
                    }
                } else {
                    return StatusCode(500, "[TemplateFileController/edit] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[TemplateFileController/edit] " + ex.Message);
            }
        }

        private string uploadTemplate(IFormFile file, int ownerId, int groupId, String name, int version, out string error) {
            try {
                String fullPath = null;
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
                    error = null;
                    return fullPath;
                } else {
                    error = null;
                    return null;
                }
            } catch (Exception ex) {
                error = "[TemplateFileController/uploadTemplate] " + ex.Message;
                return null;
            }
        }

        [HttpDelete, Route("deletegroup/{id}"), Authorize]
        public IActionResult delete(int id) {
            try {
                string error = null;
                if (context.deleteGroup(id, out error)) {
                    return Ok(true);
                } else if (error == null) {
                    return StatusCode(500, "[TemplateFileController/delete] Cannot delete template group.");
                } else {
                    return StatusCode(500, "[TemplateFileController/delete] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[TemplateFileController/delete] " + ex.Message);
            }
        }

        [HttpGet, Route("detailstemplates/{id}"), Authorize(Roles = "admin")]
        public IActionResult details2(int id) {
            try {
                string error = null;
                List<TemplateFileModel> list = context.getTemplatesInGroup(id, out error);
                if (list != null) {
                    return Ok(list);
                } else if(error == null) {
                    return StatusCode(500, "[TemplateFileController/details2] Cannot find template group.");
                } else {
                    return StatusCode(500, "[TemplateFileController/details2] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[TemplateFileController/details2] " + ex.Message);
            }
        }

        [HttpGet, Route("group/{id}"), Authorize(Roles = "admin")]
        public IActionResult getGroup(int id) {
            try {
                string error = null;
                GrouppedTemplatesModel user = context.getGroup(id, out error);
                if (user != null) {
                    return Ok(user);
                } else if(error == null) {
                    return StatusCode(500, "[TemplateFileController/getGroup] Cannot find template group.");
                } else {
                    return StatusCode(500, "[TemplateFileController/getGroup] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[TemplateFileController/getGroup] " + ex.Message);
            }
        }

        [HttpGet, Route("teszt/{groupName}/{oid}"), Authorize]
        public IActionResult teszt(string groupName, int oid) {
            try {
                string error = null;
                bool answer = context.tesztGroupName(groupName, oid, out error);
                if (error == null) {
                    return Ok(answer);
                } else {
                    return StatusCode(500, "[TemplateFileController/teszt] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[TemplateFileController/teszt] " + ex.Message);
            }
        }

        [HttpDelete, Route("removetemplate/{id}"), Authorize]
        public IActionResult removeTemplate(int id) {
            try {
                string error = null;
                int groupId = context.removeTemplate(id, out error);
                if (groupId != -1) {
                    if (context.setMaxVersionDefault(groupId, out error)) {
                        return Ok(true);
                    } else if (error == null) {
                        return StatusCode(500, "[TemplateFileController/removeTemplate] Group or templates not exist");
                    } else {
                        return StatusCode(500, "[TemplateFileController/removeTemplate] " + error);
                    }
                } else if(error == null) {
                    return StatusCode(500, "[TemplateFileController/removeTemplate] Group not exist");
                } else {
                    return StatusCode(500, "[TemplateFileController/removeTemplate] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[TemplateFileController/removeTemplate] " + ex.Message);
            }
        }
    }
}