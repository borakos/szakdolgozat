using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateHandler.Models {
    public class TemplateFile {

        private ConnectionContext context;
        
        public int id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
    }
}
