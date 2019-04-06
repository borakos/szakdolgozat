using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using OfficeHandlerService.Office;
using Ionic.Zip;
using System.IO;
using System.Diagnostics;

namespace OfficeHandlerService.Controllers
{
    [RoutePrefix("api/execute")]
    public class ExecuteController : ApiController
    {
        public enum TemplateType { word, excel }


        [HttpGet, Route("hello/{name}")]
        public string hello(string name) {
            return "Hello "+name;
        }

        [HttpGet, Route("execute")]
        public IHttpActionResult execute(string templatePath, TemplateType templateType, string dataPath) {
            OfficeHandler officeHandler = null;
            string[] paths = dataPath.Split('.');
            string extension = paths[paths.Length - 1];
            switch (extension.ToLower()) {
                case "json": {
                        switch (templateType) {
                            case TemplateType.word: officeHandler = new WordHandlerJSON(); break;
                            default:
                                return Content(HttpStatusCode.InternalServerError, "Handler cannot identify the template type");
                        }
                    }
                    break;
                default:
                    return Content(HttpStatusCode.InternalServerError, "Handler cannot identify the extension of data type");
            }
            string response = officeHandler.parse(dataPath);
            paths = dataPath.Split('\\');
            string destination = "";
            for (int i = 0; i < paths.Length - 1; i++) {
                destination += paths[i] + "\\";
            }
            destination += "solutions";
            if (response == null) {
                response = officeHandler.execute(templatePath.Replace('/', '\\'), destination);
                if (response == null) {
                    ZipFile zip = new ZipFile();
                    zip.AddDirectory(destination);
                    zip.Save(destination + "\\..\\solutions.zip");
                    if (Directory.Exists(destination)) {
                        Directory.Delete(destination, true);
                    }
                    if (File.Exists(dataPath)) {
                        File.Delete(dataPath);
                    }
                    return Content(HttpStatusCode.OK, destination + "\\..\\solutions.zip");
                } else {
                    return Content(HttpStatusCode.InternalServerError, "Execution error: " + response);
                }
            } else {
                return Content(HttpStatusCode.InternalServerError, "Parse error: " + response);
            }
        }
    }
}
