using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TemplateHandler.Models;

namespace TemplateHandler.Controllers{
    [Route("api/[controller]")]
    public class TemplateFileController : Controller{
        /*public IActionResult Index() {
            ConnectionContext context = HttpContext.RequestServices.GetService(typeof(ConnectionContext)) as ConnectionContext;
            return View(context.getTemplateFiles());
        }*/
        [HttpGet("[action]")]
        public IEnumerable<TemplateFile> getTemplates() {
            ConnectionContext context = HttpContext.RequestServices.GetService(typeof(ConnectionContext)) as ConnectionContext;
            return context.getTemplateFiles();
        }
    }
}