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
using System.Threading;
using System.Net.Http;

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
                    HttpClient executor = new HttpClient();
                    executor.BaseAddress = new Uri("http://localhost:50519/api/execute/");
                    var responseTask = executor.GetAsync("execute?templatePath="+templateModel.path+"&templateType="+templateModel.type+"&dataPath="+path);
                    responseTask.Wait();
                    HttpResponseMessage result = responseTask.Result;
                    var readTask = result.Content.ReadAsAsync<string>();
                    readTask.Wait();
                    string answer = readTask.Result;
                    if (result.IsSuccessStatusCode) {
                        string type = "application/zip";
                        HttpContext.Response.ContentType = type;
                        FileContentResult file = new FileContentResult(System.IO.File.ReadAllBytes(answer), type);
                        file.FileDownloadName = "solutions.zip";
                        return file;
                    } else {
                        return StatusCode(500, answer);
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

        private string downloadDirectory(IFormFile file, int ownerId, int groupId, String name) {
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