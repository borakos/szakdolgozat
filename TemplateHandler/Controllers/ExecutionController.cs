using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TemplateHandler.Models;
using TemplateHandler.Connection;
using TemplateHandler.Parsers;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;

namespace TemplateHandler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExecutionController : ControllerBase
    {
        private TemplateFileContext context;

        public ExecutionController() {
            context = ConnectionContext.Instace.createTemplateFileContext();
        }

        [HttpPost, Route("execute"), Authorize, DisableRequestSizeLimit]
        public IActionResult execute(int templateId) {
            TemplateFileModel templateModel = context.getTemplate(templateId);
            if (templateModel != null) {
                string path = uploadData(Request.Form.Files[0], templateModel.ownerId, templateModel.groupId, templateModel.name);
                if (path != null) {
                    OfficeHandler officeHandler = null;
                    string[] paths = path.Split('.');
                    string extension = paths[paths.Length - 1];
                    switch (extension.ToLower()) {
                        case "json": {
                                switch (templateModel.type) {
                                    case TemplateFileModel.Type.word: officeHandler = new WordHandlerJSON(); break;
                                    default:
                                        return StatusCode(500, "Handler cannot identify the type");
                                }
                        } break;
                        default:
                            return StatusCode(500, "Handler cannot identify the extension");
                    }
                    string response = officeHandler.parse(path);
                    paths = path.Split('\\');
                    string destination = "";
                    for(int i = 0; i < paths.Length - 1; i++) {
                        destination += paths[i]+"\\";
                    }
                    destination += "solutions";
                    if (response == null) {
                        response = officeHandler.execute(templateModel.path.Replace('/', '\\'),destination);
                        if (response == null) {
                            return Ok();
                        } else {
                            return StatusCode(500, "Execution error: "+response);
                        }
                    } else {
                        return StatusCode(500, "Parse error: " + response);
                    }
                } else {
                    return StatusCode(500, "File upload failed");
                }
            } else {
                return StatusCode(500, "Given group not exist");
            }
        }

        private string uploadData(IFormFile file, int ownerId, int groupId, String name) {
            String fullPath = null;
            try {
                String pathToSave = Path.Combine(Directory.GetCurrentDirectory(), "Resources\\Executions\\" + ownerId + "\\" + groupId + "\\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
                if (!Directory.Exists(pathToSave)) {
                    Directory.CreateDirectory(pathToSave);
                }
                if (file.Length > 0) {
                    String fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    String[] fileNameExtension = fileName.Split(".");
                    fileNameExtension[0] = ownerId + "_" + groupId + "_data_" + name;
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
    }
}