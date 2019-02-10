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
        [HttpGet, Route("index"), Authorize(Roles = "admin")]
        public IEnumerable<TemplateFile> index() {
            return context.getAllTemplate(); ;
        }
    }
}