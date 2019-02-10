using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TemplateHandler.Connection;

namespace TemplateHandler.Models {
    public class TemplateFile {
        public enum Type { word, excel }
        public int id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string localName { get; set; }
        public Type type { get; set; }
        public int ownerId { get; set; }
        public int groupId { get; set; }
        public string ownerName { get; set; }
        public string groupName { get; set; }
    }
}
