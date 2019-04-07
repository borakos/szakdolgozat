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
            try {
                string error = null;
                TemplateFileModel templateModel = context.getTemplate(templateId, out error);
                if (templateModel != null) {
                    string path = uploadData(Request.Form.Files[0], templateModel.ownerId, templateModel.groupId, templateModel.name, out error);
                    if (path != null) {
                        HttpClient executor = new HttpClient();
                        executor.BaseAddress = new Uri("http://localhost:50519/api/execute/");
                        var responseTask = executor.GetAsync("execute?templatePath=" + templateModel.path + "&templateType=" + templateModel.type + "&dataPath=" + path);
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
                            return StatusCode(500, "[ExecutionController/execute] " + answer);
                        }
                    } else if (error == null) {
                        return StatusCode(500, "[ExecutionController/execute] File upload failed.");
                    } else {
                        return StatusCode(500, "[ExecutionController/execute] " + error);
                    }
                } else if (error == null) {
                    return StatusCode(500, "[ExecutionController/execute] Given group not exist.");
                } else {
                    return StatusCode(500, "[ExecutionController/execute] " + error);
                }
            } catch (Exception ex) {
                return StatusCode(500, "[ExecutionController/execute] " + ex.Message);
            }
        }
       
        private string uploadData(IFormFile file, int ownerId, int groupId, String name, out string error) {
            try {
                String fullPath = null;
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
                    error = null;
                    return fullPath;
                } else {
                    error = null;
                    return null;
                }
            } catch (Exception ex) {
                error = "[ExecutionController/uploadData] " + ex.Message;
                return null;
            }
        }

        private string downloadDirectory(IFormFile file, int ownerId, int groupId, String name, out string error) {
            try {
                String fullPath = null;
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
                    error = null;
                    return fullPath;
                } else {
                    error = null;
                    return fullPath;
                }
            } catch (Exception ex) {
                error = "[ExecutionController/downloadDirectory] " + ex.Message;
                return null;
            }
        }
    }
}