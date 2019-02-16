using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateHandler.Models {
    public class GroupDataModel {
        public List<TemplateFileModel> templates { get; set; }
        public GrouppedTemplatesModel group { get; set; }
    }
}
