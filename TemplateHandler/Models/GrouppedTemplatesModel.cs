using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateHandler.Models {
    public class GrouppedTemplatesModel {
        public int id { get; set; }
        public string groupName { get; set; }
        public string description { get; set; }
        public int latestVersion { get; set; }
        public int fileNumber { get; set; }
        public int defaultVersion { get; set; }
        public int ownerId { get; set; }
        public string ownerName { get; set; }
    }
}
